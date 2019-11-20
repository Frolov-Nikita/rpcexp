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

    public abstract class ServiceAbstract
    {
        private Task main;

        private CancellationTokenSource cts;

        ~ServiceAbstract() {
            cts?.Cancel();
            main.Wait(10);

            main.Dispose();
            cts.Dispose();

            cts = null;
            main = null;
        }

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

        private async Task OnServiceTask()//(Task preTask) 
        {
            await OnStarting(cts.Token).ConfigureAwait(false);
            State = ServiceState.Started;
            await ServiceTaskAsync(cts.Token).ConfigureAwait(false);
        }

        protected abstract Task ServiceTaskAsync(CancellationToken cancellationToken);

        protected virtual async Task OnStarting(CancellationToken cancellationToken)
        {
            await Task.Run(() => {
                ; 
            }).ConfigureAwait(false);
        }

        protected virtual async Task OnErrorAsync(Exception exception, CancellationToken cancellationToken)
        {
            await Task.Run(() => {
                ; 
            }).ConfigureAwait(false);
        }

        protected virtual async Task OnCompleteAsync(CancellationToken cancellationToken)
        {
            await Task.Run(() => {
                ; 
            }).ConfigureAwait(false);
        }

        public void Start()
        {
            if ((State == ServiceState.Started) || (State == ServiceState.Starting))
                return;

            State = ServiceState.Starting;
            cts = new CancellationTokenSource();

            main = OnServiceTask();

            // main.ContinueWith(OnServiceTask, TaskContinuationOptions.OnlyOnRanToCompletion);

            //main = ServiceTaskAsync(cts.Token);
            //main.ConfigureAwait(false);
#pragma warning disable CA2008 // Не создавайте задачи без передачи TaskScheduler
            main.ContinueWith(OnCompleteBaseAsync, TaskContinuationOptions.OnlyOnRanToCompletion);
            main.ContinueWith(OnErrorBaseAsync, TaskContinuationOptions.OnlyOnFaulted);
#pragma warning restore CA2008 // Не создавайте задачи без передачи TaskScheduler

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
