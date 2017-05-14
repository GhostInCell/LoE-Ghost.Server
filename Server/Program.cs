using Ghost.Server;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Server
{
    public static class Program
    {
        public static void Main()
        {
            MainAsync().Wait();
        }

        static async Task MainAsync()
        {
            Console.Title = "Legends of Equestria Server";
            ServerInstance instance = new ServerInstance();
            if (await instance.Startup())
            {
                while (instance.IsRunning)
                    instance.DoCommand(Console.ReadLine());
                instance.Stop();
                Thread.Sleep(500);
            }
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey(true);
        }
    }
}