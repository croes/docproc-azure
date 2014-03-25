using DocprocShared.DataAccessLayer;
using DocprocShared.Models;
using DocprocShared.QueueTask;
using Microsoft.WindowsAzure.Storage.Queue;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace WorkerRole.Processors
{
    class TemplateProcessor : Processor<TemplateTask>
    {
        JobDAO jobDao;
        TaskDAO taskDao;

        Regex regex;

        public TemplateProcessor()
        {
            jobDao = new JobDAO();
            taskDao = new TaskDAO();
            regex = new Regex("\\$\\{(\\w*)\\}", RegexOptions.Compiled);
        }

        public override List<CloudQueueMessage> Process(TemplateTask queueTask)
        {
            Job job = jobDao.FindJob(queueTask.JobPartitionKey, queueTask.JobRowKey);
            Task task = taskDao.FindTask(queueTask.TaskPartitionKey, queueTask.TaskRowKey);
            string filledInTemplate = FillTemplate(job.Template, task.ParamDict);
            Trace.TraceInformation("Filled in template for task {0},{1}", task.PartitionKey, task.RowKey);
            task.Template = filledInTemplate;
            taskDao.PersistTask(task);
            var outgoingMessages = new List<CloudQueueMessage>();
            outgoingMessages.Add(new CloudQueueMessage(new RenderTask(job, task).ToBinary()));
            return outgoingMessages;
        }

        private string FillTemplate(string template, Dictionary<string, string> paramDict)
        {
            StringBuilder sb = new StringBuilder(template.Length);
            Match match = regex.Match(template);
            int cursor = 0;
            while (match.Success)
            {
                string paramName = match.Groups[1].Value;
                if (paramDict.ContainsKey(paramName))
                {
                    string paramValue = paramDict[paramName];
                    sb.Append(template.Substring(cursor, match.Index - cursor));
                    sb.Append(paramDict[paramName]);
                    cursor = match.Index + match.Value.Length;
                }
                else
                {
                    Trace.TraceWarning("NO PARAMETER {0} DEFINED FOR CURRENT TASK", paramName);
                }
                match = match.NextMatch();
            }
            return sb.ToString();
        }
    }
}
