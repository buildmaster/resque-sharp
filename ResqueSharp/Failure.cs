using System;
using System.Collections.Generic;
using System.Reflection;

namespace ResqueSharp
{
    namespace Failure
    {
        public class Failure
        {
            private readonly Type _backendType;

            public Failure(Type backendType)
            {
                _backendType = backendType;
            }

            public int Count()
            {
                return (int)InvokeOnBackend("Count");
            }

            //TODO: Make a version of this method for paginating results
            public IEnumerable<string> All()
            {
                return (IEnumerable<string>)InvokeOnBackend("All");
            }

            public IEnumerable<string> All(int start, int end)
            {
                var param = new object[] { start, end };
                return (IEnumerable<string>)InvokeOnBackend("All", param);
            }

            public string Url()
            {
                return (string)InvokeOnBackend("Url");  
            }

            public void Clear()
            {
                InvokeOnBackend("Clear");
            }

            object InvokeOnBackend(string methodName, params object[] args) 
            {
                var argTypes = new Type[args.Length];

                for(int i = 0; i < args.Length; i++)
                {
                    argTypes[i] = args[i].GetType();
                }
  
                MethodInfo methodInfo = _backendType.GetMethod(methodName, argTypes);
                if (methodInfo == null)
                    throw new NotImplementedException();
                return methodInfo.Invoke(null, args);

            }


        }
    }
}
