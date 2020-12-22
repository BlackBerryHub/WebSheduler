using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
namespace WebSheduler.Models
{
    [MetadataType(typeof(UsersMetadata))]
    public partial class Users
    {
        public string ConfirmPassword { get; set; }
    }

    public class UsersMetadata
    {
        [Display(Name = "Ім'я")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Ім'я порожнє")]
        public string FirstName { get; set; }

        [Display(Name = "Прізвище")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Прізвище порожнє")]
        public string LastName { get; set; }

        [Display(Name = "Email")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Email порожній")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Display(Name = "Дата народження")]
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime DateOfBirth { get; set; }

        [Display(Name = "Пароль")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Пароль порожній")]
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "Пароль повинний містити не менше 6 символів")]
        public string Password { get; set; }

        [Display(Name = "Повторити пароль")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Повторений пароль і пароль не співпадають")]
        public string ConfirmPassword { get; set; }

        [Display(Name = "Статус")]
        public string StatusID { get; set; }

        [Display(Name = "ID Керівника")]
        public int IDEmployer { get; set; }

    }
}