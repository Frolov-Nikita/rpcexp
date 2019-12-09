using System;

namespace RPCExp.Common
{
    /// <summary>
    /// Designed to scale numerical values
    /// </summary>
    public class Scale: INameDescription
    {
        /// <inheritdoc/>
        public string Name { get; set; }
        
        /// <inheritdoc/>
        public string Description { get; set; }

        /// <summary>
        /// Begin of the Scale inside device (source)
        /// </summary>
        public decimal DevMin { get; set; } = -32768;

        /// <summary>
        /// End of the Scale inside device (source)
        /// </summary>
        public decimal DevMax { get; set; } = 32767;
        
        /// <summary>
        /// Begin of the Scale inside this server (destination)
        /// </summary>
        public decimal Min { get; set; } = -32768;

        /// <summary>
        /// End of the Scale inside this server (destination)
        /// </summary>
        public decimal Max { get; set; } = 32767;

        /// <summary>
        /// Engineering units
        /// </summary>
        public string Units { get; set; }

#pragma warning disable CA1305 // Укажите IFormatProvider

        /// <summary>
        /// Scale value from device scale to server scale
        /// </summary>
        /// <param name="valueFromDev"></param>
        /// <returns></returns>
        public decimal ScaleDevToSrv(object valueFromDev)
        {
            if (valueFromDev is null)
                return 0M;

            decimal val = (decimal)Convert.ChangeType(valueFromDev, typeof(decimal));
            //TODO: ускорить за счет предварительно вычисленных коэф-тов 
            return ((val - DevMin) * (Max - Min) / (DevMax - DevMin)) + Min;
        }

        /// <summary>
        /// Scale value from server scale to device scale
        /// </summary>
        /// <param name="valueFromSrv"></param>
        /// <returns></returns>
        public decimal ScaleSrvToDev(object valueFromSrv)
        {
            if (valueFromSrv is null)
                return 0M;

            decimal val = (decimal)Convert.ChangeType(valueFromSrv, typeof(decimal));
            //TODO: ускорить за счет предварительно вычисленных коэф-тов
            return ((val - Min) * (DevMax - DevMin) / (Max - Min)) + DevMin;
        }

#pragma warning restore CA1305 // Укажите IFormatProvider

    }
}
