using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LocalNews.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Message = "";

            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Local news is great way to find out what's happening around you. Stay alert with local news.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Head office";

            return View();
        }
    }
}
