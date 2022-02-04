using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace CromulentBisgetti.DemoApp
{
    public class Program
    {
        #region Public Methods

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();

        public static void Main(string[] args) =>
                    CreateWebHostBuilder(args).Build().Run();

        #endregion Public Methods
    }
}