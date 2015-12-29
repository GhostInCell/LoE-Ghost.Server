using Fiddler;
using System;
using System.Collections.Generic;

namespace Proxy
{
    class Program
    {
        private static string _url;
        private const string LOEData = "loedata.legendsofequestria.com";
        private static bool APPIsRunning = true;
        private static Dictionary<string, Action> _cmd = new Dictionary<string, Action>()
        {
            { "exit", ()=> { APPIsRunning = false; } }
        };
        static void Main(string[] args)
        {
            Console.Title = "Legends of Equestria Proxy";
            Console.SetWindowSize(120, 30);
            if (args.Length >= 1)
            {
                _url = args[0];
                Console.WriteLine("Proxy Started");
            Restart:
                FiddlerApplication.BeforeRequest += FiddlerApplication_BeforeRequest;
                FiddlerApplication.Startup(8081, FiddlerCoreStartupFlags.RegisterAsSystemProxy);
                try
                {
                    while (APPIsRunning)
                    {
                        var cmd = Console.ReadLine();
                        if (_cmd.ContainsKey(cmd)) _cmd[cmd]();
                    }
                }
                catch (Exception exp)
                {
                    FiddlerApplication.BeforeRequest -= FiddlerApplication_BeforeRequest;
                    try
                    {
                        FiddlerApplication.Shutdown();
                    }
                    catch { }
                    Console.WriteLine(exp.GetType().FullName);
                    Console.WriteLine(exp.Message);
                    Console.WriteLine(exp.StackTrace);
                    goto Restart;
                }
                FiddlerApplication.Shutdown();
            }
            else
            {
                Console.WriteLine("Using: proxy [server host]");
                Console.ReadLine();
            }
        }
        private static void FiddlerApplication_BeforeRequest(Session oSession)
        {
            if (oSession?.hostname == LOEData)
            {
                oSession.host = _url;
                Console.WriteLine(oSession?.fullUrl);
            }
        }
    }
}