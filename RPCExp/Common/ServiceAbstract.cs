using System;
using System.Threading;
using System.Threading.Tasks;

namespace RPCExp.Common
{
    /// <summary>
    /// State of the service
    /// </summary>
    public enum ServiceState
    {
        Stopped,
        Stopping,
        Fault,
        Started,
        Starting,
    }

    /// <summary>
    /// <para>
    /// Base class for all services in this project.
    /// Provide service state. Trace messages when state changing.
    /// </para>
    /// Steps to run service:
    /// <list type="number">
    /// <item>Create object</item>
    /// <item>SettingUp created object</item>
    /// <item>Fire method Start() and service will start starting in separate task:)</item>
    /// <item>when you need to stop it call method Stop() and service will start stopping</item>
    /// </list>
    /// </summary>
    public abstract class ServiceAbstract
    {
        private Task main;

        private CancellationTokenSource cts;

        /// <summary>
        /// Stopping service (hard way).
        /// Disposes all resources.
        /// Delete this object from memory
        /// </summary>
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

        /// <summary>
        /// State of the service
        /// </summary>
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

        /// <summary>
        /// Base method for implementation service logic
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected abstract Task ServiceTaskAsync(CancellationToken cancellationToken);

        /// <summary>
        /// This method can be override to make preparation steps before starting
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected virtual async Task OnStarting(CancellationToken cancellationToken)
        {
            await Task.Run(() =>
            {
                ;
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// This method can be override to handle exceptions thrown by service logic in ServiceTaskAsync()
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected virtual async Task OnErrorAsync(Exception exception, CancellationToken cancellationToken)
        {
            await Task.Run(() =>
            {
                ;
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// This method can be override to make some logic when service stopped. Save results, cleanup, etc..
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected virtual async Task OnCompleteAsync(CancellationToken cancellationToken)
        {
            await Task.Run(() =>
            {
                ;
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Non blocking method to start starting service in separate task.
        /// </summary>
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

        /// <summary>
        /// Non blocking method to start stopping service normally.
        /// </summary>
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
