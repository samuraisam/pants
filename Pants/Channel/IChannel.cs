using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Collections;

namespace Pants.Channel
{
    public interface IChannel<T>
    {
        /// <summary>
        /// Sends a message to the receiver.
        /// </summary>
        /// <param name="message"></param>
        void Send(T message);
    }

    public class Channel<T> : IChannel<T>
    {
        private Queue<T> _queue;
        private int _bufferSize;
        private object _lockObj = new object();

        public Channel(int bufferSize)
        {
            _queue = new Queue<T>(bufferSize);
            _bufferSize = bufferSize;
        }

        /// <summary>
        /// Sends a message to the receiver. If there are already too many messages buffered this method will block.
        /// </summary>
        /// <param name="message">Message to be sent</param>
        public void Send(T message)
        {
            lock (_lockObj) {
                if (_queue.Count < _bufferSize) {
                    _queue.Enqueue(message);
                }
            }
            
        }
    }
}
