using System;
using System.Collections.Generic;
using System.Reflection;

namespace AmphetamineSerializer.Common
{
    /// <summary>
    /// Save and restore public properties of a give object.
    /// </summary>
    public class StatusSaver : IDisposable
    {
        private Dictionary<PropertyInfo, object> states = new Dictionary<PropertyInfo, object>();
        private object instance;

        /// <summary>
        /// Save the state of the object (i.e. all the public fields).
        /// See <seealso cref="Dispose"/>
        /// </summary>
        public StatusSaver(object instance)
        {
            if (instance == null)
                throw new ArgumentNullException("instance");

            this.instance = instance;
            var properties = instance.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var item in properties)
            {
                if (item.SetMethod != null && item.GetMethod != null)
                    states.Add(item, item.GetMethod.Invoke(instance, null));
            }
        }

        /// <summary>
        /// Restore previous state of the object (i.e. all the public fields).
        /// </summary>
        public void Dispose()
        {
            if (states == null)
                throw new InvalidOperationException("Unable to restore the state.");
            
            foreach (var item in states)
                item.Key.SetMethod.Invoke(instance, new object[] { item.Value });
        }
    }
}
