using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace System.Collections.Generic
{
    public class LimitedObservableCollection<T> : ObservableCollection<T>
    {
        public int Limit { get; set; } = 10;

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
