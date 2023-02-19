using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwatApp.Models
{
    public class Expose<T, TClass> : ReactiveObject
    {
        public Expose(TClass instance, string property_name)
        {
            m_instance = instance;
            m_property_name = property_name;
        }

        public Expose(TClass instance, string property_name, T val)
            : this(instance, property_name)
        {
            Value = val;
        }

        public T Value
        {
            get => (T)typeof(TClass).GetProperty(m_property_name)!.GetValue(m_instance)!;
            set
            {
                typeof(TClass).GetProperty(m_property_name)!.SetValue(m_instance, value);
                this.RaisePropertyChanged(nameof(Value));
            }
        }

        protected TClass m_instance;
        protected string m_property_name;

    }
}
