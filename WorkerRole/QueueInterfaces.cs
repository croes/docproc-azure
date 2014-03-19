using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkerRole
{
    /// Defines a contract that must be implemented by an extension responsible for listening on a Windows Azure queue.
    public interface ICloudQueueServiceWorkerRoleExtension
    {
        /// Starts a multi-threaded queue listener that uses the specified number of dequeue threads.
        void StartListener(int threadCount);

        /// Returns the current state of the queue listener to determine point-in-time load characteristics.
        CloudQueueListenerInfo QueryState();

        /// Gets or sets the batch size when performing dequeue operation against a Windows Azure queue.
        int DequeueBatchSize { get; set; }

        /// Gets or sets the default interval that defines how long a queue listener will be idle for between polling a queue.
        TimeSpan DequeueInterval { get; set; }

        /// Defines a callback delegate which will be invoked whenever the queue is empty.
        event WorkCompletedDelegate QueueEmpty;

        /// <summary>
        /// Defines a callback delegate which will be invoked whenever an unit of work has been completed and the worker is
        /// requesting further instructions as to next steps.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="idleCount">The value indicating how many times the worker has been idle.</param>
        /// <param name="delay">Time interval during which the worker is instructed to sleep before performing next unit of work.</param>
        /// <returns>A flag indicating that the worker should stop processing any further units of work and must terminate.</returns>
        public delegate bool WorkCompletedDelegate(object sender, int idleCount, out TimeSpan delay);
    }

    /// Implements a structure containing point-in-time load characteristics for a given queue listener.
    public struct CloudQueueListenerInfo
    {
        /// Returns the approximate number of items in the Windows Azure queue.
        public int CurrentQueueDepth { get; internal set; }

        /// Returns the number of dequeue tasks that are actively performing work or waiting for work.
        public int ActiveDequeueTasks { get; internal set; }

        /// Returns the maximum number of dequeue tasks that were active at a time.
        public int TotalDequeueTasks { get; internal set; }
    }

    /// <summary>
    /// Defines a contract that must be supported by an extension that implements a generics-aware queue listener.
    /// </summary>
    /// <typeparam name="T">The type of queue item data that will be handled by the queue listener.</typeparam>
    public interface ICloudQueueListenerExtension<T> : ICloudQueueServiceWorkerRoleExtension, IObservable<T>
    {
    }


}
