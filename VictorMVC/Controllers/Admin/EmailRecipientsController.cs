using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VictorMVC.Models;
namespace VictorMVC.Controllers.Admin
{
    public class EmailRecipientsController : BaseAdminController, IFilelessController<EmailRecipient>
    {
        public ActionResult Manage()
        {
            Response.Cookies.Add(ValidateAccessToken(Request.Cookies[AuthContext.CookieName]));
            using (ContentContext db = new ContentContext())
            {
                ViewBag.Title = "Manage Email Recipients";
                ViewBag.EmailRecipients = db.EmailRecipients.OrderBy(e => e.Email).ToArray();
                return View();
            }
        }
        
        public ActionResult Modify(int? id)
        {
            Response.Cookies.Add(ValidateAccessToken(Request.Cookies[AuthContext.CookieName]));
            if (id == null)
            {
                ViewBag.Title = "Add new Email";
                ViewBag.IsNew = true;
                return View(new EmailRecipient());
            }
            else
            {
                using (ContentContext db = new ContentContext())
                {
                    EmailRecipient row = db.EmailRecipients.SingleOrDefault(d => d.ID == id);
                    if (row is null)
                    {
                        //log here
                        throw new HttpException(404, "Email not found");
                    }
                    else
                    {
                        ViewBag.Title = $"Update {row.Email}";
                        ViewBag.IsNew = false;
                        return View(row);
                    }
                }
            }
        }

        [HttpPost]
        public ActionResult Create(EmailRecipient recipient)
        {
            Response.Cookies.Add(ValidateAccessToken(Request.Cookies[AuthContext.CookieName]));
            if (!ModelState.IsValid)
            {
                Response.StatusCode = 400;
                return Json(new { Response.StatusCode, Message = "Unable to create Email. Improper Email format requested" });
            }
            using (ContentContext db = new ContentContext())
            {
                try
                {
                    db.CreateUnordered(db.EmailRecipients, recipient);
                    db.SaveChanges();
                    Response.StatusCode = 200;
                    return RedirectToAction("Manage");
                }
                catch
                {
                    //log here
                    Response.StatusCode = 500;
                    return Json(new { Response.StatusCode, Message = "Unable to create Email; Internal Server Error." });
                }
            }
        }

        [HttpPost]
        public ActionResult Update(EmailRecipient recipient)
        {
            Response.Cookies.Add(ValidateAccessToken(Request.Cookies[AuthContext.CookieName]));
            if (!ModelState.IsValid)
            {
                Response.StatusCode = 400;
                return Json(new {Response.StatusCode, Message = "Unable to update Email. Improper Email format requested" });
            }
            using (ContentContext db = new ContentContext())
            {
                try
                {
                    db.UpdateUnordered(db.EmailRecipients, recipient);
                    db.SaveChanges();
                    Response.StatusCode = 200;
                    return RedirectToAction("Manage");
                }
                catch (KeyNotFoundException)
                {
                    //log here
                    Response.StatusCode = 404;
                    return Json(new {Response.StatusCode, Message = "Unable to update Email; Email not found." });
                }
                catch
                {
                    //log here
                    Response.StatusCode = 500;
                    return Json(new {Response.StatusCode, Message = "Unable to update Email; Internal Server Error." });
                }
            }
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            Response.Cookies.Add(ValidateAccessToken(Request.Cookies[AuthContext.CookieName]));
            using (ContentContext db = new ContentContext())
            {
                try
                {
                    if (db.EmailRecipients.Count() <= 1)
                    {
                        //log here
                        Response.StatusCode = 403;
                        return Json(new { Response.StatusCode, Message = "Unable to Delete Email. At least one email must be on record." });
                    }
                    db.DeleteUnordered(db.EmailRecipients,id);
                    db.SaveChanges();
                    Response.StatusCode = 200;
                    return Json(new { Response.StatusCode, Message = "Email Deleted From Database."});
                }
                catch (KeyNotFoundException)
                {
                    //log here
                    Response.StatusCode = 404;
                    return Json(new { Response.StatusCode, Message = "Unable to delete Email; Email not found." });
                }
                catch
                {
                    //log here
                    Response.StatusCode = 500;
                    return Json(new { Response.StatusCode, Message = "Unable to delete Email; Internal Server Error." });
                }
            }
        }
    }
}