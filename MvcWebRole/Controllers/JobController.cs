using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage.Table.DataServices;
using MvcWebRole.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Linq.Expressions;

namespace MvcWebRole.Controllers
{
    public class JobController : Controller
    {

        private CloudTable jobTable;

        public JobController()
        {
            var storageAccount = Microsoft.WindowsAzure.Storage.CloudStorageAccount.Parse(RoleEnvironment.GetConfigurationSettingValue("StorageConnectionString"));
            var tableClient = storageAccount.CreateCloudTableClient();
            jobTable = tableClient.GetTableReference("job");
        }

        [HandleError(ExceptionType = typeof(EntityNotFoundException))]
        private Job FindRow(string partitionKey, string rowKey)
        {
            if (partitionKey == null || rowKey == null)
            {
                throw new EntityNotFoundException("Cannot load job with null partitionKey or rowKey");
            }
            var retrieveOperation = TableOperation.Retrieve<Job>(partitionKey, rowKey);
            var retrievedResult = jobTable.Execute(retrieveOperation);
            var job = retrievedResult.Result as Job;
            if (job == null)
            {
                throw new EntityNotFoundException("No job found for partitionKey: " + partitionKey + " rowKey: " + rowKey);
            }

            return job;
        }

        public ActionResult Index()
        {
            TableRequestOptions reqOptions = new TableRequestOptions()
            {
                MaximumExecutionTime = TimeSpan.FromSeconds(1.5),
                RetryPolicy = new LinearRetry(TimeSpan.FromSeconds(3), 3)
            };
            String userName = System.Web.HttpContext.Current.User.Identity.Name;
            List<Job> jobs;
            try
            {
            jobs = (from job in jobTable.CreateQuery<Job>()
                                     where job.PartitionKey == "JOB:" + userName
                                     select job)
                        .Where(HasRowKeyPrefix("JOB:"))
                        .ToList();
            
            }
            catch (StorageException se)
            {
                ViewBag.errorMessage = "Timeout error, try again. ";
                Trace.TraceError(se.Message);
                return View("Error");
            }
            
            return View(jobs);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Create(Job job)
        {
            job.Template = WebUtility.HtmlEncode(job.Template);
            job.PartitionKey = "JOB:" + job.Owner;
            Trace.TraceInformation("created job: {0}", job);
            if (ModelState.IsValid)
            {
                Trace.TraceInformation("job is valid.");
                var insertOperation = TableOperation.Insert(job);
                jobTable.Execute(insertOperation);
                return RedirectToAction("Index");
            }
            var errors = ModelState.Values.SelectMany(v => v.Errors);
            return View(job);
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Edit(string partitionKey, string rowKey, Job editedJob)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        var job = new Job();
        //        UpdateModel(job);
        //        try
        //        {
        //            var replaceOperation = TableOperation.Replace(job);
        //            jobTable.Execute(replaceOperation);
        //            return RedirectToAction("Index");
        //        }
        //        catch (StorageException ex)
        //        {
        //            if (ex.RequestInformation.HttpStatusCode == 412)
        //            {
        //                // Concurrency error
        //                var currentJob = FindRow(partitionKey, rowKey);
        //                if (currentJob.Id != editedJob.Id)
        //                {
        //                    ModelState.AddModelError("Id", "Current value: " + currentJob.Id);
        //                }
        //                if (currentJob.Template != currentJob.Template)
        //                {
        //                    ModelState.AddModelError("Template", "Current value: " + currentJob.Template);
        //                }
        //                if (currentJob.Data != currentJob.Data)
        //                {
        //                    ModelState.AddModelError("Data", "Current value: " + currentJob.Data);
        //                }
        //                if (currentJob.Owner != currentJob.Owner)
        //                {
        //                    ModelState.AddModelError("Owner", "Current value: " + currentJob.Owner);
        //                }
        //                if (currentJob.Result != currentJob.Result)
        //                {
        //                    ModelState.AddModelError("Result", "Current value: " + currentJob.Result);
        //                }
        //                if (currentJob.StartTime != currentJob.StartTime)
        //                {
        //                    ModelState.AddModelError("StartTime", "Current value: " + currentJob.StartTime);
        //                }
        //                if (currentJob.EndTime != currentJob.EndTime)
        //                {
        //                    ModelState.AddModelError("EndTime", "Current value: " + currentJob.EndTime);
        //                }
        //                ModelState.AddModelError(string.Empty, "The record you attempted to edit "
        //                    + "was modified by another user after you got the original value. The "
        //                    + "edit operation was canceled and the current values in the database "
        //                    + "have been displayed. If you still want to edit this record, click "
        //                    + "the Save button again. Otherwise click the Back to List hyperlink.");
        //                ModelState.SetModelValue("ETag", new ValueProviderResult(currentJob.ETag, currentJob.ETag, null));
        //            }
        //            else
        //            {
        //                throw;
        //            }
        //        }
        //    }
        //    return View(editedJob);
        //}

        public ActionResult Delete(string partitionKey, string rowKey)
        {
            var job = FindRow(partitionKey, rowKey);
            return View(job);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string partitionKey, string rowKey)
        {
            //find job
            Job job = FindRow(partitionKey, rowKey);
            //find tasks of job
            //find results of tasks + job
            //delete results
            //delete tasks
            //delete job
            jobTable.Execute(TableOperation.Delete(job));
            return RedirectToAction("Index");
        }

        public ActionResult Details(string partitionKey, string rowKey)
        {
            var job = FindRow(partitionKey, rowKey);
            return View(job);
        }

        public static Expression<Func<Job, bool>> HasRowKeyPrefix(String prefix)
        {
            char lastChar = prefix[prefix.Length - 1];
            char nextLastChar = (char)((int)lastChar + 1);
            string nextPrefix = prefix.Substring(0, prefix.Length - 1) + nextLastChar;

            return e => e.RowKey.CompareTo(prefix) >= 0 && e.RowKey.CompareTo(nextPrefix) < 0;
        }

    }
}