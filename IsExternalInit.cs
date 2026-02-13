// ReSharper disable once CheckNamespace
namespace System.Runtime.CompilerServices
{
    /**
     * This is required to fix a compiler failure to use the record keyword with netstandard2.0
     * while using a C# version that supports records.
     */
    public class IsExternalInit { }
}