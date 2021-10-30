using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace fyptest.Controllers
{
  public class HomeController : Controller
  {
    // GET: Home
    public ActionResult Index()
    {
      System.Diagnostics.Debug.WriteLine("home");
      ViewBag.Message = "controller";
      TempData["Info"] = "test";
      return View();
    }

  }
}
