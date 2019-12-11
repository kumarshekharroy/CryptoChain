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



           //var keyPair= EllipticCurve.GenerateKeys();

           // Console.WriteLine("Original Key Pair ==============");
           // Console.WriteLine($"pub===> {keyPair.PublicKey()}");
           // Console.WriteLine($"priv===> {keyPair.PrivateKey()}");

           // var serializeKeyPair = keyPair.SerializeKeyPair();

           // Console.WriteLine("Serialized Key Pair ==============");
           // Console.WriteLine($"pub===> {serializeKeyPair.pub}");
           // Console.WriteLine($"priv===> {serializeKeyPair.priv}");
             

           // Console.WriteLine("Deserialized Key Pair ============== Should be equal to Original Key Pair");
           // Console.WriteLine($"pub===> {EllipticCurve.DeserializePublicKey(serializeKeyPair.pub).ToEncodedString()}");
           // Console.WriteLine($"priv===> {EllipticCurve.DeserializePrivateKey(serializeKeyPair.priv).ToEncodedString()}");


             
           // Console.WriteLine("ReSerialized Key Pair ============== Should be equal to Serialized Key Pair");
           // Console.WriteLine($"pub===> {EllipticCurve.DeserializePublicKey(serializeKeyPair.pub).SerializePublicKey()}");
           // Console.WriteLine($"priv===> {EllipticCurve.DeserializePrivateKey(serializeKeyPair.priv).SerializePrivateKey()}");













            var logger = NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();
            try
            {
                logger.Info("init main function"); 
                var host = CreateHostBuilder(args).Build(); 
                
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

                     var port = args.Where(x => x.ToLower().StartsWith("port:")).Select(x => int.Parse(x.Replace("port:", string.Empty).Trim())).FirstOrDefault();


                     if (Debugger.IsAttached)
                     { 
                         options.Listen(IPAddress.Loopback, Constants.HTTP_PORT+1);
                     }
                     else
                     {
                         options.Listen(IPAddress.Loopback, port);
                     }
                 });
             });

    }
}
