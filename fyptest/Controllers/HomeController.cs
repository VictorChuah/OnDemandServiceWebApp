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
      List<JobViewModel> model = getJobList();
      return View(model);
    }

    private List<JobViewModel> getJobList()
    {

      //List<Request> jobs = db.Requests.Where(a => a.SId.Equals("Active")).ToList();
      List<Request> jobs = db.Requests.OrderByDescending(a => a.dateCreated).ToList();
      List<JobViewModel> jobList = new List<JobViewModel>();
      foreach (var j in jobs)
      {
        JobViewModel model = new JobViewModel();
        model.JobID = j.SId;
        model.Title = j.title;
        model.SelectedType = j.Type;
        model.Price = j.price;
        model.Location = j.address;
        if (j.image == null)
          model.Image = "/UploadedDocument/noimage.jpg";
        else
          model.Image = j.image;
        model.Description = j.description;
        model.Date = (DateTime)j.dateCreated;
        if (j.dateCompleted != null)
          model.CompleteDate = (DateTime)j.dateCompleted;
        model.Category = j.Category;
        model.Provider = j.Provider;
        model.Seeker = j.Seeker;
        model.Status = j.status.ToString();
        jobList.Add(model);
      }
      return jobList;
    }

  }
}
