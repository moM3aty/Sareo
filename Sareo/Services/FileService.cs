using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Sareoo.Services
{
    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        public FileService(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<string> SaveFileAsync(IFormFile imageFile, string subfolder)
        {
            if (imageFile == null || imageFile.Length == 0)
            {
                return null;
            }

            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", subfolder);
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            string uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(fileStream);
            }

            return $"/uploads/{subfolder}/{uniqueFileName}";
        }

        // New Method Implementation
        public async Task<string> SaveBase64FileAsync(string base64String, string subfolder)
        {
            if (string.IsNullOrEmpty(base64String))
            {
                return null;
            }

            var base64DataMatch = Regex.Match(base64String, @"data:image/(?<type>.+?);base64,(?<data>.+)");
            if (!base64DataMatch.Success)
            {
                return null; // Invalid Base64 format
            }

            var fileType = base64DataMatch.Groups["type"].Value;
            var base64Data = base64DataMatch.Groups["data"].Value;
            var bytes = Convert.FromBase64String(base64Data);

            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", subfolder);
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            string uniqueFileName = Guid.NewGuid().ToString() + "." + fileType;
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            await File.WriteAllBytesAsync(filePath, bytes);

            return $"/uploads/{subfolder}/{uniqueFileName}";
        }


        public void DeleteFile(string imagePath)
        {
            if (string.IsNullOrEmpty(imagePath))
            {
                return;
            }

            string physicalPath = Path.Combine(_webHostEnvironment.WebRootPath, imagePath.TrimStart('/'));

            if (File.Exists(physicalPath))
            {
                File.Delete(physicalPath);
            }
        }
    }
}
