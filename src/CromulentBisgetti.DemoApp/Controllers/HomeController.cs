using Microsoft.AspNetCore.Mvc;

namespace CromulentBisgetti.DemoApp.Controllers
{
    public class HomeController : Controller
    {
        #region Public Methods

        public IActionResult Index() => View();

        #endregion Public Methods
    }
}