using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VictorMVC.Models;

namespace VictorMVC.Controllers.Admin
{
    public class SmtpController : BaseAdminController, IFilelessController<SmtpHost>
    {
        /// <summary>
        /// Unimplemented Manage command reserved for possible future increases in complexity. Since there is
        /// currently one and only one smtp server expected to be used, the UI will jump straight to modify.
        /// </summary>
        /// <returns></returns>
        public ActionResult Manage()
        {
            return Modify(null);
        }


        public ActionResult Modify(int? id)
        {
            Response.Cookies.Add(ValidateAccessToken(Request.Cookies[AuthContext.CookieName]));
            ViewBag.Title = "Edit Smtp Host";
            using (ContentContext db = new ContentContext())
            {
                SmtpHost existingHost = db.SmtpHosts.FirstOrDefault();
                if (id == null)
                {
                    if (existingHost == null)
                    {
                        ViewBag.IsNew = true;
                        return View(new SmtpHost());
                    }
                    ViewBag.IsNew = false;
                    return View(existingHost);
                }
                else
                {
                    SmtpHost queriedHost = db.SmtpHosts.Where(s => s.ID == id).FirstOrDefault();
                    if (db.SmtpHosts.Where(s => s.ID == id).FirstOrDefault() == null)
                    {
                        throw new HttpException(404, "SmtpHost not Found");
                    }
                    ViewBag.IsNew = false;
                    return View(queriedHost);
                }
            }
        }

        /// <summary>
        /// Creates new smtp user if none exists, otherwise defers to update.
        /// </summary>
        /// <param name="smtp">smtp host record we would like to add</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Create(SmtpHost smtp)
        {
            Response.Cookies.Add(ValidateAccessToken(Request.Cookies[AuthContext.CookieName]));
            using (ContentContext db = new ContentContext())
            {
                if (!ModelState.IsValid)
                {
                    Response.StatusCode = 400;
                    return Json(new { Response.StatusCode, Message = "Invalid Article Input" });
                }
                SmtpHost existingHost = db.SmtpHosts.FirstOrDefault();
                try
                {
                    if (existingHost == null)
                    {
                        db.SmtpHosts.Add(smtp);
                        db.SaveChanges();
                        return Json(new { Response.StatusCode, Message = "Smtp Host record created successfully" });
                    }
                    else
                    {
                        smtp.ID = existingHost.ID;
                        return Update(smtp);
                    }
                }
                catch (Exception ex)
                {
                    Response.StatusCode = 500;
                    return Json(new { Response.StatusCode, ex.Message });
                }
            }
        }
        /// <summary>
        /// Updates existing smtpHost record
        /// </summary>
        /// <param name="smtp">updated value for given smtp host id</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Update(SmtpHost smtp)
        {
            Response.Cookies.Add(ValidateAccessToken(Request.Cookies[AuthContext.CookieName]));
            if (!ModelState.IsValid)
            {
                Response.StatusCode = 400;
                return Json(new { Response.StatusCode, Message = "Invalid Smtp Input" });
            }
            using (ContentContext db = new ContentContext())
            {
                try
                {
                    db.UpdateUnordered(db.SmtpHosts, smtp);
                    db.SaveChanges();
                    return Json(new { Response.StatusCode, Message="Smtp Host record updated successfully"});
                }
                catch (KeyNotFoundException ex)
                {
                    Response.StatusCode = 404;
                    return Json(new { Response.StatusCode, ex.Message});
                }
                catch (Exception ex)
                {
                    Response.StatusCode = 500;
                    return Json(new { Response.StatusCode, ex.Message });
                }

            }
        }
        /// <summary>
        /// Allows deletion if more than one smtpHost exists
        /// </summary>
        /// <param name="id"></param>
        /// <returns>500 error if called</returns>
        [HttpPost]
        public ActionResult Delete(int id)
        {
            Response.Cookies.Add(ValidateAccessToken(Request.Cookies[AuthContext.CookieName]));
            using (ContentContext db = new ContentContext())
            {
                try
                {
                    if (db.SmtpHosts.Count() > 1)
                    {
                        db.DeleteUnordered(db.SmtpHosts, id);
                        db.SaveChanges();
                        return Json(new { Response.StatusCode, Message = "Smtp Host record deleted successfully" });
                    }
                    else
                    {
                        Response.StatusCode = 400;
                        return Json(new { Response.StatusCode, Message = "Unable to delete smtp record. At least one record must exist" });
                    }
                }
                catch (KeyNotFoundException ex)
                {
                    Response.StatusCode = 404;
                    return Json(new { Response.StatusCode, ex.Message });
                }
                catch (Exception ex)
                {
                    Response.StatusCode = 500;
                    return Json(new { Response.StatusCode, ex.Message });
                }
            }
        }
    }
}