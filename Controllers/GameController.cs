using System.Web.Mvc;
using Riddle_Web.Models;

namespace Riddle_Web.Controllers
{
    public class GameController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public JsonResult NewGame()
        {
            return Json(ToJson(Puzzle.NewGame()), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Move(MoveRequest request)
        {
            var result = Puzzle.Move(request == null ? null : request.State,
                                     request == null ? null : request.Passenger);
            return Json(ToJson(result));
        }

        // camelCase shape used by Scripts/game.js
        private static object ToJson(MoveResult r)
        {
            return new
            {
                state = new
                {
                    farmer = r.State.Farmer,
                    fox = r.State.Fox,
                    chicken = r.State.Chicken,
                    corn = r.State.Corn
                },
                status = r.Status,
                message = r.Message
            };
        }
    }
}