// ReSharper disable InconsistentNaming

using System;
using System.Linq;
using Machine.Specifications;
using ServiceStack.Redis;

namespace ResqueSharp.Specs
{
    [Subject(typeof(Worker))]
    [SetupForEachSpecification]
    public class WorkerTest
    {
       
        private static Worker _worker;
        static string _server;


        Establish context = () =>

                                {
                                    //server = "ec2-184-73-7-218.compute-1.amazonaws.com";
                                    _server = "localhost";

                                    Resque.SetRedis(new BasicRedisClientManager(_server));
                                    Resque.Redis().FlushAll();
                                    _worker = new Worker("jobs");
                                    //Job.create("jobs", "resque.DummyJob", 20, "/tmp");
                                };

        Cleanup TearDown = () =>
                               {
                                   Resque.Redis().FlushAll();
                                   Resque.Redis().Dispose();
                               };

        It CanFailJobs = () =>
                             {
                                 Job.Create("jobs", "resque.BadJob");
                                 _worker.Work(0);
                                 Resque.Failure.Count().ShouldEqual(1);
                             };

        It CanPeekAtFailedJobs = () =>
                                     {
                                         for (int i = 0; i < 10; i++)
                                         {
                                             Job.Create("jobs", "resque.BadJob");
                                         }
                                         _worker.Work(0);

                                        Resque.Failure.Count().ShouldEqual(10);

                                         Resque.Failure.All().Count().ShouldEqual(10);

                                     };

        It CanClearFailedJobs = () =>
                                    {
                                        Job.Create("jobs", "resque.BadJob");
                                        _worker.Work(0);
                                        Resque.Failure.Count().ShouldEqual(1);
                                        Resque.Failure.Clear();
                                        Resque.Failure.Count().ShouldEqual(0);
                                    };

        It CatchesExceptionalJobs = () =>
                                        {
                                            Job.Create("jobs", "resque.BadJob");
                                            Job.Create("jobs", "resque.BadJob");
                                            _worker.Work(0);
                                            _worker.Work(0);
                                            _worker.Work(0);

                                            Resque.Failure.Count().ShouldEqual(2);
                                        };

        It CanWorkOnMultipleQueues = () =>
                                         {
                                             Job.Create("high", "resque.GoodJob");
                                             Job.Create("critical", "resque.GoodJob");

                                             _worker = new Worker(new[] {"critical", "high"});

                                             _worker.Process();
                                             Resque.Size("high").ShouldEqual(1);
                                             Resque.Size("critical").ShouldEqual(0);

                                             _worker.Process();
                                             Resque.Size("high").ShouldEqual(0);

                                         };

        It CanWorkOnAllQueues = () =>
                                    {
                                        Job.Create("high", "resque.GoodJob");
                                        Job.Create("critical", "resque.GoodJob");
                                        Job.Create("blahblah", "resque.GoodJob");

                                        _worker = new Worker("*");

                                        _worker.Work(0);

                                        Console.WriteLine(Resque.Size("high"));
                                        Console.WriteLine(Resque.Size("critical"));
                                        Console.WriteLine(Resque.Size("blahblah"));

                                        Resque.Size("high").ShouldEqual(0);
                                        Resque.Size("critical").ShouldEqual(0);
                                        Resque.Size("blahblah").ShouldEqual(0);

                                    };

        It ProcesesAllQueuesInAlphabeticalOrder = () =>
                                                      {
                                                          Job.Create("high", "resque.GoodJob");
                                                          Job.Create("critical", "resque.GoodJob");
                                                          Job.Create("blahblah", "resque.GoodJob");

                                                          _worker = new Worker("*");

                                                          //worker.work(0, (List<String> queueList) => { int a; });


                                                      };

        It HasAUniqueId;


        It ComplainsIfNoQueuesAreGiven;


        It FailsIfAJobClassHasNoPerformMethod;



        It KnowsWhenItsWorking = () => _worker.Work(0, Job =>
                                                           {
                                                               _worker.IsWorking().ShouldBeTrue();
                                                               return true;
                                                           });

        It KnowsWhenItIsIdle = () =>
                                   {
                                       _worker.Work(0);
                                      _worker.IsIdle().ShouldBeTrue();
                                   };

        It KnowsWhoIsWorking = () => _worker.Work(0,
                                                  job =>
                                                      {
                                                          Resque.Working()[0].WorkerId().ShouldEqual(_worker.WorkerId());
                                                          return true;
                                                      });


        It KeepsTrackOfHowManyJobsItHasProcessed;


        It KeepsTrackOfHowManyFailuresItHasSeen;


        It StatsAreErasedWhenTheWorkerGoesAway;

        It KnowsWhenItIsStarted;

        It KnowsWhetherItExistsOrNot;


        It CanBeFound = () => _worker.Work(0, job =>
                                                  {
                                                      Worker.Find(_worker.WorkerId()).WorkerId().ShouldEqual(_worker.WorkerId());
                                                      return true;
                                                  });

        It InsertsItselfIntoTheWorkersListOnStartup = () => _worker.Work(0,
                                                                         job =>
                                                                             {
                                                                                 Resque.Workers()[0].WorkerId().
                                                                                     ShouldEqual(_worker.WorkerId());
                                                                                 return true;
                                                                             });

        It RemovesItselfFromTheWorkersListOnShutdown = () =>
                                                           {
                                                               _worker.Work(0,
                                                                            job =>
                                                                                {
                                                                                    Resque.Workers()[0].WorkerId().
                                                                                   ShouldEqual(_worker.WorkerId());
                                                                                    return true;
                                                                                });
                                                               Resque.Workers().Length.ShouldEqual(0);
                                                           };

        It RemovesWorkerWithStringifiedId = () => _worker.Work(0,
                                                               job =>
                                                                   {
                                                                       var workerId = Resque.Workers()[0].WorkerId();
                                                                       Resque.RemoveWorker(workerId);
                                                                       Resque.Workers().Length.ShouldEqual(0);
                                                                       return true;
                                                                   });

       It ClearsItsStatusWhenNotWorkingOnAnything;


        //[Test]
        //public void RecordsWhatItIsWorkingOn()
        //{
        //    worker.work(0,
        //        (Job job) =>
        //        {
        //            Dictionary<string, object> data = worker.job();
        //            Dictionary<string, object> payload = (Dictionary<string, object>)data["payload"];
        //           // throw new Exception(String.Join(",", data.Keys.ToArray<string>()));
        //            Assert.That(data["class"], Is.EqualTo("resque.DummyJob"));
        //            return true;
        //        });
        //}


    }
}
