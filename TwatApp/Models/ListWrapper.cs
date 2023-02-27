using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwatApp.ViewModels;

namespace TwatApp.Models
{
    public class DictWrapper<TKey, TWrap, TSource> where TKey : notnull where TWrap : IViewModel<TSource>, new()
    {

        public DictWrapper(IReadOnlyDictionary<TKey, TSource> source)
        {
            m_source = source;
        }

        public IDictionary<TKey, TWrap> Data { get
            {
                sync();
                return m_data;
            }
        }

        public void sync()
        {
            lock (m_data)
            {
                // remove all deleted elements

                m_data = m_data.Where(kv => m_source.ContainsKey(kv.Key)).ToDictionary(kv => kv.Key, kv => kv.Value);

                // construct wrapper types with all new elements passed to constructor

                foreach (var kv_pair in m_source.Where(kv => !m_data.ContainsKey(kv.Key)))
                {
                    m_data[kv_pair.Key] = new TWrap();
                    m_data[kv_pair.Key].supplyModel(kv_pair.Value);
                }
            }
        }

        IReadOnlyDictionary<TKey, TSource> m_source;
        Dictionary<TKey, TWrap> m_data = new();
    }
}
