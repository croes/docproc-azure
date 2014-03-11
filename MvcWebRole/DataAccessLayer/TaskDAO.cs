using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using Microsoft.WindowsAzure.Storage.Table;
using MvcWebRole.Controllers;
using MvcWebRole.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;

namespace MvcWebRole.DataAccessLayer
{
    public class TaskDAO
    {

        private CloudTable taskTable;

        public TaskDAO()
        {
            var storageAccount = Microsoft.WindowsAzure.Storage.CloudStorageAccount.Parse(
                RoleEnvironment.GetConfigurationSettingValue("StorageConnectionString"));
            var tableClient = storageAccount.CreateCloudTableClient();
            taskTable = tableClient.GetTableReference("task");
        }

        public Task FindTask(string partitionKey, string rowKey)
        {
            if (partitionKey == null || rowKey == null)
            {
                throw new EntityNotFoundException(
                    "Cannot load task with null partitionKey or rowKey");
            }
            var retrieveOperation = TableOperation.Retrieve<Job>(partitionKey, rowKey);
            var retrievedResult = taskTable.Execute(retrieveOperation);
            var task = retrievedResult.Result as Task;
            if (task == null)
            {
                throw new EntityNotFoundException(
                    "No task found for partitionKey: " + partitionKey + " rowKey: " + rowKey);
            }
            return task;
        }

        public List<Task> FindTasksOfJob(Job job)
        {
            TableRequestOptions reqOptions = new TableRequestOptions()
            {
                MaximumExecutionTime = TimeSpan.FromSeconds(1.5),
                RetryPolicy = new LinearRetry(TimeSpan.FromSeconds(3), 3)
            };
            List<Task> tasks = (from task in taskTable.CreateQuery<Task>()
                              where task.PartitionKey == "JOB:" + job.RowKey
                              select task)
                            .Where(HasRowKeyPrefix("TASK:"))
                            .ToList();
            return tasks;
        }

        public void PersistTask(Task task)
        {
            var insertOperation = TableOperation.Insert(task);
            taskTable.Execute(insertOperation);
        }

        public void DeleteTask(string partitionKey, string rowKey)
        {
            var task = FindTask(partitionKey, rowKey);
            DeleteTask(task);
        }

        public void DeleteTask(Task task)
        {
            //TODO find tasks of job
            //TODO find results of tasks + job
            //TODO delete results
            //TODO delete tasks
            taskTable.Execute(TableOperation.Delete(task));

        }

        public static Expression<Func<Task, bool>> HasRowKeyPrefix(String prefix)
        {
            char lastChar = prefix[prefix.Length - 1];
            char nextLastChar = (char)((int)lastChar + 1);
            string nextPrefix = prefix.Substring(0, prefix.Length - 1) + nextLastChar;

            return e => e.RowKey.CompareTo(prefix) >= 0
                && e.RowKey.CompareTo(nextPrefix) < 0;
        }
    }
}