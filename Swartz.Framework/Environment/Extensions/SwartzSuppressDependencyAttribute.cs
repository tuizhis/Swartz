using System;

namespace Swartz.Environment.Extensions
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class SwartzSuppressDependencyAttribute : Attribute
    {
        public SwartzSuppressDependencyAttribute(string fullName)
        {
            FullName = fullName;
        }

        public string FullName { get; set; }
    }
}