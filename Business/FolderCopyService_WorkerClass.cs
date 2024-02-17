using FolderCopyService.DAL;
using FolderCopyService.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;
using System.Threading.Channels;
using System.Diagnostics;
using System.Threading;

namespace FolderCopyService.Business
{
    internal class FolderCopyService_WorkerClass

    {
        #region Global variables
        private List<FolderDetailsModel> FolderDetails;
        private List<FileDetailsModel> FileDetails;
        #endregion

        #region constructor
        public FolderCopyService_WorkerClass() { 
            FolderDetails = new List<FolderDetailsModel>();
            FileDetails = new List<FileDetailsModel>();
        }
        #endregion

        #region Process
        public void process()
        {
            WriteToFile("Service is recall at " + DateTime.Now);
            string sourcepath = ConfigurationManager.AppSettings["Source"];
            InsertDir(sourcepath);
            CreateQueue();
            Thread.Sleep(3000);
            for (int i = 0; i < 3; i++)
            {
                InvokeConsumer();
            }
            Thread.Sleep(1000);
            AppendInQueue();
            Thread.Sleep(2000);

        }
        #endregion

        #region LogFile writer Function
        public void WriteToFile(string Message)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "\\Logs";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            string filepath = AppDomain.CurrentDomain.BaseDirectory + "\\Logs\\ServiceLog_" + DateTime.Now.Date.ToShortDateString().Replace('/', '_') + ".txt";
            if (!File.Exists(filepath))
            {
                // Create a file to write to.
                using (StreamWriter sw = File.CreateText(filepath))
                {
                    sw.WriteLine("[" + DateTime.Now + "] " + Message);
                }
            }
            else
            {
                using (StreamWriter sw = File.AppendText(filepath))
                {
                    sw.WriteLine("[" + DateTime.Now + "] " + Message);
                }
            }
        }
        #endregion

        #region InsertDir
        public void InsertDir(string path)
        {
            try
            {
                if (Directory.Exists(path))
                {
                    FCS_DAL fCS_DAL = new FCS_DAL();
                    var folder = new DirectoryInfo(path);
                    FolderDetailsModel folderDetailsModel = new FolderDetailsModel();
                    folderDetailsModel.FolderName = folder.Name;
                    folderDetailsModel.ParentFolder = folder.Parent.Name;
                    folderDetailsModel.OriginalLocation = path;
                    folderDetailsModel.FolderCount = folder.GetDirectories().Length;
                    folderDetailsModel.FileCount = folder.GetFiles().Length;
                    if (folderDetailsModel.FileCount == 0) folderDetailsModel.Watchstatus = 5;
                    //inserting in DB
                    if (!fCS_DAL.isFolderPresent(path)) fCS_DAL.InsertFolderDetails(folderDetailsModel);
                    //pushing in global list
                    FolderDetails.Add(folderDetailsModel);
                    //call for all dirs
                    GetSubDirs(path);
                }
            }
            catch (Exception ex)
            {
                WriteToFile(ex.StackTrace);
            }
        }
        #endregion

        #region Get SubDirs
        private List<string> GetSubDirs(string path)
        {
            try
            {
                // Check if the source directory exists
                if (Directory.Exists(path))
                {
                    FCS_DAL fCS_DAL = new FCS_DAL();

                    List<string> directories = new List<string>(Directory.GetDirectories(path));
                    //SharedCollectionClass.AllFolderList.AddRange(directories);

                    foreach (string dir in directories)
                    {
                        var folder = new DirectoryInfo(dir);
                        FolderDetailsModel folderDetailsModel = new FolderDetailsModel();
                        folderDetailsModel.FolderName = folder.Name;
                        folderDetailsModel.ParentFolder = folder.Parent.Name;
                        folderDetailsModel.OriginalLocation = dir;
                        folderDetailsModel.FolderCount = folder.GetDirectories().Length;
                        folderDetailsModel.FileCount = folder.GetFiles().Length;
                        if (folderDetailsModel.FileCount == 0) folderDetailsModel.Watchstatus = 5;
                        if (!fCS_DAL.isFolderPresent(dir)) fCS_DAL.InsertFolderDetails(folderDetailsModel);
                        FolderDetails.Add(folderDetailsModel);
                        GetSubDirs(dir);
                    }
                    int fldID = fCS_DAL.GetFolderId(path);
                    List<string> files = new List<string>(Directory.GetFiles(path));
                    foreach (string file in files)
                    {
                        FileDetailsModel fileDetailsModel = new FileDetailsModel();
                        fileDetailsModel.FileName = Path.GetFileNameWithoutExtension(file);
                        fileDetailsModel.FileExtension = Path.GetExtension(file);
                        fileDetailsModel.FolderId = fldID;
                        fileDetailsModel.FilePath = file;
                        fileDetailsModel.ModificationDate = (new FileInfo(file).LastWriteTime);
                        FileDetails.Add(fileDetailsModel);
                        int filepresent = fCS_DAL.isFilePresent(file);
                        if (filepresent == 0) fCS_DAL.InsertFileDetails(fileDetailsModel);
                        else if (filepresent != 1) fCS_DAL.updateFileDetails(fileDetailsModel);
                    }
                    SharedCollectionClass.AllFileList.AddRange(new List<string>(Directory.GetFiles(path)));
                }
            }
            catch (Exception ex)
            {
                WriteToFile($"{ex.Message}");
            }
            return new List<string>();
        }
        #endregion

        #region Queue Genration
        public void CreateQueue()
        {
            var factory = new ConnectionFactory { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    string queueName = channel.QueueDeclare(queue: "task_queue",
                     durable: false,
                     exclusive: false,
                     autoDelete: false,
                     arguments: null);
                    channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);
                };

            };
        }
        #endregion

        #region SendMessage
        public void SendMessage(string message,string msgType)
        {
            var factory = new ConnectionFactory { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    // Create message properties
                    var properties = channel.CreateBasicProperties();
                    properties.Persistent = true; // Ensure messages are persistent

                    // Add custom headers to the message properties (Dir or File)
                    properties.Headers = new Dictionary<string, object>
                    {
                        { "MessageType", msgType }
                    };
                    Console.WriteLine(message);
                    var body = Encoding.UTF8.GetBytes(message);
                    channel.BasicPublish(exchange: string.Empty,
                    routingKey: "task_queue",
                    basicProperties: properties,
                    body: body);
                }
            };
        }
        #endregion

        #region Sending Folders in queue
        public void AppendInQueue()
        {
            try
            {
                foreach (var item in FileDetails)
                {
                    FCS_DAL fCS_DAL = new FCS_DAL();
                    int res = fCS_DAL.isFilePresent(item.FilePath);
                    if (res != 1)
                    { 
                        SendMessage(item.FilePath, "file");
                    }
                }
            }
            catch (Exception ex)
            {
                WriteToFile(ex.StackTrace);
            }
        }
        #endregion

        #region Invoke Consumer
        public void InvokeConsumer()
        {
            string filepath = @"E:\DOTNET\FolderCopier\bin\Debug\net8.0\FolderCopier.exe";

            ProcessStartInfo processStartInfo = new ProcessStartInfo
            {
                FileName = filepath,
                UseShellExecute = true,
                Arguments = ConfigurationManager.AppSettings["Destination"].Replace(" ","*")+" "+ ConfigurationManager.AppSettings["Source"].Replace(" ", "*"),
                RedirectStandardOutput = false,
                CreateNoWindow = false
            };
            Process.Start(processStartInfo);
        }
        #endregion

        #region Print FolderDetails
        public void printFolderDetails(List<FolderDetailsModel> items) 
        {
            foreach(FolderDetailsModel folderDetailsModel in items)
            {
                WriteToFile("FolderName: " + folderDetailsModel.FolderName);
                WriteToFile("FolderCount: " + folderDetailsModel.FolderCount);
                WriteToFile("FileCount: " + folderDetailsModel.FileCount);
                WriteToFile("ParentFolder: " + folderDetailsModel.ParentFolder);
                WriteToFile("Location: " + folderDetailsModel.OriginalLocation);
                WriteToFile("WatchStatus: " + folderDetailsModel.Watchstatus);
                WriteToFile("---------------------------------------------\n");
            }
        }
        #endregion
    }
}
