using System;
using System.Web.Mvc;

namespace ZpdWebClient.Controllers
{
    public class HomeController : Controller
    {
        //
        // GET: /Home/

        public ActionResult Index()
        {
            var model = Models.ClientManager.Client.GetCurrentPlayerState();
            return View(model);
        }

        public JsonResult GetCurrentPlayerState()
        {
            Response.CacheControl = "no-cache";
            Response.Cache.SetETag((Guid.NewGuid()).ToString());
            var currentState = Models.ClientManager.Client.GetCurrentPlayerState();
            return Json(currentState, JsonRequestBehavior.AllowGet);
        }

    }
}
