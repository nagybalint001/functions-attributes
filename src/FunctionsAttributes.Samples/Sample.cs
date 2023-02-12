using FunctionsAttributes;
using FunctionsAttributes.Samples;

using Microsoft.Azure.Functions.Worker;

using System;

// here we provide the attributes to listen to:
[assembly: GenerateFunctionsWith(typeof(CustomAttribute))]
[assembly: GenerateFunctionsWith(typeof(OtherCustomAttribute))]
namespace FunctionsAttributes.Samples
{
    public class MyFunctions
    {
        [Function("MyFunction")]
        [Custom]
        [OtherCustom]
        public void MyFunctionMethod()
        {
        }

        [Function(nameof(MyOtherFunction))]
        [OtherCustom]
        public void MyOtherFunction()
        {
        }

        // this does not have a Function attribute, so this is skipped
        [Custom]
        public void ThisIsSkipped()
        {
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class CustomAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class OtherCustomAttribute : Attribute
    {
    }

    public class SomeOtherPlace
    {
        public void MyMethod()
        {
            GenerateFunctionsWithAttribute.OtherCustomAttribute.Contains("MyFunction"); // true
            GenerateFunctionsWithAttribute.CustomAttribute.Contains("MyOtherFunction"); // false
        }
    }
}
