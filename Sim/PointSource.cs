using NModbus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sim
{
    public class PointSource<T> : IPointSource<T> where T : struct
    {
        IDictionary<ushort, T> data = new Dictionary<ushort, T>(8);

        public PointSource()
        {
        }

        ~PointSource()
        {
        }

        T GetOrCreateByKey(ushort address)
        {
            if (!data.ContainsKey(address))
                data.Add(address, default(T));
            return data[address];
        }

        void SetOrCreateByKey(ushort address, T value)
        {
            if (data.ContainsKey(address))
                data[address] = value;
            else
                data.Add(address, value); 
        }

        public T[] ReadPoints(ushort startAddress, ushort numberOfPoints)
        {
            T[] values = new T[numberOfPoints];
            for (var i = 0; i < numberOfPoints; i++)
                values[i] = GetOrCreateByKey((ushort)(startAddress + i));
            return values;
        }

        public void WritePoints(ushort startAddress, T[] points)
        {
            for (var i = 0; i < points.Length; i++)
                SetOrCreateByKey((ushort)(startAddress + i), points[i]);
        }
    }
}
