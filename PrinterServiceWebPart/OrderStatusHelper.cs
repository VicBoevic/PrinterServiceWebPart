using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PrinterServiceWebPart
{
    public static class OrderStatusHelper
    {
        private static readonly Dictionary<string, string> StatusTranslations = new Dictionary<string, string>
    {
        {"Not_Starded", "Не начат"},
        {"In_Progress", "В работе"},
        {"Ready", "Готов"},
        {"Closed", "Закрыт"},
        {"Canceled", "Отменён"}
    };

        public static string GetRussianStatus(string englishStatus)
        {
            return StatusTranslations.TryGetValue(englishStatus, out var russianStatus)
                ? russianStatus
                : englishStatus;
        }
    }
}