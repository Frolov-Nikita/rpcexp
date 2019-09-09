using System;
using ModbusBasic.IO;

namespace RPCExp.Connections
{
    public interface IConnectionSource: INameDescription, IValidatable, IConfigString
    {
        bool IsOpen { get; }
     
        IStreamResource Get();

        bool TryOpen();
    }
}