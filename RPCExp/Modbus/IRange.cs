using System;
using System.Collections.Generic;
using System.Text;

namespace RPCExp.Modbus
{
    public interface IRange
    { //TODO: Make it SortedSet! & place all logic from MTagsGroup
        int Begin { get; }
        int Length { get; }
        int End { get; }
    }
}
