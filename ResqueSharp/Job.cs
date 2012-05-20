using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.Collections;


namespace ResqueSharp
{


    public class Job
    {
        public Dictionary<string, object> Payload { get; set; }
        public string Queue { get; set; }
        public Worker Worker{get; set;}

        public Job()
        {
            throw new NotImplementedException();
        }

        Job(string queue, Dictionary<string, object> payload)
        {
            Queue = queue;
            Payload = payload;
        }

        public Type PayloadClass()
        {
            var className = (string)Payload["class"];
            if (Resque.GetAssemblyQualifier() != null)
            {
                className = className + Resque.GetAssemblyQualifier();
            }

            return Type.GetType(className, true);
            //return Type.GetType("GoodJob", true);
        }

        public static bool Create(string queue, string className, params object[] args)
        {
            if (String.IsNullOrEmpty(className))
            {
                throw new NoClassError();
            }
            Resque.Push(queue, new Dictionary<String, Object>(){{"class", className}, {"args", args}});
            return true;
        }

        internal static Job Reserve(string queue)
        {
            Dictionary<string,object> payload = Resque.Pop(queue);
            if (payload == null)
                return null;
            return new Job(queue, payload);

        }

        internal void Perform()
        {

            System.Reflection.MethodInfo methodInfo = PayloadClass().GetMethod("Perform", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.FlattenHierarchy);
            if (methodInfo == null)
                throw new NotImplementedException("Jobs must have a Perform static method");
            object[] parameters = new object[]{Args().ToArray()};
            methodInfo.Invoke(null, parameters);

        }

        public ArrayList Args()
        {
            var list = new ArrayList();
            var args = (JArray)Payload["args"];
                foreach (JValue o in args)
                {
                    list.Add(o.Value);
                }
               return list;
            }



        public void recreate()
        {
            Job.Create(Queue, PayloadClass().FullName, Args().ToArray());
        }
        public override bool Equals(object other)
        {
            if (other == null)
                return false;
            if (ReferenceEquals(this, other))
                return true;
            if (GetType() != other.GetType())
                return false;
            Job job = (Job)other;
            return (this.Queue == job.Queue && this.PayloadClass() == job.PayloadClass() && arrayListElementsAreEqual(Args(), job.Args()));
        }

        private bool arrayListElementsAreEqual(ArrayList list, ArrayList otherList)
        {
            if (list.Count != otherList.Count)
            {
                return false;
            }
            int i = 0;
            foreach (object o in list)
            {
                if (!o.Equals(otherList[i]))
                {
                    return false;
                }
                i++;
            }
            return true;
        }

        internal void Fail(Exception e)
        {
            var failure = new Failure.Redis(e, Worker, Queue, Payload);
            failure.Save();
        }
    }


    public class DummyJob
    {
        public static string Queue()
        {
            return "tester";
        }
        public static void Perform(params object[] args)
        {
            Console.WriteLine("This is the dummy job reporting in");
        }

        public static string AssemblyQualifiedName()
        {
            return System.Reflection.Assembly.GetExecutingAssembly().FullName;
        }
        //public DummyJob(string queue, Dictionary<string,object> dictionary) : base(queue, dictionary)
        //{
          
        //}
        // for testing
    }
    public class NotDummyJob
    {
        public static string Queue()
        {
            return "tester";
        }
        public static void Perform(params object[] args)
        {
            Console.WriteLine("This is the not dummy job reporting in");
        }
    }

    public class BadJob
    {
        public static string Queue()
        {
            return "tester";
        }
        public static void Perform(params object[] args)
        {
            throw new Exception("Bad Job!!");
        }
    }

    public class GoodJob
    {
        public static string Queue()
        {
            return "tester";
        }
        public static void Perform(params object[] args)
        {
            System.Threading.Thread.Sleep(1000);
        }

        public static string AssemblyQualifiedName()
        {
            return System.Reflection.Assembly.GetExecutingAssembly().FullName;
        }
    }

    public class WindowsJobs
    {
        public static string Queue()
        {
            return "WindowsJobs";
        }
        public static void Perform(params object[] args)
        {
            System.Threading.Thread.Sleep(1000);
        }
    }


    public class UninferrableInvalidJob
    {
    }

}
