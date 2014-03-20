using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MvcWebRole.Models
{
    public class Task : TableEntity
    {
        public Task()
        {
            this.PartitionKey = "JOB:NOJOB";
            this.RowKey = "TASK:" + Guid.NewGuid().ToString();
            this.ParamDict = new Dictionary<string,string>();
        }

        public Task(Job job)
        {
            this.PartitionKey = job.RowKey;
            this.RowKey = "TASK:" + Guid.NewGuid().ToString();
            this.ParamDict = new Dictionary<string, string>();
        }

        [Required]
        [Display(Name="ID")]
        public string Id
        {
            get { return this.RowKey; }
        }

        [Display(Name="Result")]
        public string Result { get; set; }

        [Display(Name = "Template")]
        public string Template { get; set; }

        [Display(Name="Params")]
        public Dictionary<string, string> ParamDict { get; private set;}

        public void addParam(string key, string value)
        {
            this.ParamDict.Add(key, value);
        }

        public void removeParam(string key, string value)
        {
            this.ParamDict.Remove(key);
        }

        public override string ToString()
        {
            string result = string.Format("Task \n" +
                                 "ID: {0} \n" +
                                 "Template: {1} \n" +
                                 "Result: {2} \n" +
                                 "Param map: \n", this.Id, this.Template, this.Result);
            foreach (var pair in ParamDict)
            {
                result += pair.Key + ": " + pair.Value;
            }
            return result;
        }

        public string KeyString
        {
            get { return PartitionKey + "," + RowKey; }
        }
    }
}