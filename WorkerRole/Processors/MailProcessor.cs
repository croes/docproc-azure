using DocprocShared.DataAccessLayer;
using DocprocShared.QueueTask;
using Microsoft.WindowsAzure.Storage.Queue;
using SendGridMail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Mail;
using DocprocShared.Models;
using DocprocShared.BlobAccessLayer;
using System.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace WorkerRole.Processors
{
    class MailProcessor : Processor<MailTask>
    {
        static int SASLinkExpirationTimeInMins = 7 * 24 * 60;
        
        JobDAO jobDao;
        TaskDAO taskDao;
        BlobAccess blobAccess;

        public MailProcessor()
        {
            jobDao = new JobDAO();
            taskDao = new TaskDAO();
            blobAccess = new BlobAccess();
        }

        public override List<CloudQueueMessage> Process(MailTask queueTask)
        {
            Job job = jobDao.FindJob(queueTask.JobPartitionKey, queueTask.JobRowKey);
            Task task = taskDao.FindTask(queueTask.TaskPartitionKey, queueTask.TaskRowKey);
            var email = CreateMail(job, task);
            SendMail(email);
            Trace.TraceInformation("Send email for task {0},{1}", task.PartitionKey, task.RowKey);
            List<CloudQueueMessage> outgoingmessages = new List<CloudQueueMessage>();
            return outgoingmessages;
        }

        private SendGrid CreateMail(Job job, Task task)
        {
            string SASLink = blobAccess.GetSASUri(job, task, SASLinkExpirationTimeInMins);
            var email = SendGrid.GetInstance();
            email.From = new MailAddress("no-reply@docproc-azure.appspot.com");
            email.AddTo(task.ParamDict["email"]);
            email.Subject = "A docproc document has been generated";
            email.Html = String.Format("<p>A docproc document has been generated. <a href=\"{0}\">Download it now</a>. The link is valid for a week.</p>", SASLink);
            email.Text = String.Format("A docproc document has been generated. Download it using the link below. The link is valid for a week. \n{0}", SASLink);
            return email;
        }

        private void SendMail(SendGrid email)
        {
            var user = RoleEnvironment.GetConfigurationSettingValue("SendGridUser");
            var password = RoleEnvironment.GetConfigurationSettingValue("SendGridPwd");
            var credentials = new NetworkCredential(user, password);
            var transportWeb = Web.GetInstance(credentials);
            transportWeb.Deliver(email);
        }
    }
}
