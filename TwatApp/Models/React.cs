using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwatApp.Models
{
    public class React<T> : ReactiveObject
    {
        public React()
        {
            Value = default!;
        }

        public React(T val)
        {
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
            set
            {
                m_data = value;
                this.RaisePropertyChanged(nameof(Value));
            }
        }

        protected T m_data;
    }
}
