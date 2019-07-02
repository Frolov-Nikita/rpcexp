using System;
using System.Collections.Generic;
using System.Text;

namespace RPCExp.Modbus
{
    // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    // Код неплохой, но не используется. Использовать только как снипет

    public class Range : List<Range> // переделать на multiset
    {
        public Range()
            :base()
        {}

        public Range(int begin, int length)
        : base()
        {
            Begin = begin;
            End = begin + length - 1;
        }

        public virtual int Begin { get; set; }

        public virtual int Length => End - Begin + 1;

        public virtual int End { get; set; }

        public new void Add(Range item)
        {
            if (Count != 0)
            {
                Begin = min(item.Begin, Begin);
                End = max(item.End, End);
            }
            else
            {
                Begin = item.Begin;
                End = item.End;
            }

            var i = base.FindIndex(x => x.Begin > item.Begin);
            if (i >= 0)
                base.Insert(i, item);
            else
                base.Add(item);
        }
        
        public new bool Remove(Range item)
        {
            if (!base.Remove(item))
                return false; 

            if (Count == 0)
            {
                Begin = 0;
                End = 0;
                return true;
            }

            if (item.Begin == Begin)
                Begin = this[0].Begin;

            if (item.End == End)
            {
                End = this[this.Count - 1].End;
                foreach (var r in this)
                    if (r.End > End)
                        End = r.End;
            }

            return true;
        }

        int max(int x, int y) => x > y ? x : y;

        int min(int x, int y) => x < y ? x : y;

        /// <summary>
        /// пробел между группами
        /// </summary>
        /// <param name="r"></param>
        /// <returns></returns>
        int spareTo(Range r)
        {
            if (Count == 0)
                return 0;
            var e = r.Begin - End;
            var b = Begin - r.End;
            return max(e, b) - 1;
        }

        /// <summary>
        /// Увеличение группы при добавлении
        /// </summary>
        /// <param name="r"></param>
        /// <returns></returns>
        int incrSize(Range r)
        {
            if (Count == 0)
                return r.Length;
            var e = r.End - End;
            var b = Begin - r.Begin;
            return max(e, b);
        }

        /// <summary>
        /// Новый размер группы
        /// </summary>
        /// <param name="r"></param>
        /// <returns></returns>
        int newSize(Range r)
        {
            if (Count == 0)
                return r.Length;
            int e = max(End, r.End);
            int b = min(Begin, r.Begin);
            return e - b;
        }

        /// <summary>
        /// Поиск группы в подгруппах при добавлении в которую увеличение её размера будет минимальным
        /// </summary>
        /// <param name="r"></param>
        /// <returns></returns>
        Range nearTo(Range r)
        {
            // найдем группу при добавлении в которую её размер меняется минимально
            Range nearRange = null;
            int minIncr = int.MaxValue;
            foreach (var curRange in this)
            {
                var d = curRange.incrSize(r);
                if (minIncr > d)
                {
                    nearRange = curRange;
                    minIncr = d;
                }
            }
            return nearRange;
        }

        /// <summary>
        /// Поделить подгруппы на группы
        /// </summary>
        /// <param name="maxGroupLength">максимальный размер групп</param>
        /// <param name="maxSpareSize">Максимальный размер "пробелов" в группах</param>
        /// <returns></returns>
        public List<Range> Slice(int maxGroupLength = 100, int maxSpareSize = 0)
        {
            this.Sort((x1, x2) => x1.Begin.CompareTo(x2.Begin));

            var grouped = new Range();
            // группируем все подряд без ограничения по длине
            foreach (var r in this)
            {
                var gp = grouped.nearTo(r);
                // Если не нашлось подходящей создаем новую группу
                if (gp == null || gp.spareTo(r) > maxSpareSize) // || gp.newSize(r) > maxGroupLength)
                {
                    gp = new Range();
                    gp.Add(r);
                    grouped.Add(gp);
                    continue;
                }
                gp.Add(r);
            }

            var spared = new Range();
            // Слишком большие группы 
            foreach (var g in grouped)
            {
                if (g.Length > maxGroupLength)
                {
                    int optLen = (int)(g.Length / Math.Ceiling(g.Length * 1.0 / maxGroupLength));
                    for (int i = 0; i < g.Count;)
                    {
                        var newGroup = new Range();
                        while ((i < g.Count) && (newGroup.newSize(g[i]) <= optLen))
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
