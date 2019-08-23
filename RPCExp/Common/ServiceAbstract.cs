using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RPCExp.Common
{
    public enum ServiceState
    {
        Stopped,
        Stopping,
        Fault,
        Started,
        Starting,
    }

    public abstract class ServiceAbstract : IServiceAbstract
    {
        private Task main;

        private CancellationTokenSource cts;

        public ServiceState State { get; private set; }

        private async Task OnErrorBaseAsync(Task preTask)
        {
            State = ServiceState.Fault;
            await OnErrorAsync(preTask.Exception, cts.Token).ConfigureAwait(false);
        }

        private async Task OnCompleteBaseAsync(Task preTask)
        {
            State = ServiceState.Stopped;
            await OnCompleteAsync(cts.Token).ConfigureAwait(false);
        }

        protected abstract Task ServiceTaskAsync(CancellationToken cancellationToken);

        protected virtual async Task OnErrorAsync(Exception exception, CancellationToken cancellationToken)
        {
            await Task.Run(() => {; });
        }

        protected virtual async Task OnCompleteAsync(CancellationToken cancellationToken)
        {
            await Task.Run(() => {; });
        }

        public void Start()
        {
            if ((State == ServiceState.Started) || (State == ServiceState.Starting))
                return;

            State = ServiceState.Starting;
            cts = new CancellationTokenSource();

            main = ServiceTaskAsync(cts.Token);

            main.ConfigureAwait(false);

            main.ContinueWith(OnCompleteBaseAsync, TaskContinuationOptions.OnlyOnRanToCompletion);
            main.ContinueWith(OnErrorBaseAsync, TaskContinuationOptions.OnlyOnFaulted);

            State = ServiceState.Started;
        }

        public void Stop()
        {
            if ((State == ServiceState.Started) || (State == ServiceState.Starting))
                return;

            State = ServiceState.Stopping;
            cts.Cancel();

            State = ServiceState.Stopped;
        }

    }
}
