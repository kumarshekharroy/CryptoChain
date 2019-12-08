using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CryptoChain.Models;
using CryptoChain.Services.Classes;
using CryptoChain.Utility;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Web;

namespace CryptoChain
{
    public class Program
    {

        public static void Main(string[] args)
        { 
            var logger = NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();
            try
            { 
                logger.Info("init main function");
                var host = CreateHostBuilder(args).Build();

                //using (var scope = host.Services.CreateScope())
                //{
                //    var services = scope.ServiceProvider;

                //    try
                //    {
                //        var context = services.GetRequiredService<Database>();

                //        //context.Database.Migrate(); // apply all migrations 
                //    }
                //    catch (Exception ex)
                //    {
                //        logger.Error(ex, "an error occurred while applying mgration to DB.");
                //    }
                //}
                host.Run();

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception => {ex.Message}");
                logger.Error(ex, "error in init");
                throw;
            }
            finally
            {
                NLog.LogManager.Shutdown();
            } 
        }

        public static IHostBuilder CreateHostBuilder(string[] args) => Host.CreateDefaultBuilder(args)
             .ConfigureWebHostDefaults(webBuilder =>
             {
                 webBuilder.ConfigureKestrel(serverOptions =>
                 {
                     // Set properties and call methods on options
                 }).UseStartup<Startup>().ConfigureLogging(logging =>
                 {
                     logging.ClearProviders();
                     logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);

                 }).UseNLog().UseKestrel().ConfigureKestrel((context, options) =>
                 {
                     // options.Limits.MaxConcurrentConnections = 100;
                     //options.Limits.MaxConcurrentUpgradedConnections = 100;
                     options.Limits.MaxRequestBodySize = 10 * 10 * 1024;
                     //options.Limits.MinRequestBodyDataRate =
                     //    new MinDataRate(bytesPerSecond: 100, gracePeriod: TimeSpan.FromSeconds(10));
                     //options.Limits.MinResponseDataRate =
                     //    new MinDataRate(bytesPerSecond: 100, gracePeriod: TimeSpan.FromSeconds(10));
                      
                     if (Debugger.IsAttached)
                     { 
                         options.Listen(IPAddress.Loopback, Constants.HTTP_PORT+1);
                     }
                     else
                     {
                         options.Listen(IPAddress.Loopback, Constants.HTTP_PORT);
                     }
                 });
             });

    }
}
