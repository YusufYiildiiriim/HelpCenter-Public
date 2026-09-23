using HelpCenter.Application.Common.Models;
using HelpCenter.Application.Interfaces;
using Microsoft.AspNetCore.Hosting;

namespace HelpCenter.Infrastructure.Services
{
    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _environment;
        private const string RootFolderName = "Uploads";

        public FileService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public Task<string> SaveFileAsync(FileUpload file, string folderPath, CancellationToken cancellationToken = default)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("Dosya boş olamaz.");

            // Create a unique file name
            return WriteAsync(file, folderPath, Guid.NewGuid().ToString(), cancellationToken);
        }

        public Task<string> SaveFileAsAsync(FileUpload file, string folderPath, string fileNameWithoutExtension, CancellationToken cancellationToken = default)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("Dosya boş olamaz.");

            if (string.IsNullOrWhiteSpace(fileNameWithoutExtension))
                throw new ArgumentException("Dosya adı boş olamaz.", nameof(fileNameWithoutExtension));

            return WriteAsync(file, folderPath, fileNameWithoutExtension, cancellationToken);
        }

        private async Task<string> WriteAsync(FileUpload file, string folderPath, string fileNameWithoutExtension, CancellationToken cancellationToken)
        {
            // Root Uploads folder and subfolder path
            string uploadsPath = Path.Combine(_environment.ContentRootPath, RootFolderName);
            string finalDirectory = Path.Combine(uploadsPath, folderPath);

            if (!Directory.Exists(finalDirectory))
            {
                Directory.CreateDirectory(finalDirectory);
            }

            string fileName = fileNameWithoutExtension + Path.GetExtension(file.FileName);
            string filePath = Path.Combine(finalDirectory, fileName);

            await using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream, cancellationToken);
            }

            // Return the web access path (e.g.: /Uploads/MessageDocuments/...)
            // Path.Combine may use \ or / depending on the OS; for the web we must always use /.
            string webPath = "/" + RootFolderName + "/" + folderPath.Replace("\\", "/") + "/" + fileName;
            return webPath.Replace("//", "/");
        }

        public void DeleteFile(string path)
        {
            if (string.IsNullOrEmpty(path)) return;

            // If the path is a web path, convert it to a physical path
            string physicalPath = path;
            if (path.StartsWith("/"))
            {
                physicalPath = Path.Combine(_environment.ContentRootPath, path.TrimStart('/'));
            }

            if (File.Exists(physicalPath))
            {
                File.Delete(physicalPath);
            }
        }

        public void EnsureDirectoryExists(string folderPath)
        {
            string uploadsPath = Path.Combine(_environment.ContentRootPath, RootFolderName);
            string finalDirectory = Path.Combine(uploadsPath, folderPath);

            if (!Directory.Exists(finalDirectory))
            {
                Directory.CreateDirectory(finalDirectory);
            }
        }
    }
}
