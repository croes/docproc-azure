using DocprocShared.DataAccessLayer;
using DocprocShared.Models;
using DocprocShared.QueueTask;
using Microsoft.WindowsAzure.Storage.Queue;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.IO.Compression;
using DocprocShared.BlobAccessLayer;
using System.Diagnostics;

namespace WorkerRole.Processors
{
    class ZipProcessor : Processor<ZipTask>
    {
        JobDAO jobDao;
        TaskDAO taskDao;
        BlobAccess blobAccess;

        public ZipProcessor()
        {
            jobDao = new JobDAO();
            taskDao = new TaskDAO();
            blobAccess = new BlobAccess();
        }

        public override List<CloudQueueMessage> Process(ZipTask queueTask)
        {
            Job job = jobDao.FindJob(queueTask.JobPartitionKey, queueTask.JobRowKey);
            List<Task> tasks = taskDao.FindTasksOfJob(job);

            byte[] zipArchive = BuildZipArchive(job, tasks);
            blobAccess.UploadJobResult(job, zipArchive);
            Trace.TraceInformation("Uploaded zip for job {0},{1}", job.PartitionKey, job.RowKey);
            job.EndTime = DateTime.Now;
            jobDao.PersistJob(job);
            Trace.TraceInformation("Finished job {0},{1}", job.PartitionKey, job.RowKey);
            return new List<CloudQueueMessage>();
        }

        private byte[] BuildZipArchive(Job job, List<Task> tasks)
        {
            MemoryStream zipMemoryStream = new MemoryStream();
            using (ZipArchive archive = new ZipArchive(zipMemoryStream, ZipArchiveMode.Create))
            {
                foreach (var task in tasks)
                {
                    Stream taskResultStream = blobAccess.DownloadTaskResultToStream(job, task);
                    ZipArchiveEntry zipEntry = archive.CreateEntry(task.RowKey + ".pdf");
                    using (Stream zipEntryStream = zipEntry.Open())
                    {
                        taskResultStream.CopyTo(zipEntryStream);
                    }
                }
            }
            return zipMemoryStream.ToArray();
        }
    }
}
