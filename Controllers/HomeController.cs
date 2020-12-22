using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using WebSheduler.Models;

namespace WebSheduler.Controllers
{
    public class HomeController : Controller
    {
        [Authorize]
        public ActionResult Index()
        {
            return View();
        }

        public JsonResult GetEvents()
        {
            using (ScheduleEntities dc = new ScheduleEntities())
            {
                var username = User.Identity.Name;
                var u = dc.Users.Where(a => a.Email == username).FirstOrDefault();
                var events = dc.Events.Where(a => a.UserID == u.UserID).ToList();

                return new JsonResult { Data = events, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
        }

        [HttpPost]
        public JsonResult SaveEvent(Events e)
        {
            var status = false;
            using (ScheduleEntities dc = new ScheduleEntities())
            {
                var username = User.Identity.Name;
                var u = dc.Users.Where(t => t.Email == username).FirstOrDefault();
                if (e.EventID > 0)
                {
                    var v = dc.Events.Where(a => a.EventID == e.EventID).FirstOrDefault();
                    if (v != null)
                    {
                        v.Subject = e.Subject;
                        v.Start = e.Start;
                        v.End = e.End;
                        v.Description = e.Description;
                        v.IsFullDay = e.IsFullDay;
                        v.ThemeColor = e.ThemeColor;
                        v.UserID = u.UserID;
                    }
                }
                else
                {
                    e.UserID = u.UserID;
                    dc.Events.Add(e);
                }

                dc.SaveChanges();
                status = true;

            }
            return new JsonResult { Data = new { status = status } };
        }

        [HttpPost]
        public JsonResult DeleteEvent(int eventID)
        {
            var status = false;
            using (ScheduleEntities dc = new ScheduleEntities())
            {
                var v = dc.Events.Where(a => a.EventID == eventID).FirstOrDefault();
                if (v != null)
                {
                    dc.Events.Remove(v);
                    dc.SaveChanges();
                    status = true;
                }
            }
            return new JsonResult { Data = new { status = status } };
        }
    }
}