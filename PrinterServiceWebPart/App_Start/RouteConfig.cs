using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace PrinterServiceWebPart
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "OrdersList",
                url: "OrdersList",
                defaults: new { controller = "OrdersList", action = "Index" }
            );

            routes.MapRoute(
                name: "Model",
                url: "Model/Render/{modelId}",
                defaults: new { controller = "Model", action = "Render" }
            );

            routes.MapRoute(
                name: "AddReview",
                url: "Orders/AddReview/{orderId}",
                defaults: new { controller = "OrderList", action = "AddReview" }
            );

            routes.MapRoute(
               name: "Registration",
               url: "register",
               defaults: new { controller = "ClientAccount", action = "Register" }
           );
            
            routes.MapRoute(
                name: "CancelOrder",
                url: "OrderList/CancelOrder/{orderId}",
                defaults: new { controller = "OrdersList", action = "CancelOrder" }
            );

        }
    }
}
