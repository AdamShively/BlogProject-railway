namespace BlogProject.Services
{
    public class BasicImageService : IImageService
    {
        public string ContentType(IFormFile file)
        {
            return file?.ContentType;
        }

        public string DecodeImage(byte[] data, string fileType)
        {
            if (data == null || fileType == null)
            {
                return null;
            }
            return $"data:image/{fileType};base64,{Convert.ToBase64String(data)}";
        }

        public async Task<byte[]> EncodeImageAsync(IFormFile file)
        {
            if (file == null)
            {
                return null;
            }
            using var memStream = new MemoryStream();
            await file.CopyToAsync(memStream);
            return memStream.ToArray(); //return file as byte array.
        }

        public async Task<byte[]> EncodeImageAsync(string fileName)
        {
            var file = $"{Directory.GetCurrentDirectory()}/wwwroot/assets/img/{fileName}"; 
            return await File.ReadAllBytesAsync(file);
        }

        public int Size(IFormFile file)
        {
            return Convert.ToInt32(file?.Length);
        }
    }
}
