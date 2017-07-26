

using System;
using System.Linq;
using System.Reflection;

// ReSharper disable once CheckNamespace
namespace Redux
{
    public static class Assignable 
    {

        public static  T Assign<T>(this T source, string path, object value) where T : new()
        {
            return (T) AssignRecursion(source, path.Split('.'), value);
        }

        public static T Assign<T>(this T source, string[] paths, object[] values) where T : new()
        {
            return (T)AssignRecursion(source, paths, values, null);
        }

        /// <summary>
        /// creates shallow copy of the input source and replaces only the properties that are on the provided path. 
        /// This ensures that the input is not mutated and its properties are either copied or replaced if mutated
        /// </summary>
        /// <param name="source">the source object</param>
        /// <param name="path">the path to the mutated</param>
        /// <param name="value">the new value for the mutated property</param>
        /// <returns></returns>
        private static object AssignRecursion(object source, string[] path, object value)
        {
            if (path.Length == 0) return value;

            var sourceType = source.GetType();
            object destination = Activator.CreateInstance(sourceType);

            foreach (
                var property in
                sourceType.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance))
            {
                object propertyValue = property.GetValue(source);
                if (property.Name.Equals(path[0]))
                {
                    propertyValue = AssignRecursion(propertyValue, path.Skip(1).ToArray(), value);
                }
                property.SetValue(destination, propertyValue);
            }
            return destination;
        }
        
        private static object AssignRecursion(object source, string[] paths, object[] values, string currentPath)
        {
            int ix = Array.IndexOf(paths, currentPath);
            if (ix >= 0) return values[ix];

            var sourceType = source.GetType();
            object destination = Activator.CreateInstance(sourceType);

            foreach (
                var property in
                sourceType.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance))
            {
                object propertyValue = property.GetValue(source);
                var propertyPath = currentPath == null ? property.Name : currentPath + "." + property.Name;                
                if (paths.Any(_=>_.StartsWith(propertyPath)))
                {
                    propertyValue = AssignRecursion(propertyValue, paths, values, propertyPath);
                }
                property.SetValue(destination, propertyValue);
                
            }
            return destination;
        }
        
    }
}
