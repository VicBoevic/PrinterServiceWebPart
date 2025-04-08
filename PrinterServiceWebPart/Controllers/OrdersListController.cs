using PrinterServiceWebPart.Models;
using PrinterServiceWebPart.Repositories;
using PrinterServiceWebPart.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace PrinterServiceWebPart.Controllers
{
    public class OrdersListController : Controller
    {
        private readonly OrderRepository _orderRepository;
        private readonly ClientRepository _clientRepository;
        private readonly ReviewRepository _reviewRepository;

        public OrdersListController(
            OrderRepository orderRepository,
            ClientRepository clientRepository,
            ReviewRepository reviewRepository)
        {
            _orderRepository = orderRepository;
            _clientRepository = clientRepository;
            _reviewRepository = reviewRepository;
        }

        public async Task<ActionResult> Index()
        {
            // Проверяем аутентификацию
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            // Получаем данные из аутентификационного куки
            var authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];
            if (authCookie == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var authTicket = FormsAuthentication.Decrypt(authCookie.Value);
            var clientEmail = authTicket.Name;
            var client = _clientRepository.GetByEmail(clientEmail);
            if (client == null)
            {
                return HttpNotFound("Клиент не найден");
            }
            var clientId = client.Id;
            var orders = await _orderRepository.GetOrdersByClientIdAsync(clientId);

            var orderViewModels = new List<OrderViewModel>();

            foreach (var order in orders)
            {
                var hasReview = await _reviewRepository.HasReviewAsync(order.Id);

                orderViewModels.Add(new OrderViewModel
                {
                    Id = order.Id,
                    OrderName = order.OrderName,
                    Status = order.Status,
                    Price = order.Price.ToString("N2"),
                    CreateDate = order.CreateDate.ToString("dd.MM.yyyy HH:mm"),
                    Models = _orderRepository.GetOrderModels(order.Id),
                    HasReview = hasReview,
                    CanAddReview = order.Status == "Closed",
                    CanBeCanceled = order.CanBeCanceled
                });
            }

            var viewModel = new OrdersListViewModel
            {
                ClientName = client.FullName,
                Orders = orderViewModels
            };

            return View(viewModel);
        }

        [HttpGet]
        public ActionResult AddReview(Guid orderId)
        {
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Login", "Account");

            return View(new ReviewViewModel { OrderId = orderId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddReview(ReviewViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return View(model);


                var review = new Review
                {
                    Id = Guid.NewGuid(),
                    OrderId = model.OrderId,
                    Score = model.Score,
                    ReviewText = model.ReviewText
                };

                await _reviewRepository.AddReviewAsync(review);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Ошибка при сохранении отзыва");
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CancelOrder(Guid orderId)
        {
            try
            {
                if (!User.Identity.IsAuthenticated)
                    return RedirectToAction("Login", "Account");


                var authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];
                if (authCookie == null)
                {
                    return RedirectToAction("Login", "Account");
                }
                var authTicket = FormsAuthentication.Decrypt(authCookie.Value);
                var clientEmail = authTicket.Name;
                var client = _clientRepository.GetByEmail(clientEmail);
                if (client == null)
                {
                    return HttpNotFound("Клиент не найден");
                }
                var clientId = client.Id;
                var success = await _orderRepository.CancelOrderAsync(orderId, clientId);

                if (!success)
                {
                    TempData["ErrorMessage"] = "Невозможно отменить этот заказ";
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Ошибка при отмене заказа";
                return RedirectToAction("Index");
            }
        }
    }
}