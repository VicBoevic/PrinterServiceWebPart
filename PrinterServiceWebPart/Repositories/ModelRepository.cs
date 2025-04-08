using Dapper;
using Npgsql;
using PrinterServiceWebPart.Models;
using PrinterServiceWebPart.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PrinterServiceWebPart.Repositories
{
    public class ModelRepository
    {
        private readonly string _connectionString;

        public ModelRepository(AppConfigService config)
        {
            _connectionString = config.GetConnectionString();
        }

        public Model GetById(Guid modelId)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                return conn.QuerySingleOrDefault<Model>(
                    "SELECT * FROM model WHERE model_id = @ModelId",
                    new { ModelId = modelId }
                );
            }
        }
    }
}