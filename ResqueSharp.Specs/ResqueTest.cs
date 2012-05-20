// ReSharper disable InconsistentNaming

using System.Collections.Generic;
using System.Collections;
using Machine.Specifications;
using ServiceStack.Redis;

namespace ResqueSharp.Specs
{

    [Subject(typeof (Resque))]
    [SetupForEachSpecification]
    public class BaseResqueTest
    {
        
        Establish context = () =>
                                {
                                    //String server = "ec2-184-73-7-218.compute-1.amazonaws.com";

                                    const string server = "localhost";
                                    Resque.SetRedis(new BasicRedisClientManager(server));
                                    Resque.Redis().FlushAll();
                                    Resque.Push("people", new Dictionary<string, string> {{"name", "chris"}});
                                    Resque.Push("people", new Dictionary<string, string> {{"name", "bob"}});
                                    Resque.Push("people", new Dictionary<string, string> {{"name", "mark"}});
                                };

        Cleanup TearDown = () =>
                               {
                                   Resque.Redis().FlushAll();
                                   Resque.Redis().Dispose();
                               };
    }

    public class ResqueSpecs : BaseResqueTest
    {
        It CanPutJobsOnAQueue = () =>
                                    {
                                        Job.Create("jobs", "DummyJob", 20, "/tmp").ShouldBeTrue();
                                        Job.Create("jobs", "DummyJob", 20, "/tmp").ShouldBeTrue();
                                    };
    
        It CanGrabJobsOffAQueue = () =>
                                      {
                                          //Job.create("jobs", "dummy-job", 20, "/tmp"); FIXME NEED TO DEAL WITH THIS
                                          Job.Create("jobs", "resque.DummyJob", 20, "/tmp");
                                          Job job = Resque.Reserve("jobs");
                                          job.PayloadClass().FullName.ShouldEqual("resque.DummyJob");
                                          job.Args()[0].ShouldEqual((long)20);
                                          job.Args()[1].ShouldEqual("/tmp");
                                      };
   
        It CanReQueueJobs = () =>
                                {
                                    Job.Create("jobs", "resque.DummyJob", 20, "/tmp");
                                    Job job = Resque.Reserve("jobs");
                                    job.recreate();
                                    Resque.Reserve("jobs").ShouldEqual(job);
                                };
  
        It CanAskResqueForQueueSize = () =>
                                          {
                                              Resque.Size("a_queue").ShouldEqual(0);
                                              Job.Create("a_queue", "resque.DummyJob", 1, "asdf");
                                              Resque.Size("a_queue").ShouldEqual(1);
                                          };
   

        It CanPutJobsOnTheQueueByAskingWhichQueueTheyAreInterestedIn = () =>
                                                                           {
                                                                               Resque.Size("tester").ShouldEqual(0);
                                                                               Resque.Enqueue("resque.DummyJob", 20, "/tmp").
                                                                                   ShouldBeTrue();
                                                                               Resque.Enqueue("resque.DummyJob", 20,
                                                                                              "/tmp").ShouldBeTrue();
                                                                               Job job = Resque.Reserve("tester");
                                                                               job.Args()[0].ShouldEqual((long)20);
                                                                               job.Args()[1].ShouldEqual("/tmp");
                                                                           };
   
        It CanTestForEquality = () =>
                                    {
                                        Job.Create("jobs", "resque.DummyJob", 20, "/tmp").ShouldBeTrue();
                                        Job.Create("jobs", "resque.DummyJob", 20, "/tmp").ShouldBeTrue();
                                        //Assert.IsTrue(Job.create("jobs", "dummy-job", 20, "/tmp"));  NEED TO  MAKE THIS WORK
                                        Resque.Reserve("jobs").ShouldEqual(Resque.Reserve("jobs"));

                                        Job.Create("jobs", "resque.NotDummyJob", 20, "/tmp");
                                        Job.Create("jobs", "resque.DummyJob", 20, "/tmp");
                                        Resque.Reserve("jobs").ShouldNotEqual(Resque.Reserve("jobs"));

                                        Job.Create("jobs", "resque.DummyJob", 20, "/tmp").ShouldBeTrue();
                                        Job.Create("jobs", "resque.DummyJob", 30, "/tmp").ShouldBeTrue();
                                        Resque.Reserve("jobs").ShouldNotEqual(Resque.Reserve("jobs"));

                                    };
   
        It QueueMustBeInferrable = () => Catch.Exception(() => Resque.Enqueue("resque.UninferrableInvalidJob", 123)).
                                             ShouldBeOfType<NoQueueError>();
   

        It CanPutItemsOnAQueue = () =>
                                     {
                                         var person = new Dictionary<string, string> {{"name", "chris"}};
                                         Resque.Push("people", person).ShouldBeTrue();
                                     };
    
        It CanPullItemsOffAQueue = () =>
                                       {
                                           Resque.Pop("people")["name"].ShouldEqual("chris");
                                           Resque.Pop("people")["name"].ShouldEqual("bob");
                                           Resque.Pop("people")["name"].ShouldEqual("mark");
                                           Resque.Pop("people").ShouldBeNull();
                                       };
   
        It KnowsHowBigAQueueIs = () =>
                                     {
                                         Resque.Size("people").ShouldEqual(3);
                                         Resque.Pop("people")["name"].ShouldEqual("chris");
                                         Resque.Size("people").ShouldEqual(2);
                                         Resque.Pop("people");
                                         Resque.Pop("people");
                                         Resque.Size("people").ShouldEqual(0);
                                     };
  
        It CanPeekAtAQueue = () =>
                                 {
                                     Resque.Peek("people")["name"].ShouldEqual("chris");
                                     Resque.Size("people").ShouldEqual(3);
                                 };
    
        It CanPeekAtMultipleItemsOnQueue = () =>
                                               {
                                                   ArrayList result = Resque.Peek("people", 1, 1);
                                                   ((Dictionary<string, object>) result[0])["name"].ShouldEqual("bob");

                                                   result = Resque.Peek("people", 1, 2);
                                                   ((Dictionary<string, object>) result[0])["name"].ShouldEqual("bob");
                                                   ((Dictionary<string, object>) result[1])["name"].ShouldEqual("mark");

                                                   result = Resque.Peek("people", 0, 2);
                                                   ((Dictionary<string, object>) result[0])["name"].ShouldEqual("chris");
                                                   ((Dictionary<string, object>) result[1])["name"].ShouldEqual("bob");

                                                   result = Resque.Peek("people", 2, 1);
                                                   ((Dictionary<string, object>) result[0])["name"].ShouldEqual("mark");
                                                   Resque.Peek("people", 3).ShouldBeNull();
                                               };
  
        It KnowsWhatQueuesItIsManaging = () =>
                                             {
                                                 Resque.Queues().ShouldContainOnly("people");
                                                 Resque.Push("cars", new Dictionary<string, string> {{"make", "bmw"}});
                                                 Resque.Queues().ShouldContainOnly("cars", "people");
                                             };
    
        It QueuesAreAlwaysAList = () =>
                                      {
                                          Resque.Redis().FlushAll();
                                          Resque.Queues().ShouldEqual(new string[0]);
                                      };
    
        It CanDeleteAQueue = () =>
                                 {
                                     Resque.Push("cars", new Dictionary<string, string> {{"make", "bmw"}});
                                     Resque.Queues().ShouldEqual(new[] {"cars", "people"});
                                     Resque.RemoveQueue("people");
                                     Resque.Queues().ShouldEqual(new[] {"cars"});
                                 };
   
        It KeepsTrackOfResqueKeys = () => Resque.Keys().ShouldEqual(new[] {"queue:people", "queues"});
    
        It BadlyWantsAClassName = () => Catch.Exception(() => Job.Create("jobs", null)).ShouldBeOfType<NoClassError>();
    
        It KeepsStats = () => Job.Create("jobs", "resque.DummyJob", 20, "/tmp");
    
        It AlwaysReturnsSomeKindOfFailureWhenAsked = () => Resque.Failure.ShouldNotBeNull();
    }





}
