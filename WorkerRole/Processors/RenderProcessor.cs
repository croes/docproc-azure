using DocprocShared.DataAccessLayer;
using DocprocShared.Models;
using DocprocShared.QueueTask;
using DocprocShared.BlobAccessLayer;
using Microsoft.WindowsAzure.Storage.Queue;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Net;

namespace WorkerRole.Processors
{
    class RenderProcessor : Processor<RenderTask>
    {

        TaskDAO taskDao;
        JobDAO jobDao;

        public RenderProcessor()
        {
            taskDao = new TaskDAO();
            jobDao = new JobDAO();
        }

        public override List<CloudQueueMessage> Process(RenderTask queueTask)
        {
            Job job = jobDao.FindJob(queueTask.JobPartitionKey, queueTask.JobRowKey);
            Task task = taskDao.FindTask(queueTask.TaskPartitionKey, queueTask.TaskRowKey);
            string appRootDir = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory);
            MemoryStream templateStream = new MemoryStream(Encoding.UTF8.GetBytes(WebUtility.HtmlDecode(task.Template)));
            MemoryStream pdfStream = new MemoryStream();
            bool success = PDFPrinter.GeneratePdf(appRootDir, new StreamReader(templateStream), pdfStream);
            if (success)
            {
                BlobAccess blobAccess = new BlobAccess();
                string uri = blobAccess.writeTaskResult(job, task, pdfStream);
                task.Result = uri;
                Trace.TraceInformation("Generated PDF for task {0}", task.RowKey);
            }
            taskDao.PersistTask(task);
            List<CloudQueueMessage> outgoingMessages = new List<CloudQueueMessage>();
            return outgoingMessages;
        }
    }
}
