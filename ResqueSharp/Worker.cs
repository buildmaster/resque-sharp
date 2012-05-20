using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Net;
using System.Text.RegularExpressions;

namespace ResqueSharp
{
    public class Worker:IDisposable
    {
        readonly string[] _queues;
        public string Id { get; set; }

        public Worker(params string[] queues)
        {
            _queues = queues;
        }

        public Worker(string queue)
        {
            string[] queues = queue == "*" ? Resque.Queues() : new[] { queue };

            _queues = queues;
        }


        public void Work(int interval)
        {
            Work(interval, null);
        }

        public void Work(int interval, Func<Job,bool> block)
        {
            try
            {
                Startup();
                while (true)
                {
                    Job job = Reserve();
                    if (job != null)
                    {
                        Process(job, block);
                    }
                    else
                    {
                        if (interval == 0)
                            break;
                        System.Threading.Thread.Sleep(interval * 1000);
                    }


                }
            }
            finally
            {
                UnregisterWorker();
            }
        }

        public void UnregisterWorker()
        {
            Resque.Redis().RemoveItemFromSet("resque:workers", Id);
            Resque.Redis().Remove("resque:worker:" + WorkerId());
            Resque.Redis().Remove(StartedKey());

            Stat.Clear("processed:" + WorkerId());
            Stat.Clear("failed:" + WorkerId());
        }

        private void Process(Job job, Func<Job, bool> block)
        {
            try
            {
                SetWorkingOn(job);
                job.Perform();
            }
            catch (Exception e)
            {
                job.Fail(e.InnerException ?? e);
                SetFailed();
            }
            finally
            {
                if (block != null)
                {
                    block(job);
                }
                SetDoneWorking();
            }
        }

        public void Process()
        {
            Job job = Reserve();

            if(job != null)
            {
                Process(job, null);
            }
            
        }




        private void SetFailed()
        {
             // FIXME : do stats stuff
            Stat.Increment("failed:"+WorkerId());
            Stat.Increment("failed");
            Stat.Increment("failure");
        }

        private void SetWorkingOn(Job job)
        {
            job.Worker = this;
            string data = Resque.Encode(new Dictionary<string, object> { { "queue", job.Queue }, { "run_at", CurrentTimeFormatted() }, { "payload", job.Payload } });
            //Resque.redis().Set(new Dictionary<string, byte[]>() { { startedKey(), Encoding.UTF8.GetBytes(currentTimeFormatted()) } });
            
            Resque.Redis().Set("resque:worker:" + WorkerId(),data);
            //Resque.redis().Set("resque:worker:" + workerId(), data);
        }

        public Dictionary<string, object> Job()
        {
            return (Dictionary<string,object>)Resque.Decode(Resque.Redis().Get<string>("resque:worker:" + WorkerId()));
        }

        public Dictionary<string, object> Payload()
        {
            return (Dictionary<string,object>)Resque.Decode(Job()["payload"].ToString());
        }

        private void SetDoneWorking()
        {
            SetProcessed();
            Resque.Redis().Remove("resque:worker:" + WorkerId());
        }

        private void SetProcessed()
        {
            //FIXME
            Stat.Increment("processed:" + WorkerId());
            Stat.Increment("processed");
        }

        private void Startup()
        {
            //pruneDeadWorkers();
            RegisterWorker();
        }

        private void RegisterWorker()
        {
            Resque.Redis().AddItemToSet("resque:workers",WorkerId());
            SetStarted();
        }

        private Job Reserve()
        {
            return _queues.Select(ResqueSharp.Job.Reserve).FirstOrDefault(job => job != null);
        }

        private void SetStarted()
        {
            CurrentTimeFormatted();
            Resque.Redis().Set(StartedKey(),CurrentTimeFormatted());
        }

        private static string CurrentTimeFormatted()
        {
            DateTime currentTime = DateTime.Now;
            string currentTimeFormatted = currentTime.ToString("ddd MMM dd hh:mm:ss zzzz yyyy");
            return currentTimeFormatted;
        }

        private string StartedKey()
        {
            return "resque:worker:" + WorkerId() + ":started";
        }

        public bool IsWorking()
        {
            return State() == "working";
        }
        public bool IsIdle()
        {
            return State() == "idle";
        }

        string State()
        {
            return Resque.Redis().ContainsKey("resque:worker:" + WorkerId()) ? "working" : "idle";
        }

        public string WorkerId()
        {
            if(Id == null) {
                var sb = new StringBuilder();
                sb.Append(hostname());
                sb.Append(":");
                sb.Append(currentProcessIdAsString());
                sb.Append(":");
                sb.Append(String.Join(",", _queues));
                Id = sb.ToString();
            }
            return Id;
        }

        internal static Worker[] All()
        {
            var workers = from id in Resque.Redis().GetAllItemsFromSet("resque:workers")
                               select Find(id);

            return workers.ToArray();
        }

        public static Worker Find(string id)
        {
            if (IsExisting(id))
            {
                string[] components = id.Split(":".ToCharArray());
                string queuesPacked = components[components.Length - 1];
                string[] queues = queuesPacked.Split(",".ToCharArray());
                var worker = new Worker(queues) {Id = id};
                return worker;
            }
            return null;
        }

        public static bool IsExisting(string id)
        {
            return Resque.Redis().SetContainsItem("resque:workers", id);

            //IsMemberOfSet(0,"resque:workers", id).Result;
        }

        internal static Worker[] Working()
        {
            Worker[] workers = All();
            if (workers.Length == 0)
                return workers;
            string[] names = (from worker in workers
                             select "resque:worker:" + worker.WorkerId()).ToArray<string>();
            string[] redisValues = (from bytes in Resque.Redis().GetAll<string>(names)
                                    select bytes.Value).ToArray<string>();
            return names.Where((t, i) => !String.IsNullOrEmpty(redisValues[i])).Select(t => Find(Regex.Replace(t, "resque:worker:", ""))).ToArray();
        }

        private string hostname()
        {
            return Dns.GetHostName();
        }

        private string currentProcessIdAsString()
        {
            int processId = System.Diagnostics.Process.GetCurrentProcess().Id;
            return processId.ToString(CultureInfo.InvariantCulture);
        }
        
        public void Dispose()
        {
           UnregisterWorker();
        }
    }
}
