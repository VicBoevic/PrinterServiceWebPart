using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace PrinterServiceWebPart.Models
{
    public class Review
    {
        [Column("review_id")]
        public Guid Id { get; set; }
        [Column("order_id")]
        public Guid OrderId { get; set; }
        [Column("score")]
        public int Score { get; set; }
        [Column("review_text")]
        public string ReviewText { get; set; }


    }
}