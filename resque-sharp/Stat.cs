using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace resque
{
    public class Stat 
    {
        public Stat()
        {
            throw new NotImplementedException();
        }

        public static int get(String stat)
        {
            return Int32.Parse(Resque.redis().Strings.GetString(0,"resque:stat:" + stat).Result);
        }

        public static void increment(String stat, int amt)
        {
            Resque.redis().Strings.Increment(0,"resque:stat:" + stat, amt);
        }

        public static void increment(String stat)
        {
            Resque.redis().Strings.Increment(0, "resque:stat:" + stat, 1);
        }

        public static void decrement(String stat)
        {
            Resque.redis().Strings.Decrement(0, "resque:stat:" + stat);
        }

        public static void clear(String stat)
        {
            Resque.redis().Keys.Remove(0,"resque:stat:" + stat);
        }
    }
}
