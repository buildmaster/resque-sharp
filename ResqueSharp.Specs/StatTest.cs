// ReSharper disable InconsistentNaming

using System;
using Machine.Specifications;
using ServiceStack.Redis;

namespace ResqueSharp.Specs
{
    [Subject(typeof(Stat))]
    [SetupForEachSpecification]
    class StatTest
    {
       

        private static string _server;
        private Object _payload;


        Establish context = () =>

                                {
                                    // This is the IP address of my computer running Redis. 
                                    //server = "ec2-184-73-7-218.compute-1.amazonaws.com";
                                    _server = "localhost";

                                    Resque.SetRedis(new BasicRedisClientManager(_server));
                                    Resque.Redis().FlushAll();
                                };

        Cleanup TearDown = () =>
                               {
                                   Resque.Redis().FlushAll();
                                   Resque.Redis().Dispose();
                               };

        It canCreateAStat = () =>
                                {
                                    Stat.Increment("fakeStat");
                                    int statRetrieveValue = Stat.Get("fakeStat");
                                    statRetrieveValue.ShouldEqual(1);
                                };


        It canCreateAndCreateAndIncrementStat = () =>
                                                    {
                                                        var rand = new Random(DateTime.Now.Second);
                                                        int statExpectValue = rand.Next(5, 20);

                                                        for (int i = 0; i < statExpectValue; i++)
                                                        {
                                                            Stat.Increment("fakeStat");
                                                        }

                                                        int statRetrieveValue = Stat.Get("fakeStat");
                                                        statRetrieveValue.ShouldEqual(statExpectValue);
                                                    };

        It canCreateStatGreaterThanOne = () =>
                                             {
                                                 var rand = new Random(DateTime.Now.Second);
                                                 int statExpectValue = rand.Next(5, 20);

                                                 Stat.Increment("fakeStat", statExpectValue);

                                                 int statRetrieveValue = Stat.Get("fakeStat");
                                                 statRetrieveValue.ShouldEqual(statExpectValue);
                                             };

        It canDecrementAStat = () =>
                                   {
                                       var rand = new Random(DateTime.Now.Second);
                                       int statExpectValue = rand.Next(5, 20);

                                       for (int i = 0; i < statExpectValue; i++)
                                       {
                                           Stat.Increment("fakeStat");
                                       }

                                       Stat.Decrement("fakeStat");
                                       int statRetrieveValue = Stat.Get("fakeStat");
                                       statRetrieveValue.ShouldEqual(statExpectValue-1);
                                   };

        It canClearStats = () =>
                               {
                                   var rand = new Random(DateTime.Now.Second);
                                   int statExpectValue = rand.Next(5, 20);

                                   for (int i = 0; i < statExpectValue; i++)
                                   {
                                       Stat.Increment("fakeStat");
                                   }

                                   int statRetrieveValue = Stat.Get("fakeStat");
                                   statRetrieveValue.ShouldEqual(statExpectValue);

                                   Stat.Clear("fakeStat");

                                   Stat.Increment("fakeStat");

                                   statRetrieveValue = Stat.Get("fakeStat");
                                   statRetrieveValue.ShouldEqual(1);
                               };


    }
}