using Microsoft.WindowsAzure.Storage.Queue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkerRole.Processors
{

    public interface IProcessor
    {
        List<CloudQueueMessage> Process(object obj);
    }

    public abstract class Processor<T> : IProcessor
    {
        public abstract List<CloudQueueMessage> Process(T obj);
        public List<CloudQueueMessage> Process(object obj)
        {
            return Process((T)obj);
        }
    }
}
