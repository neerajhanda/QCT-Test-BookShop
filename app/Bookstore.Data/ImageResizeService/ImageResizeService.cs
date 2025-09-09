using Bookstore.Domain;
using System.IO;
using System.Threading.Tasks;

namespace Bookstore.Data.ImageResizeService
{
    public class ImageResizeService : IImageResizeService
    {
        private const int BookCoverImageWidth = 400;
        private const int BookCoverImageHeight = 600;

        public async Task<Stream> ResizeImageAsync(Stream image)
        {
            // TODO: Implement actual image resizing using ImageMagick or System.Drawing
            // For now, return the original image stream to allow the project to build
            
            if (image == null)
                throw new System.ArgumentNullException(nameof(image));

            // Reset position to beginning
            image.Position = 0;
            
            // Create a copy of the stream
            var result = new MemoryStream();
            await image.CopyToAsync(result);
            result.Position = 0;
            
            return result;
        }
    }
}