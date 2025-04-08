using Ninject;
using Ninject.Web.Common;
using Ninject.Web.Common.WebHost;
using Ninject.Web.Mvc;
using PrinterServiceWebPart.Repositories;
using PrinterServiceWebPart.Services;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace PrinterServiceWebPart
{
    public enum OrderStatusEnum
    {
        Not_Starded,
        In_Progress,
        Ready,
        Closed
    }

    public enum PrinterStateEnum
    {
        Free,
        Busy,
        Not_Responding
    }

    public enum MaterialTypeEnum
    {
        ABS,
        PLA,
        PETG,
        Nylon,
        TPU,
        PVA,
        HIPS,
        Composite
    }

    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            // Регистрация сервисов
            var kernel = new StandardKernel();
            RegisterServices(kernel);
            DependencyResolver.SetResolver(new NinjectDependencyResolver(kernel));
            MyDependencyResolver.Register(new AppConfigService());
            MyDependencyResolver.Register(new ClientRepository(MyDependencyResolver.Resolve<AppConfigService>()));
            MyDependencyResolver.Register(new OrderRepository(MyDependencyResolver.Resolve<AppConfigService>()));
            MyDependencyResolver.Register(new ModelRepository(MyDependencyResolver.Resolve<AppConfigService>()));
            MyDependencyResolver.Register(new ReviewRepository(MyDependencyResolver.Resolve<AppConfigService>()));
            MyDependencyResolver.Register(new FileService(MyDependencyResolver.Resolve<AppConfigService>()));
        }

        private void RegisterServices(IKernel kernel)
        {
            // Регистрация репозиториев и сервисов
            kernel.Bind<ClientRepository>().ToSelf().InRequestScope();
            kernel.Bind<OrderRepository>().ToSelf().InRequestScope();
            kernel.Bind<FileService>().ToSelf().InRequestScope();
            //kernel.Bind<ModelAnalyzer>().ToSelf().InRequestScope();
        }
    }
}
