using DocprocShared.Models;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace DocprocShared.BlobAccessLayer
{
    public class BlobAccess
    {

        CloudBlobContainer container;

        public BlobAccess()
        {
            // Retrieve storage account from connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                CloudConfigurationManager.GetSetting("StorageConnectionString"));

            // Create the blob client.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Retrieve a reference to a container. 
            container = blobClient.GetContainerReference("docproc");

            // Create the container if it doesn't already exist.
            container.CreateIfNotExists();
        }

        public string UploadTaskResult(Job job, Task task, Stream pdfStream)
        {
            CloudBlobDirectory jobDir = container.GetDirectoryReference(job.RowKey);
            CloudBlockBlob blob = jobDir.GetBlockBlobReference(task.RowKey + ".pdf");
            return UploadToBlobFromStream(blob, pdfStream);
        }

        public string UploadJobResult(Job job, Stream zipStream)
        {
            CloudBlobDirectory jobDir = container.GetDirectoryReference(job.RowKey);
            CloudBlockBlob blob = jobDir.GetBlockBlobReference(job.RowKey + ".zip");
            return UploadToBlobFromStream(blob, zipStream);
        }

        public string UploadToBlobFromStream(CloudBlockBlob blobReference, Stream stream)
        {
            blobReference.UploadFromStream(stream);
            return blobReference.Uri.AbsoluteUri;
        }

        public string GetSASUri(Job job, Task task, int durationInMins)
        {
            CloudBlobDirectory jobDir = container.GetDirectoryReference(job.RowKey);
            CloudBlockBlob blob = jobDir.GetBlockBlobReference(task.RowKey + ".pdf");
            SharedAccessBlobPermissions permissions = SharedAccessBlobPermissions.Read;
            string sas = blob.GetSharedAccessSignature(new SharedAccessBlobPolicy()
            {
                Permissions = permissions,
                SharedAccessStartTime = DateTime.UtcNow.AddMinutes(-5),
                SharedAccessExpiryTime = DateTime.UtcNow.AddMinutes(durationInMins)
            });
            return string.Format(CultureInfo.InvariantCulture, "{0}{1}", blob.Uri, sas);
        }

        public Stream DownloadTaskResultToStream(Job job, Task task)
        {
            CloudBlobDirectory jobDir = container.GetDirectoryReference(job.RowKey);
            CloudBlockBlob blob = jobDir.GetBlockBlobReference(task.RowKey + ".pdf");
            Stream blobStream = new MemoryStream();
            blob.DownloadToStream(blobStream);
            return blobStream;
        }
    }
}
