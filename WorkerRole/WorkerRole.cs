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

namespace WorkerRole
{
    public class WorkerRole : RoleEntryPoint
    {
        public override void Run()
        {
            // This is a sample worker implementation. Replace with your logic.
            Trace.TraceInformation("CsvToDataWorkerRole entry point called");

            while (true)
            {

                var contents = File.ReadAllText(filename).Split('\n');
                var csv = from line in contents
                          select line.Split(',').ToArray();
                Trace.TraceInformation("Working", "Information");
            }
        }

        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections 
            ServicePointManager.DefaultConnectionLimit = 12;

            // For information on handling configuration changes
            // see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.

            return base.OnStart();
        }

        /// <summary>
        /// Starts the specified number of dequeue tasks.
        /// </summary>
        /// <param name="threadCount">The number of dequeue tasks.</param>
        public void StartListener(int threadCount)
        {
            Guard.ArgumentNotZeroOrNegativeValue(threadCount, "threadCount");

            // The collection of dequeue tasks needs to be reset on each call to this method.
            if (this.dequeueTasks.IsAddingCompleted)
            {
                this.dequeueTasks = new BlockingCollection<Task>(this.dequeueTaskList);
            }

            for (int i = 0; i < threadCount; i++)
            {
                CancellationToken cancellationToken = this.cancellationSignal.Token;
                CloudQueueListenerDequeueTaskState<T> workerState = new CloudQueueListenerDequeueTaskState<T>(Subscriptions, cancellationToken, this.queueLocation, this.queueStorage);

                // Start a new dequeue task and register it in the collection of tasks internally managed by this component.
                this.dequeueTasks.Add(Task.Factory.StartNew(DequeueTaskMain, workerState, cancellationToken, TaskCreationOptions.LongRunning, TaskScheduler.Default));
            }

            // Mark this collection as not accepting any more additions.
            this.dequeueTasks.CompleteAdding();
        }
    }
}
