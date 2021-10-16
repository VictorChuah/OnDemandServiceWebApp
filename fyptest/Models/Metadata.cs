using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace fyptest.Models
{
  public class RegisterModel
  {
    [Required]
    [StringLength(50)]
    public string Name { get; set; }

    [Required]
    [StringLength(20,MinimumLength = 6)]
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

    [StringLength(50)]
    [Display(Name = "Company Name")]
    [Remote("CheckCompany", "JobProvider", ErrorMessage = "Duplicated {0}.")]
    public string CompanyName { get; set; }

    [StringLength(100)]
    [Display(Name = "Company Address")]
    public string CompanyAddress { get; set; }

    [Required]
    [StringLength(50)]
    [EmailAddress]
    [Remote("CheckEmail", "JobProvider", ErrorMessage = "Duplicated {0}.")]
    [RegularExpression(@"^.+@.+mail.com$", ErrorMessage = "Invalid Email format")]
    public string Email { get; set; }

    [Required]
    [StringLength(11)]
    [Phone]
    [RegularExpression(@"^(601)([0-9]{8})$", ErrorMessage = "Invalid Phone Number")]
    public string Phone { get; set; }

    [Required]
    public string Formality { get; set; }

    [Required]
    public string Category { get; set; }

    [Required]
    public string Documement { get; set; } //doc file how handle

    public HttpPostedFileBase Photo { get; set; }

  }
}
