using PrinterServiceWebPart.Models;
using PrinterServiceWebPart.Repositories;
using PrinterServiceWebPart.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace PrinterServiceWebPart.Controllers
{
    public class ClientAccountController : Controller
    {
        private readonly ClientRepository _clientRepo;

        public ClientAccountController(ClientRepository clientRepo)
        {
            _clientRepo = clientRepo;
        }

        // GET: /Account/Register
        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var client = new Client
                {
                    Email = model.Email,
                    Password = BCrypt.Net.BCrypt.HashPassword(model.Password),
                    FullName = model.FullName,
                    PhoneNumber = model.PhoneNumber
                };

                _clientRepo.Register(client);
                return RedirectToAction("Index", "Home");
            }
            return View(model);
        }


        // GET: /Account/Login
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var client = _clientRepo.GetByEmail(model.Email);
                if (client != null && BCrypt.Net.BCrypt.Verify(model.Password, client.Password))
                {
                    // Создаем билет аутентификации с ClientId
                    var authTicket = new FormsAuthenticationTicket(
                        version: 1,
                        name: client.Email,
                        issueDate: DateTime.Now,
                        expiration: DateTime.Now.AddMinutes(30),
                        isPersistent: false,
                        userData: client.Id.ToString() // Сохраняем ClientId в userData
                    );
                    string encryptedTicket = FormsAuthentication.Encrypt(authTicket);
                    var authCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket);
                    Response.Cookies.Add(authCookie);

                    return RedirectToAction("Create", "Order"); // Перенаправляем на создание заказа
                }
                ModelState.AddModelError("", "Неверный email или пароль");
            }
            return View(model);
        }
    }
}