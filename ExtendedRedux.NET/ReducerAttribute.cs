

using System;

namespace Redux
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ReducerAttribute: Attribute
    {
        internal string[] Path { get; private set; }

        public ReducerAttribute(string path)
        {
            Path = path.Split(new[] {'.'}, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
