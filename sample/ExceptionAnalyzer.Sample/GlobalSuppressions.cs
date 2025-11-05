// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Major Code Smell", "S112:General or reserved exceptions should never be thrown", Justification = "<Pending>", Scope = "member", Target = "~M:ExceptionAnalyzer.Sample.EX002.Method")]
[assembly: SuppressMessage("Usage", "CA2200:Rethrow to preserve stack details", Justification = "<Pending>", Scope = "member", Target = "~M:ExceptionAnalyzer.Sample.EX004.Method")]
[assembly: SuppressMessage("Major Code Smell", "S3445:Exceptions should not be explicitly rethrown", Justification = "<Pending>", Scope = "member", Target = "~M:ExceptionAnalyzer.Sample.EX004.Method")]
[assembly: SuppressMessage("Style", "IDE0059:Unnecessary assignment of a value", Justification = "<Pending>", Scope = "member", Target = "~M:ExceptionAnalyzer.Sample.EX005.Method")]
[assembly: SuppressMessage("Minor Code Smell", "S2737:\"catch\" clauses should do more than rethrow", Justification = "<Pending>", Scope = "member", Target = "~M:ExceptionAnalyzer.Sample.EX007.Method")]
[assembly: SuppressMessage("Major Code Smell", "S108:Nested blocks of code should not be left empty", Justification = "<Pending>", Scope = "member", Target = "~M:ExceptionAnalyzer.Sample.EX009.Method")]
[assembly: SuppressMessage("Major Code Smell", "S1144:Unused private types or members should be removed", Justification = "<Pending>", Scope = "type", Target = "~T:ExceptionAnalyzer.Sample.EX020.CustomException")]
[assembly: SuppressMessage("Critical Code Smell", "S3871:Exception types should be \"public\"", Justification = "<Pending>", Scope = "type", Target = "~T:ExceptionAnalyzer.Sample.EX020.CustomException")]
[assembly: SuppressMessage("Critical Code Smell", "S3871:Exception types should be \"public\"", Justification = "<Pending>", Scope = "type", Target = "~T:ExceptionAnalyzer.Sample.EX021.MyCustomException")]
[assembly: SuppressMessage("Critical Code Smell", "S3871:Exception types should be \"public\"", Justification = "<Pending>", Scope = "type", Target = "~T:ExceptionAnalyzer.Sample.EX022.MyCustomException")]
[assembly: SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>", Scope = "member", Target = "~M:ExceptionAnalyzer.Sample.EX022.MyCustomException.#ctor(System.String)")]
[assembly: SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>", Scope = "member", Target = "~M:ExceptionAnalyzer.Sample.EX022.MyCustomException.#ctor(System.String,System.Exception)")]
[assembly: SuppressMessage("Minor Code Smell", "S3376:Attribute, EventArgs, and Exception type names should end with the type being extended", Justification = "<Pending>", Scope = "type", Target = "~T:ExceptionAnalyzer.Sample.EX023.MyCustomError")]
[assembly: SuppressMessage("Critical Code Smell", "S3871:Exception types should be \"public\"", Justification = "<Pending>", Scope = "type", Target = "~T:ExceptionAnalyzer.Sample.EX023.MyCustomError")]
[assembly: SuppressMessage("Major Code Smell", "S108:Nested blocks of code should not be left empty", Justification = "<Pending>", Scope = "member", Target = "~M:ExceptionAnalyzer.Sample.EX011.Method")]
