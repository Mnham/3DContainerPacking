using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using CromulentBisgetti.DemoApp.Models;

namespace CromulentBisgetti.DemoApp.Controllers
{
    public class HomeController : Controller
    {
        #region Public Methods

        public IActionResult Index() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() =>
            View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });

        #endregion Public Methods
    }
}