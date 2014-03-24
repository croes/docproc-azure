using DocprocShared.DataAccessLayer;
using DocprocShared.Models;
using DocprocShared.QueueTask;
using Microsoft.WindowsAzure.Storage.Queue;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace WorkerRole.Processors
{
    public class CsvToDataProcessor : Processor<CsvToDataTask>
    {

        JobDAO jobDao;
        TaskDAO taskDao;

        public CsvToDataProcessor()
        {
            jobDao = new JobDAO();
            taskDao = new TaskDAO();
        }

        public override List<CloudQueueMessage> Process(CsvToDataTask queueTask){

            Trace.TraceInformation("Received message {0}", queueTask.JobPartitionKey + "," + queueTask.JobRowKey);
            Job job = jobDao.FindJob(queueTask.JobPartitionKey, queueTask.JobRowKey);
            var csvlines = job.Data.Split('\n');
            String[] headers = csvlines.First().Split(';');
            List<Task> tasks = new List<Task>();
            foreach (string line in csvlines.Skip(1))
            {
                if (!String.IsNullOrEmpty(line))
                {
                    Task task = new Task(job);
                    String[] cells = line.Split(';');
                    for (int i = 0; i < cells.Length; i++)
                    {
                        task.addParam(headers[i], cells[i]);
                        Trace.TraceInformation("Parsed param {0}:{1}", headers[i], cells[i]);
                    }
                    tasks.Add(task);
                }
            }
            taskDao.PersistTasks(tasks);
            var outgoingMessages = new List<CloudQueueMessage>();
            foreach (Task task in tasks)
            {
                outgoingMessages.Add(new CloudQueueMessage(new TemplateTask(job, task).ToBinary()));
                Trace.TraceInformation("Put message on template queue for task {0}", task.PartitionKey);
            }
            return outgoingMessages;
        }
    }
}
