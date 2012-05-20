using System;
using System.Collections.Generic;

namespace ResqueSharp.Failure
{
    public class Redis : Backend
    {
        public Redis(Exception exception, Worker worker, String queue, Object payload)
        {
            Exception = exception;
            Worker = worker;
            Queue = queue;
            Payload = payload;

        }

        public override void Save()
        {
            var data = new Dictionary<string,object>
                           {
                               {"failed_at", DateTime.UtcNow.ToString("yyyy/MM/dd HH:mm:ss")},
                               {"payload", Payload},
                               {"error", Exception.Message},
                               {"exception", Exception.Message}
                           };

            if (Exception.StackTrace != null)
            {
                data.Add("backtrace", Exception.StackTrace.Split('\n'));
            }
            else
            {
                data.Add("backtrace","");
            }

            data.Add("worker", Worker.WorkerId());
            data.Add("queue", Queue);

            Resque.Redis().PushItemToList("resque:failed", Resque.Encode(data));
        }

        public static int Count()
        {
            return Resque.Redis().Lists["resque:failed"].Count;
        }

        public static IEnumerable<string> All(int start, int end)
        {
            return Resque.Redis().GetRangeFromList("resque:failed", start, end);
        }

        public static IEnumerable<string> All()
        {
            return Resque.Redis().GetAllItemsFromList("resque:failed");
        }

        public static string Url()
        {
            return Resque.Redis().Host;
        }

        //TODO: Redo this to delete the resque:failure queue from the redis object
        public static void Clear()
        {
            Resque.Redis().Remove("resque:failed");
        }
    }
}
