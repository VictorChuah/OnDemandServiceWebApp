using fyptest.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using fyptest.Infrastructure.Attributes;
using System.IO;
using System.Net.Mail;
using PayPal.Api;
using System.Web.Security;
using Microsoft.AspNet.SignalR;
using fyptest.SignalR.Hubs;
using System.Net;

namespace fyptest.Controllers
{
  public class JobProviderController : Controller
  {
    PasswordHasher ph = new PasswordHasher();
    ServerDBEntities db = new ServerDBEntities();
    double paypalTopup = 20;


    // GET: JobProvider
    public ActionResult Index()
    {
      return View();
    }

    // GET: JobProvider/RegisterProvide
    public ActionResult RegisterProvider()
    {
      var type = new SelectList(db.Service_Types.ToList(), "STId", "Name");
      ViewData["type"] = type;

      var bank = new SelectList(db.Banks.ToList(), "Id", "name");
      ViewData["Bank"] = bank;

      return View();
    }

    // POST: JobProvider/RegisterProvide
    [HttpPost]
    [ValidateAntiForgeryToken]
    [ValidateGoogleCaptcha]
    public ActionResult RegisterProvider(RegisterProviderModel m)
    {
      //System.Diagnostics.Debug.WriteLine(
      //  m.Email + " " +
      //  m.Password + " " +
      //  m.Phone + " " +
      //  m.ProfileImage + " " +
      //  m.Formality + " " +
      //  m.CompanyIndividual + " " +
      //  m.Name + " " +
      //  m.CompanyName + " " +
      //  m.Address + " " +
      //  m.ServiceType + " ",
      //  m.Document[0]
      //  );

      //check email duplicate
      if (ModelState.IsValidField("Email") && (db.Providers.Find(m.Email) != null) || (db.Seekers.Find(m.Email) != null))
      {
        ModelState.AddModelError("Email", "Duplicated Email.");
      }
      //check companyname duplicate
      else if (ModelState.IsValidField("CompanyName") && m.CompanyName != null)
      {
        if (db.Providers.Any(a => a.companyName == m.CompanyName))
          ModelState.AddModelError("CompanyName", "Duplicated Company Name.");
      }



      //check image
      string image = "~/Image/Profile/photo.jpg";
      string err = ValidatePhoto(m.ProfileImage); // validate the photo
      if (err != null)
        ModelState.AddModelError("Photo", err);
      else
        image = SavePhoto(m.ProfileImage);



      //string validationErrors = string.Join(",",
      //              ModelState.Values.Where(E => E.Errors.Count > 0)
      //              .SelectMany(E => E.Errors)
      //              .Select(E => E.ErrorMessage)
      //              .ToArray());

      //System.Diagnostics.Debug.WriteLine(validationErrors);




      //Ensure model state is valid  
      if (ModelState.IsValid)
      {
        string username = m.Name != null ? m.Name : m.CompanyName != null ? m.CompanyName : "";

        string generate = username + Guid.NewGuid().ToString("n"); //activation code

        //iterating through multiple file collection   
        foreach (HttpPostedFileBase file in m.Document)
        {
          //Checking file is available to save.  
          if (file != null)
          {
            var InputFileName = Path.GetFileName(file.FileName);
            var ServerSavePath = Path.Combine(Server.MapPath("~/UploadedFiles/") + username + InputFileName);
            //Save file to server folder  
            file.SaveAs(ServerSavePath);
            //assigning file uploaded status to ViewBag for showing message to user.  
            //ViewBag.UploadStatus = m.Document.Count().ToString() + " files uploaded successfully.";

            var dbDoc = new Document
            {
              file = username + InputFileName,
              holder = m.Email
            };

            db.Documents.Add(dbDoc);
          }
        }

        //null check
        if (m.Address == null)
        {
          m.Address = "-";
        }

        var dbProvider = new Provider
        {
          email = m.Email,
          password = Crypto.Hash(m.Password),
          phone = m.Phone,
          profileImage = image,
          status = 0,
          formality = m.Formality,
          companyIndividual = m.CompanyIndividual,
          namecard = null,
          name = m.Name,
          companyName = m.CompanyName,
          address = m.Address,
          loginCount = 0,
          token = generate,
          tokenExpire = null,
          connectionGroup = null,
          walletAmount = 0,
          postNamecard = false,
          dataPostNamecard = null,
          STId = m.ServiceType,
          RId = m.Email,
          reset_pwd = null,
        };

        var dbRate = new Rating
        {
          RId = m.Email,
          attitude = 0,
          quality = 0,
          efficiency = 0,
          professionalism = 0
        };

        var dup = db.Providers.Find(m.Email);
        if (dup != null)
          db.Providers.Remove(dup);

        db.Providers.Add(dbProvider);
        db.Ratings.Add(dbRate);
        db.SaveChanges();

        SendEmail(m.Email, generate);

        TempData["Info"] = "Account registered. Please check your email to confirm.";

      }

      //db.Customers.Add(m);
      //db.SaveChanges();

      //SendEmail(m, null, generate, 'A');    //send email

      //TempData["Info"] = "Account registered. Please go to email to active account.";
      //return RedirectToAction("CustLogin", "Account");
      var type = new SelectList(db.Service_Types.ToList(), "STId", "Name");
      ViewData["type"] = type;

      var bank = new SelectList(db.Banks.ToList(), "Id", "name");
      ViewData["Bank"] = bank;
      return View(m);
      //return RedirectToAction("Index", "Home");
    }

    public ActionResult ActivateAccount(string token, string email)
    {
      var user = db.Providers.Find(email);

      ViewBag.MetaRefresh = "<meta http-equiv='refresh' content='5;url=" + Url.Action("Index", "Home") + "' />";

      if (token == user.token)
      {
        user.status = 1;
        user.token = null;
        user.tokenExpire = null;
        db.SaveChanges();
        TempData["Info"] = "Account Comfirmed!";
        ViewBag.message = "Please wait for admin approval in 3 to 5 working days.";
      }
      else
      {
        TempData["Info"] = "Invalid activate token!";
        ViewBag.message = "Please confirm you have select the right email, or contact customer service if problem presist.";
      }


      return View();
    }


    public ActionResult ProviderProfile(string viewProfile = "")
    {
      string user = "";
      //string user = "victorritdemo+p1@gmail.com";


      if (Session["Email"] == null) //no login, or seeker email but no parameter, redirect, else enter profile according to mode
      {
        TempData["Info"] = "Please login!";
        return RedirectToAction("ProviderLogin", "JobProvider");
      }
      else if (Session["Role"].ToString() == "Provider")
      {
        user = Session["Email"].ToString();
      }
      else if (Session["Role"].ToString() == "Seeker" && viewProfile == "")
      {
        TempData["Info"] = "Invalid Action!";
        return RedirectToAction("Index", "Home");
      }
      else if (Session["Role"].ToString() == "Seeker")
      {
        user = viewProfile;
      }


      var data = db.Providers.Find(user);
      var type = db.Service_Types.Find(data.STId);
      var rating = db.Ratings.Find(user);
      var request = db.Requests.Where(r => r.Provider == user);
      var recommend = db.Requests.Where(r => r.Type == data.STId && r.Provider == null && r.status == 0);
      var category = db.Service_Categories;


      var accModel = new AccountDetailVM
      {
        //Email = data.email,
        //Phone = data.phone,
        //Address = data.address,
        //CompanyIndividual = data.companyIndividual,
        //Name = data.name,
        //CompanyName = data.companyName,
        //ServiceType = type.name,
        //ProfileImage = data.profileImage,
        //Wallet = data.walletAmount,
        //Namecard = data.namecard,
        //Attitude = rating.attitude,
        //Quality = rating.quality,
        //Efficiency = rating.efficiency,
        //Professionalism = rating.professionalism,

      };


      var model = new OverallAccVM
      {
        Account = data,
        Rate = rating,
        RequestDetail = request,
        Categories = category,
        Recommend = recommend,
        Role = Session["Role"].ToString()

      };

      return View(model);
    }

    [HttpPost]
    public JsonResult EditProfile(EditProfileModel model)
    {
      //System.Diagnostics.Debug.WriteLine(editProfileModel.Address +" "+ editProfileModel.Phone + " " + editProfileModel.ProfileImage);
      var p = db.Providers.Find(model.Email);

      if (p == null)
        return Json(String.Format("'Error' : '{0}'", "Failed"));


      if (ModelState.IsValid)
      {
        p.phone = model.Phone;
        p.address = model.Address;
        //if (model.ProfileImage != null)
        //  p.profileImage = SavePhoto(model.ProfileImage);

        db.SaveChanges();
        return Json(model);
      }

      return Json(String.Format("'Error' : '{0}'", "Failed"));
    }

    [HttpPost]
    public JsonResult EditProfilePic(HttpPostedFileBase file)
    {
      var p = db.Providers.Find(Session["Email"].ToString());//User.Identity.Name

      if (file == null)
        return Json(String.Format("'Error' : '{0}'", "Failed"));

      DeletePhoto(p.profileImage, 'p');

      p.profileImage = SavePhoto(file);
      db.SaveChanges();

      return Json(String.Format("'Success':'true'"));
    }

    public ActionResult Withdraw()
    {
      var bank = new SelectList(db.Banks.ToList(), "Id", "name");
      ViewData["Bank"] = bank;

      return View("Withdraw");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Withdraw(WithdrawModel m)
    {

      if (ModelState.IsValid)
      {
        var p = db.Providers.Find(Session["Email"].ToString());//User.Identity.Name
        p.walletAmount -= m.Amount;

        TempData["Info"] = "Money Withdrawn";
        db.SaveChanges();
      }


      return RedirectToAction("ProviderProfile", "JobProvider");
    }

    public ActionResult Topup()
    {
      return View();
    }

    [HttpPost]
    public ActionResult Topup(TopupModel m)
    {
      if (ModelState.IsValid)
      {
        string cardNo = m.CardNumber;
        cardNo = Regex.Replace(cardNo, "[-]", String.Empty);

        var card = db.Cards.Find(cardNo);

        if (card.ccv == m.CCV && card.expireDate == m.ExpireDate)
        {
          var p = db.Providers.Find(Session["Email"].ToString());//User.Identity.Name
          p.walletAmount += m.Amount;

          //send sms?
          TempData["Info"] = "Top up sucessfully";
          db.SaveChanges();
        }
        else
        {
          ViewBag.message = "Invalid Card";
          return View();
        }

      }

      return RedirectToAction("ProviderProfile", "JobProvider");
    }

    [HttpPost]
    public JsonResult PaypalAmount(double amount)
    {
      paypalTopup = amount;

      System.Diagnostics.Debug.WriteLine(paypalTopup);

      return Json(String.Format("'Success':'true'"));
    }


    public ActionResult TopupPaypal(string Cancel = null) //payment got problem
    {
      //getting the apiContext  
      APIContext apiContext = PaypalConfiguration.GetAPIContext();
      try
      {
        //A resource representing a Payer that funds a payment Payment Method as paypal  
        //Payer Id will be returned when payment proceeds or click to pay  
        string payerId = Request.Params["PayerID"];
        if (string.IsNullOrEmpty(payerId))
        {
          //this section will be executed first because PayerID doesn't exist  
          //it is returned by the create function call of the payment class  
          // Creating a payment  
          // baseURL is the url on which paypal sendsback the data.  
          string baseURI = Request.Url.Scheme + "://" + Request.Url.Authority + "/JobProvider/ProviderProfile?";
          //here we are generating guid for storing the paymentID received in session  
          //which will be used in the payment execution  
          var guid = Convert.ToString((new Random()).Next(100000));
          //CreatePayment function gives us the payment approval url  
          //on which payer is redirected for paypal account payment  
          var createdPayment = this.CreatePayment(apiContext, baseURI + "guid=" + guid);
          //get links returned from paypal in response to Create function call  
          var links = createdPayment.links.GetEnumerator();
          string paypalRedirectUrl = null;
          while (links.MoveNext())
          {
            Links lnk = links.Current;
            if (lnk.rel.ToLower().Trim().Equals("approval_url"))
            {
              //saving the payapalredirect URL to which user will be redirected for payment  
              paypalRedirectUrl = lnk.href;
            }
          }
          // saving the paymentID in the key guid  
          Session.Add(guid, createdPayment.id);

          var p = db.Providers.Find("victorritdemo+p1@gmail.com");//User.Identity.Name
          p.walletAmount += paypalTopup;
          db.SaveChanges();

          TempData["Info"] = "Top up sucessfully";

          return Redirect(paypalRedirectUrl);
        }
        else
        {
          // This function exectues after receving all parameters for the payment  
          var guid = Request.Params["guid"];
          var executedPayment = ExecutePayment(apiContext, payerId, Session[guid] as string);
          //If executed payment failed then we will show payment failure message to user  
          if (executedPayment.state.ToLower() != "approved")
          {
            return View("FailureView");
          }
        }
      }
      catch (Exception ex)
      {
        return View("FailureView");
      }
      //on successful payment, show success page to user.
      return View("SuccessView");
    }
    private Payment payment;
    private Payment ExecutePayment(APIContext apiContext, string payerId, string paymentId)
    {
      var paymentExecution = new PaymentExecution()
      {
        payer_id = payerId
      };
      this.payment = new Payment()
      {
        id = paymentId
      };
      return this.payment.Execute(apiContext, paymentExecution); //paypal amount ???
    }

    private Payment CreatePayment(APIContext apiContext, string redirectUrl)
    {
      //create itemlist and add item objects to it  
      var itemList = new ItemList()
      {
        items = new List<Item>()
      };
      //Adding Item Details like name, currency, price etc  
      itemList.items.Add(new Item()
      {
        name = "Top up",
        currency = "MYR",
        price = paypalTopup.ToString(),
        quantity = "1",
        sku = "sku"
      });
      var payer = new Payer()
      {
        payment_method = "paypal"
      };
      // Configure Redirect Urls here with RedirectUrls object  
      var redirUrls = new RedirectUrls()
      {
        cancel_url = redirectUrl + "&Cancel=true",
        return_url = redirectUrl
      };
      // Adding Tax, shipping and Subtotal details  
      //var details = new Details()
      //{
      //  tax = "1",
      //  shipping = "1",
      //  subtotal = "1"
      //};
      //Final amount with details  
      var amount = new Amount()
      {
        currency = "MYR",
        total = paypalTopup.ToString() // Total must be equal to sum of tax, shipping and subtotal.  
        //details = details
      };
      var transactionList = new List<Transaction>();
      // Adding description about the transaction  
      transactionList.Add(new Transaction()
      {
        description = "Top",
        invoice_number = "test1", //Generate an Invoice No  
        amount = amount,
        item_list = itemList
      });
      this.payment = new Payment()
      {
        intent = "sale",
        payer = payer,
        transactions = transactionList,
        redirect_urls = redirUrls
      };
      // Create a payment using a APIContext  
      return this.payment.Create(apiContext);
    }

    public ActionResult EditNameCard()
    {
      var p = db.Providers.Find(Session["Email"].ToString()); //victorritdemo+p1@gmail.com" 

      ViewBag.namecard = p.namecard;

      return View();
    }

    [HttpPost]
    public JsonResult EditNameCard(string img)
    {
      var p = db.Providers.Find(Session["Email"].ToString()); //victorritdemo+p1@gmail.com" 

      string name = p.name != null ? p.name : p.companyName;

      string fileName = name + " namecard.jpg";
      string fileNameWitPath = Path.Combine(Server.MapPath("~/Image/NameCard/"), fileName);

      using (FileStream fs = new FileStream(fileNameWitPath, FileMode.Create))
      {
        using (BinaryWriter bw = new BinaryWriter(fs))
        {
          byte[] data = Convert.FromBase64String(img);
          bw.Write(data);
          bw.Close();
        }
        fs.Close();
      }

      if (p.namecard != null)
        DeletePhoto(p.profileImage, 'n');

      p.namecard = fileName;
      db.SaveChanges();

      TempData["Info"] = "Namecard Saved";

      return Json(String.Format("'Success':'true'"));
    }

    [HttpPost]
    public JsonResult PostNamecard(bool posting)
    {
      var p = db.Providers.Find(Session["Email"].ToString()); //victorritdemo+p1@gmail.com" 
      string message = "Failed, please check your wallet amount";

      if (posting && p.walletAmount >= 5.00)
      {
        p.postNamecard = true;
        p.walletAmount -= 5.00;
        p.dataPostNamecard = DateTime.Now.ToLocalTime();
        message = "Successfully Posted'";
      }
      else if (!posting)
      {
        p.postNamecard = false;
        p.dataPostNamecard = null;

        message = "Successfully unposted";
      }

      db.SaveChanges();

      return Json(String.Format(message));
    }

    public ActionResult NamecardPage()
    {
      var p = db.Providers.Where(n => n.postNamecard == true && n.namecard != null);
      DateTime today = DateTime.Now.ToLocalTime();

      foreach (var n in p)
      {
        DateTime postDate = (DateTime)n.dataPostNamecard;
        int date = DateTime.Compare(today.AddMonths(-1), postDate);

        if (date > 0)
        {
          if (n.walletAmount < 5.00)
          {
            n.postNamecard = false;
          }
          else
          {
            n.walletAmount -= 5.00;
            n.dataPostNamecard = DateTime.Now.ToLocalTime();
          }

        }
      }
      db.SaveChanges();

      IEnumerable<Provider> model = p;

      return View(model);
    }


    //================================================================================================================

    public ActionResult CheckEmail(string email)
    {
      bool isValid = false;
      if (db.Providers.Find(email) == null || db.Seekers.Find(email) == null)
        isValid = true;
      else if (db.Providers.Find(email) != null && db.Providers.Find(email).status == 0)
        isValid = true;

      return Json(isValid, JsonRequestBehavior.AllowGet);
    }

    public ActionResult CheckCompany(string companyName)
    {
      bool isValid = false;
      if (companyName != null)
        isValid = (db.Providers.Any(a => a.companyName != companyName));

      return Json(isValid, JsonRequestBehavior.AllowGet);
    }

    public ActionResult CheckAmount(double amount)
    {
      string user = Session["Email"].ToString(); //User.Identity.Name

      bool isValid = (db.Providers.Find(user).walletAmount > amount);
      return Json(isValid, JsonRequestBehavior.AllowGet);
    }


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
         .Save(path, "jpeg");

      return name;
    }

    private void DeletePhoto(string name, char type)
    {
      name = System.IO.Path.GetFileName(name);
      string path = Server.MapPath($"~/Image/Profile/{name}");
      if (type == 'n')
        path = Server.MapPath($"~/Image/NameCard/{name}");

      System.IO.File.Delete(path);
    }

    private void SendEmail(string email, string generate)
    {
      var user = db.Providers.Find(email);

      string name = "";
      name = user.name != null ? user.name : user.companyName;


      string url = Url.Action("ActivateAccount", "JobProvider", new { token = generate, email = email }, "http");     //http
      string url2 = Url.Action("ActivateAccount", "JobProvider", new { token = generate, email = email }, "https");

      var m = new MailMessage();

      string path = Server.MapPath($"~/Image/Profile/{user.profileImage}");
      var att = new Attachment(path);
      att.ContentId = "photo";
      m.Attachments.Add(att);

      m.To.Add($"{name} <{user.email}>");
      m.Subject = "Activate Account";
      m.IsBodyHtml = true;
      m.Body = $@"
                <img src='cid:photo' style='width: 100px; height: 100px;
                                            border: 1px solid #333'>
                <p>Dear {name},<p>
                <p>Thank you for registering, please noted that after activation, it would take 3 to 5 working days for account approval</p>
                <p>Please click the following link to activate your account</p>
                <h1 style='color: red'><a href='{url2}'>Activate Account</a></h1>
                <p>If link above failed, please try the following link</p>
                <p><a href='{url}'>Second Link</a></p>
                <p>From, Funny Hotel</p>'>";


      new SmtpClient().Send(m);
    }

    //==============================================================================

    [AllowAnonymous]
    public ActionResult ProviderLogin(string returnUrl)
    {
      return View();
    }

    //
    // POST: /Account/Login
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public ActionResult ProviderLogin(ProviderLoginModel model, string returnUrl)
    {
      //var user = _userManager.FindByNameAsync(model.Email);
      //var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);
      if (ModelState.IsValid)
      {
        var hashedPwd = Crypto.Hash(model.Password);
        //find Provider table
        var provider = db.Providers.Where(m => m.email == model.Email && m.password == hashedPwd).ToList();
        var providerAcc = db.Providers.Where(m => m.email == model.Email).ToList();

        if (provider.Count() <= 0)
        {
          ModelState.AddModelError("Error", "Invalid email or password");
          return View();
        }

        else if (provider.Count() > 0 && provider != null)
        {

          var logindetails = provider.First();
          // Login In.    
          //SignInUser(logindetails.email, "Provider", false);
          // setting.    
          Session["Role"] = "Provider";
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
        else if (providerAcc.Count() != 0)
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
      var verifyUrl = "/JobProvider/ProviderResetPassword/" + activationCode;
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
    public ActionResult ProviderResetPassword(string id)
    {
      using (ServerDBEntities db = new ServerDBEntities())
      {
        var seeker = db.Providers.Where(s => s.reset_pwd == id).FirstOrDefault();
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
    public ActionResult ProviderResetPassword(ResetPasswordModel model)
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
    public ActionResult ProviderForgetPasword()
    {
      return View();
    }

    [HttpPost]
    [AllowAnonymous]
    public ActionResult ProviderForgetPasword(string Email)
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
