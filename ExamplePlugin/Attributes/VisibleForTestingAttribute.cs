using System;

namespace ExamplePlugin.Attributes
{
    /// <summary>
    /// Marker attribute to call out something is visible for testing only
    /// </summary>
    [AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Property |
                    AttributeTargets.Field)]
    public class VisibleForTestingAttribute : Attribute
    {
    }
}
