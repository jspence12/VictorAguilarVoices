using System.Web.Mvc;
using VictorMVC.Models;

namespace VictorMVC.Controllers
{
    public class AdminController : BaseAdminController
    {
        private const string _managePage = "~/Views/Admin/Manage.cshtml";

        public ActionResult Login()
        {
            ViewBag.Title = "Login";
            return View(new Login());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpStatusCodeResult Login(Login LoginAttempt)
        {
            using (AuthContext auth = new AuthContext())
            {
                bool login = auth.ValidateCredentials(LoginAttempt.Username, LoginAttempt.Password);
                if (login)
                {
                    Response.Cookies.Add(auth.SetAccessToken(LoginAttempt.Username));
                    return new HttpStatusCodeResult(200);
                }
                else
                {
                    return new HttpStatusCodeResult(401);
                }
            }
        }
        public ActionResult Menu()
        {
            Response.Cookies.Add(ValidateAccessToken(Request.Cookies[AuthContext.CookieName]));
            ViewBag.Title = "Admin";
            return View();
        }
    }
}
