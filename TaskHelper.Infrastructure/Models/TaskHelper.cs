using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TaskHelper.Infrastructure.Models
{
    public class RecurringTaskHelper
    {
        private static Dictionary<string, CancellationTokenSource> _cancelationTokenSources = new Dictionary<string, CancellationTokenSource>();

        public static void StopRecurringTask(string cancelationTokenKey)
        {
            if (_cancelationTokenSources.TryGetValue(cancelationTokenKey, out CancellationTokenSource cts))
            {
                cts.Cancel();
                _cancelationTokenSources.Remove(cancelationTokenKey);
            }
        }

        public static bool RecurringTaskExists(string cancelationTokenKey)
        {
            return _cancelationTokenSources.TryGetValue(cancelationTokenKey, out CancellationTokenSource cts);
        }

        public static void StartRecurringTask(Func<Task> funcTask, int seconds, string cancelationTokenKey)
        {
            if (funcTask == null || RecurringTaskExists(cancelationTokenKey)) return;

            var token = new CancellationTokenSource();
            _cancelationTokenSources.Add(cancelationTokenKey, token);

            Task.Run(async () => {
                while (!token.IsCancellationRequested)
                {
                    await funcTask();
                    await Task.Delay(TimeSpan.FromSeconds(seconds), token.Token);
                }
            }, token.Token);
        }

    }
}
