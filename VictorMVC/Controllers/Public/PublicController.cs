using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VictorMVC.Models;
using MailKit.Net.Smtp;
using MailKit;
using MimeKit;


namespace VictorMVC.Controllers
{
    public class PublicController : Controller
    {

        public ActionResult Index()
        {
            ViewBag.Title = "Victor Aguilar: Voice Actor";
            return View();
        }

        /// <summary>
        /// Loads Dynamic Content onto index page. Loads separately due to hosting's slow spin-up time on db.
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult getContent()
        {
            using (ContentContext db = new ContentContext())
            {
                List<Demo> demos = db.Demos.OrderBy(d => d.List_Order).ToList();
                List<Article> articles = db.Articles.OrderBy(d => d.List_Order).ToList();
                Response.Expires = 10800;
                return Json(new { demos, articles });
            }
        }

        [HttpPost]
        public ActionResult Contact(Models.EmailForm EmailForm)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    MimeMessage message = new MimeMessage();
                    //TODO Generalize Recipients
                    message.Subject = $"New Message from {EmailForm.Name}";
                    message.Body = new TextPart("plain")
                    {
                        Text = $"Email Address: {EmailForm.Email} \n {EmailForm.Message}"
                    };
                    using (ContentContext db = new ContentContext())
                    {
                        //SmtpHost smtp = db.SmtpHosts.First();
                        EmailRecipient[] recipients = db.EmailRecipients.ToArray();
                        SmtpHost host = db.SmtpHosts.FirstOrDefault();
                        message.From.Add(new MailboxAddress(host.username)); //check if username matches with smtp email
                        foreach (EmailRecipient recipient in recipients)
                        {
                            message.To.Add(new MailboxAddress(recipient.Email));
                        }

                        //the code below can probably be bundled into a function; sending mail may end up being a fairly common operation
                        using (var client = new SmtpClient())
                        {
                            client.Connect(host.Host, host.Port, host.UseSSL);
                            client.Authenticate(host.username, host.password);
                            client.Send(message);
                            client.Disconnect(true);
                        }
                        return Json(new { success = true });
                    }
                }
                catch
                {
                    Response.StatusCode = 500;
                    return Json(new { success = false });
                }
            }
            else
            {
                Response.StatusCode = 400;
                return Json(new { sucess = false });
            }

        }
    }
}