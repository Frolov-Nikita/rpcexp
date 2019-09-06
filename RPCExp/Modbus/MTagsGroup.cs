using System;
using System.Collections.Generic;
using System.Text;

namespace RPCExp.Modbus
{


    public class MTagsGroup : List<MTag>, IRange //TODO: Сделать multiset :)
    {
        int begin;
        int end;

        public int Begin => begin;

        public int Length => end - begin + 1;

        public int End => end;

        public string Name { get; set; }

        int max(int x, int y) => x > y ? x : y;
        int min(int x, int y) => x < y ? x : y;

        int spareTo( IRange r)
        {
            if (Count == 0)
                return 0;
            var e = r.Begin - End;
            var b = Begin - r.End;
            return max(e, b) - 1;
        }
        int incrSize(IRange r)
        {
            if (Count == 0)
                return r.Length;
            var e = r.End - End;
            var b = Begin - r.Begin;
            return max(e, b);
        }
        int newSize(IRange r)
        {
            if (Count == 0)
                return r.Length;
            int e = max(End, r.End);
            int b = min(Begin, r.Begin);
            return e - b;
        }

        MTagsGroup nearest(List<MTagsGroup> ranges, IRange r)
        {
            // найдем группу при добавлении в которую её размер меняется минимально
            MTagsGroup gp = null;
            int minIncr = int.MaxValue;
            foreach (var g in ranges)
            {
                var d = g.incrSize(r);
                if (minIncr > d)
                {
                    gp = g;
                    minIncr = d;
                }
            }
            return gp;
        }

        public new void Add(MTag item)
        {
            if (Count > 0)
            {
                begin = min( item.Begin, begin );
                end = max(item.End, end) ;

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

        public List<MTagsGroup> Slice(int maxGroupLength = 100, int maxSpareSize = 0)
        {
            //124 - макс // (255 - slaveId - func - start*2 - length*2 - crc*2)/2 = 248/2 = 124
            var grouped = new List<MTagsGroup>();
            
            // Для ускорения сортируем // По идее должно быть уже отсортировано
            this.Sort((x1, x2) => x1.Begin.CompareTo(x2.Begin));

            // Группируем все тэги 
            foreach (var tag in this)
            {
                var gp = nearest(grouped, tag);
                // Если не нашлось подходящей создаем новую группу
                if ((gp == null) || (gp.spareTo( tag) > maxSpareSize))
                {
                    gp = new MTagsGroup { tag };
                    grouped.Add(gp);
                    continue;
                }
                gp.Add(tag);
            }
            
            // Разделить по максимальному размеру пакета. Пакеты длиннее, делятся на ~равные интервалы.
            var spared = new List<MTagsGroup>();
            foreach (var g in grouped)
            {
                if (g.Length > maxGroupLength)
                {
                    int optLen = (int)(g.Length / Math.Ceiling(g.Length * 1.0 / maxGroupLength));
                    for (int i = 0; i < g.Count;)
                    {
                        var newGroup = new MTagsGroup();
                        while ((i < g.Count) && (newGroup.newSize( g[i]) <= optLen))
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
