using PrinterServiceWebPart.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PrinterServiceWebPart.ViewModels
{
    public class OrderViewModel
    {
        public Guid Id { get; set; }
        public string OrderName { get; set; }
        public string Status { get; set; }
        public string RussianStatus => OrderStatusHelper.GetRussianStatus(Status);
        public string Price { get; set; }
        public string CreateDate { get; set; }
        public IEnumerable<Model> Models { get; set; }
        public bool CanAddReview { get; set; }
        public bool HasReview { get; set; }
        public bool CanBeCanceled { get; set; }
    }
}