# FunctionsAttributes

[![license](https://img.shields.io/github/license/nagybalint001/functions-attributes.svg?maxAge=2592000)](https://github.com/nagybalint001/functions-attributes/blob/main/LICENSE) [![NuGet](https://img.shields.io/nuget/v/FunctionsAttributes.svg?maxAge=2592000)](https://www.nuget.org/packages/FunctionsAttributes/) ![downloads](https://img.shields.io/nuget/dt/FunctionsAttributes)

Generating constants from specified Azure Functions attributes. This can be handy if we want to create middlewares that can act differently based on attribute usage on a function. Like starting a DB transaction for non-readonly functions.

Sample code:

```csharp
// here we provide the attributes to listen to:
[assembly: FunctionsAttributes.GenerateFunctionsWith(typeof(CustomAttribute))]
[assembly: FunctionsAttributes.GenerateFunctionsWith(typeof(OtherCustomAttribute))]
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
```

Generated file should look like this:

```csharp
using System.Collections.ObjectModel;

namespace FunctionsAttributes
{
    public partial class GenerateFunctionsWithAttribute
    {
        public static readonly ReadOnlyCollection<string> CustomAttribute = new ReadOnlyCollection<string>(new[]
        {
            "MyFunction",
        });
        public static readonly ReadOnlyCollection<string> OtherCustomAttribute = new ReadOnlyCollection<string>(new[]
        {
            "MyFunction",
            "MyOtherFunction",
        });
    }
}
```