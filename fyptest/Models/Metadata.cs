using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace fyptest.Models
{

  //view model
  public class OverallAccVM
  {
    public AccountDetailVM accDetail { get; set; }
    public IEnumerable<Request> requestDetail { get; set; }
    public IEnumerable<Service_Category> categories { get; set; }
    public IEnumerable<Request> recommend { get; set; }
  }

  public class AccountDetailVM
  {
    public string Email { get; set; }
    public string Phone { get; set; }
    public string Address { get; set; }
    public bool CompanyIndividual { get; set; }
    public string Name { get; set; }
    [Display(Name = "Company Name")]
    public string CompanyName { get; set; }
    [Display(Name = "Service")]
    public string ServiceType { get; set; }
    public string ProfileImage { get; set; }
    public double? Wallet { get; set; }
    public string Namecard { get; set; }
    public int Attitude { get; set; }
    public int Quality { get; set; }
    public int Efficiency { get; set; }
    public int Professionalism { get; set; }
  }

  public class AdminApprovalVM
  {
    public string Email { get; set; }
    public string Phone { get; set; }
    public string Address { get; set; }
    public bool CompanyIndividual { get; set; }
    public string Name { get; set; }
    public string CompanyName { get; set; }
    public string ServiceType { get; set; }
    public string ProfileImage { get; set; }
    public IEnumerable<Document> document { get; set; }
  }

  public class ServiceCRUDVM
  {
    public IEnumerable<Service_Type> Types { get ; set;}
    public IEnumerable<Service_Category> Categories { get; set; }
  }

  public class RequestDetail
  {
    public IEnumerable<Request> request { get; set; }
    public string Id { get; set; }
    public string Title { get; set; }
    public string Address { get; set; }
    public string Description { get; set; }
    public string Image { get; set; }
    public string File { get; set; }
    public DateTime? DateCreated { get; set; }
    public double Price { get; set; }
    public string Category { get; set; }
    public int Status { get; set; }
    public bool? SeekerComplete { get; set; }
    public bool? ProviderComplete { get; set; }
  }


  //input model

  public class RegisterProviderModel
  {
    [Required]
    [StringLength(50)]
    [EmailAddress]
    [Remote("CheckEmail", "JobProvider", ErrorMessage = "Duplicated {0}.")]
    [RegularExpression(@"^.+@.+mail.com$", ErrorMessage = "Invalid Email format")]
    public string Email { get; set; }

    [Required]
    [StringLength(20, MinimumLength = 6)]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[-!#@$%^&*()_+|~=`{}\[\]:;'<>?,.\/\\])[A-Za-z\d-!#@$%^&*()_+|~=`{}\[\]:;'<>?,.\/\\]{8,20}$",
            ErrorMessage = "Password must be between 6 to 20, contain at least 1 lower and uppercase, a digit and a symbol")]
    public string Password { get; set; }

    [Required]
    [Display(Name = "Confirm Password")]
    [StringLength(20, MinimumLength = 6)]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[-!#@$%^&*()_+|~=`{}\[\]:;'<>?,.\/\\])[A-Za-z\d-!#@$%^&*()_+|~=`{}\[\]:;'<>?,.\/\\]{8,20}$",
            ErrorMessage = "Password must be between 6 to 20, contain at least 1 lower and uppercase, a digit and a symbol")]
    [System.ComponentModel.DataAnnotations.Compare("Password")]
    public string ConfirmPass { get; set; }

    [Required]
    [StringLength(11)]
    [Phone]
    [RegularExpression(@"^(601)([0-9]{8})$", ErrorMessage = "Entered phone format is not valid.")]
    public string Phone { get; set; }

    public bool? Formality { get; set; }

    [StringLength(100)]
    public string Address { get; set; }

    [Required]
    [Display(Name = "Service Type")]
    public string ServiceType { get; set; }

    [Required]
    public bool CompanyIndividual { get; set; }

    [StringLength(50)]
    public string Name { get; set; }

    [StringLength(50)]
    [Display(Name = "Company Name")]
    [Remote("CheckCompany", "JobProvider", ErrorMessage = "Duplicated {0}.")]
    public string CompanyName { get; set; }

    [Required(ErrorMessage = "Please select file.")]
    [Display(Name = "Browse File")]
    public HttpPostedFileBase[] Document { get; set; }

    [Required(ErrorMessage = "Please select a profile picture. (.png .jpg)")]
    public HttpPostedFileBase ProfileImage { get; set; }
  }

  public class EditProfileModel
  {
    public string Email { get; set; }
    [Required]
    [StringLength(11)]
    [Phone]
    [RegularExpression(@"^(601)([0-9]{8})$", ErrorMessage = "Invalid Phone Number")]
    public string Phone { get; set; }

    [StringLength(100)]
    public string Address { get; set; }

    //public HttpPostedFileBase ProfileImage { get; set; }
  }

  public class WithdrawModel
  {
    [Required(ErrorMessage = "Please select a bank.")]
    public string Bank { get; set; }

    [Required(ErrorMessage = "Please enter account number.")]
    [RegularExpression(@"[0-9]{9,20}", ErrorMessage = "Please enter a valid account number.")]
    public string AccountNumber { get; set; }

    [Required(ErrorMessage = "Please enter an amount.")]
    [RegularExpression(@"[0-9]+(.[0-9]{2})?", ErrorMessage = "Please enter a valid amount.")]
    [Remote("CheckAmount", "JobProvider", ErrorMessage = "Insufficent balance.")]
    public double Amount { get; set; }
  }

  public class TopupModel
  {
    [Required(ErrorMessage = "Please enter card number.")]
    [RegularExpression(@"[0-9]{4}\-[0-9]{4}\-[0-9]{4}\-[0-9]{4}", ErrorMessage = "Please enter in correct format.")]
    public string CardNumber { get; set; }

    [Required(ErrorMessage = "Please enter CCV number.")]
    [RegularExpression(@"[0-9]{3}", ErrorMessage = "Please enter in correct format.")]
    public int CCV { get; set; }

    [Required(ErrorMessage = "Please enter card expiry date.")]
    [RegularExpression(@"[0-9]{2}\/[0-9]{2}", ErrorMessage = "Please enter in correct format.")]
    public string ExpireDate { get; set; }

    [Required(ErrorMessage = "Please enter an amount.")]
    [RegularExpression(@"[0-9]+(\.[0-9]{2})?", ErrorMessage = "Please enter a valid amount.")]
    public double Amount { get; set; }
  }

  public class AdminViewModel
  {
    public List<AdminUserView> UserList { get; set; }
    public List<AdminJobViewModel> JobList { get; set; }
  }

  public class AdminUserView
  {
    public string FullName { get; set; }
    public int UserID { get; set; }

    public string Email { get; set; }

    public string Password { get; set; }

    public string HandphoneNo { get; set; }

    public string Gender { get; set; }

    public string Photo { get; set; }

    public string Role { get; set; }

    public string CompanyOrIndividual { get; set; }

    public string DocumentsPath { get; set; }
    public string Status { get; set; }
    public string Address { get; set; }
  }

  public class AdminJobViewModel
  {
    public int JobID { get; set; }
    public string Title { get; set; }

    public string Handphone { get; set; }

    public string Category { get; set; }

    public string Location { get; set; }

    public string SelectedType { get; set; }

    public double Price { get; set; }

    public string Description { get; set; }

    public DateTime Date { get; set; }
    public DateTime CompleteDate { get; set; }

    public string Image { get; set; }
    public string Comment { get; set; }
    public int Rating { get; set; }
    public string Provider { get; set; }
    public string Seeker { get; set; }
    public string Status { get; set; }
  }

  public class SeekerRegisterModel
  {
    [Required(AllowEmptyStrings = false, ErrorMessage = "Please enter your name.")]
    [Display(Name = "Name")]
    public string FullName { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessage = "Please enter your email.")]
    [EmailAddress(ErrorMessage = "Invalid Email Address")]
    [Display(Name = "Email")]
    public string Email { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessage = "Please enter your password.")]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessage = "Please reenter your password.")]
    [DataType(DataType.Password)]
    [System.ComponentModel.DataAnnotations.Compare("Password", ErrorMessage = "Your passwords do not match.")]
    [Display(Name = "Confirm Password")]
    public string ConfirmPassword { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessage = "Please enter your contact number.")]
    [Display(Name = "Contact number")]
    [RegularExpression(@"^(601)([0-9]{8})$", ErrorMessage = "Entered phone format is not valid.")]
    public string Phone { get; set; }

    [Display(Name = "Profile picture")]
    public string Photo { get; set; }
    public string SuccessMessage { get; set; }

    public System.Web.HttpPostedFileBase ImageFile { get; set; }
    public string Status { get; set; }
  }

  public class SeekerLoginModel
  {
    [Required]
    [Display(Name = "Email")]
    [EmailAddress(ErrorMessage = "Invalid Email Address")]
    public string Email { get; set; }

    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; }

    [Display(Name = "Remember me?")]
    public bool RememberMe { get; set; }
  }
  public class ProviderLoginModel
  {
    [Required]
    [Display(Name = "Email")]
    [EmailAddress(ErrorMessage = "Invalid Email Address")]
    public string Email { get; set; }

    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; }

    [Display(Name = "Remember me?")]
    public bool RememberMe { get; set; }
  }

  public class AdminLoginModel
  {
    [Required]
    [Display(Name = "Email")]
    [EmailAddress(ErrorMessage = "Invalid Email Address")]
    public string Email { get; set; }

    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; }

    [Display(Name = "Remember me?")]
    public bool RememberMe { get; set; }
  }

  public class ConversationModel
  {
    public ChatConnectionModel ConnectionModelObj { get; set; }
    public List<MessageInfo> MessageList { get; set; }
  }

  public class ChatConnectionModel
  {
    public int ConnectionID { get; set; }
    public string Seeker { get; set; }
    public string Provider { get; set; }
    public string Message { get; set; }
    public DateTime FirstCreateDate { get; set; }
    public DateTime LastConnectedDate { get; set; }
    public string SeekerPhoto { get; set; }
    public string ProviderPhoto { get; set; }
    public string GroupName { get; set; }
    public string Receiver { get; set; }
  }

  public class MessageInfo
  {
    public string MessageId { get; set; }
    public string UserName { get; set; }

    public string Message { get; set; }
    public string Sender { get; set; }
    public string Receiver { get; set; }

    public string UserGroup { get; set; }

    public string StartTime { get; set; }

    public string EndTime { get; set; }

    public string MsgDate { get; set; }
    public string IsMedia { get; set; }
  }

  public class ResetPasswordModel
  {
    [Required(ErrorMessage = "New password required", AllowEmptyStrings = false)]
    [DataType(DataType.Password)]
    public string NewPassword { get; set; }



    [Required(AllowEmptyStrings = false, ErrorMessage = "Please reenter your password.")]
    [DataType(DataType.Password)]
    [System.ComponentModel.DataAnnotations.Compare("NewPassword", ErrorMessage = "Your passwords do not match.")]
    public string ConfirmPassword { get; set; }

    [Required]
    public string ResetCode { get; set; }
  }

  public class ForgotPasswordViewModel
  {
    [Required]
    [EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; }
  }

  public class JobCreateModel
  {

    [Required(AllowEmptyStrings = false, ErrorMessage = "Please enter the title.")]
    [Display(Name = "Job Title")]
    public string Title { get; set; }

    [Display(Name = "Category")]
    public string Category { get; set; }

    [Display(Name = "Location")]
    public string Location { get; set; }

    [Display(Name = "Type")]
    public string SelectedType { get; set; }
    public string Seeker { get; set; }
    public string Contact { get; set; }

    [Display(Name = "Price")]
    public double Price { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessage = "Please describe your job.")]
    [Display(Name = "Description")]
    public string Description { get; set; }


    [Display(Name = "Date")]
    [DataType(DataType.DateTime)]
    public DateTime Date { get; set; }

    [Display(Name = "Image")]
    public string Image { get; set; }

    public Dictionary<string, string> Type { get; set; }
    public IEnumerable<SelectListItem> CategoryList { get; set; }

    public string SuccessMessage { get; set; }

    public HttpPostedFileBase ImageFile { get; set; }

    [Display(Name = "Browse File")]
    public HttpPostedFileBase[] Document { get; set; }
    public string DocumentPath { get; set; }

  }

  public class JobViewModel
  {
    public string JobID { get; set; }
    public string Title { get; set; }

    public string Handphone { get; set; }

    public string Category { get; set; }

    public string Location { get; set; }

    public string SelectedType { get; set; }

    public double Price { get; set; }

    public string Description { get; set; }

    public DateTime Date { get; set; }
    public DateTime CompleteDate { get; set; }

    public string Image { get; set; }
    public string Comment { get; set; }
    public int Rating { get; set; }
    public string Provider { get; set; }
    public string Seeker { get; set; }
    public string Status { get; set; }
  }

}

