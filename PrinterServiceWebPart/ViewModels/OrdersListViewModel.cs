using PrinterServiceWebPart.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PrinterServiceWebPart.ViewModels
{
    public class OrdersListViewModel
    {
        public string ClientName { get; set; }
        public IEnumerable<OrderViewModel> Orders { get; set; }
    }
}