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
            if (!Directory.Exists(_uploadPath))
                Directory.CreateDirectory(_uploadPath);

            // Очищаем имя файла от недопустимых символов
            var originalFileName = Path.GetFileName(file.FileName);
            var safeFileName = string.Join("_", originalFileName.Split(Path.GetInvalidFileNameChars()));

            // Создаем уникальное имя файла
            var fileName = await GetUniqueFileNameAsync(safeFileName);
            var filePath = Path.Combine(_uploadPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.InputStream.CopyToAsync(stream);
            }

            return fileName;
        }

        private async Task<string> GetUniqueFileNameAsync(string fileName)
        {
            var count = 1;
            var newFileName = fileName;
            var extension = Path.GetExtension(fileName);
            var nameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);

            // Проверяем существование файла асинхронно
            while (await Task.Run(() => File.Exists(Path.Combine(_uploadPath, newFileName))))
            {
                newFileName = $"{nameWithoutExtension}_{count++}{extension}";
            }

            return newFileName;
        }
    }
}   