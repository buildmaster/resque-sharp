using System;
using System.Threading;
using ServiceStack.Redis;
using resque;
using System.Reflection;


// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedParameter.Global
class DummyJob
{
    public static string Queue()
    {
        return "jobs";
    }

    public static void Perform(params object[] args)

    {
        Console.WriteLine("This is the dummy job reporting in:{0},{1},{2}",args);
    }
}
class ErrorJob
{

    public static string Queue()
    {
        return "jobs";
    }


    public static void Perform(params object[] args)
    {
        throw new Exception("I failed at my job");
    }
}
class LongJob
{

    public static string Queue()
    {
        return "jobs";
    }

    public static void Perform(params object[] args)
    {
        Thread.Sleep(10000);
        Console.WriteLine("This is the long job reporting in");
    }
}
// ReSharper restore UnusedMember.Global
// ReSharper restore UnusedParameter.Global
namespace ExampleRunner
{
    static class Program
    {
        static void Main()
        {
            Console.WriteLine(resque.DummyJob.AssemblyQualifiedName());

            Type t = typeof (DummyJob);
            Assembly.GetExecutingAssembly();

            Console.WriteLine(t.AssemblyQualifiedName);
            const string assemblyQualification = ", ExampleRunner, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
            Resque.SetAssemblyQualifier(assemblyQualification);
            const string server = "xrowlgdv-rd01.dev.xero.com:80";
            Resque.SetRedis(new BasicRedisClientManager(server));
         
            //var w = new Worker("jobs");
            //var thread = new Thread(() =>
            //                       {
                                      
            //                           w.work(1);
            //                       });
            //thread.Start();
            Job.Create("jobs", "LongJob");
            Job.Create("jobs", "DummyJob", "foo", 20, "bar");

            Job.Create("jobs", "ErrorJob");

            Console.ReadLine();
            //w.Dispose();
            //thread.Abort();
            

        }
    }
}
