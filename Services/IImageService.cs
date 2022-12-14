namespace BlogProject.Services
{
    public interface IImageService
    {
        Task<byte[]> EncodeImageAsync(IFormFile file);
        Task<byte[]> EncodeImageAsync(string fileName);
        string DecodeImage(byte[] data, string fileType);
        string ContentType(IFormFile file); 
        int Size(IFormFile file);   
    }
}
