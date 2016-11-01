namespace LimitsMiddleware.Tests.Helpers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    // http://stackoverflow.com/a/22786826

    public sealed class EapTask<TEventArgs, TEventHandler>
        where TEventHandler: class
    {
        private readonly TaskCompletionSource<TEventArgs> _completionSource;
        private readonly TEventHandler _eventHandler;

        public EapTask(TaskCompletionSource<TEventArgs> completionSource, TEventHandler eventHandler)
        {
            _completionSource = completionSource;
            _eventHandler = eventHandler;
        }

        public Task<TEventArgs> Start(Action<TEventHandler> subscribe, Action<TEventHandler> unsubscribe) => Start(subscribe, unsubscribe, CancellationToken.None);

        public async Task<TEventArgs> Start(Action<TEventHandler> subscribe, Action<TEventHandler> unsubscribe, CancellationToken cancellationToken)
        {
            subscribe(_eventHandler);
            try
            {
                using (cancellationToken.Register(() => _completionSource.SetCanceled()))
                {
                    return await _completionSource.Task;
                }
            }
            finally
            {
                unsubscribe(_eventHandler);
            }
        }
    }
}