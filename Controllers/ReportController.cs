using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebSheduler.Models;

namespace WebSheduler.Controllers
{
    public class ReportController : Controller
    {
        // GET: Report
        public ActionResult Index()
        {
            bool Status = false;
            string message = "";
            using (ScheduleEntities dc = new ScheduleEntities())
            {
                var username = User.Identity.Name;
                var u = dc.Users.Where(a => a.Email == username).FirstOrDefault();

                if (u.StatusID == "Керівник")
                {
                    ViewBag.Message = "Ваш ID Керівника: " + u.UserID;
                    var workers = dc.Users.Where(a => a.IDEmployer == u.UserID).ToList();
                    List<List<Events>> events = new List<List<Events>>();
                    int i = 0;
                    foreach(var item in workers)
                    {
                        events.Add(dc.Events.Where(a => a.UserID == item.UserID).ToList());
                        i++;
                    }
                    var modelWorkers = new Workers { Users = workers, Events = events };
                    return View(modelWorkers);
                }
                else
                {
                    message = "Ви не керівник!";
                    Status = true;
                }
            }
            ViewBag.Message = message;
            ViewBag.Status = Status;
            return View();
        }
    }
}