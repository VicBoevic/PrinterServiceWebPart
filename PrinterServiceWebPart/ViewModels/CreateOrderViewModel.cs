using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace PrinterServiceWebPart.ViewModels
{
    public class CreateOrderViewModel
    {
        [Required(ErrorMessage = "Введите название заказа")]
        [Display(Name = "Название заказа")]
        public string OrderName { get; set; }
        [Display(Name = "Комментарий")]
        public string Comment { get; set; }
        [Display(Name = "Материал")]
        [Required(ErrorMessage = "Выберите материал")]
        public Guid SelectedMaterialId { get; set; }

        [Display(Name = "Плотность печати")]
        [Required(ErrorMessage = "Выберите плотность печати")]
        [Range(1, 3, ErrorMessage = "Некорректное значение плотности")]
        public int Density { get; set; }

        [Display(Name = "Файлы моделей")]
        [Required(ErrorMessage = "Загрузите файлы моделей")]
        public List<HttpPostedFileBase> Files { get; set; }
    }
}