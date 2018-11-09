using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Caasiope.Node
{
    public class InjectedAttribute : Attribute { }

    public static class Injector
    {
        private static readonly Dictionary<Type, object> Injecteds = new Dictionary<Type, object>();

        public static T Inject<T>(T target)
        {
            Debug.Assert(Injecteds.Any(), "Please call this on Initialization!");

            foreach (var field in target.GetType().GetFields())
            {
                var attribute = Attribute.GetCustomAttribute(field, typeof(InjectedAttribute));
                if (attribute != null)
                {
                    object injected;
                    if(Injecteds.TryGetValue(field.FieldType, out injected))
                    {
                        field.SetValue(target, injected);
                    }
                }
            }
            return target;
        }

        public static T Add<T>(T injected)
        {
            Injecteds.Add(typeof(T), injected);
            return injected;
        }

        public static void Clear()
        {
            Injecteds.Clear();
        }
    }
}