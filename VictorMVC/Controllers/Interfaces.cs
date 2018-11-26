using System.Web;
using System.Web.Mvc;
using VictorMVC.Models;

namespace VictorMVC.Controllers
{
    /// <summary>
    /// Base Interface for all CRUD controllers. This should be used for inheritence ony
    /// </summary>
    /// <typeparam name="T">Model controller implements CRUD functionality for</typeparam>
    interface IBaseContentController<T>
        where T : ITable
    {
        ActionResult Manage();
        ActionResult Modify(int? id);
        ActionResult Delete(int id);
    }
    /// <summary>
    /// CRUD Controller interface for tables which DO NOT work in tandem with a file system.
    /// </summary>
    /// <typeparam name="T">Model controller implements CRUD functionality for</typeparam>
    interface IFilelessController<T> : IBaseContentController<T>
        where T: ITable
    {
        ActionResult Create(T model);
        ActionResult Update(T model);
    }

    /// <summary>
    /// CRUD controller for tables which work in tandem with a file system
    /// </summary>
    /// <typeparam name="T">Model controller implements CRUD functionality for</typeparam>
    interface IFileController<T> : IBaseContentController<T>
        where T : ITable
    {
        ActionResult Create(T model, HttpPostedFileBase file);
        ActionResult Update(T model, HttpPostedFileBase file);
    }

    /// <summary>
    /// Abstract class which implements Authentication functionality for all Admin Controllers.
    /// </summary>
    public abstract class BaseAdminController : Controller
    {
        protected HttpCookie ValidateAccessToken(HttpCookie cookie)
        {
            if (cookie != null)
            {
                using (AuthContext auth = new AuthContext())
                {
                    return auth.ValidateAccessToken(cookie.Value);
                }
            }
            else
            {
                throw new HttpException(401, "No Access Token present");
            }
        }
    }
}