using Kehlet.Generators.LoadAdditionalFiles;

namespace Generator;

[LoadAdditionalFiles(MemberNameSuffix = "Source", RegexFilter = @"\.cs$")]
public static partial class StaticContentModule;
