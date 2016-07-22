using Ghost.Server;
using System;
using System.Threading;

namespace Server
{
    public static class Program
    {
        public static void Main()
        {
            Console.Title = "Legends of Equestria Server";
            ServerInstance instance = new ServerInstance();
            if (instance.Startup())
            {
                while (instance.IsRunning)
                    instance.DoCommand(Console.ReadLine());
                instance.Stop();
                Thread.Sleep(500);
            }
        }
    }
}