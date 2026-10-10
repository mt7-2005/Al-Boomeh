using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_BoomehServices.Consumers
{
    public static class ReadAttempts
    {
        public static long GetFailedAttempts(IReadOnlyBasicProperties props,string queueName)
        {
            if (props.Headers is null ||
                !props.Headers.TryGetValue("x-death", out var raw) ||
                raw is not List<object> deaths)
            {
                return 0;
            }

            foreach (var item in deaths)
            {
                if (item is not Dictionary<string, object> death) continue;

                var queue = AsString(death.GetValueOrDefault("queue"));
                var reason = AsString(death.GetValueOrDefault("reason"));

                if (queue == queueName && reason == "rejected")
                {
                    return death.TryGetValue("count", out var count) ? Convert.ToInt64(count) : 0;
                }
            }

            return 0;
        }

        private static string? AsString(object? value) => value switch
        {
            byte[] bytes => Encoding.UTF8.GetString(bytes),
            string s => s,
            _ => null
        };
    }
}
