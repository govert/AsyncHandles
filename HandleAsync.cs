using ExcelDna.Integration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsyncHandles
{
    internal class HandleAsync : IHandle, IExcelObservable, IDisposable
    {
        private static readonly object m_lock = new object();
        private static int m_index;

        private readonly HandleStorage m_storage;
        private IExcelObserver m_observer;
        private readonly string m_name;

        private Task<object> m_valueAsync;
        private object m_value;

        public HandleAsync(HandleStorage storage, string tag, Task<object> valueAsync)
        {
            m_storage = storage;
            m_valueAsync = valueAsync;
            valueAsync.ContinueWith(ProcessResult);

            lock (m_lock)
            {
                m_name = String.Format("{0}:{1}", tag, m_index++);
            }
        }

        void ProcessResult(Task<object> valueAsync)
        {
            // TODO: Some Error handling

            lock (m_lock)
            {
                try
                {
                    m_value = valueAsync.Result;
                    m_observer?.OnNext(m_name); // Only now set the result of the outer async call as the handle
                }
                catch (Exception ex)
                {
                    m_observer?.OnError(ex);
                }
            }
        }

        public IDisposable Subscribe(IExcelObserver observer)
        {
            lock (m_lock)
            {
                m_observer = observer;
                if (m_valueAsync.IsCompletedSuccessfully)
                    m_observer.OnNext(m_name);
                else if (m_valueAsync.IsFaulted)
                    m_observer.OnError(m_valueAsync.Exception);
            }
            return this;
        }

        public void Dispose()
        {
            m_storage.Remove(this);
        }

        public string Name
        {
            get
            {
                return m_name;
            }
        }

        public object Value
        {
            get
            {
                return m_value;
            }
        }
    }
}