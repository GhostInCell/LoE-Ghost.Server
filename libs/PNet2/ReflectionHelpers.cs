using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PNet
{
    static class ReflectionHelpers
    {
        /// <summary>
        /// Get all types that implement the specified type
        /// </summary>
        /// <param name="interfaceType"></param>
        /// <returns></returns>
        public static IEnumerable<Type> GetImplementations(Type interfaceType)
        {
            return GetImplementations(interfaceType, AppDomain.CurrentDomain.GetAssemblies());
        }

        /// <summary>
        /// Get all types that implement that specified type in the specified assemblies
        /// </summary>
        /// <param name="interfaceType"></param>
        /// <param name="assemblies"></param>
        /// <returns></returns>
        public static IEnumerable<Type> GetImplementations(Type interfaceType, IEnumerable<Assembly> assemblies)
        {
            return assemblies.SelectMany(assembly => assembly.GetExportedTypes()).Where(interfaceType.IsAssignableFrom);
        }

        /// <summary>
        /// Get all types that are marked with the specified attribute - also check implemented interfaces
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assemblies"></param>
        /// <returns></returns>
        public static IEnumerable<TypeAttrib<T>> GetTypesWithAttribute<T>(IEnumerable<Assembly> assemblies) where T : Attribute
        {
            var attribs = new List<TypeAttrib<T>>();
            foreach (var a in assemblies)
            {
                try
                {
                    foreach (var t in a.GetTypes())
                    {
                        var attributes = t.GetCustomAttributes<T>(true);
                        if (attributes != null && attributes.Length > 0)
                            attribs.Add(new TypeAttrib<T> { Type = t, Attributes = attributes });
                    }
                }
                catch (ReflectionTypeLoadException)
                {

                }
            }
            return attribs;
        }

        public static IEnumerable<Assembly> GetDependentAssemblies(Assembly analyzedAssembly)
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => GetNamesOfAssembliesReferencedBy(a)
                                    .Contains(analyzedAssembly.FullName));
        }

        public static IEnumerable<string> GetNamesOfAssembliesReferencedBy(Assembly assembly)
        {
            return assembly.GetReferencedAssemblies()
                .Select(assemblyName => assemblyName.FullName);
        }

        public class TypeAttrib<T> where T : Attribute
        {
            public Type Type;
            public IEnumerable<T> Attributes;
        }

        public static bool HasDefaultConstructor(this Type t)
        {
            return t.IsValueType || t.GetConstructor(Type.EmptyTypes) != null;
        }
    }
}