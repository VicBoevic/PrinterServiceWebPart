using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace PrinterServiceWebPart.Models
{
    public class Order
    {
        [Column("order_id")]
        public Guid Id { get; set; }
        [Column("status")]
        public string Status { get; set; }
        public Guid ClientId { get; set; }
        [Column("create_date")]
        public DateTime CreateDate { get; set; }
        [Column("price")]
        public decimal Price { get; set; }
        [Column("comment")]
        public string Comment { get; set; }
        [Column("order_name")]
        public string OrderName { get; set; }

        public bool CanBeCanceled =>
            Status == "Not_Starded" ||
            Status == "In_Progress" ||
            Status == "Ready";

    }
}