using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TwatApp.Models
{
    public interface ICopyable<T> where T : class, new() {}

    public static class CopyableExtension
    {
        public static T copy<T>(this ICopyable<T> copyable) where T : class, new()
        {
            T result = new();

            var properties = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            foreach(var prop in properties)
            {
                prop.SetValue(result, prop.GetValue(copyable));
            }

            var fields = typeof(T).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            foreach(var field in fields)
            {
                field.SetValue(result, field.GetValue(copyable));
            }

            return result;
        }
    }

    public static class Copyer
    {
        public static T copy<T>(T obj) where T : new()
        {
            T result = new();

            var properties = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (var prop in properties)
            {
                prop.SetValue(result, copy(prop.GetValue(obj)));
            }

            var fields = typeof(T).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (var field in fields)
            {
                field.SetValue(result, copy(field.GetValue(obj)));
            }

            return result;
        }
    }

}
