using Kehlet.Generators.Attributes;

namespace Generator;

[LoadAdditionalFiles(MemberNameSuffix = "Source", RegexFilter = @"\.cs$")]
public static partial class StaticContentModule;
