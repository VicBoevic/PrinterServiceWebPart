using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace PrinterServiceWebPart.Services
{
    public class AppConfigService
    {
        public readonly IConfiguration _configuration;

        public AppConfigService()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            _configuration = builder.Build();
        }

        public string GetConnectionString()
        {
            return _configuration.GetConnectionString("Default");
        }
    }
}