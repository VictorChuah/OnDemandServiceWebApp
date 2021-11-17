using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.SignalR;
using fyptest.Models;
using fyptest.SignalR.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Web.Helpers;

namespace fyptest.Controllers
{
    public class AdminController : Controller
    {
    // GET: Admin
    private ServerDBEntities db = new ServerDBEntities();
    // GET: Admin
    public ActionResult AdminProviderView()
    {
      var data = db.Providers.Where(s => s.status == 1);
      return View(data);
      
    }

    public ActionResult AdminSeekerView()
    {
      var data = db.Seekers.Where(s => s.status == 1);
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
    //private List<AdminUserView> getUserList()
    //{

    //  List<Seeker> users = db.Seekers.ToList();
    //  List<Provider> providers = db.Providers.ToList();
    //  List<AdminUserView> userList = new List<AdminUserView>();
    //  foreach (var user in users)
    //  {
    //    AdminUserView model = new AdminUserView();
    //    model.UserID = user.id;
    //    model.Email = user.email;
    //    model.FullName = user.name;
    //    model.Gender = user.gender;
    //    model.HandphoneNo = user.handphone;
    //    model.Photo = user.image;
    //    if (user.image == null)
    //      model.Photo = "~/UploadedDocument/user.jpg";
    //    model.CompanyOrIndividual = user.comp_ind;
    //    model.Status = user.status;
    //    model.Role = user.role;
    //    model.Address = user.address;
    //    model.DocumentsPath = user.documents;
    //    userList.Add(model);
    //  }
    //  foreach (var user in providers)
    //  {
    //    AdminUserView model = new AdminUserView();
    //    model.UserID = user.id;
    //    model.Email = user.email;
    //    model.FullName = user.name;
    //    model.Gender = user.gender;
    //    model.HandphoneNo = user.handphone;
    //    model.Photo = user.image;
    //    if (user.image == null)
    //      model.Photo = "~/UploadedDocument/user.jpg";
    //    model.CompanyOrIndividual = user.comp_ind;
    //    model.Status = user.status;
    //    model.Role = user.role;
    //    model.Address = user.address;
    //    model.DocumentsPath = user.documents;
    //    userList.Add(model);
    //  }
    //  return userList;
    //}

    //[HttpPost]
    //public ActionResult EditRole(Provider user)
    //{
    //  var userProfile = db.Providers.FirstOrDefault(a => a.id.Equals(user.id));
    //  var action = "";
    //  //if (userProfile != null && ModelState.IsValid)
    //  //{
    //  userProfile.id = user.id;
    //  if (user.status == "approve")
    //  {
    //    userProfile.status = "Active";
    //    userProfile.role = "Provider";
    //    action = "Approved";
    //  }
    //  else if (user.status == "block")
    //  {
    //    userProfile.status = "Blocked";
    //    userProfile.role = "Blocked Provider";
    //    action = "Blocked";
    //  }
    //  else if (user.status == "unblock")
    //  {
    //    userProfile.status = "Active";
    //    userProfile.role = "Provider";
    //    action = "Unblocked";
    //  }
    //  objDbEntity.SaveChanges();
    //  string message = action;
    //  return Json(new { Message = message, JsonRequestBehavior.AllowGet });

    //  //}
    //  //return Json(new { Message = "Failed to update", JsonRequestBehavior.AllowGet });
    //}

    //[HttpPost]
    //public ActionResult EditJobStatus(Request job)
    //{
    //  var jobs = db.Requests.Where(a => a.SId == job.SId).ToList();
    //  var action = "";
    //  //if (jobProfile != null && ModelState.IsValid)
    //  //{
    //  //jobProfile.job_id= job.job_id;
    //  var jobProfile = jobs.First();
    //  //if (job.job_status == "approve")
    //  //{
    //  //  jobProfile.job_status = "Active";
    //  //  jobProfile.job_actived_date = DateTime.Now;
    //  //  action = "Approved";
    //  //}
    //  //else
    //  if (job.status == 3)
    //  {
    //    jobProfile.status = "Blocked";
    //    action = "Blocked";
    //  }
    //  else if (job.status == 1)
    //  {
    //    jobProfile.status = "Active";
    //    action = "Unblocked";
    //  }
    //  else
    //  {
    //    return Json(new { Message = job.status, JsonRequestBehavior.AllowGet });
    //  }
    //  db.SaveChanges();
    //  //if (job.status == "approve" && job.job_type == "Immediate")
    //  //{
    //  //  var notificationHub = GlobalHost.ConnectionManager.GetHubContext<ChatHub>();

    //  //  notificationHub.Clients.All.notify("added", "<li><a href='/Job/JobView/" + job.job_id + "'>A job with immediated action is required (Job ID" + job.job_id + ") </a></li>");
    //  //  SendEmail(job.job_id);
    //  //  //reregister notification
    //  //  //RegisterNotification(DateTime.Now);
    //  //}
    //  string message = action;
    //  return Json(new { Message = message, JsonRequestBehavior.AllowGet });

    //  //}
    //  //return Json(new { Message = "Failed to update", JsonRequestBehavior.AllowGet });
    //}

    public ActionResult AdminJobListView()
    {
      List<AdminJobViewModel> model = getJobList();

      return PartialView("_AdminPendingJobView", model);
    }

    public ActionResult AdminAllJobListView()
    {
      List<AdminJobViewModel> model = getJobList();

      return PartialView("_AdminJobView", model);
    }

    private List<AdminJobViewModel> getJobList()
    {

      List<Request> jobs = db.Requests.ToList();
      List<AdminJobViewModel> jobList = new List<AdminJobViewModel>();
      foreach (var job in jobs)
      {
        AdminJobViewModel model = new AdminJobViewModel();
        model.JobID = job.SId;
        model.Location = job.address;
        model.Category = job.Category;
        model.Price = (double)job.price;
        model.Image = job.image;
        if (job.image == null)
          model.Image = "~/UploadedDocument/noimage.jpg";
        model.Provider = job.Provider;
        model.Status = job.status;
        model.SelectedType = job.Type;
        model.Title = job.title;

        jobList.Add(model);
      }
      return jobList;
    }

    [NonAction]
    public void SendEmail(string jobId)
    {
      List<Provider> providerList = db.Providers.ToList();
      var job = db.Requests.Where(m => m.SId == jobId).FirstOrDefault();
      var fromEmail = new MailAddress("OnDemand.Service.2021@gmail.com", "Service On-Demand");
      //var toEmail = new MailAddress(email);
      var fromEmailPassword = "Abc123!@#";//real password
      string subject = "Reset Password";
      var body = "Hi, <br><br> There is a job that required immediate reaction by " + job.Provider + ". The details are as below:<br>" +
          "Title: " + job.title + "<br>" +
          "Category: " + job.Category + "<br>" +
          "Description: " + job.description + "<br>" +
          "Price: " + job.price + "<br>";

      var smtp = new SmtpClient
      {
        Host = "smtp.gmail.com",
        Port = 587,
        EnableSsl = true,
        DeliveryMethod = SmtpDeliveryMethod.Network,
        UseDefaultCredentials = false,
        Credentials = new NetworkCredential(fromEmail.Address, fromEmailPassword)
      };

      foreach (var provider in providerList)
      {
        var toEmail = new MailAddress(provider.email);
        using (var message = new MailMessage(fromEmail, toEmail)
        {
          Subject = subject,
          Body = body,
          IsBodyHtml = true
        })



          smtp.Send(message);
      }

    }

    [AllowAnonymous]
    public ActionResult AdminLogin(string returnUrl)
    {
      return View();
    }

    //
    // POST: /Account/Login
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public ActionResult AdminLogin(AdminLoginModel model, string returnUrl)
    {
      //var user = _userManager.FindByNameAsync(model.Email);
      //var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);
      if (!ModelState.IsValid)
      {
        ModelState.AddModelError(string.Empty, "Invalid email or password.");
      }
      else
      {
        var hashedPwd = Crypto.Hash(model.Password);
        //find seeker table
        var admin = db.Admins.Where(m => m.email == model.Email && m.password == hashedPwd).ToList();
        var adminAcc = db.Admins.Where(m => m.email == model.Email).ToList();

        if (admin.Count() <= 0)
        {
          ModelState.AddModelError("Error", "Invalid email or password");
          return View();
        }

        else if (admin.Count() > 0 && admin != null)
        {

          var logindetails = admin.First();
          // Login In.    
          //SignInUser(logindetails.email, "Seeker", false);
          // setting.    
          Session["Role"] = "Admin";
          Session["Email"] = logindetails.email;
          Session["user"] = logindetails;

          //create the authentication ticket
          var authTicket = new FormsAuthenticationTicket(
              1,
              model.Email,  //user id
              DateTime.Now,
              DateTime.Now.AddMinutes(525600),  // expiry
              model.RememberMe,  //true to remember
              "", //roles 
              "/"
          );
          HttpCookie cookie = new HttpCookie(FormsAuthentication.FormsCookieName, FormsAuthentication.Encrypt(authTicket));
          Response.Cookies.Add(cookie);
          var group = "";
          //var notificationHub = GlobalHost.ConnectionManager.GetHubContext<ChatHub>();
          //notificationHub.Clients.All.connect(Session["Email"].ToString(), Session["Role"].ToString());
          //encrypt the ticket and add it to a cookie
          return RedirectToLocal(returnUrl);
        }
        else if (adminAcc.Count() != 0)
        {
          ModelState.AddModelError(string.Empty, "No account found! Please register.");
        }

      }
      return View(model);
    }

    private ActionResult RedirectToLocal(string returnUrl)
    {
      if (Url.IsLocalUrl(returnUrl))
      {
        return Redirect(returnUrl);
      }
      return RedirectToAction("Index", "Home");
    }

  }
}
