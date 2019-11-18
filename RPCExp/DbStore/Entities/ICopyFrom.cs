using System;
using System.Collections.Generic;
using System.Text;

namespace RPCExp.DbStore.Entities
{
    /// <summary>
    /// Копирует все, кроме ID
    /// </summary>
    public interface ICopyFrom
    {
        void CopyFrom(object original);
    }
}
