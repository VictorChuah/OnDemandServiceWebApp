using fyptest.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;

namespace fyptest.Controllers
{
  public class JobProviderController : Controller
  {
    PasswordHasher ph = new PasswordHasher();
    ServerDBEntities db = new ServerDBEntities();


    // GET: JobProvider
    public ActionResult Index()
    {
      return View();
    }

    // GET: JobProvider/RegisterProvide
    public ActionResult RegisterProvider()
    {
      return View();
    }

    // POST: JobProvider/RegisterProvide
    [HttpPost]
    public ActionResult RegisterProvider(RegisterModel model)
    {
      return View(model);
    }


    //================================================================================================================

    //// GET: Account/CheckEmail 
    //public ActionResult CheckEmail(string email)
    //{
    //  //bool isValid = (db.Admins.Any(a => a.Email != email) && db.Customers.Any(c => c.Email != email));
    //  //return Json(isValid, JsonRequestBehavior.AllowGet);
    //}

    //public ActionResult CheckCompany (string companyName)
    //{

    //}

    private string HashPassword(string password)
    {
      return ph.HashPassword(password);
    }

    private string ValidatePhoto(HttpPostedFileBase f)
    {
      var reType = new Regex(@"^image\/(jpeg|png)$", RegexOptions.IgnoreCase);
      var reName = new Regex(@"^.+\.(jpg|jpeg|png)$", RegexOptions.IgnoreCase);

      if (f == null)
      {
        return null;
      }
      else if (!reType.IsMatch(f.ContentType) || !reName.IsMatch(f.FileName))
      {
        return "Only JPG or PNG photo is allowed.";
      }
      else if (f.ContentLength > 1 * 1024 * 1024)
      {
        return "Photo size cannot more than 1MB.";
      }

      return null;
    }

    private string SavePhoto(HttpPostedFileBase f)
    {
      string name = Guid.NewGuid().ToString("n") + ".jpg";
      string path = Server.MapPath($"~/Image/Profile/{name}");

      var img = new WebImage(f.InputStream);

      if (img.Width > img.Height)
      {
        int px = (img.Width - img.Height) / 2;
        img.Crop(0, px, 0, px);
      }
      else
      {
        int px = (img.Height - img.Width) / 2;
        img.Crop(px, 0, px, 0);
      }

      img.Resize(201, 201)
         .Crop(1, 1)
         .Save(path, "jpeg");

      return name;
    }

    private void DeletePhoto(string name)
    {
      name = System.IO.Path.GetFileName(name);
      string path = Server.MapPath($"~/Image/Profile/{name}");
      System.IO.File.Delete(path);
    }

  }
}
