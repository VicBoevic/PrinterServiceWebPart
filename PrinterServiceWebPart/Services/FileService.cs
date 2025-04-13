using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Npgsql;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System.IO;
using Microsoft.AspNetCore.Http;

namespace PrinterServiceWebPart.Services
{
    public class FileService
    {
        private readonly string _uploadPath;
        private readonly long _maxFileSize;

        public FileService(AppConfigService config)
        {
            _uploadPath = Path.Combine(AppContext.BaseDirectory, config._configuration.GetValue<string>("FileSettings:UploadPath"));
            _maxFileSize = config._configuration.GetValue<long>("FileSettings:MaxFileSizeMB") * 1024 * 1024;

            Directory.CreateDirectory(_uploadPath);
        }

        public async Task<string> SaveFileAsync(HttpPostedFileBase file)
        {
            
            if (file == null || file.ContentLength == 0)
                throw new ArgumentException("Файл не предоставлен или пуст.");

            if (!Directory.Exists(_uploadPath))
                Directory.CreateDirectory(_uploadPath);

            // Очистка имени файла от недопустимых символов и путей
            var originalFileName = Path.GetFileName(file.FileName.Trim());
            var cleanedFileName = CleanFileName(originalFileName);

            // Генерация уникального имени файла
            var finalFileName = GetUniqueFileName(cleanedFileName);
            var filePath = Path.Combine(_uploadPath, finalFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.InputStream.CopyToAsync(stream);
                file.InputStream.Seek(0, SeekOrigin.Begin); // ← Перемотка
            }

            return finalFileName;
        }

        private string CleanFileName(string fileName)
        {
            // Удаление недопустимых символов
            var invalidChars = Path.GetInvalidFileNameChars();
            var cleaned = new string(fileName
                .Where(ch => !invalidChars.Contains(ch))
                .ToArray());

            // Замена пустого имени на "file"
            if (string.IsNullOrWhiteSpace(cleaned))
            {
                cleaned = "file" + Path.GetExtension(fileName) ?? "";
            }

            return cleaned;
        }

        private string GetUniqueFileName(string baseFileName)
        {
            var counter = 1;
            var fileName = baseFileName;
            var nameWithoutExt = Path.GetFileNameWithoutExtension(baseFileName);
            var extension = Path.GetExtension(baseFileName);

            while (File.Exists(Path.Combine(_uploadPath, fileName)))
            {
                fileName = $"{nameWithoutExt} ({counter++}){extension}";
            }

            return fileName;
        }

    }  
}   