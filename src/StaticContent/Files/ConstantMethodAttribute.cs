using System;

namespace Kehlet.Generators.ConstantMethodGenerator;

/// <summary>
/// Annotate partial method.
/// </summary>
/// <param name="impl">Implementation. Must be a method in the same type.</param>
[AttributeUsage(AttributeTargets.Method)]
internal class ConstantMethodAttribute(string impl, params object[] args) : Attribute
{
    
}
