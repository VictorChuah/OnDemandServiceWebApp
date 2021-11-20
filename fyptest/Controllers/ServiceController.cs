using fyptest.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
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

    public List<SelectListItem> GetCategory()
    {
      var list = db.Service_Categories.OrderBy(m => m.name).ToList();
      List<SelectListItem> categoryList = new List<SelectListItem>();
      foreach (var item in list)
      {
        categoryList.Add(new SelectListItem { Text = item.name, Value = item.SCId });
      }
      return categoryList;
    }

    public Dictionary<string, string> GetJobType()
    {
      Dictionary<string, string> jobType = new Dictionary<string, string>();
      var list = db.Service_Types.OrderBy(m => m.name).ToList();
      foreach (var item in list)
      {
        jobType.Add(item.name, item.STId);
      }
      return jobType;
    }
    public ActionResult PostJobView()
    {
      List<SelectListItem> categoryList = new List<SelectListItem>();
      categoryList = GetCategory();
      Dictionary<string, string> typeList = new Dictionary<string, string>();
      typeList = GetJobType();
      var user = Session["Email"];
      if (user != null)
      {
        var seeker = db.Seekers.Where(m => m.email == user.ToString()).FirstOrDefault();
        JobCreateModel model = new JobCreateModel();
        model.Seeker = seeker.email;
        model.Contact = seeker.phone;
        model.CategoryList = categoryList;
        model.Type = typeList;
        return View(model);
      }
      else
      {
        JobCreateModel model = new JobCreateModel();
        model.Seeker = "Please login";
        model.Contact = "Please login";
        model.CategoryList = categoryList;
        model.Type = typeList;
        return View(model);
      }

    }


    [HttpPost]
    public ActionResult PostJobView(JobCreateModel model)
    {
      var seeker = Session["Email"].ToString();
      var SId = GenerateSId();
      if (ModelState.IsValid && model != null)
      {
        Request job = new Request();
        job.SId = SId;
        job.title = model.Title;
        job.Category = model.Category;
        job.description = model.Description;
        job.address = model.Location;
        job.price = model.Price;
        job.Seeker = seeker;
        job.Type = model.SelectedType;
        if (model.SelectedType == "Immediate")
          job.immediate = true;
        else
          job.immediate = false;
        job.dateCompleted = null;
        job.dateCreated = DateTime.Now;
        job.status = 0;
        job.providerComplete = false;
        job.seekerComplete = false;
        job.repeat = false;
        job.Rating = "1";
        model.SuccessMessage = "Register successfully.";
        db.Requests.Add(job);
        try
        {
          db.SaveChanges();
          var path = "/UploadedDocument/" + seeker;
          var directory = path;
          var filename = "Job" + SId;
          if (model.ImageFile != null)
          {
            var extension = Path.GetExtension(model.ImageFile.FileName).ToLower();
            if (!Directory.Exists(Server.MapPath(path + "/Job" + SId)))
            {
              Directory.CreateDirectory(Server.MapPath(path + "/Job" + SId));
            }
            if (extension == ".jpg" || extension == ".png" || extension == ".jpeg")
            {
              model.Image = path + "/Job" + SId + "/" + model.ImageFile.FileName;
              path = Path.Combine(Server.MapPath(path + "/Job" + SId), model.ImageFile.FileName);
              model.ImageFile.SaveAs(path);
              job.image = model.Image;
              var docs = "";
              foreach (var item in model.Document)
              {
                if (item != null)
                {
                  if (item.ContentLength > 0)
                  {
                    var fileExtension = Path.GetExtension(item.FileName).ToLower();
                    if (fileExtension == ".jpg" || fileExtension == ".png" || fileExtension == ".pdf" || fileExtension == ".docx" || fileExtension == ".docx")
                    {
                      path = Path.Combine(Server.MapPath(directory + "/Job" + SId), item.FileName);
                      item.SaveAs(path);
                      docs += item.FileName + "#";
                      ViewBag.UploadSuccess = true;
                    }
                  }
                }
              }
              model.DocumentPath = docs;
              job.file = model.DocumentPath;
              db.SaveChanges();
              ViewBag.UploadMessage = true;
            }
          }
          return RedirectToAction("Index", "Home");
        }
        catch (DbEntityValidationException e)
        {
          foreach (var eve in e.EntityValidationErrors)
          {
            model.SuccessMessage = "Entity of type " + eve.Entry.Entity.GetType().Name + " in state " + eve.Entry.State + " has the following validation errors:";
            foreach (var ve in eve.ValidationErrors)
            {
              model.SuccessMessage = "- Property: " + ve.PropertyName + ", Error: " + ve.ErrorMessage;
            }
          }
          throw;
        }
      }
      else
      {
        ModelState.AddModelError("Error", "Failed to create job.");
      }
      List<SelectListItem> categoryList = new List<SelectListItem>();
      categoryList = GetCategory();
      Dictionary<string, string> typeList = new Dictionary<string, string>();
      typeList = GetJobType();
      model.CategoryList = categoryList;
      model.Type = typeList;
      return View(model);
    }

    public string GenerateSId()
    {
      var sid = db.Requests.OrderByDescending(m => m.SId).FirstOrDefault();
      var newSId = Convert.ToInt32(sid.SId.Replace("s", "")) + 1;
      return "s" + newSId;
    }

    public ActionResult JobView(string id)
    {
      var user = Session["Email"];
      JobViewModel model = new JobViewModel();
      var job = db.Requests.Where(m => m.SId == id).FirstOrDefault();
      model.JobID = job.SId;
      model.Title = job.title;
      model.Category = job.Category;
      model.Description = job.description;
      model.Price = (double)job.price;
      model.Image = job.image;
      model.Seeker = job.Seeker;
      model.Status = job.status.ToString();
      if (job.file != null)
      {
        model.Files = job.file.Split('#');
      }
      else
      {
        model.Files = new List<string>();
      }
      if (model.Image == null)
        model.Image = "/Service/noimage.jpg";
      //model.Contact = job.Contact;
      model.SelectedType = job.Type;

      return View(model);
    }

    public ActionResult ApplyJob(Request job)
    {
      var jobProfile = db.Requests.FirstOrDefault(a => a.SId.Equals(job.SId));
      if (jobProfile != null && ModelState.IsValid)
      {
        jobProfile.Provider = Session["Email"].ToString();
        jobProfile.status = 1;
        db.SaveChanges();

        string message = "You have accepted this job. You may view this job at your job list.";
        return Json(new { Message = message, JsonRequestBehavior.AllowGet });

      }
      return Json(new { Message = "Failed to apply. Please try again.", JsonRequestBehavior.AllowGet });
    }

    public ActionResult Tracking(string requestId)
    {
      var r = db.Requests.Find(requestId);

      if (r?.status != 1)
      {
        TempData["Info"] = "Tracking is unavailable or expired";
        return RedirectToAction("RequestList", "Service", new { status = 1 });
      }
      return View(r);
    }

    [HttpPost]
    public JsonResult ProviderCompleteJob(JobViewModel model)
    {
      var job = db.Requests.Where(m => m.SId == model.JobID).FirstOrDefault();

      if (ModelState.IsValid)
      {
        job.providerComplete = true;
        if (job.seekerComplete == true)
        {
          job.dateCompleted = DateTime.Now;
          job.status = 2;
        }
        db.SaveChanges();
        return Json("You saved this job successfully.");
      }

      return Json(String.Format("'Error' : '{0}'", "Failed"));
    }

    [HttpPost]
    public JsonResult SeekerCompleteJob(JobViewModel model)
    {
      var job = db.Requests.Where(m => m.SId == model.JobID).FirstOrDefault();

      if (ModelState.IsValid)
      {
        job.seekerComplete = true;
        if (job.providerComplete== true)
        {
          job.dateCompleted = DateTime.Now;
          job.status = 2;
        }
        db.SaveChanges();
        return Json("You saved this job successfully.");
      }

      return Json(String.Format("'Error' : '{0}'", "Failed"));
    }


    [HttpPost]
    public ActionResult CommentAndRateJob(Rate jobRate)
    {

      var jobProfile = db.Requests.FirstOrDefault(a => a.SId.Equals(jobRate.JobID));
      var ratedJobCount = db.Requests.Where(a => a.Provider == jobProfile.Provider && jobProfile.seekerComplete == true).Count() -1;
      
      var providerRate = db.Ratings.FirstOrDefault(a =>a.RId==jobProfile.Provider);
      if (providerRate != null && ModelState.IsValid)
      {
        var profesionalism = providerRate.professionalism * ratedJobCount;
        var quality = providerRate.quality * ratedJobCount;
        var efficiency = providerRate.efficiency * ratedJobCount;
        var attitude = providerRate.attitude * ratedJobCount;
        providerRate.professionalism = (profesionalism + Convert.ToInt32(jobRate.Profesionalism)) / (ratedJobCount + 1);
        providerRate.quality = (quality + Convert.ToInt32(jobRate.Quality)) / (ratedJobCount + 1);
        providerRate.efficiency = (efficiency + Convert.ToInt32(jobRate.Efficiency)) / (ratedJobCount + 1);
        providerRate.attitude = (attitude + Convert.ToInt32(jobRate.Attitude)) / (ratedJobCount + 1);
        db.SaveChanges();

        string message = "Comment successfully.";
        return Json(new { Message = message, JsonRequestBehavior.AllowGet });

      }
      return Json(new { Message = "Failed to update", JsonRequestBehavior.AllowGet });
    }
  }

}
