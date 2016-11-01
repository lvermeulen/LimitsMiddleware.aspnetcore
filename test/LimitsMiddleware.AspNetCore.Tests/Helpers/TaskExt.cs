namespace LimitsMiddleware.Tests.Helpers
{
    using System;
    using System.Threading.Tasks;

    // http://stackoverflow.com/a/22786826

    public static class TaskExt
    {
        public static EapTask<TEventArgs, EventHandler<TEventArgs>> FromEvent<TEventArgs>()
        {
            var tcs = new TaskCompletionSource<TEventArgs>();
            var handler = new EventHandler<TEventArgs>((_, eventArgs) => tcs.TrySetResult(eventArgs));
            return new EapTask<TEventArgs, EventHandler<TEventArgs>>(tcs, handler);
        }
    }
}