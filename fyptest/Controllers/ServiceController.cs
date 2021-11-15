using fyptest.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace fyptest.Controllers
{
  public class ServiceController : Controller
  {
    ServerDBEntities db = new ServerDBEntities();

    // GET: Service
    public ActionResult Index()
    {
      return View();
    }

    public ActionResult RequestList(int status, string query = "", string selected = "")
    {
      string user = "victorritdemo+p1@gmail.com"; //User.Identity.Name
      query = query.Trim();

      //viewbag for display 
      string page = "";
      switch (status)
      {
        case 0: page = "Request List"; break;
        case 1: page = "Ongoing"; break;
        case 2: page = "History"; break;
        case 3: page = "Request Received"; break;
      }
      ViewBag.page = page;
      ViewBag.status = status;

      //data to pass to moddel
      IQueryable<Request> rqt = null;

      if (status == 0)
        rqt = db.Requests.Where(r => r.status == status && r.title.Contains(query));
      else if (status == 1 || status == 2)
        rqt = db.Requests.Where(r => r.Provider == user && r.status == status && r.title.Contains(query));
      else if (status == 3)
        rqt = db.Requests.Where(r => r.status == status && r.title.Contains(query));


      var model = new RequestDetail
      {
        request = rqt
      };

      //if is ajax search
      if (Request.IsAjaxRequest())
        return PartialView("_List", rqt);

      return View(model);
    }

    [HttpPost]
    public ActionResult RequestDetail(string id)
    {
      var r = db.Requests.Find(id);
      var c = db.Service_Categories.Find(r.Category);

      var model = new RequestDetail
      {
        Id = r.SId,
        Title = r.title,
        Address = r.address,
        Description = r.description,
        Image = r.image,
        File = r.file,
        DateCreated = r.dateCreated,
        Price = r.price,
        Category = c.name,
        Status = r.status,
        SeekerComplete = r.seekerComplete,
        ProviderComplete = r.providerComplete
      };

      return PartialView("_Detail", model);
    }

    //[HttpPost]
    public ActionResult HandleRequest(string id, string rAction)
    {
      int param = 1;
      string user = "victorritdemo+p1@gmail.com"; //User.Identity.Name
      var r = db.Requests.Find(id);
      var p = db.Providers.Find(user);
      var s = db.Seekers.Find(r.Seeker);

      //0 pending, 1 accept, 2 complete, 3 request specific vendor
      if (rAction == "accept")
      {
        r.status = 1;
        r.Provider = user;
      }
      else if (rAction == "decline")
      {
        r.status = 0;
        r.Provider = null;
        param = 3;
      }
      else if (rAction == "complete")
      {
        r.providerComplete = true;
        if (r.seekerComplete == true)
        {
          r.status = 2;
          r.dateCompleted = DateTime.Now;
          p.walletAmount += r.price;
          s.walletAmount -= r.price;
          param = 2;
        }
      }

      db.SaveChanges();
      return RedirectToAction("RequestList", new { status = param });
    }

    //[HttpPost]
    public FileResult DownloadFile(string file)
    {
      string path = Server.MapPath("~/Request/") + file;

      byte[] bytes = System.IO.File.ReadAllBytes(path);

      return File(bytes, "application/octet-stream", file);
    }

    public ActionResult Tracking(string requestId)
    {
      var r = db.Requests.Find(requestId);

      if (r?.status != 1)
      {
        TempData["Info"] = "Tracking is unavailable or expired";
        return RedirectToAction("RequestList", "Service", new { status = 1 } );
      }
      return View(r);
    }
  }

}
