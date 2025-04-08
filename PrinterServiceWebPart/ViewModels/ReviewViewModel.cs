using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace PrinterServiceWebPart.ViewModels
{
    public class ReviewViewModel
    {
        public Guid OrderId { get; set; }

        [Required(ErrorMessage = "Поле обязательно для заполнения")]
        [Range(1, 5, ErrorMessage = "Оценка должна быть от 1 до 5")]
        public int Score { get; set; }

        [StringLength(500, ErrorMessage = "Максимальная длина отзыва 500 символов")]
        public string ReviewText { get; set; }
    }
}