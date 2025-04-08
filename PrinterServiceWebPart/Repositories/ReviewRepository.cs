using Dapper;
using Npgsql;
using PrinterServiceWebPart.Models;
using PrinterServiceWebPart.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace PrinterServiceWebPart.Repositories
{
    public class ReviewRepository
    {
        private readonly string _connectionString;

        public ReviewRepository(AppConfigService config)
        {
            _connectionString = config.GetConnectionString();
        }

        public async Task AddReviewAsync(Review review)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.ExecuteAsync(@"
                INSERT INTO review (review_id, order_id, score, review_text)
                VALUES (@Id, @OrderId, @Score::score_enum, @ReviewText)",
                new
                {
                    Id = review.Id,
                    OrderId = review.OrderId,
                    Score = review.Score.ToString(),
                    ReviewText = review.ReviewText
                });
            }
        }

        public async Task<bool> HasReviewAsync(Guid orderId)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                return await connection.ExecuteScalarAsync<bool>(@"
                SELECT EXISTS(
                    SELECT 1 FROM review 
                    WHERE order_id = @OrderId)", 
                    new { OrderId = orderId});
            }
        }
    }
}