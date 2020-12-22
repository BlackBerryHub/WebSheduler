using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebSheduler.Models;
using System.Net.Mail;
using System.Net;
using System.Web.Security;

namespace WebSheduler.Controllers
{
    public class UserController : Controller
    {
        [HttpGet]
        public ActionResult Registration()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Registration([Bind(Exclude = "IsEmailVerified,ActivationCode")] Users user)
        {
            bool Status = false;
            string message = "";

            if (ModelState.IsValid)
            {
                #region 
                var isExist = IsEmailExist(user.Email);
                if (isExist)
                {
                    ModelState.AddModelError("EmailExist", "Електронна пошта вже існує");
                    return View(user);
                }
                #endregion

                #region 
                if(user.StatusID == "Працівник")
                {
                    var isEmployerExist = IsEmployerExist(user.IDEmployer);
                    if (!isEmployerExist)
                    {
                        ModelState.AddModelError("EmployerExist", "Такого керівника не існує");
                        return View(user);
                    }
                }
                #endregion

                #region Generate Activation Code 
                user.ActivationCode = Guid.NewGuid();
                #endregion

                #region  Password Hashing 
                user.Password = Crypto.Hash(user.Password);
                user.ConfirmPassword = Crypto.Hash(user.ConfirmPassword); 
                #endregion
                user.IsEmailVerified = false;

                #region Save to Database
                using (ScheduleEntities dc = new ScheduleEntities())
                {
                    dc.Users.Add(user);
                    dc.SaveChanges();

                    SendVerificationLinkEmail(user.Email, user.ActivationCode.ToString());
                    message = "Реєстрація успішно виконана. Посилання для активації облікового запису " +
                        " було надіслано на вашу електронну пошту: " + user.Email;
                    Status = true;
                }
                #endregion
            }
            else
            {
                message = "Невірний запит";
            }

            ViewBag.Message = message;
            ViewBag.Status = Status;
            return View(user);
        }

        [HttpGet]
        public ActionResult VerifyAccount(string id)
        {
            bool Status = false;
            using (ScheduleEntities dc = new ScheduleEntities())
            {
                dc.Configuration.ValidateOnSaveEnabled = false; 
                                                               
                var v = dc.Users.Where(a => a.ActivationCode == new Guid(id)).FirstOrDefault();
                if (v != null)
                {
                    v.IsEmailVerified = true;
                    dc.SaveChanges();
                    Status = true;
                }
                else
                {
                    ViewBag.Message = "Невірний запит";
                }
            }
            ViewBag.Status = Status;
            return View();
        }

        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(UserLogin login, string ReturnUrl = "")
        {
            string message = "";
            using (ScheduleEntities dc = new ScheduleEntities())
            {
                var v = dc.Users.Where(a => a.Email == login.Email).FirstOrDefault();

                if (v != null)
                {
                    if (!v.IsEmailVerified)
                    {
                        ViewBag.Message = "Спершу підтвердьте свою електронну адресу";
                        return View();
                    }

                    if (string.Compare(Crypto.Hash(login.Password), v.Password) == 0)
                    {
                        int timeout = login.RememberMe ? 525600 : 20; 
                        var ticket = new FormsAuthenticationTicket(login.Email, login.RememberMe, timeout);
                        string encrypted = FormsAuthentication.Encrypt(ticket);
                        var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encrypted);
                        cookie.Expires = DateTime.Now.AddMinutes(timeout);
                        cookie.HttpOnly = true;
                        Response.Cookies.Add(cookie);


                        if (Url.IsLocalUrl(ReturnUrl))
                        {
                            return Redirect(ReturnUrl);
                        }
                        else
                        {
                            return RedirectToAction("Index", "Home");
                        }
                    }
                    else
                    {
                        message = "Надано недійсні облікові дані";
                    }
                }
                else
                {
                    message = "Надано недійсні облікові дані";
                }
            }
            ViewBag.Message = message;
            return View();
        }

        [Authorize]
        [HttpPost]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login", "User");
        }

        [NonAction]
        public bool IsEmailExist(string emailID)
        {
            using (ScheduleEntities dc = new ScheduleEntities())
            {
                var v = dc.Users.Where(a => a.Email == emailID).FirstOrDefault();
                return v != null;
            }
        }

        [NonAction]
        public bool IsEmployerExist(int? employerID)
        {
            using (ScheduleEntities dc = new ScheduleEntities())
            {
                var v = dc.Users.Where(a => a.UserID == employerID).FirstOrDefault();
                return v != null;
            }
        }

        [NonAction]
        public void SendVerificationLinkEmail(string emailID, string activationCode)
        {
            var verifyUrl = "/User/VerifyAccount/" + activationCode;
            var link = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, verifyUrl);

            var fromEmail = new MailAddress("marat.khusnutdinov.knm.2018@lpnu.ua", "Календар Планувальник");
            var toEmail = new MailAddress(emailID);
            var fromEmailPassword = "25.01.2000"; 
            string subject = "Ваш аккаунт успішно створений!";

            string body = "<br/><br/>Ми повідомляємо вам, що ваш аккаунт у Календарі-планувальнику є" +
                " успішно створено. Клацніть на посилання нижче, щоб підтвердити свій аккаунт" +
                " <br/><br/><a href='" + link + "'>" + link + "</a> ";

            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromEmail.Address, fromEmailPassword)
            };

            using (var message = new MailMessage(fromEmail, toEmail)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            })
            smtp.Send(message);
        }
    }
}