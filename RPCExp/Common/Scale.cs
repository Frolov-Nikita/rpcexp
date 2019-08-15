using System;

namespace RPCExp.Common
{
    public class Scale: ICloneable
    {
        public decimal DevMin { get; set; } = -32768;

        public decimal DevMax { get; set; } = 32767;

        public decimal Min { get; set; } = -32768;

        public decimal Max { get; set; } = 32767;

        public decimal ScaleDevToSrv<T>(object valueFromDev) =>
            ((((decimal)valueFromDev - DevMin) * (Max - Min) / (DevMax - DevMin)) + Min);
        
        public decimal ScaleSrvToDev(object valueFromSrv) =>
            ((((decimal)valueFromSrv - Min) * (DevMax - DevMin) / (Max - Min)) + DevMin);

        public object Clone() => new Scale
        {
            DevMax = this.DevMax,
            DevMin = this.DevMin,
            Max = this.Max,
            Min = this.Min,
        };
    }
}
