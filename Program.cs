using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace FolderCopyService
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            if (Environment.UserInteractive)
            {
                FolderCopyservice service1 = new FolderCopyservice(args);
                service1.TestStartupAndStop(args);
            }
            else
            {
                // Put the body of your old Main method here.  
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[]
                {
                    new FolderCopyservice(args)
                };
                ServiceBase.Run(ServicesToRun);
            }
        }
    }
}





# Modified by script





# Modified by script
