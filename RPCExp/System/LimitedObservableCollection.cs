using System.Collections.ObjectModel;

namespace System.Collections.Generic
{
    /// <summary>
    /// Observable collection with limited capacity.
    /// New elements crowd out old ones
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class LimitedObservableCollection<T> : ObservableCollection<T>
    {
        /// <summary>
        /// Max count of items inside the collection
        /// </summary>
        public int Limit { get; set; } = 10;

        /// <summary>
        /// This method do all magic. It calling by other Add, AddRange, etc.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="item"></param>
        protected override void InsertItem(int index, T item)
        {
            while (Count >= Limit)
            {
                base.RemoveItem(0);
                index--;
            }

            base.InsertItem(index, item);

        }

    }
}
