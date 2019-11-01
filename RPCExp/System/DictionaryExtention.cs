
using System.Collections.Generic;

namespace System
{
    public static class DictionaryExtention
    {
        public static void AddByName<T>(this IDictionary<string, T> dictionary, T item)
            where T: INameDescription
        {
            if (dictionary.ContainsKey(item.Name))
                dictionary[item.Name] = item;
            else
                dictionary.Add(item.Name, item);
        }
    }
}
