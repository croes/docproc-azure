using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using DocprocShared.DataAccessLayer;
using DocprocShared.Models;
using DocprocShared.QueueTask;
using WorkerRole.Processors;

namespace WorkerRole
{
    public class WorkerRole : RoleEntryPoint
    {

        CloudQueue workerQueue;
        JobDAO jobDao;
        TaskDAO taskDao;

        int minInterval = 1;
        int interval = 1;
        int exponent = 2;
        int maxInterval = 30;

        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections 
            ServicePointManager.DefaultConnectionLimit = 12;

            // For information on handling configuration changes
            // see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.

            // Retrieve storage account from connection string
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                CloudConfigurationManager.GetSetting("StorageConnectionString"));

            // Create the queue client
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();

            // Retrieve a reference to a queue
            workerQueue = queueClient.GetQueueReference("workerqueue");
            workerQueue.CreateIfNotExists();

            jobDao = new JobDAO();
            taskDao = new TaskDAO();
            
            return base.OnStart();
        }


        public override void Run()
        {
            // This is a sample worker implementation. Replace with your logic.
            Trace.TraceInformation("WorkerRole entry point called");

            var AllProcessors = new List<IProcessor>();
            AllProcessors.Add(new CsvToDataProcessor());
            AllProcessors.Add(new TemplateProcessor());
            AllProcessors.Add(new RenderProcessor());

            while (true)
            {
                CloudQueueMessage retrievedMessage = workerQueue.GetMessage();
                if (retrievedMessage != null)
                {
                    var queueTask = retrievedMessage.ToQueueTask<object>();
                    Trace.TraceInformation("processing task type: {0}", queueTask.GetType());
                    var processor = AllProcessors.First(p => p.GetType().BaseType.GetGenericArguments()[0] == queueTask.GetType());
                    var nextMessages = processor.Process(queueTask);
                    foreach(var message in nextMessages){
                        workerQueue.AddMessage(message);
                    }
                    workerQueue.DeleteMessage(retrievedMessage);
                    interval = minInterval;
                }
                else
                {
                    Trace.WriteLine(string.Format("Sleep for {0} seconds", interval)); 
                    Thread.Sleep(TimeSpan.FromSeconds(interval));
                    interval = Math.Min(maxInterval, interval * exponent);
                }
            }
        }

    }
}
