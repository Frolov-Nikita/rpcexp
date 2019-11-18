using System;
using System.Collections.Generic;
using System.Text;

namespace RPCExp.Modbus
{


    public class MTagsCollection : List<MTag>, IRange //Hack: Сделать multiset :)
    {
        private int begin;
        private int end;

        public int Begin => begin;

        public int Length => end - begin + 1;

        public int End => end;

        public string Name { get; set; }

        private static int Max(int x, int y) => x > y ? x : y;
        private static int Min(int x, int y) => x < y ? x : y;

        private int SpareTo( IRange r)
        {
            if (Count == 0)
                return 0;
            var e = r.Begin - End;
            var b = Begin - r.End;
            return Max(e, b) - 1;
        }
        private int IncrSize(IRange r)
        {
            if (Count == 0)
                return r.Length;
            var e = r.End - End;
            var b = Begin - r.Begin;
            return Max(e, b);
        }
        private int NewSize(IRange r)
        {
            if (Count == 0)
                return r.Length;
            int e = Max(End, r.End);
            int b = Min(Begin, r.Begin);
            return e - b;
        }

        private static MTagsCollection Nearest(List<MTagsCollection> ranges, IRange r)
        {
            if (ranges is null)
                return null;
            // найдем группу при добавлении в которую её размер меняется минимально
            MTagsCollection nearestGroup = null;
            int minIncr = int.MaxValue;
            foreach (var g in ranges)
            {
                var d = g.IncrSize(r);
                if (minIncr > d)
                {
                    nearestGroup = g;
                    minIncr = d;
                }
            }
            return nearestGroup;
        }

        public new void Add(MTag item)
        {
            if (item is null)
                return;

            if (Count > 0)
            {
                begin = Min( item.Begin, begin );
                end = Max(item.End, end) ;

                var i = base.FindIndex(x => x.Begin > item.Begin);
                if (i >= 0)
                    base.Insert(i, item);
                else
                    base.Add(item);
            }
            else
            {
                begin = item.Begin;
                end = item.End;
                base.Add(item);
            }
        }

        public new bool Remove(MTag item)
        {
            if (item is null)
                return false;

            if (!base.Remove(item))
                return false;

            if (Count == 0)
            {
                begin = 0;
                end = 0;
                return true;
            }

            if (item.Begin == begin)
                begin = this.Count > 0 ? this[0].Begin : 0;

            if (item.End == end)
            {
                end = this.Count > 0 ? this[this.Count - 1].End : 0;
                foreach (var t in this)
                    if (t.End > end)
                        end = t.End;
            }

            return true;
        }

        public List<MTagsCollection> Slice(int maxGroupLength = 100, int maxSpareSize = 0)
        {
            //124 - макс // (255 - slaveId - func - start*2 - length*2 - crc*2)/2 = 248/2 = 124
            var grouped = new List<MTagsCollection>();
            
            // Для ускорения сортируем // По идее должно быть уже отсортировано
            this.Sort((x1, x2) => x1.Begin.CompareTo(x2.Begin));

            // Группируем все тэги 
            foreach (var tag in this)
            {
                var gp = Nearest(grouped, tag);
                // Если не нашлось подходящей создаем новую группу
                if ((gp == null) || (gp.SpareTo( tag) > maxSpareSize))
                {
                    gp = new MTagsCollection { tag };
                    grouped.Add(gp);
                    continue;
                }
                gp.Add(tag);
            }
            
            // Разделить по максимальному размеру пакета. Пакеты длиннее, делятся на ~равные интервалы.
            var spared = new List<MTagsCollection>();
            foreach (var g in grouped)
            {
                if (g.Length > maxGroupLength)
                {
                    int optLen = (int)(g.Length / Math.Ceiling(g.Length * 1.0 / maxGroupLength));
                    for (int i = 0; i < g.Count;)
                    {
                        var newGroup = new MTagsCollection();
                        while ((i < g.Count) && (newGroup.NewSize( g[i]) <= optLen))
                            newGroup.Add(g[i++]);

                        if (newGroup.Count > 0)
                            spared.Add(newGroup);
                    }
                }
                else
                    spared.Add(g);
            }

            return spared;
        }
    }
}
