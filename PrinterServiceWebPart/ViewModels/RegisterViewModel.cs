using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace PrinterServiceWebPart.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Email обязателен")]
        [EmailAddress(ErrorMessage = "Некорректный Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Пароль обязателен")]
        [DataType(DataType.Password)]
        [StringLength(255, ErrorMessage = "Пароль должен содержать минимум 6 символов", MinimumLength = 6)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Укажите ваше имя")]
        [Display(Name = "Полное имя")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Номер телефона обязателен")]
        [Phone(ErrorMessage = "Некорректный номер телефона")]
        [Display(Name = "Номер телефона")]
        public string PhoneNumber { get; set; }
    }
}
