using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ResqueSharp
{
    public class Stat 
    {
        public Stat()
        {
            throw new NotImplementedException();
        }

        public static int Get(String stat)
        {
            return Resque.Redis().Get<int>("resque:stat:" + stat);
        }

        public static void Increment(String stat, int amt)
        {
            Resque.Redis().Increment("resque:stat:" + stat, (uint) amt);
        }

        public static void Increment(String stat)
        {
            Resque.Redis().Increment("resque:stat:" + stat, 1);
        }

        public static void Decrement(String stat)
        {
            Resque.Redis().Decrement("resque:stat:" + stat,1);
        }

        public static void Clear(String stat)
        {
            Resque.Redis().Remove("resque:stat:" + stat);
        }
    }
}
