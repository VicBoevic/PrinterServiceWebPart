using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace PrinterServiceWebPart.Models
{
    public class Material
    {
        [Column("material_id")]
        public Guid Id { get; set; }
        public MaterialTypeEnum MaterialType { get; set; }
        public string Name { get; set; }
        public int PriceMultiplier { get; set; }
    }
}