using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RPCExp.Common;

namespace RPCExp.AlarmLogger
{
    public class AlarmLogger : ServiceAbstract
    {
        protected override Task ServiceTaskAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
