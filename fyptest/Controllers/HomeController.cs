using fyptest.Models;
using fyptest.SignalR.Hubs;
using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace fyptest.Controllers
{
  public class HomeController : Controller
  {
    ServerDBEntities db = new ServerDBEntities();
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
