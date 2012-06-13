using System;
using System.Web.Mvc;
using ZpdWebClient.Models;
using ZpdWebClient.ZPDService;

namespace ZpdWebClient.Controllers
{
    public class HomeController : Controller
    {
        //
        // GET: /Home/

        public ActionResult Index()
        {
            var model = new ZpdCurrentPlayerState();
            try
            {

                model = ClientManager.Client.GetCurrentPlayerState();
            }
            catch
            {
                //eat the exception
            }
            return View(model);
        }

        public JsonResult GetCurrentPlayerState()
        {
            Response.CacheControl = "no-cache";
            Response.Cache.SetETag((Guid.NewGuid()).ToString());
            var currentState = new ZpdCurrentPlayerState();
            try
            {
                currentState = ClientManager.Client.GetCurrentPlayerState();
            }
            catch
            {
                // eat exception
            }
            return Json(currentState, JsonRequestBehavior.AllowGet);
        }

        public JsonResult Search(string query)
        {
            Response.CacheControl = "no-cache";
            Response.Cache.SetETag((Guid.NewGuid()).ToString());
            var results = new ZpdTrack[0];
            try
            {
                results = String.IsNullOrWhiteSpace(query) ? null : ClientManager.Client.Search(query);
            }
            catch
            {
                // eat the exception
            }
            return Json(results, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetCurrentQueue()
        {
            Response.CacheControl = "no-cache";
            Response.Cache.SetETag((Guid.NewGuid()).ToString());
            var results = new ZpdTrack[0];
            try
            {
                results = ClientManager.Client.GetCurrentQueue();
            }
            catch
            {
                // eat the exception
            }
            return Json(results, JsonRequestBehavior.AllowGet);
        }

        public JsonResult QueueTrack(TrackToQueue track)
        {
            try
            {
                ClientManager.Client.QueueTrack(track.MediaId, track.MediaTypeId);
            }
            catch
            {
                // eat the exception
            }
            return Json(track);
        }

    }
}
