using GuestsLogix.Business.Repository.Routes;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace GuestLogix.WebApi
{
    public class Bootstrapper
    {
        public static void Initialise()
        {
           
            var container = BuildUnityContainer();

            GlobalConfiguration.Configuration.DependencyResolver =  new Unity.WebApi.UnityDependencyResolver(container); ;

            
        }
        private static IUnityContainer BuildUnityContainer()
        {
            var container = new UnityContainer();

            container.LoadConfiguration();
            container.RegisterType<IRoutesRepository, MySqlRoutesRepository>();
            return container;
        }
    }
}