using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Collections;
using Newtonsoft.Json;
using ServiceStack.Redis;


namespace ResqueSharp
{
    public class NoQueueError : Exception { }

    public class NoClassError : Exception { }

    public class Resque
    {
        private static string _staticAssemblyQualifier;
        private static IRedisClientsManager _redisManager;
        private static Failure.Failure _failure;
        private static readonly object RedisLock= new object();

        public static string GetAssemblyQualifier()
        {
            return _staticAssemblyQualifier;
        }


        public static void SetAssemblyQualifier(string assemblyQualifier)
        {
            _staticAssemblyQualifier = assemblyQualifier;
        }

        public static Failure.Failure Failure { 
            get { return _failure ?? (_failure = new Failure.Failure(typeof (Failure.Redis))); }
        }
        public static void SetRedis(IRedisClientsManager redis)
        {
            _redisManager = redis;
        }
        public static IRedisClient Redis()
        {
            lock (RedisLock)
            {
                if (_redisManager == null)
                {
                    _redisManager = new PooledRedisClientManager("localhost");
                }
                return _redisManager.GetClient();
            }
        }

        public static Worker[] Working()
        {
            return Worker.Working();
        }

        public static Worker[] Workers()
        {
            return Worker.All();
        }

        public static void RemoveWorker(string workerId)
        {
            Worker.Find(workerId).UnregisterWorker();
        }

        public static bool Push(string queue, object item)
        {
         
            WatchQueue(queue);
            Redis().PushItemToList("resque:queue:" + queue, Encode(item));
            return true;
        }

        public static Dictionary<string, object> Pop(string queue)
        {

            var data = Redis().DequeueItemFromList("resque:queue:" + queue);
            if (string.IsNullOrEmpty(data))
                return null;
            return (Dictionary<string, object>)Decode(data);
        }



        public static Dictionary<string, object> Peek(string queue)
        {
            var data = Redis().GetItemFromList("resque:queue:" + queue,0);
            if (string.IsNullOrEmpty(data))
                return null;
            return (Dictionary<string, object>)Decode(data);
        }

        public static Dictionary<string, object> Peek(string queue, int start)
        {
            var resultData = Redis().GetRangeFromList("resque:queue:" + queue, start, start);
            if (resultData.Count == 0)
            {
                return null;
            }
            if (string.IsNullOrEmpty(resultData[0]))
                return null;
            return (Dictionary<string, object>)Decode(resultData[0]);
        }

        public static ArrayList Peek(string queue, int start, int count)
        {
            var results = new ArrayList();
            if (count == 1)
            {
                results.Add(Peek(queue, start));
            }
            else
            {
                foreach (string data in Redis().GetRangeFromList("resque:queue:" + queue, start, start + count - 1))
                {
                    results.Add(Decode(data));
                }
            }
            return results;
        }

        public static void RemoveQueue(string queue)
        {
            Redis().RemoveItemFromSet("resque:queues", queue);
            Redis().Remove("resque:queue:" + queue);
        }

        private static void WatchQueue(string queue)
        {
            Redis().AddItemToSet("resque:queues", queue);
        }

        public static string[] Queues()
        {
            var rawResults = Redis().GetAllItemsFromSet("resque:queues");
            if (rawResults.Count == 0)
                return new string[0];
            var results = new string[rawResults.Count];
            int i = 0;
            foreach(string data in rawResults)
            {
                results[i] = data;
                i++;
            }
            return results;
        }



        public static Job Reserve(string queue)
        {
            return Job.Reserve(queue);
        }



        public static int Size(string queue)
        {
            return Redis().Lists["resque:queue:" + queue].Count;
        }

        public static string[] Keys()
        {
            return Redis().SearchKeys("resque:*").Select(k=>k.Remove(0,7)).ToArray();
        }

        public static bool Enqueue(string className, params object[] args)
        {
            Type workerType = Type.GetType(className);
            if (workerType != null)
            {
                System.Reflection.MethodInfo methodInfo = workerType.GetMethod("Queue", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.FlattenHierarchy);
                if(methodInfo == null)
                    throw new NoQueueError();
                var queue = (string)methodInfo.Invoke(null, null);
                if (String.IsNullOrEmpty(queue))
                    throw new NoQueueError();
                return Job.Create(queue, className, args);
            }
            return false;
        }



        #region encoding
        public static string Encode(object item)
        {
            return JsonConvert.SerializeObject(item);
        }

        public static object Decode(string json)
        {
            return JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
        }
        public static object Decode(byte[] json)
        {
            return Decode(Encoding.UTF8.GetString(json));
        }

        #endregion





    }

    public class Info
    {
        /*
              :pending => queues.inject(0) { |m,k| m + size(k) },
              DONE :processed => Stat[:processed],
              DONE :queues => queues.size,
              DONE :workers => workers.size.to_i,
              DONE :working => working.size,
              DONE :failed => Stat[:failed],
              DONE :servers => [redis.server]
         */
        public static int Workers
        {
            get { return Resque.Workers().Length; }
        }

        public static int Processed
        {
            get { return Stat.Get("processed"); }
        }

        public static int Failed
        {
            get { return Stat.Get("failed"); }
        }

        public static string Servers
        {
            get { return Resque.Redis().Host; }
        }

        public static int Queues
        {
            get { return Resque.Queues().Length; }
        }

        public static int Working
        {
            get { return Resque.Working().Length; }
        }

        public static string[] Pending
        {
            get { return Resque.Queues(); }
        }

    }
}
