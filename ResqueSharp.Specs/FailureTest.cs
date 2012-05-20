// ReSharper disable InconsistentNaming

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Machine.Specifications;
using ServiceStack.Redis;

namespace ResqueSharp.Specs
{
    [Subject(typeof(Failure.Failure))]
    [SetupForEachSpecification]
    class FailureTest
    {
       

        private static string _server;
        private static Failure.Redis _myRedis;
        private static String _testString = "failed";
        private static Object _payload;

        Establish context = () =>
                                {
                                    // This is the IP address of my computer running Redis. 
                                    //server = "ec2-184-73-7-218.compute-1.amazonaws.com";
                                    _server = "localhost";

                                    Resque.SetRedis(new BasicRedisClientManager(new[]{_server}));
                                    Resque.Redis().FlushAll();

                                    var ex = new Exception(_testString);
                                    var worker = new Worker();
                                    String queue = _testString;
                                    _payload = Encoding.UTF8.GetBytes(_testString);

                                    _myRedis = new Failure.Redis(ex, worker, queue, _payload);
                                };


        Cleanup teardown = () =>

                               {
                                   Resque.Redis().FlushAll();
                                   Resque.Redis().Dispose();
                               };

        It CanCreateFailure = () =>
                                  {

                                      
                                      _myRedis.Exception.Message.ShouldEqual(_testString);
                                      _myRedis.Queue.ShouldEqual(_testString);

                                      Object temp = _myRedis.Payload;

                                      temp.ShouldEqual(_payload);

                                  };

        It CanGetURL = () => Resque.Failure.Url().ShouldEqual(_server);

        It CanCheckEmptyQueue = () => Resque.Failure.Count().ShouldEqual(0);

        It CanSaveOnItemToQueue = () =>
                                      {
                                          _myRedis.Save();
                                          Resque.Failure.Count().ShouldEqual(1);
                                      };

        It CanSaveRandomNumberOfItemsToQueue = () =>
                                                   {
                                                       int random = new Random().Next(5, 20);

                                                       for (int i = 0; i < random; i++)
                                                       {
                                                           _myRedis.Save();
                                                       }

                                                       Resque.Failure.Count().ShouldEqual(random);
                                                   };

        It CanClear = () =>
                          {
                              int randNumOfJobs = new Random().Next(5, 20);

                              for (int i = 0; i < randNumOfJobs; i++)
                              {
                                  _myRedis.Save();
                              }

                              Resque.Failure.Count().ShouldEqual(randNumOfJobs);
                              Resque.Failure.Clear();
                              Resque.Failure.Count().ShouldEqual(0);
                          };

        It CanRetrieveAllKeys = () =>
                                    {
                                        int randNumOfJobs = new Random().Next(5, 20);

                                        for (int i = 0; i < randNumOfJobs; i++)
                                        {
                                            _myRedis.Save();
                                        }

                                        IEnumerable<string> allKeys = Resque.Failure.All(0, randNumOfJobs);

                                       allKeys.Count().ShouldEqual(randNumOfJobs);

                                    };

    }
}
