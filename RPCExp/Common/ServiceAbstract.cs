using System;
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

        ~ServiceAbstract()
        {
            System.Diagnostics.Trace.WriteLine(GetType().Name + " Destructing");
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
            System.Diagnostics.Trace.Fail(GetType().Name + " FAIL: " + preTask.Exception.InnerMessage());

            State = ServiceState.Fault;
            await OnErrorAsync(preTask.Exception, cts.Token).ConfigureAwait(false);
        }

        private async Task OnCompleteBaseAsync(Task preTask)
        {
            var ctsState = cts.Token.IsCancellationRequested ? "canceled " : "not canceled";
            System.Diagnostics.Trace.WriteLine(GetType().Name + $" Complete. Cts:{ctsState} State:{preTask.Status}");
            State = ServiceState.Stopped;
            await OnCompleteAsync(cts.Token).ConfigureAwait(false);
        }

        private async Task OnServiceTask()//(Task preTask) 
        {
            System.Diagnostics.Trace.WriteLine(GetType().Name + " Starting");

            await OnStarting(cts.Token).ConfigureAwait(false);
            State = ServiceState.Started;
            await ServiceTaskAsync(cts.Token).ConfigureAwait(false);
        }

        protected abstract Task ServiceTaskAsync(CancellationToken cancellationToken);

        protected virtual async Task OnStarting(CancellationToken cancellationToken)
        {
            await Task.Run(() =>
            {
                ;
            }).ConfigureAwait(false);
        }

        protected virtual async Task OnErrorAsync(Exception exception, CancellationToken cancellationToken)
        {
            await Task.Run(() =>
            {
                ;
            }).ConfigureAwait(false);
        }

        protected virtual async Task OnCompleteAsync(CancellationToken cancellationToken)
        {
            await Task.Run(() =>
            {
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

            System.Diagnostics.Trace.WriteLine(GetType().Name + " Started");
            State = ServiceState.Started;
        }

        public void Stop()
        {
            if ((State == ServiceState.Started) || (State == ServiceState.Starting))
                return;

            System.Diagnostics.Trace.WriteLine(GetType().Name + " Stopping normal");

            State = ServiceState.Stopping;
            cts.Cancel();

            State = ServiceState.Stopped;
            System.Diagnostics.Trace.WriteLine(GetType().Name + " Stopped");
        }

    }
}
