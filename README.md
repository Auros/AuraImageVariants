# AuraImageVariants
An Azure Function which can generate multiple image variants during a blob storage upload.

Only works on gifs, pngs, jpegs, and webp.

It will only resize if the uploaded image is larger than the target size.

## Environment Variables

`AIV_OUTPUT_CONTAINER` (string, required): The name of the storage container.

`AIV_VARIANTS` (string, required): The variants to generate.

* Every variant type is separated by `;`. Use key pair values to configure each variant.
* `w` is the width of the image (in pixels)
* `h` is the height of the image (in pixels)
* `name` is the name of the variant. Used in the generated file name.
* Example: `name=large,w=1024,h=1024;name=small,w=256,h=256`
* If you exclude the width or height, it will proportionally resize the image if the target width or height is smaller than the source image.
* If you exclude the name, it will default to `variant`

`AIV_OUTPUT_TYPE` (string, optional): The output format of all images uploaded. Any uploaded image will be converted to this format. Valid options: `gif`, `png`, `jpg`, `jpeg`, and `webp`. This also corresponds to the file extension.

`AIV_VARIANT_NAME_SEPARATOR` (string, optional): The separator of the name and variant name. Defaults to `_`

## Example

If you upload an image file called `MyCoolImage.png` with `AIV_OUTPUT_CONTAINER` set to `images` and `AIV_VARIANTS` set to `name=large,w=1024,h=1024;name=small,w=256,h=256`.

It will upload two images to the `images` container called `MyCoolImage_large.png` and `MyCoolImage_small.png`.

If your container has public address, the two images will have the URL path: 
* https://[your-service-name].blob.core.windows.net/images/MyCoolImage_large.png
* https://[your-service-name].blob.core.windows.net/images/MyCoolImage_small.png
