using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Threading.Tasks;
using PrinterServiceWebPart.Services;
using PrinterServiceWebPart.Repositories;
using System.Security.Claims;
using System.Web.Security;
using PrinterServiceWebPart.ViewModels;
using System.IO;
using Assimp;

namespace PrinterServiceWebPart.Controllers
{
    [Authorize]
    public class OrderController : System.Web.Mvc.Controller
    {
        private readonly OrderRepository _orderRepo;
        private readonly MaterialRepository _materialRepo;
        private readonly FileService _fileService;
        private const decimal ServiceMarkup = 1.15m; // Наценка 15%

        public OrderController(OrderRepository orderRepo, MaterialRepository materialRepo, FileService fileService)
        {
            _orderRepo = orderRepo;
            _materialRepo = materialRepo;
            _fileService = fileService;
        }

        [System.Web.Mvc.HttpGet]
        public ActionResult Create()
        {
            // Переносим сюда получение материалов
            var materials = _materialRepo.GetAllMaterials();
            ViewBag.Materials = new SelectList(materials, "Id", "Name");
            ViewBag.DensityList = new SelectList(new[]
            {
                new { Value = 1, Text = "Низкая" },
                new { Value = 2, Text = "Средняя" },
                new { Value = 3, Text = "Высокая" }
            }, "Value", "Text");

            return View(new CreateOrderViewModel());
        }

        [System.Web.Mvc.HttpPost]
        [System.Web.Mvc.ValidateAntiForgeryToken]
        public async Task<System.Web.Mvc.ActionResult> Create(CreateOrderViewModel model)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "ClientAccount");
            }

            var materials = _materialRepo.GetAllMaterials();
            ViewBag.Materials = new SelectList(materials, "Id", "Name");
            ViewBag.DensityList = new SelectList(new[]
            {
                new { Value = 1, Text = "Низкая" },
                new { Value = 2, Text = "Средняя" },
                new { Value = 3, Text = "Высокая" }
            }, "Value", "Text");

            if (!ModelState.IsValid)
            {
                return View(model);
            }


            string orderName = Request.Form["OrderName"];
            string comment = Request.Form["Comment"];

            if (string.IsNullOrEmpty(orderName))
            {
                ModelState.AddModelError("OrderName", "Введите название заказа.");
            }
            // Получаем ClientId из аутентификационного билета
            var authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];
            if (authCookie == null) return RedirectToAction("Login", "ClientAccount");

            var authTicket = FormsAuthentication.Decrypt(authCookie.Value);
            if (authTicket == null || string.IsNullOrEmpty(authTicket.UserData))
            {
                return RedirectToAction("Login", "ClientAccount");
            }

            if (!Guid.TryParse(authTicket.UserData, out Guid clientId))
            {
                ModelState.AddModelError("", "Ошибка идентификации пользователя");
                return View();
            }

            if (model.Files == null || model.Files.Count == 0)
            {
                ModelState.AddModelError("", "Необходимо загрузить хотя бы один файл");
                return View();
            }

            try
            {
                // Расчет стоимости
                var material = _materialRepo.GetMaterialById(model.SelectedMaterialId);
                decimal total = CalculatePriceInner(material.Id, model.Density, model.Files);
                var savedFiles = new List<string>();
                foreach (var file in model.Files)
                {
                    if (file != null)
                    {
                        savedFiles.Add(await _fileService.SaveFileAsync(file));
                    }
                }
                await _orderRepo.CreateOrderAsync(clientId, savedFiles, orderName, comment, material.Id, total);
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Ошибка при создании заказа: {ex.Message}");
                return View();
            }
        }

        [System.Web.Mvc.HttpPost]
        public JsonResult CalculatePrice(Guid SelectedMaterialId, int Density, List<HttpPostedFileBase> Files)
        {
            try
            {

                decimal total = CalculatePriceInner(SelectedMaterialId, Density, Files);

                return Json(new { success = true, totalPrice = total.ToString("N2") });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        private decimal CalculatePriceInner(Guid SelectedMaterialId, int density, List<HttpPostedFileBase> files)
        {


            decimal total = 0;
            decimal basePrice = 50;
            foreach (var file in files)
            {
                if (file != null && file.ContentLength > 0)
                {
                    var material = _materialRepo.GetMaterialById(SelectedMaterialId);
                    decimal[] dimensions = CalculateDimensions(file);
                    total = total +
                                basePrice *
                                material.PriceMultiplier *
                                density *
                                dimensions[0] * dimensions[1] * dimensions[2];
                }
            }
            total = Math.Round(total * ServiceMarkup, 2);
            return total;
        }

        private decimal[] CalculateDimensions(HttpPostedFileBase file)
        {
            if (file == null || file.ContentLength == 0)
            {
                throw new ArgumentException("The provided file is empty or null.");
            }

            AssimpContext importer = new AssimpContext();
            using (var stream = file.InputStream)
            {

                Scene scene = importer.ImportFileFromStream(stream, PostProcessSteps.Triangulate | PostProcessSteps.JoinIdenticalVertices | PostProcessSteps.SortByPrimitiveType);


                if (!scene.HasMeshes)
                {
                    throw new InvalidDataException("The model file does not contain any meshes.");
                }


                decimal minX = decimal.MaxValue, maxX = decimal.MinValue;
                decimal minY = decimal.MaxValue, maxY = decimal.MinValue;
                decimal minZ = decimal.MaxValue, maxZ = decimal.MinValue;

                foreach (Mesh mesh in scene.Meshes)
                {
                    foreach (Vector3D vertex in mesh.Vertices)
                    {
                        minX = Math.Min(minX, (sbyte)vertex.X);
                        maxX = Math.Max(maxX, (sbyte)vertex.X);
                        minY = Math.Min(minY, (sbyte)vertex.Y);
                        maxY = Math.Max(maxY, (sbyte)vertex.Y);
                        minZ = Math.Min(minZ, (sbyte)vertex.Z);
                        maxZ = Math.Max(maxZ, (sbyte)vertex.Z);
                    }
                }

                if (minX == decimal.MaxValue)  // check if any vertices were actually processed
                {
                    throw new InvalidDataException("No vertex data found in the model.");
                }

                // Вычисляем размеры по осям
                decimal sizeX = maxX - minX;
                decimal sizeY = maxY - minY;
                decimal sizeZ = maxZ - minZ;

                // Если размер по какой-либо оси оказался 0, заменяем его на 1
                sizeX = sizeX == 0 ? 1 : sizeX;
                sizeY = sizeY == 0 ? 1 : sizeY;
                sizeZ = sizeZ == 0 ? 1 : sizeZ;

                return new decimal[] { sizeX, sizeY, sizeZ };
            }
        }
    }
}
