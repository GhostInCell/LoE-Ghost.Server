using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
#if LIDGREN
using Lidgren.Network;
#endif

namespace PNet
{
    public static class Utilities
    {
        public static bool TryParseGuid(string str, out Guid guid)
        {
            if (str.Length != 32)
            {
                guid = new Guid();
                return false;
            }

            if (!Regex.IsMatch(str, "^[A-Fa-f0-9]{32}$|"))
            {
                guid = new Guid();
                return false;
            }

            try
            {
                guid = new Guid(str);
                return true;
            }
            catch (Exception)
            {
                guid = new Guid();
                return false;
            }
        }

        public static T[] RemoveAt<T>(this T[] source, int index)
        {
            T[] dest = new T[source.Length - 1];
            if (index > 0)
                Array.Copy(source, 0, dest, 0, index);

            if (index < source.Length - 1)
                Array.Copy(source, index + 1, dest, index, source.Length - index - 1);

            return dest;
        }

#if LIDGREN
        /// <summary>
        /// the current network time
        /// </summary>
        public static double Now { get { return NetTime.Now; } }
#else
        public static double Now { get { throw new NotImplementedException(); } }
#endif
    }

    public static class AttributeExtensions
    {
        /// <summary>Searches and returns attributes. The inheritance chain is not used to find the attributes.</summary>
        /// <typeparam name="T">The type of attribute to search for.</typeparam>
        /// <param name="type">The type which is searched for the attributes.</param>
        /// <returns>Returns all attributes.</returns>
        public static T[] GetCustomAttributes<T>(this Type type) where T : Attribute
        {
            return GetCustomAttributes(type, typeof(T), false).Select(arg => (T)arg).ToArray();
        }

        /// <summary>Searches and returns attributes.</summary>
        /// <typeparam name="T">The type of attribute to search for.</typeparam>
        /// <param name="type">The type which is searched for the attributes.</param>
        /// <param name="inherit">Specifies whether to search this member's inheritance chain to find the attributes. Interfaces will be searched, too.</param>
        /// <returns>Returns all attributes.</returns>
        public static T[] GetCustomAttributes<T>(this Type type, bool inherit) where T : Attribute
        {
            return GetCustomAttributes(type, typeof(T), inherit).Select(arg => (T)arg).ToArray();
        }

        /// <summary>Private helper for searching attributes.</summary>
        /// <param name="type">The type which is searched for the attribute.</param>
        /// <param name="attributeType">The type of attribute to search for.</param>
        /// <param name="inherit">Specifies whether to search this member's inheritance chain to find the attribute. Interfaces will be searched, too.</param>
        /// <returns>An array that contains all the custom attributes, or an array with zero elements if no attributes are defined.</returns>
        private static object[] GetCustomAttributes(Type type, Type attributeType, bool inherit)
        {
            if (!inherit)
            {
                return type.GetCustomAttributes(attributeType, false);
            }

            var attributeCollection = new Collection<object>();
            var baseType = type;

            do
            {
                baseType.GetCustomAttributes(attributeType, true).Apply(attributeCollection.Add);
                baseType = baseType.BaseType;
            }
            while (baseType != null);

            foreach (var interfaceType in type.GetInterfaces())
            {
                GetCustomAttributes(interfaceType, attributeType, true).Apply(attributeCollection.Add);
            }

            var attributeArray = new object[attributeCollection.Count];
            attributeCollection.CopyTo(attributeArray, 0);
            return attributeArray;
        }

        /// <summary>Applies a function to every element of the list.</summary>
        private static void Apply<T>(this IEnumerable<T> enumerable, Action<T> function)
        {
            foreach (var item in enumerable)
            {
                function.Invoke(item);
            }
        }
    }
}
