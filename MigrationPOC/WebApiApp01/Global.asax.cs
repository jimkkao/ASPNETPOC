
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Xml;
using KeyVaultHelper;


namespace WebApiApp01
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        private readonly int minThreads = 2000;
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            ThreadPool.SetMinThreads(minThreads, minThreads);
            InitLog4Net();
        }

        protected XmlDocument UpdateLog4NetConfig(string connStr)
        { 
            XmlDocument doc = new XmlDocument();
            string configSetting = ConfigurationManager.AppSettings["logConfigFile"];
            string configFile = Server.MapPath(configSetting);
            doc.Load(configFile);
            var node = doc.DocumentElement.SelectSingleNode("/log4net/appender[@name='AzureBlobAppender']/param[@name='ConnectionString']");

            node.Attributes["value"].Value = connStr;

            return doc;

        }
        protected void InitLog4Net()
        {
            string connStr = GetConnectionStr();

            var doc = UpdateLog4NetConfig(connStr);

              log4net.Config.XmlConfigurator.Configure(doc.DocumentElement);  
            //log4net.Config.XmlConfigurator.Configure();
        }

        protected string GetKeyVaultUrl()
        {
            return ConfigurationManager.AppSettings["kvUrl"];
        }

        protected string GetConnectionStr()
        {
            string kvUrl = GetKeyVaultUrl();
            var kvHelper = new KeyVaultClientHelper();

            var connstr = kvHelper.GetSecretAsync(kvUrl, "storageConnStr").Result;

            return connstr;
        }
    }
}
