using System;
using System.Collections.Generic;
using System.Text;

namespace RPCExp.Store.Entities
{
    /// <summary>
    /// Копирует все, кроме ID
    /// </summary>
    public interface ICopyFrom
    {
        void CopyFrom(object original);
    }
}
