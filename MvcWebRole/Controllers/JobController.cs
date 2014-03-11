﻿using Microsoft.WindowsAzure.ServiceRuntime;
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
using MvcWebRole.DataAccessLayer;

namespace MvcWebRole.Controllers
{
    public class JobController : Controller
    {

        private JobDAO jobDao;

        public JobController()
        {
            this.jobDao = new JobDAO();
        }

        public ActionResult Index()
        {
            string userName = System.Web.HttpContext.Current.User.Identity.Name;
            List<Job> jobsOfUser = jobDao.FindJobsOfUser(userName);
            return View(jobsOfUser);
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
            job.PartitionKey = "USER:" + job.Owner;
            Trace.TraceInformation("created job: {0}", job);
            if (ModelState.IsValid)
            {
                Trace.TraceInformation("job is valid.");
                jobDao.PersistJob(job);                
                return RedirectToAction("Index");
            }
            var errors = ModelState.Values.SelectMany(v => v.Errors);
            return View(job);
        }

        public ActionResult Delete(string partitionKey, string rowKey)
        {
            var job = jobDao.FindJob(partitionKey, rowKey);
            return View(job);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string partitionKey, string rowKey)
        {
            jobDao.DeleteJob(partitionKey, rowKey);
            return RedirectToAction("Index");
        }

        public ActionResult Details(string partitionKey, string rowKey)
        {
            var job = jobDao.FindJob(partitionKey, rowKey);
            return View(job);
        }

    }
}