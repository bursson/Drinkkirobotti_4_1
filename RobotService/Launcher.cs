using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using NLog;
using Topshelf;

namespace RobotService
{
    public static class Launcher
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, eventArgs) =>
            {
                Log.FatalEx(nameof(Main), $"Unhandled exception: {(Exception) eventArgs.ExceptionObject}");
            };

            HostFactory.Run(x =>
            {
                x.UseNLog();

                x.Service<RobotService>(sc =>
                {
                    sc.ConstructUsing(() => new RobotService());
                    sc.WhenStarted(s => s.Start());
                    sc.WhenStopped(s => s.Stop());
                });

                x.RunAsLocalSystem();
            });
        }
    }
}
