using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BookSleeve;
using resque;
using System.Reflection;

class DummyJob
{
    public static string queue()
    {
        return "jobs";
    }
    public static void perform(params object[] args)
    {
        Console.WriteLine("This is the dummy job reporting in:{0},{1},{2}",args);
    }
}

namespace ExampleRunner
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(resque.DummyJob.assemblyQualifiedName());
            
            Type t = typeof(DummyJob);
            Assembly.GetExecutingAssembly();

            Console.WriteLine(t.AssemblyQualifiedName);
            string assemblyQualification = ", ExampleRunner, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
            Resque.setAssemblyQualifier(assemblyQualification);
            String server = "172.19.104.133";
            Resque.setRedis(new RedisConnection(server, 6379));
            Job.create("jobs", "DummyJob", "foo", 20, "bar");   
            Worker w = new Worker("*");
            w.work(1);
        }
    }
}
