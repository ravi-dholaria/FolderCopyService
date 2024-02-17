using FolderCopyService.Business;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace FolderCopyService
{
    public partial class FolderCopyservice : ServiceBase
    {
        Timer timer = new Timer();
        FolderCopyService_WorkerClass worker;
        public FolderCopyservice(string[] args)
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            worker = new FolderCopyService_WorkerClass();
            worker.WriteToFile("Service is started at " + DateTime.Now);
            timer.Elapsed += new ElapsedEventHandler(OnElapsedTime);
            timer.Interval = 1*1000*60; //service will running evry hour
            timer.Enabled = true;
            OnElapsedTime(null, null);
        }

        protected override void OnStop()
        {
            worker.WriteToFile("Service is stopped at " + DateTime.Now);

        }

        private void OnElapsedTime(object source, ElapsedEventArgs e)
        {
            try
            {

                Stopwatch sw = new Stopwatch();
                sw.Start();
                FolderCopyService_WorkerClass folderCopyService_WorkerClass = new FolderCopyService_WorkerClass();
                folderCopyService_WorkerClass.process();
                sw.Stop();
                worker.WriteToFile("------------->Time Taken To Execute Service: " + sw.Elapsed.ToString());
            }
            catch (Exception ex)
            {
                worker.WriteToFile(ex.StackTrace);
            }
        }

        internal void TestStartupAndStop(string[] args)
        {
            this.OnStart(args);
            Console.ReadLine();
            this.OnStop();
        }
    }
}
