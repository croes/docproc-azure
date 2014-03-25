using DocprocShared.DataAccessLayer;
using DocprocShared.QueueTask;
using Microsoft.WindowsAzure.Storage.Queue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkerRole.Processors
{
    public class MailProcessor : Processor<MailTask>
    {
        JobDAO jobDao;
        TaskDAO taskDao;

        public MailProcessor()
        {
            jobDao = new JobDAO();
            taskDao = new TaskDAO();
        }

        public override List<CloudQueueMessage> Process(MailTask queueTask)
        {

            List<CloudQueueMessage> outgoingmessages = new List<CloudQueueMessage>();
            return outgoingmessages;
        } 
    }
}
