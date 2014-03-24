using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Web;

namespace DocprocShared.Models
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

        public override IDictionary<string, EntityProperty> WriteEntity(OperationContext operationContext)
        {
            var results = base.WriteEntity(operationContext);
            results.Add("ParamDict", new EntityProperty(serializeParamDict()));
            return results;
        }

        public override void ReadEntity(IDictionary<string, EntityProperty> properties, OperationContext operationContext)
        {
            base.ReadEntity(properties, operationContext);
            ParamDict = deserializeParamDict(properties["ParamDict"].BinaryValue);
        }

        private byte[] serializeParamDict()
        {
            MemoryStream stream = new MemoryStream();
            BinaryFormatter formatter = new BinaryFormatter();

            formatter.Serialize(stream, ParamDict);

            return stream.ToArray();
        }

        private Dictionary<string, string> deserializeParamDict(byte[] binaryData)
        {
            MemoryStream stream = new MemoryStream(binaryData);
            BinaryFormatter formatter = new BinaryFormatter();
            return (Dictionary<string, string>)formatter.Deserialize(stream);
        }
    }
}