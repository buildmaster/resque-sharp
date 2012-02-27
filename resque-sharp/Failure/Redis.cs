using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using resque;

namespace resque.Failure
{
    public class Redis : Backend
    {
        public Redis(Exception exception, Worker worker, String queue, Object payload)
        {
            this.exception = exception;
            this.worker = worker;
            this.queue = queue;
            this.payload = payload;

        }

        public override void save()
        {
            Dictionary<string, object> data = new Dictionary<string,object>();

            data.Add("failed_at", System.DateTime.UtcNow.ToString("yyyy/MM/dd HH:mm:ss"));
            data.Add("payload", payload);
            data.Add("error", exception.Message);
            data.Add("exception",exception.Message);
            data.Add("backtrace", exception.StackTrace.Split('\n'));

            data.Add("worker", worker.workerId());
            data.Add("queue", queue);

            Resque.redis().Lists.AddLast(0,"resque:failed", Resque.encode(data));
        }

        public static int count()
        {
            return (int)Resque.redis().Lists.GetLength(0,"resque:failed").Result;
        }

        public static Byte[][] all(int start, int end)
        {
            return Resque.redis().Lists.Range(0,"resque:failed", start, end).Result;
        }

        public static Byte[][] all()
        {
            return Resque.redis().Lists.Range(0,"resque:failed", 0, (int) Resque.redis().Lists.GetLength(0,"resque:failed").Result).Result;
        }

        public static string url()
        {
            return Resque.redis().Host;
        }

        //TODO: Redo this to delete the resque:failure queue from the redis object
        public static void clear()
        {
            Resque.redis().Keys.Remove(0,"resque:failed");
        }
    }
}
