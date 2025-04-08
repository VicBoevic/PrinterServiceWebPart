using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using Npgsql;
using Microsoft.Extensions.Configuration;
using PrinterServiceWebPart.Models;
using PrinterServiceWebPart.Services;

namespace PrinterServiceWebPart.Repositories
{
    
    public class OrderRepository
    {
        private readonly string _connectionString;

        public OrderRepository(AppConfigService config)
        {
            // Получаем строку подключения из конфигурации
            _connectionString = config.GetConnectionString();
        }

        // Метод для создания нового заказа
        public async Task<Guid> CreateOrderAsync(Guid clientId, List<string> filePaths, string orderName, string comment, Guid materialId, decimal price)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                // Начинаем транзакцию
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Создаем заказ
                        Guid orderId = await connection.ExecuteScalarAsync<Guid>(
                            @"INSERT INTO ""Order"" (order_name, client_id, status, create_date, price, comment)
                              VALUES (@OrderName, @ClientId, 'Not_Starded'::order_status_enum, @CreateDate, @Price, @Comment)
                              RETURNING order_id;",
                            new { OrderName = orderName, ClientId = clientId, CreateDate = DateTime.UtcNow, Price = price, Comment = comment },
                            transaction);

                        // Добавляем модели к заказу
                        foreach (var filePath in filePaths)
                        {

                            await connection.ExecuteAsync(
                                @"INSERT INTO model (order_id, material_id, filepath)
                                  VALUES (@OrderId, @MaterialId, @FilePath);",
                                new
                                {
                                    OrderId = orderId,
                                    MaterialId = materialId,
                                    FilePath = filePath,
                                },
                                transaction);
                        }

                        // Фиксируем транзакцию
                        await transaction.CommitAsync();

                        return orderId;
                    }
                    catch
                    {
                        // Откатываем транзакцию в случае ошибки
                        await transaction.RollbackAsync();
                        throw;
                    }
                }
            }
        }

        // Метод для получения списка заказов клиента
        public async Task<IEnumerable<Order>> GetOrdersByClientIdAsync(Guid clientId)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var orders = await connection.QueryAsync<Order>(
                   @"SELECT order_id::uuid AS Id, 
                             client_id::uuid AS ClientId, 
                             status::text AS Status, 
                             price::numeric AS Price, 
                             create_date::timestamp AS CreateDate,
                             order_name::text AS OrderName
                      FROM ""Order""
                      WHERE client_id = @ClientId;",
                    new { ClientId = clientId });

                return orders;
            }
        }

        public IEnumerable<Model> GetOrderModels(Guid orderId)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                return connection.Query<Model>(
                    "SELECT * FROM model WHERE order_Id = @OrderId",
                    new { OrderId = orderId }
                );
            }
        }

        // Метод для обновления статуса заказа
        public async Task UpdateOrderStatusAsync(Guid orderId, string newStatus)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                await connection.ExecuteAsync(
                    @"UPDATE orders
                      SET status = @NewStatus
                      WHERE id = @OrderId;",
                    new { OrderId = orderId, NewStatus = newStatus });
            }
        }

        public async Task<bool> CancelOrderAsync(Guid orderId, Guid clientId)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                var result = await connection.ExecuteAsync(@"
                UPDATE ""Order"" 
                SET status = 'Canceled' 
                WHERE 
                    order_id = @OrderId 
                    AND client_id = @ClientId
                    AND status IN ('Not_Starded', 'In_Progress', 'Ready')",
                    new
                    {
                        OrderId = orderId,
                        ClientId = clientId
                    });

                return result > 0;
            }
        }
    }
}