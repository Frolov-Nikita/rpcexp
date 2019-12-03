using System;

namespace RPCExp.Common
{
    public class Scale: ICloneable
    {
        public decimal DevMin { get; set; } = -32768;

        public decimal DevMax { get; set; } = 32767;

        public decimal Min { get; set; } = -32768;

        public decimal Max { get; set; } = 32767;

#pragma warning disable CA1305 // Укажите IFormatProvider

        /// <summary>
        /// Scale value from device scale to server scale
        /// </summary>
        /// <param name="valueFromDev"></param>
        /// <returns></returns>
        public decimal ScaleDevToSrv(object valueFromDev)
        {
            decimal val = (decimal)Convert.ChangeType(valueFromDev, typeof(decimal));

            return ((val - DevMin) * (Max - Min) / (DevMax - DevMin)) + Min;
        }

        /// <summary>
        /// Scale value from server scale to device scale
        /// </summary>
        /// <param name="valueFromSrv"></param>
        /// <returns></returns>
        public decimal ScaleSrvToDev(object valueFromSrv)
        {
            decimal val = (decimal)Convert.ChangeType(valueFromSrv, typeof(decimal));

            return ((val - Min) * (DevMax - DevMin) / (Max - Min)) + DevMin;

        }            

#pragma warning restore CA1305 // Укажите IFormatProvider
        public object Clone() => new Scale
        {
            DevMax = this.DevMax,
            DevMin = this.DevMin,
            Max = this.Max,
            Min = this.Min,
        };
    }
}
