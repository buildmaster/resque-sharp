using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
class ErrorJob
{
    public static string queue()
    {
        return "jobs";
    }
    public static void perform(params object[] args)
    {
        throw new Exception("I failed at my job");
    }
}
class LongJob
{
    public static string queue()
    {
        return "jobs";
    }
    public static void perform(params object[] args)
    {
        Thread.Sleep(10000);
        Console.WriteLine("This is the long job reporting in");
    }
}
namespace ExampleRunner
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(resque.DummyJob.assemblyQualifiedName());

            Type t = typeof (DummyJob);
            Assembly.GetExecutingAssembly();

            Console.WriteLine(t.AssemblyQualifiedName);
            string assemblyQualification = ", ExampleRunner, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
            Resque.setAssemblyQualifier(assemblyQualification);
            String server = "172.19.104.133";
            Resque.setRedis(new RedisConnection(server, 6379));
         
            var w = new Worker("jobs");
            var thread = new Thread(() =>
                                   {
                                      
                                       w.work(1);
                                   });
            thread.Start();
            Job.create("jobs", "LongJob");
            Job.create("jobs", "DummyJob", "foo", 20, "bar");

            Job.create("jobs", "ErrorJob");

            System.Console.ReadLine();
            w.Dispose();
            thread.Abort();
            

        }
    }
}
