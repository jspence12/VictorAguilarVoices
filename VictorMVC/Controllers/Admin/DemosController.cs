using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VictorMVC.Models;

namespace VictorMVC.Controllers
{
    public class DemosController : BaseAdminController, IFileController<Demo>
    {
        
        public ActionResult Manage()
        {
            ViewBag.Title = "Manage Demos";
            Response.Cookies.Add(ValidateAccessToken(Request.Cookies[AuthContext.CookieName]));
            using (ContentContext db = new ContentContext())
            {
                ViewBag.Demos = db.Demos.OrderBy(d => d.List_Order)?.ToArray();
            }
            return View();
        }
        public ActionResult Modify(int? id)
        {
            Response.Cookies.Add(ValidateAccessToken(Request.Cookies[AuthContext.CookieName]));
            if (id == null)
            {
                ViewBag.Title = "Add new Demo";
                ViewBag.IsNew = true;
                return View(new Demo());
            }
            else
            {
                using (ContentContext db = new ContentContext())
                {
                    Demo row = db.Demos.SingleOrDefault(d => d.ID == id);
                    if (row is null)
                    {
                        throw new HttpException(404, "Demo not found");
                    }
                    else
                    {
                        ViewBag.Title = $"Update {row.Title}";
                        ViewBag.IsNew = false;
                        return View(row);
                    }
                }
            }
        }
        [HttpPost]
        public ActionResult Update(Demo demo, HttpPostedFileBase upload)
        {
            Response.Cookies.Add(ValidateAccessToken(Request.Cookies[AuthContext.CookieName]));
            if (!ModelState.IsValid)
            {
                Response.StatusCode = 400;
                return Json(new { Response.StatusCode, Message = "Invalid Demo Input" });
            }
            if (upload != null && !upload.ContentType.Contains("audio")) {
                Response.StatusCode = 400;
                return Json(new { StatusCode = 400, Message = "Unable to update Demo; Uploaded file must be an audio file." });
            }
            using (ContentContext db = new ContentContext())
            {
                Demo oldRecord;
                try
                {
                    oldRecord = db.Demos.Single(d => d.ID == demo.ID);
                }
                catch //attempt to access non-existent file.
                {
                    Response.StatusCode = 404;
                    return Json(new {Response.StatusCode, Message = "Unable to update Demo; Demo does not exist."});
                }
                demo.File = upload?.FileName ?? oldRecord.File;
                try
                {
                    db.UpdateOrdered(db.Demos, demo);
                    if (upload != null)
                    {
                        upload.SaveAs($"{HttpRuntime.AppDomainAppPath}/Content/Audio/{demo.File}");
                        System.IO.File.Delete($"{HttpRuntime.AppDomainAppPath}/Content/Audio/{oldRecord.File}");
                    }
                    db.SaveChanges();
                }
                catch //roll back any file changes
                {
                    if (upload != null && System.IO.File.Exists($"{HttpRuntime.AppDomainAppPath}/Content/Audio/{upload.FileName}"))
                    {
                        System.IO.File.Delete($"{HttpRuntime.AppDomainAppPath}/Content/Audio/{upload.FileName}");
                    }
                    Response.StatusCode = 500;
                    return Json(new { Response.StatusCode, Message = "Unable to delete Demo; Internal Server Error." });
                }
            }
            return RedirectToAction("Manage");
        }

        [HttpPost]
        public ActionResult Create(Demo demo, HttpPostedFileBase upload)
        {
            Response.Cookies.Add(ValidateAccessToken(Request.Cookies[AuthContext.CookieName]));
            demo.File = upload.FileName;
            if (!ModelState.IsValid || upload == null)
            {
                Response.StatusCode = 400;
                return Json(new { Response.StatusCode, Message = "Invalid Demo Input" });
            }
            if (!upload.ContentType.Contains("audio"))
            {
                Response.StatusCode = 400;
                return Json(new { Response.StatusCode, Message = "Uploaded file must be an audio file" });
            }
            using (ContentContext db = new ContentContext())
            {
                db.CreateOrdered(db.Demos,demo);
                db.SaveChanges();
            }
            upload.SaveAs($"{HttpRuntime.AppDomainAppPath}/Content/Audio/{upload.FileName}");
            return RedirectToAction("Manage");
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            Response.Cookies.Add(ValidateAccessToken(Request.Cookies[AuthContext.CookieName]));
            using (ContentContext db = new ContentContext())
            {
                string FileName;
                try
                {
                    FileName = db.Demos.Find(id).File;
                    db.DeleteOrdered(db.Demos, id);
                    System.IO.File.Delete($"{HttpRuntime.AppDomainAppPath}/Content/Audio/{FileName}");
                    db.SaveChanges();
                }
                catch (KeyNotFoundException)
                {
                    Response.StatusCode = 404;
                    return Json(new {Response.StatusCode, message = "Unable to Delete Demo; Demo does not exist"});
                }
                catch
                {
                    Response.StatusCode = 500;
                    return Json(new {Response.StatusCode, Message = "Unable to Delete Demo; Internal Server Error." });
                }
            }
            return Json(new { Response.StatusCode, Message = "Demo Deleted Successfully"});
        }
    }
}