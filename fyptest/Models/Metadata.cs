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
  public class RegisterSeekerModel
  {

  }

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
    [RegularExpression(@"^(601)([0-9]{8})$", ErrorMessage = "Invalid Phone Number")]
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
}

