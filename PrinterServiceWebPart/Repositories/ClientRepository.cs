using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Npgsql;
using Microsoft.Extensions.Configuration;
using BCrypt.Net;
using PrinterServiceWebPart.Models;
using Dapper;
using PrinterServiceWebPart.Services;

namespace PrinterServiceWebPart.Repositories
{
    public class ClientRepository
    {
        private readonly string _connectionString;

        public ClientRepository(AppConfigService config)
        {
            _connectionString = config.GetConnectionString();
        }



        public void Register(Client client)
        {
            var conn = new NpgsqlConnection(_connectionString);
            conn.Execute(
                @"INSERT INTO client (fullname, phone_number, email, password)
            VALUES (@FullName, @PhoneNumber, @Email, @PasswordHash)",
                client);
        }

        public Client GetByEmail(string email)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                string query = @"SELECT client_id AS Id, email, password, phone_number AS PhoneNumber, fullname AS FullName FROM client WHERE email = @Email";
                var result = conn.QuerySingleOrDefault<Client>(query, new { Email = email });
                if (result != null)
                {
                    return new Client
                    {
                        Email = result.Email,
                        Password = result.Password,
                        PhoneNumber = result.PhoneNumber,
                        FullName = result.FullName,
                        Id = result.Id
                    };
                }
                return null;
            }
        }
    }
}