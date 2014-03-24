using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using Microsoft.WindowsAzure.Storage.Table;
using DocprocShared.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Web;

namespace DocprocShared.DataAccessLayer
{
    public class JobDAO
    {

        private CloudTable jobTable;

        public JobDAO()
        {
            var storageAccount = Microsoft.WindowsAzure.Storage.CloudStorageAccount.Parse(
                RoleEnvironment.GetConfigurationSettingValue("StorageConnectionString"));
            var tableClient = storageAccount.CreateCloudTableClient();
            jobTable = tableClient.GetTableReference("job");
            jobTable.CreateIfNotExists();
        }

        public Job FindJob(string partitionKey, string rowKey)
        {
            if (partitionKey == null || rowKey == null)
            {
                throw new EntityNotFoundException(
                    "Cannot load job with null partitionKey or rowKey");
            }
            var retrieveOperation = TableOperation.Retrieve<Job>(partitionKey, rowKey);
            var retrievedResult = jobTable.Execute(retrieveOperation);
            var job = retrievedResult.Result as Job;
            if (job == null)
            {
                throw new EntityNotFoundException(
                    "No job found for partitionKey: " + partitionKey + " rowKey: " + rowKey);
            }
            return job;
        }

        public List<Job> FindJobsOfUser(String userName)
        {
            TableRequestOptions reqOptions = new TableRequestOptions()
            {
                MaximumExecutionTime = TimeSpan.FromSeconds(1.5),
                RetryPolicy = new LinearRetry(TimeSpan.FromSeconds(3), 3)
            };
            List<Job> jobs = new List<Job>();
            try
            {
                jobs = (from job in jobTable.CreateQuery<Job>()
                                  where job.PartitionKey == "USER:" + userName
                                  select job)
                            //.Where(HasRowKeyPrefix("JOB:"))
                            .ToList();
            }
            catch (StorageException se)
            {
                Trace.Write(se.Message);
                Trace.Write(se.StackTrace);
            }
            return jobs;
        }

        public void PersistJob(Job job)
        {
            var insertOperation = TableOperation.Insert(job);
            jobTable.Execute(insertOperation);
        }

        public void DeleteJob(string partitionKey, string rowKey)
        {
            var job = FindJob(partitionKey, rowKey);
            DeleteJob(job);
        }

        public void DeleteJob(Job job)
        {
            //TODO find tasks of job
            //TODO find results of tasks + job
            //TODO delete results
            //TODO delete tasks
            jobTable.Execute(TableOperation.Delete(job));
           
        }

        public static Expression<Func<Job, bool>> HasRowKeyPrefix(String prefix)
        {
            char lastChar = prefix[prefix.Length - 1];
            char nextLastChar = (char)((int)lastChar + 1);
            string nextPrefix = prefix.Substring(0, prefix.Length - 1) + nextLastChar;

            return e => e.RowKey.CompareTo(prefix) >= 0 
                && e.RowKey.CompareTo(nextPrefix) < 0;
        }

        public Job FindJob(string keyString)
        {
            string partitionkey = keyString.Split(',')[0];
            string rowkey = keyString.Split(',')[1];
            return FindJob(partitionkey, rowkey);
        }
    }
}