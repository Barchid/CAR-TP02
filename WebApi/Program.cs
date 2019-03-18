using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace CAR_TP02_Barchid
{
    /// <summary>
    /// Main class that will run the application
    /// </summary>
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .Build();
    }
}
