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
        ModelState.AddModelError("Email", "Duplicated Email.");

      //check companyname duplicate
      if (ModelState.IsValidField("CompanyName") && m.CompanyName != null)
        if (db.Providers.Any(a => a.companyName == m.CompanyName))
          ModelState.AddModelError("CompanyName", "Duplicated Company Name.");


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
          password = HashPassword(m.Password),
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
          STId = m.ServiceType,
          RId = m.Email
        };

        var dbRate = new Rating
        {
          RId = m.Email,
          attitude = 0,
          quality = 0,
          efficiency = 0,
          professionalism = 0
        };



        db.Providers.Add(dbProvider);
        db.Ratings.Add(dbRate);
        db.SaveChanges();

        SendEmail(m.Email, generate);

        TempData["Info"] = "Account registered. Please go to email to active account.";

      }

      //db.Customers.Add(m);
      //db.SaveChanges();

      //SendEmail(m, null, generate, 'A');    //send email

      //TempData["Info"] = "Account registered. Please go to email to active account.";
      //return RedirectToAction("CustLogin", "Account");

      return RedirectToAction("Index", "Home");
    }

    public ActionResult ActivateAccount(string token, string email)
    {
      var user = db.Providers.Find(email);

      ViewBag.MetaRefresh = "<meta http-equiv='refresh' content='3;url=" + Url.Action("Index", "Home") + "' />";

      if (token == user.token)
      {
        user.status = 1;
        user.token = null;
        user.tokenExpire = null;
        db.SaveChanges();
        TempData["Info"] = "Account Activated!";
      }
      else
        TempData["Info"] = "Invalid activate token!";

      return View();
    }


    public ActionResult ProviderProfile()
    {
      string user = "victorritdemo+p1@gmail.com"; //User.Identity.Name
      var data = db.Providers.Find(user);
      var type = db.Service_Types.Find(data.STId);
      var rating = db.Ratings.Find(user);
      var request = db.Requests.Where(r => r.Provider == user);
      var recommend = db.Requests.Where(r => r.Type == data.STId && r.Provider == null && r.status == 0);
      var category = db.Service_Categories;


      var accModel = new AccountDetailVM
      {
        Email = data.email,
        Phone = data.phone,
        Address = data.address,
        Name = data.name,
        ProfileImage = data.profileImage,
        Wallet = data.walletAmount,
        CompanyIndividual = data.companyIndividual,
        CompanyName = data.companyName,
        ServiceType = type.name,
        Attitude = rating.attitude,
        Quality = rating.quality,
        Efficiency = rating.efficiency,
        Professionalism = rating.professionalism
      };


      var model = new OverallAccVM
      {
        accDetail = accModel,
        requestDetail = request,
        categories = category,
        recommend = recommend
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
      var p = db.Providers.Find("victorritdemo+p1@gmail.com");//User.Identity.Name

      if (file == null)
        return Json(String.Format("'Error' : '{0}'", "Failed"));

      DeletePhoto(p.profileImage);

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
        var p = db.Providers.Find("victorritdemo+p1@gmail.com");//User.Identity.Name
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
          var p = db.Providers.Find("victorritdemo+p1@gmail.com");//User.Identity.Name
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

      return RedirectToAction("TopupPaypal", "JobProvider");
    }

    [HttpPost]
    public JsonResult PaypalAmount (double amount)
    {
      paypalTopup = amount;
      
      return Json(String.Format("'Success':'true'"));
    }


    public ActionResult TopupPaypal(string Cancel = null)
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
          string baseURI = Request.Url.Scheme + "://" + Request.Url.Authority + "/JobProvider/TopupPaypal?";
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
    private PayPal.Api.Payment payment;
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
      return this.payment.Execute(apiContext, paymentExecution);
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
        name = "Top Up",
        currency = "MYR",
        price = paypalTopup.ToString()
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
      //// Adding Tax, shipping and Subtotal details  
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
        total = "3", // Total must be equal to sum of tax, shipping and subtotal.  
      };
      var transactionList = new List<Transaction>();
      // Adding description about the transaction  
      transactionList.Add(new Transaction()
      {
        description = "Transaction description",
        invoice_number = "your generated invoice number", //Generate an Invoice No  
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


    //================================================================================================================

    public ActionResult CheckEmail(string email)
    {
      bool isValid = (db.Providers.Find(email) == null || db.Seekers.Find(email) == null);
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
      string user = "victorritdemo+p1@gmail.com"; //User.Identity.Name

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

    private void DeletePhoto(string name)
    {
      name = System.IO.Path.GetFileName(name);
      string path = Server.MapPath($"~/Image/Profile/{name}");
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
                <p>Please click the following link to activate your account</p>
                <h1 style='color: red'><a href='{url2}'>Activate Account</a></h1>
                <p>If link above failed, please try the following link</p>
                <p><a href='{url}'>Second Link</a></p>
                <p>From, Funny Hotel</p>'>";


      new SmtpClient().Send(m);
    }


  }


}
