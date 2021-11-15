using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using fyptest.Models;


namespace fyptest.Controllers
{
  public class HomeController : Controller
  {
    // GET: Home
    public ActionResult Index()
    {
      ServerDBEntities db = new ServerDBEntities();

      //count total request
      int count = db.Requests.Where(r => r.Provider == "victorritdemo+p1@gmail.com" && r.status == 2).Count();

      //get providers current rating for each type
      var proRating = db.Ratings.Find("victorritdemo+p1@gmail.com").professionalism;
      var effRating = db.Ratings.Find("victorritdemo+p1@gmail.com").efficiency;
      var quaRating = db.Ratings.Find("victorritdemo+p1@gmail.com").quality;
      var attRating = db.Ratings.Find("victorritdemo+p1@gmail.com").attitude;

      //take the rating from one request after seeker finish rating
      //-code- eg:
      int currentPro = 2;

      //current rating + request rating / total request
      float finalRating = (proRating + currentPro) / count;
      int roundedRate = (int)Math.Round(finalRating);



      double x = 17.83457884934;
      x = (int)System.Math.Round(x);

      System.Diagnostics.Debug.WriteLine("home");
      ViewBag.Message = roundedRate;
      TempData["Info"] = count;
      return View();

    }
  }
}
