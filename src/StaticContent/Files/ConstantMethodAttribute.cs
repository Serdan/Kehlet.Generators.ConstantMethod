using System;

namespace Kehlet.Generators.ConstantMethod;

/// <summary>
/// Annotate partial method.
/// </summary>
/// <param name="impl">Implementation. Must be a method in the same type.</param>
[AttributeUsage(AttributeTargets.Method)]
#pragma warning disable CS9113 // Parameter is unread.
internal class ConstantMethodAttribute(string impl, params object[] args) : Attribute;
#pragma warning restore CS9113 // Parameter is unread.
