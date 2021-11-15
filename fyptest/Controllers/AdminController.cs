using fyptest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;

namespace fyptest.Controllers
{
  public class AdminController : Controller
  {
    ServerDBEntities db = new ServerDBEntities();

    public ActionResult ServiceCRUD()
    {
      var type = db.Service_Types;
      var category = db.Service_Categories;

      var data = new ServiceCRUDVM
      {
        Types = type,
        Categories = category
      };

      return View(data);
    }

    //[HttpPost]
    //public ActionResult WorkCRUD()










    // GET: Admin
    public ActionResult ApprovalList()
    {
      var data = db.Providers.Where(s => s.status == 1);

      return View(data);
    }

    public ActionResult ApprovalDetail(string id)
    {
      var p = db.Providers.Find(id);

      if (p != null)
      {
        var d = db.Documents.Where(h => h.holder == id);

        var data = new AdminApprovalVM
        {
          Email = p.email,
          Phone = p.phone,
          Address = p.address,
          CompanyIndividual = p.companyIndividual,
          Name = p.name,
          CompanyName = p.companyName,
          ServiceType = p.Service_Type.name,
          ProfileImage = p.profileImage,
          document = d
        };


        return View(data);
      }

      TempData["Info"] = "Invalid ID";
      return RedirectToAction("ApprovalList", "Admin");
    }

    [HttpPost]
    public JsonResult ApprovalRespond(string id, bool approval)
    {
      var p = db.Providers.Find(id);

      if (p != null)
      {
        if (approval)
        {
        //admin approved 
          p.status = 2;
          SendEmail(p.email, approval);
        }
        else
        {
          //admin decline
          p.status = 0;
          SendEmail(p.email, approval);
        }


        db.SaveChanges();
        return Json(String.Format("'Success':'true'"));
      }

      return Json(String.Format("'Error' : '{0}'", "Failed"));
    }



    //==================================================================================
    private void SendEmail(string email, bool approval)
    {
      var user = db.Providers.Find(email);

      string name = "";
      name = user.name != null ? user.name : user.companyName;

      var m = new MailMessage();

      string path = Server.MapPath($"~/Image/Profile/{user.profileImage}");
      var att = new Attachment(path);
      att.ContentId = "photo";
      m.Attachments.Add(att);

      m.To.Add($"{name} <{user.email}>");
      m.Subject = "Account Approval";
      m.IsBodyHtml = true;
      if (approval)
      {
        m.Body = $@"
                <img src='cid:photo' style='width: 100px; height: 100px;
                                            border: 1px solid #333'>
                <p>Dear {name},<p>
                <p>Congratulation, your provider account has been approval, you now have proceed to login your account</p>
                <br/>
                <p>Best Regard,</p>
                <p>Admin</p>
                '>";
      }
     else
     {
        string url = Url.Action("RegisterProvider", "JobProvider", "https"); 

        m.Body = $@"
                <img src='cid:photo' style='width: 100px; height: 100px;
                                            border: 1px solid #333'>
                <p>Dear {name},<p>
                <p>Your account has been declined by admin. Please <a href='{url}'>register</a> again and follow the guideline of the registration. </p>
                <p>Sorry for the inconvenience.</p>
                <br/>
                <p>Best Regard,</p>
                <p>Admin</p>
                '>";
      }


      new SmtpClient().Send(m);
    }

    public FileResult DownloadFile(string file)
    {
      string path = Server.MapPath("~/UploadedFiles/") + file;

      byte[] bytes = System.IO.File.ReadAllBytes(path);

      return File(bytes, "application/octet-stream", file);
    }

    //@Html.ActionLink("Document", "DownloadFile", new { filename = Model.File })
  }

}
