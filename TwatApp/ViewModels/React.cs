using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwatApp.ViewModels
{
    public class React<T> : ReactiveObject
    {
        public React(T val = default!, string? name = null, ReactiveObject? obj = null)
        {
            // removes null reference warning, even though it is assigned already in the Value propertyw.
            m_data = val;

            m_name = name ?? nameof(Value);
            m_obj = obj ?? this;
            Value = val;
        }

        public static implicit operator T(React<T> react)
        {
            return react.Value;
        }

        public static implicit operator React<T>(T val)
        {
            return new(val);
        }

        public T Value
        {
            get => m_data;
            set => m_obj.RaiseAndSetIfChanged(ref m_data, value, m_name);
        }

        protected T m_data;
        protected string m_name;
        protected ReactiveObject m_obj;
    }
}
