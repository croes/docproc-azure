using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using Microsoft.WindowsAzure.Storage.Table;
using DocprocShared.DataAccessLayer;
using DocprocShared.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;
using DocprocShared.BlobAccessLayer;

namespace MvcWebRole.Controllers
{
    public class TaskController : Controller
    {

        private TaskDAO taskDao;
        private BlobAccess blobAccess;

        public TaskController()
        {
            taskDao = new TaskDAO();
            blobAccess = new BlobAccess();
        }

        public ActionResult Index(string jobPartitionKey, string jobRowKey)
        {
            JobDAO jobDAO = new JobDAO();
            Job job = jobDAO.FindJob(jobPartitionKey, jobRowKey);
            ViewBag.Job = job;
            return View(taskDao.FindTasksOfJob(job));
        }

        public ActionResult Delete(string partitionKey, string rowKey)
        {
            var task = taskDao.FindTask(partitionKey, rowKey);
            return View(task);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string partitionKey, string rowKey)
        {
            taskDao.DeleteTask(partitionKey, rowKey);
            return RedirectToAction("Index");
        }

        [ValidateInput(false)]
        public ActionResult Details(string partitionKey, string rowKey)
        {
            var task = taskDao.FindTask(partitionKey, rowKey);
            return View(task);
        }


        public static Expression<Func<Task, bool>> HasRowKeyPrefix(String prefix)
        {
            char lastChar = prefix[prefix.Length - 1];
            char nextLastChar = (char)((int)lastChar + 1);
            string nextPrefix = prefix.Substring(0, prefix.Length - 1) + nextLastChar;

            return e => e.RowKey.CompareTo(prefix) >= 0 && e.RowKey.CompareTo(nextPrefix) < 0;
        }
    }
}