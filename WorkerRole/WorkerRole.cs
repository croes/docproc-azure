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
using MvcWebRole.DataAccessLayer;
using MvcWebRole.Models;

namespace WorkerRole
{
    public class WorkerRole : RoleEntryPoint
    {

        CloudQueue incomingQueue;
        CloudQueue outgoingQueue;
        JobDAO jobDao;
        TaskDAO taskDao;

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
            incomingQueue = queueClient.GetQueueReference("csvtodata");
            incomingQueue.CreateIfNotExists();

            outgoingQueue = queueClient.GetQueueReference("template");
            outgoingQueue.CreateIfNotExists();

            jobDao = new JobDAO();
            taskDao = new TaskDAO();
            
            return base.OnStart();
        }


        public override void Run()
        {
            // This is a sample worker implementation. Replace with your logic.
            Trace.TraceInformation("CsvToDataWorkerRole entry point called");

            while (true)
            {

                CloudQueueMessage retrievedMessage = incomingQueue.GetMessage();
                if (retrievedMessage != null)
                {
                    Trace.TraceInformation("Received message {0}", retrievedMessage.AsString);
                    Job job = jobDao.FindJob(retrievedMessage.AsString);
                    var csvlines = job.Data.Split('\n');
                    String[] headers = csvlines.First().Split(';');
                    List<Task> tasks = new List<Task>();
                    foreach (string line in csvlines.Skip(1))
                    {
                        Task task = new Task(job);
                        String[] cells = line.Split(';');
                        for(int i=0; i < cells.Length; i++){
                            task.addParam(headers[i], cells[i]);
                            Trace.TraceInformation("Parsed param {0}:{1}", headers[i], cells[i]);
                        }
                        tasks.Add(task);
                    }
                    taskDao.PersistTasks(tasks);
                    foreach (Task task in tasks)
                    {
                        var outgoingMessage = new CloudQueueMessage(task.KeyString);
                        outgoingQueue.AddMessage(outgoingMessage);
                        Trace.TraceInformation("Put message on template queue {0}", outgoingMessage.AsString);
                    }
                    incomingQueue.DeleteMessage(retrievedMessage);
                }

                Thread.Sleep(5000);
            }
        }

    }
}
