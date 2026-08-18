using Ecommerce.Contracts.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Ecommerce.BLL
{
    public class LocalStorageService : IBlobStorageService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public LocalStorageService(IWebHostEnvironment webHostEnvironment, IHttpContextAccessor httpContextAccessor)
        {
            _webHostEnvironment = webHostEnvironment;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string containerName, string contentType)
        {
            string webRoot = _webHostEnvironment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            string uploadsFolder = Path.Combine(webRoot, "uploads", containerName);

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            string filePath = Path.Combine(uploadsFolder, fileName);
            using (var destinationStream = new FileStream(filePath, FileMode.Create))
            {
                await fileStream.CopyToAsync(destinationStream);
            }

            var request = _httpContextAccessor.HttpContext?.Request;
            if (request != null)
            {
                string baseUrl = $"{request.Scheme}://{request.Host}";
                return $"{baseUrl}/uploads/{containerName}/{fileName}";
            }

            return $"/uploads/{containerName}/{fileName}";
        }

        public Task DeleteFileAsync(string fileUrl, string containerName)
        {
            try
            {
                string fileName = Path.GetFileName(new Uri(fileUrl, UriKind.RelativeOrAbsolute).LocalPath);
                string webRoot = _webHostEnvironment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                string filePath = Path.Combine(webRoot, "uploads", containerName, fileName);

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            catch
            {
                // Best effort file deletion
            }
            return Task.CompletedTask;
        }
    }
}
