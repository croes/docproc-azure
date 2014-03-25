using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocprocShared.Models
{
    public class Join : TableEntity
    {

        public Join()
        {
            this.Count = 0;
            this.PartitionKey = "INVALIDPK";
            this.RowKey = "INVALIDRK";
        }

        public Join(Job job, int count)
        {
            if (count <= 0)
                throw new ArgumentOutOfRangeException("Count must be greater than 0.");
            this.Count = count;
            this.PartitionKey = job.PartitionKey;
            this.RowKey = "JOIN-" + job.RowKey;
        }

        [Required]
        [Display(Name = "Count")]
        public int Count { get; set; }

        public void Decrement()
        {
            if(this.Count > 0)
                this.Count--;
        }

    }
}
