using System.IO;
using System.Threading.Tasks;
namespace Ecommerce.Contracts.Services
{
    public interface IBlobStorageService
    {
        Task<string> UploadFileAsync(Stream fileStream, string fileName, string containerName, string contentType);
        Task DeleteFileAsync(string fileUrl, string containerName);
    }
}