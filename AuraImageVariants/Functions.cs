using Azure.Messaging.EventGrid;
using Azure.Messaging.EventGrid.SystemEvents;
using Azure.Storage.Blobs;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Processing;
using System.Runtime.CompilerServices;
using SixLabors.ImageSharp.Formats.Webp;
using System.Text.RegularExpressions;

[assembly: InternalsVisibleTo("AuraImageVariants.Tests")]
namespace AuraImageVariants;

public static class Functions
{
    private const char Terminator = ';';
    private const char OptionSeparator = ',';
    private const char PropertySeparator = '=';
    private const string DefaultVariantNameSeparator = "_";

    private static readonly string WidthProperty = "w" + PropertySeparator;
    private static readonly string HeightProperty = "h" + PropertySeparator;
    private static readonly string NameProperty = "name" + PropertySeparator;

    private static readonly string BLOB_STORAGE_CONNECTION_STRING = Environment.GetEnvironmentVariable("AzureWebJobsStorage")!;

    [FunctionName("AuraVariantGenerator")]
    public static async Task Run([EventGridTrigger] EventGridEvent eventGridEvent, [Blob("{data.url}", FileAccess.Read)] Stream input, ILogger logger)
    {
        try
        {
            if (input is null)
                return;

            logger.LogInformation("Reading blob creation info");
            var blobCreationEvent = eventGridEvent.Data.ToObjectFromJson<StorageBlobCreatedEventData>();
            var fileExtension = Path.GetExtension(blobCreationEvent.Url).Replace(".", string.Empty);
            logger.LogInformation("Blob Url {BlobName}", blobCreationEvent.Url);

            // If the input file extension is not something we support, end the function.
            if (!Regex.IsMatch(fileExtension, "gif|png|jpe?g|webp", RegexOptions.IgnoreCase))
                return;

            var convertTo = Environment.GetEnvironmentVariable("AIV_OUTPUT_TYPE");
            if (convertTo is not null)
                fileExtension = convertTo;

            logger.LogInformation("Acquiring encoder");
            // If the file extension isn't of a supported format, end the function.
            var encoder = GetEncoder(fileExtension.ToLower());
            if (encoder is null)
                return;

            var originalBlobClient = new BlobClient(new Uri(blobCreationEvent.Url));
            var outputContainerName = Environment.GetEnvironmentVariable("AIV_OUTPUT_CONTAINER") ?? originalBlobClient.BlobContainerName;

            var blobServiceClient = new BlobServiceClient(BLOB_STORAGE_CONNECTION_STRING);
            var blobContainerClient = blobServiceClient.GetBlobContainerClient(outputContainerName);
            var blobName = Path.GetFileNameWithoutExtension(originalBlobClient.Name);

            string variantSeparator = Environment.GetEnvironmentVariable("AIV_VARIANT_NAME_SEPARATOR") ?? DefaultVariantNameSeparator;

            var variantsString = Environment.GetEnvironmentVariable("AIV_VARIANTS");
            var variants = variantsString is not null ? Parse(variantsString) : new VariantInfo[] { new VariantInfo() };

            logger.LogInformation("Starting variant construction");
            foreach (var variant in variants)
            {
                input.Position = 0;
                using MemoryStream outputStream = new();
                using Image image = Image.Load(input);
                var newSize = GetResizedDimensions(image.Size(), variant.Width, variant.Height);
                image.Mutate(ctx => ctx.Resize(newSize));
                image.Save(outputStream, encoder);
                outputStream.Position = 0; // Reset the stream position so it can be properly uploaded

                var finalBlobName = $"{blobName}{variantSeparator}{variant.Name}.{fileExtension}";
                logger.LogInformation("Uploading {BlobName}", finalBlobName);
                await blobContainerClient.UploadBlobAsync(finalBlobName, outputStream);
            }
        }
        catch (Exception e)
        {
            logger.LogInformation(e.Message);
            throw;
        }
    }

    internal static VariantInfo[] Parse(string rawVariantString)
    {
        // Remove all spaces, as they are banned for the variant string.
        rawVariantString = rawVariantString.Replace(" ", string.Empty);

        var variantStrings = rawVariantString.Split(Terminator, StringSplitOptions.RemoveEmptyEntries);
        if (variantStrings.Length == 0) // We don't need to allocate an empty array each time.
            return Array.Empty<VariantInfo>();

        List<VariantInfo> variants = new();
        for (int i = 0; i < variantStrings.Length; i++)
        {
            VariantInfo variant = new();
            var variantString = variantStrings[i];
            var options = variantString.Split(OptionSeparator, StringSplitOptions.RemoveEmptyEntries);
            foreach (var option in options)
            {
                // Only allow 1 option char per option.
                if (option.Count(o => o == PropertySeparator) != 1)
                    continue;

                if (option.StartsWith(WidthProperty))
                {
                    // Width
                    var widthStr = option[WidthProperty.Length..];
                    variant.Width = Convert.ToInt32(widthStr);
                }
                else if (option.StartsWith(HeightProperty))
                {
                    // Height
                    var heightStr = option[HeightProperty.Length..];
                    variant.Height = Convert.ToInt32(heightStr);
                }
                else if (option.StartsWith(NameProperty))
                {
                    // Name
                    var nameStr = option[NameProperty.Length..];
                    variant.Name = nameStr;
                }
            }

            // Prevent duplicates from being added.
            if (variants.Any(v => v.Name.Equals(variant.Name, StringComparison.OrdinalIgnoreCase)))
                break;

            variants.Add(variant);
        }
        return variants.ToArray();
    }

    internal static Size GetResizedDimensions(Size originalSize, int? targetWidth, int? targetHeight)
    {
        // If there is no target dimension, return the original size.
        if (!targetWidth.HasValue && !targetHeight.HasValue)
            return originalSize;

        var targetIsComplete = targetWidth.HasValue && targetHeight.HasValue;

        // Do not resize to zero.
        if ((targetWidth.HasValue && targetWidth.Value <= 0) || (targetHeight.HasValue && targetHeight.Value <= 0))
            return originalSize;

        // Do not resize complete images that are smaller than the requested target.
        if (targetIsComplete && (targetWidth!.Value > originalSize.Width || targetHeight!.Value > originalSize.Height))
            return originalSize;

        // If the target width is larger than or the same size as the original, return the original.
        if (!targetIsComplete && targetWidth.HasValue && targetWidth.Value >= originalSize.Width)
            return originalSize;

        // If the target height is larger than or the same size as the original, return the original.
        if (!targetIsComplete && targetHeight.HasValue && targetHeight.Value >= originalSize.Height)
            return originalSize;

        // Handle square images. The process is easier for 1:1 aspect ratios.
        if (targetIsComplete && targetWidth!.Value == targetHeight!.Value)
            return new Size(targetWidth!.Value, targetHeight!.Value);

        var actualWidth = targetWidth ?? originalSize.Width / (originalSize.Height / (decimal)targetHeight!.Value);
        var actualHeight = targetHeight ?? originalSize.Height / (originalSize.Width / (decimal)targetWidth!.Value);
        
        // Convert the width and height so their values are rounded.
        return new Size(Convert.ToInt32(actualWidth), Convert.ToInt32(actualHeight));
    }

    internal static IImageEncoder? GetEncoder(string extension)
    {
        return extension switch
        {
            "png" => new PngEncoder(),
            "jpg" => new JpegEncoder(),
            "jpeg" => new JpegEncoder(),
            "gif" => new GifEncoder(),
            "webp" => new WebpEncoder(),
            _ => null
        };
    }
}