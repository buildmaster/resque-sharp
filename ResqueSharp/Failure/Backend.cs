using System;
using System.Collections.Generic;
using System.Text;

namespace ResqueSharp.Failure
    {
        public abstract class Backend
        {
            public Exception Exception { get; set; }
            public Worker Worker { get; set; }
            public string Queue { get; set; }
            public object Payload { get; set; }

            protected Backend(Exception exception, Worker worker, String queue, Object payload)
            {
                Exception = exception;
                Worker = worker;
                Queue = queue;
                Payload = payload;
            }

            protected Backend()
            {
                Exception = null;
                Worker = null;
                Queue = null;
                Payload = null;
            }

            //Declaring these as abstract to force subclass to
            //implement them
            public abstract void Save();

            //FIXME: Temporarily commenting out, figure out correct keywords
            //public abstract string url();
            //public abstract void clear();
            //public virtual int count()
            //{
            //    return 0;
            //}
            /*public virtual Byte[][] all()
            {
                return new Byte[][];
            }*/
            //=======FIXME========

            public void Log(string message)
            {
                //TODO: Implement worker log function
                //worker.log(message)
            }

        }

    
}
