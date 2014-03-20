using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MvcWebRole.Models
{
    public class Job : TableEntity
    {

        public Job()
        {
            this.PartitionKey = "USER:NOUSER";
            this.RowKey = "JOB:" + Guid.NewGuid().ToString();
        }


        [Required]
        [Display(Name="ID")]
        public string Id
        {
            get { return RowKey;  }
        }

        [Display(Name = "Template")]
        public string Template { get; set; }
        
        [Display(Name = "Data")]
        public string Data { get; set; }

        [Display(Name = "Owner")]
        public string Owner { get; set; }

        [Display(Name = "Start time")]
        [DataType(DataType.Date)]
        public DateTime? StartTime { get; set; }

        [Display(Name = "End time")]
        [DataType(DataType.Date)]
        public DateTime? EndTime { get; set; }

        [Display(Name = "Result")]
        public string Result { get; set; }

        public override string ToString()
        {
            return string.Format("Id: {0} \n" +
                                 "Template: {1} \n" +
                                 "Data: {2} \n" +
                                 "Owner: {3} \n" +
                                 "Result: {4}", Id, Template, Data, Owner, Result);
        }

        public string KeyString
        {
            get { return PartitionKey + "," + RowKey; }
        }
    }
}