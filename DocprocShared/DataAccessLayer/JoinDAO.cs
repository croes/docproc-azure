using DocprocShared.Models;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DocprocShared.DataAccessLayer
{
    public class JoinDAO
    {
        private CloudTable joinTable;

        public JoinDAO()
        {
            var storageAccount = Microsoft.WindowsAzure.Storage.CloudStorageAccount.Parse(
                RoleEnvironment.GetConfigurationSettingValue("StorageConnectionString"));
            var tableClient = storageAccount.CreateCloudTableClient();
            joinTable = tableClient.GetTableReference("join");
            joinTable.CreateIfNotExists();
        }

        public Join FindJoin(string partitionKey, string rowKey)
        {
            if (partitionKey == null || rowKey == null)
            {
                throw new EntityNotFoundException(
                    "Cannot load join with null partitionKey or rowKey");
            }
            var retrieveOperation = TableOperation.Retrieve<Join>(partitionKey, rowKey);
            var retrievedResult = joinTable.Execute(retrieveOperation);
            var join = retrievedResult.Result as Join;
            if (join == null)
            {
                throw new EntityNotFoundException(
                    "No join found for partitionKey: " + partitionKey + " rowKey: " + rowKey);
            }
            return join;
        }

        public Join FindJoin(Job job)
        {
            return FindJoin(job.PartitionKey, "JOIN-" + job.RowKey);
        }

        public void PersistJoin(Join join)
        {
            var insertOperation = TableOperation.InsertOrMerge(join);
            joinTable.Execute(insertOperation);
        }

        private void PersistJoinNoRetry(Join join)
        {
            var insertOperation = TableOperation.InsertOrMerge(join);
            TableRequestOptions options = new TableRequestOptions{
                RetryPolicy = new Microsoft.WindowsAzure.Storage.RetryPolicies.NoRetry()
            };
            joinTable.Execute(insertOperation, options);
        }

        public void DecrementAndStoreJoin(Join join, int retry){
            //TODO: check this concurrency handling logic
            join.Decrement();
            try
            {
                PersistJoinNoRetry(join);
            }
            catch(StorageException ex){
                if (ex.Message.ToString().Contains("412"))//ETag exception, read this join again and retry
                {
                    Thread.Sleep(Math.Min(50, 2 ^ retry));
                    DecrementAndStoreJoin(FindJoin(join.PartitionKey, join.RowKey), retry+1);
                }
            }
        }
    }
}
