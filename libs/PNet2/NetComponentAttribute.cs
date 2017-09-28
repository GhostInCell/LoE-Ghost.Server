using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace PNet
{
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class, AllowMultiple = false)]
    public class NetComponentAttribute : Attribute
    {
        public readonly byte Id;

        public NetComponentAttribute(byte id)
        {
            Id = id;
        }
    }

    public static class NetComponentHelper
    {
        private static readonly Dictionary<Type, byte> NetComponentToId = new Dictionary<Type, byte>();

        /// <summary>
        /// Find all types that are marked with NetComponentAttribute
        /// </summary>
        internal static void FindNetComponents()
        {
            //just get the assemblies dependent on the attribute assembly, as that's the only thing that makes sense for stuff to be using the attribute
            var dependentAssemblies = ReflectionHelpers.GetDependentAssemblies(typeof (NetComponentAttribute).Assembly);
            //find all types
            foreach (var tattr in ReflectionHelpers.GetTypesWithAttribute<NetComponentAttribute>(dependentAssemblies))
            {
                //should always be one, as the method returns at least one, and the attribute is set to not allow multiple.
                NetComponentToId[tattr.Type] = tattr.Attributes.First().Id;
            }
        }

        /// <summary>
        /// Get the network component id from the specified type if it's marked with <see cref="NetComponentAttribute"/>.
        /// This will work even for implemented interfaces that are marked with it.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="id">the NetComponentAttribute id</param>
        /// <returns>true, if the type id was found</returns>
        public static bool GetNetId(this Type type, out byte id)
        {
            return NetComponentToId.TryGetValue(type, out id);
        }

        /// <summary>
        /// creates a delegate that invokes the specified method info on the specified target
        /// </summary>
        /// <param name="mi"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static Delegate ToDelegate(MethodInfo mi, object target)
        {
            Type delegateType;

            var typeArgs = mi.GetParameters()
                .Select(p => p.ParameterType)
                .ToList();

            // builds a delegate type
            if (mi.ReturnType == typeof(void))
            {
                delegateType = Expression.GetActionType(typeArgs.ToArray());

            }
            else
            {
                typeArgs.Add(mi.ReturnType);
                delegateType = Expression.GetFuncType(typeArgs.ToArray());
            }

            // creates a binded delegate if target is supplied
            var result = Delegate.CreateDelegate(delegateType, target, mi);

            return result;
        }
    }
}