using System.Collections.Generic;
using System.Threading;

namespace Plugin.Core.Utility
{
    /// <summary>
    /// Lista thread-safe que usa Monitor para sincronización
    /// </summary>
    /// <typeparam name="T">Tipo de elementos en la lista</typeparam>
    public class SafeList<T>
    {
        private readonly List<T> _internalList = new List<T>();
        private readonly object _lockObject = new object();

        /// <summary>
        /// Agrega un elemento a la lista de forma thread-safe
        /// </summary>
        public void Add(T Value)
        {
            object lockObj = _lockObject;
            bool lockTaken = false;

            try
            {
                Monitor.Enter(lockObj, ref lockTaken);
                _internalList.Add(Value);
            }
            finally
            {
                if (lockTaken)
                    Monitor.Exit(lockObj);
            }
        }

        /// <summary>
        /// Limpia todos los elementos de la lista de forma thread-safe
        /// </summary>
        public void Clear()
        {
            object lockObj = _lockObject;
            bool lockTaken = false;

            try
            {
                Monitor.Enter(lockObj, ref lockTaken);
                _internalList.Clear();
            }
            finally
            {
                if (lockTaken)
                    Monitor.Exit(lockObj);
            }
        }

        /// <summary>
        /// Verifica si la lista contiene un elemento de forma thread-safe
        /// </summary>
        public bool Contains(T Value)
        {
            object lockObj = _lockObject;
            bool lockTaken = false;

            try
            {
                Monitor.Enter(lockObj, ref lockTaken);
                return _internalList.Contains(Value);
            }
            finally
            {
                if (lockTaken)
                    Monitor.Exit(lockObj);
            }
        }

        /// <summary>
        /// Obtiene el número de elementos en la lista de forma thread-safe
        /// </summary>
        public int Count()
        {
            object lockObj = _lockObject;
            bool lockTaken = false;

            try
            {
                Monitor.Enter(lockObj, ref lockTaken);
                return _internalList.Count;
            }
            finally
            {
                if (lockTaken)
                    Monitor.Exit(lockObj);
            }
        }

        /// <summary>
        /// Remueve un elemento de la lista de forma thread-safe
        /// </summary>
        public bool Remove(T Value)
        {
            object lockObj = _lockObject;
            bool lockTaken = false;

            try
            {
                Monitor.Enter(lockObj, ref lockTaken);
                return _internalList.Remove(Value);
            }
            finally
            {
                if (lockTaken)
                    Monitor.Exit(lockObj);
            }
        }
    }
}