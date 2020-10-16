using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Threading.Tasks;

namespace WebApp01.Controllers
{
    public class HomeController : Controller
    {
        //static int _pageView = 0;

        public async Task<ActionResult> Index()
        {
            if( Session["PageView"] == null)
            {
                Session["PageView"] = "1";
            }
            else
            {
                int pageView = Int32.Parse(Session["PageView"].ToString());

                Session["PageView"] = ++pageView;
            }

            Session["Customers"] = await Helper.ServiceDataHelper.GetCustomers();

            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";
            
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}