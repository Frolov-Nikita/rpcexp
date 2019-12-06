
using System.Collections.Generic;

namespace System
{
    /// <summary>
    /// Extensions methods for standard IDictionary
    /// </summary>
    public static class DictionaryExtention
    {

#pragma warning disable CA1062 // Проверить аргументы или открытые методы
        /// <summary>
        /// if value class implements INameDescription, then name property can be used as key.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dictionary"></param>
        /// <param name="item"></param>
        public static void AddByName<T>(this IDictionary<string, T> dictionary, T item)
            where T : INameDescription
        {
            if (dictionary.ContainsKey(item.Name))
                dictionary[item.Name] = item;
            else
                dictionary.Add(item.Name, item);
        }
#pragma warning restore CA1062 // Проверить аргументы или открытые методы
    }
}
