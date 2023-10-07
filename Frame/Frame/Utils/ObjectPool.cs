using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RandomGains.Frame.Utils
{
    internal class ObjectPool<T> where T : class
    {
        public Queue<T> queue = new Queue<T>();

        public Delegate createFunc;
        public Delegate reinitFunc;
        public Action<T> releaseFunc;

        public T Get(params object[] param)
        {
            if(queue.Count == 0)
            {
                return (T)createFunc.DynamicInvoke(param);
            }
            else
            {
                var result = queue.Dequeue();
                reinitFunc.DynamicInvoke(result, param);
                return result;
            }
        }

        public void Release(T obj)
        {
            releaseFunc.Invoke(obj);
            queue.Enqueue(obj);
        }
    }
}
