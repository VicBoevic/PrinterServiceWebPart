using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PrinterServiceWebPart.Models
{
    public class Model
    {
        public Guid ModelId { get; set; }
        public Guid OrderId { get; set; }
        public Guid MaterialId { get; set; }
        public string Filepath { get; set; }
        public int PolygonNumber { get; set; }
        public int RequiredMatQuantity { get; set; }
    }
}