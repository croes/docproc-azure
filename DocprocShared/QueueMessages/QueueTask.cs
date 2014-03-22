using Microsoft.WindowsAzure.Storage.Queue;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Microsoft.WindowsAzure.Storage.Table;
using DocprocShared.Models;

namespace DocprocShared.QueueTask
{
    public static class QueueTask
    {
        public static byte[] ToBinary<T>(this T dns)
        {
            var binaryFormatter = new BinaryFormatter();
            byte[] bytes = null;
            using (var memoryStream = new MemoryStream())
            {
                binaryFormatter.Serialize(memoryStream, dns);
                bytes = memoryStream.GetBuffer();
            }

            return bytes;
        }

        public static T ToQueueTask<T>(this CloudQueueMessage cloudQueueMessage)
        {
            var bytes = cloudQueueMessage.AsBytes;
            using (var memoryStream = new MemoryStream(bytes))
            {
                var binaryFormatter = new BinaryFormatter();
                return (T)binaryFormatter.Deserialize(memoryStream);
            }
        }
    }

    [Serializable]
    public class CsvToDataTask
    {
        public string JobPartitionKey { get; set; }
        public string JobRowKey { get; set; }

        public CsvToDataTask(Job job)
        {
            JobPartitionKey = job.PartitionKey;
            JobRowKey = job.RowKey;
        }
    }

    [Serializable]
    public class TemplateTask
    {
        public string JobPartitionKey { get; set; }
        public string JobRowKey { get; set; }
        public string TaskPartitionKey { get; set; }
        public string TaskRowKey { get; set; }

        public TemplateTask(Job job, Task task)
        {
            JobPartitionKey = job.PartitionKey;
            JobRowKey = job.RowKey;
            TaskPartitionKey = task.PartitionKey;
            TaskRowKey = task.RowKey;
        }
    }

    [Serializable]
    public class RenderTask
    {
        public string JobPartitionKey { get; set; }
        public string JobRowKey { get; set; }
        public string TaskPartitionKey { get; set; }
        public string TaskRowKey { get; set; }

        public RenderTask(Job job, Task task)
        {
            JobPartitionKey = job.PartitionKey;
            JobRowKey = job.RowKey;
            TaskPartitionKey = task.PartitionKey;
            TaskRowKey = task.RowKey;
        }
    }

    [Serializable]
    public class MailTask
    {
        public string JobPartitionKey { get; set; }
        public string JobRowKey { get; set; }
        public string TaskPartitionKey { get; set; }
        public string TaskRowKey { get; set; }

        public MailTask(Job job, Task task)
        {
            JobPartitionKey = job.PartitionKey;
            JobRowKey = job.RowKey;
            TaskPartitionKey = task.PartitionKey;
            TaskRowKey = task.RowKey;
        }
    }

    [Serializable]
    public class ZipTask
    {
        public string JobPartitionKey { get; set; }
        public string JobRowKey { get; set; }

        public ZipTask(Job job)
        {
            JobPartitionKey = job.PartitionKey;
            JobRowKey = job.RowKey;
        }
    }    

}
