using DocprocShared.DataAccessLayer;
using DocprocShared.QueueTask;
using Microsoft.WindowsAzure.Storage.Queue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WorkerRole.Processors
{
    class TemplateProcessor : Processor<TemplateTask>
    {
        JobDAO jobDao;
        TaskDAO taskDao;

        public TemplateProcessor()
        {
            jobDao = new JobDAO();
            taskDao = new TaskDAO();
        }

        public override List<CloudQueueMessage> Process(TemplateTask queueTask)
        {
            var outgoingMessages = new List<CloudQueueMessage>();
            return outgoingMessages;
        }
    }
}
