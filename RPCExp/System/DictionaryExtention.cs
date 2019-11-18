
using System.Collections.Generic;

namespace System
{
    public static class DictionaryExtention
    {

#pragma warning disable CA1062 // Проверить аргументы или открытые методы
        public static void AddByName<T>(this IDictionary<string, T> dictionary, T item)
            where T: INameDescription
        {
            if (dictionary.ContainsKey(item.Name))
                dictionary[item.Name] = item;
            else
                dictionary.Add(item.Name, item);
        }
#pragma warning restore CA1062 // Проверить аргументы или открытые методы
    }
}
