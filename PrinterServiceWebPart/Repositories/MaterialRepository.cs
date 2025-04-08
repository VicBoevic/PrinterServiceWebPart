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
    public class MaterialRepository
    {
        private readonly string _connectionString;

        public MaterialRepository(AppConfigService config)
        {
            _connectionString = config.GetConnectionString();
        }

        public IEnumerable<Material> GetAllMaterials()
        {
            var conn = new NpgsqlConnection(_connectionString);
            return conn.Query<Material>("SELECT material_id AS Id, material_name AS Name, material_type, price_multiplier AS PriceMultiplier FROM material");
        }

        public Material GetMaterialById(Guid materialId)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                string query = @"SELECT material_id AS Id, material_name AS Name, material_type, price_multiplier AS PriceMultiplier FROM material WHERE material_id = @MaterialId";
                return conn.QuerySingleOrDefault<Material>(query, new { MaterialId = materialId });
            }
        }

    }
}