using fyptest.Models;
using fyptest.SignalR.Hubs;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Security;


namespace fyptest.Controllers
{
  public class SeekerController : Controller
  {
    ServerDBEntities db = new ServerDBEntities();

    [AllowAnonymous]
    public ActionResult SeekerRegister(string returnUrl)
    {

      return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public ActionResult SeekerRegister(SeekerRegisterModel model)
    {
      if (ModelState.IsValid)
      {
        if (db.Seekers.FirstOrDefault(m => m.email == model.Email) == null && db.Providers.FirstOrDefault(m => m.email == model.Email) == null)
        {
          Seeker seeker = new Seeker();
          seeker.name = model.FullName;
          seeker.password = Crypto.Hash(model.Password);
          seeker.phone = model.Phone;
          seeker.email = model.Email;
          seeker.walletAmount = 0;
          //Upload file
          var path = "~/UploadedDocument/" + model.Email;
          var docPath = path;
          Directory.CreateDirectory(Server.MapPath(path));
          if (model.ImageFile != null)
          {
            var extension = Path.GetExtension(model.ImageFile.FileName).ToLower();

            if (extension == ".jpg" || extension == ".png")
            {
              model.Photo = path + "/profile_picture" + extension;
              path = Path.Combine(Server.MapPath(path), "profile_picture" + extension);
              model.ImageFile.SaveAs(path);
              seeker.profileImage = model.Photo;
            }
          }
          model.SuccessMessage = "Register successfully.";
          db.Seekers.Add(seeker);
          Session["Role"] = "Seeker";
          Session["Email"] = model.Email;
          try
          {
            db.SaveChanges();
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

          return RedirectToAction("Index", "Home");
        }
      }
      //else
      //{
      //  ModelState.AddModelError("Error", "Email already exist. Please use other.");
      //  //return View();
      //}
      // If we got this far, something failed, redisplay form
      return View(model);
    }

    [AllowAnonymous]
    public ActionResult SeekerLogin(string returnUrl)
    {
      return View();
    }

    //
    // POST: /Account/Login
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public ActionResult SeekerLogin(SeekerLoginModel model, string returnUrl)
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
        var seeker = db.Seekers.Where(m => m.email == model.Email && m.password == hashedPwd).ToList();
        var seekerAcc = db.Seekers.Where(m => m.email == model.Email).ToList();

        if (seeker.Count() <= 0)
        {
          ModelState.AddModelError("Error", "Invalid email or password");
          return View();
        }

        else if (seeker.Count() > 0 && seeker != null)
        {

          var logindetails = seeker.First();
          // Login In.    
          //SignInUser(logindetails.email, "Seeker", false);
          // setting.    
          Session["Role"] = "Seeker";
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
          var notificationHub = GlobalHost.ConnectionManager.GetHubContext<ChatHub>();
          notificationHub.Clients.All.connect(Session["Email"].ToString(), Session["Role"].ToString());
          //encrypt the ticket and add it to a cookie
          return RedirectToLocal(returnUrl);
        }
        else if (seekerAcc.Count() != 0)
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

    [NonAction]
    public void SendVerificationLinkEmail(string email, string activationCode)
    {
      var verifyUrl = "/Seeker/ResetPassword/" + activationCode;
      var link = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, verifyUrl);
      var fromEmail = new MailAddress("OnDemand.Service.2021@gmail.com", "Service On-Demand");
      var toEmail = new MailAddress(email);
      var fromEmailPassword = "Abc123!@#";//real password
      string subject = "Reset Password";
      var body = "Hi, <br><br> We got request for reset your account password. Please click on the below link to reset your password" +
      "<br><br><a href=" + link + ">Reset Password Link</a>";

      var smtp = new SmtpClient
      {
        Host = "smtp.gmail.com",
        Port = 587,
        EnableSsl = true,
        DeliveryMethod = SmtpDeliveryMethod.Network,
        UseDefaultCredentials = false,
        Credentials = new NetworkCredential(fromEmail.Address, fromEmailPassword)
      };

      using (var message = new MailMessage(fromEmail, toEmail)
      {
        Subject = subject,
        Body = body,
        IsBodyHtml = true
      })
        smtp.Send(message);
    }

    [AllowAnonymous]
    public ActionResult ResetPassword(string id)
    {
      using (ServerDBEntities db = new ServerDBEntities())
      {
        var seeker = db.Seekers.Where(s => s.reset_pwd == id).FirstOrDefault();
        if (seeker != null)
        {
          ResetPasswordModel model = new ResetPasswordModel();
          model.ResetCode = id;
          return View(model);
        }
        else
        {
          return HttpNotFound();
        }
      }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [AllowAnonymous]
    public ActionResult ResetPassword(ResetPasswordModel model)
    {
      var message = "";
      if (ModelState.IsValid)
      {
        using (ServerDBEntities db = new ServerDBEntities())
        {
          var seeker = db.Seekers.Where(a => a.reset_pwd == model.ResetCode).FirstOrDefault();
          if (seeker != null)
          {
            seeker.password = Crypto.Hash(model.NewPassword);
            seeker.reset_pwd = "";
            db.Configuration.ValidateOnSaveEnabled = false;
            db.SaveChanges();
            message = "New password updated successfully";
          }
        }
      }
      else
      {
        message = "Something invalid";
      }
      ViewBag.Message = message;
      return View(model);
    }

    [AllowAnonymous]
    public ActionResult ForgetPasword()
    {
      return View();
    }

    [HttpPost]
    [AllowAnonymous]
    public ActionResult ForgetPasword(string Email)
    {

      string message = "";

      using (ServerDBEntities db = new ServerDBEntities())
      {
        var seeker = db.Seekers.Where(s => s.email == Email).FirstOrDefault();
        var provider = db.Providers.Where(s => s.email == Email).FirstOrDefault();
        message = "Reset password link has sent to your email";
        if (seeker != null)
        {
          var resetCode = Guid.NewGuid().ToString();
          SendVerificationLinkEmail(seeker.email, resetCode);
          seeker.reset_pwd = resetCode;
          db.Configuration.ValidateOnSaveEnabled = false;
          db.SaveChanges();
          message = "Reset password link has sent to your email";
        }
        else
        {
          message = "Account not found";
        }

      }
      ViewBag.Message = message;
      return View();
    }

    public ActionResult SeekerProfile()
    {
      string user = Session["Email"].ToString();//"victorritdemo+p1@gmail.com"; //User.Identity.Name
      var data = db.Seekers.Find(user);
      //var type = db.Service_Types.Find(data.STId);
      //var rating = db.Ratings.Find(user);
      var request = db.Requests.Where(r => r.Seeker == user);
      //var recommend = db.Requests.Where(r => r.Type == data.STId && r.Provider == null && r.status == 0);
      //var category = db.Service_Categories;


      var accModel = new AccountDetailVM
      {
        Email = data.email,
        Phone = data.phone,
        //Address = data.address,
        Name = data.name,
        ProfileImage = data.profileImage,
        Wallet = data.walletAmount,
        //CompanyIndividual = data.companyIndividual,
        //CompanyName = data.companyName,
        //ServiceType = type.name,
        //Attitude = rating.attitude,
        //Quality = rating.quality,
        //Efficiency = rating.efficiency,
        //Professionalism = rating.professionalism
      };


      var model = new OverallAccVM
      {
        accDetail = accModel,
        RequestDetail = request,
        //categories = category,
        //recommend = recommend
      };

      return View(model);
    }

    [HttpPost]
    public JsonResult EditProfile(EditProfileModel model)
    {
      //System.Diagnostics.Debug.WriteLine(editProfileModel.Address +" "+ editProfileModel.Phone + " " + editProfileModel.ProfileImage);
      var p = db.Seekers.Find(model.Email);

      if (p == null)
        return Json(String.Format("'Error' : '{0}'", "Failed"));


      if (ModelState.IsValid)
      {
        p.phone = model.Phone;
        //p.address = model.Address;
        //if (model.ImageFile != null)
        //  p.profileImage = SavePhoto(model.ImageFile);

        db.SaveChanges();
        return Json(model);
      }

      return Json(String.Format("'Error' : '{0}'", "Failed"));
    }

    [HttpPost]
    public JsonResult EditProfilePic(HttpPostedFileBase file)
    {
      var p = db.Seekers.Find(Session["Email"].ToString());//User.Identity.Name

      if (file == null)
        return Json(String.Format("'Error' : '{0}'", "Failed"));

      DeletePhoto(p.profileImage);

      p.profileImage = SavePhoto(file);
      db.SaveChanges();

      return Json(String.Format("'Success':'true'"));
    }

    private void DeletePhoto(string name)
    {
      //name = System.IO.Path.GetFileName(name);
      string path = Server.MapPath(name);
      System.IO.File.Delete(path);
    }

    private string SavePhoto(HttpPostedFileBase f)
    {
      //string name = Guid.NewGuid().ToString("n") + ".jpg";
      var pathStr = "~/UploadedDocument/" + Session["Email"].ToString();
      var extension = Path.GetExtension(f.FileName).ToLower();
      pathStr = pathStr + "/profile_picture" + extension;
      //path = Path.Combine(Server.MapPath(path), "profile_picture" + extension);
      var path = Server.MapPath(pathStr);

      Stream doc = f.InputStream;

      var img = new WebImage(doc);

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
         .Save(path);

      return pathStr;
    }

    [HttpPost]

    public ActionResult Logout()
    {
      var ctx = Request.GetOwinContext();
      var authenticationManager = ctx.Authentication;

      //AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
      //Sign out
      authenticationManager.SignOut();
      Session.Clear();
      Session.Abandon();
      if (!string.IsNullOrEmpty(Convert.ToString(Session["Email"])))
      {
        return RedirectToAction("Index", "Home");
      }
      return RedirectToAction("Index", "Home");
    }
  }

}
