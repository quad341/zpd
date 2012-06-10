using System.Web.Mvc;

namespace ZpdWebClient.Controllers
{
    public class HomeController : Controller
    {
        //
        // GET: /Home/

        public ActionResult Index()
        {
            var model = ZpdWebClient.Models.ClientManager.Client.GetCurrentPlayerState();
            return View(model);
        }

    }
}
