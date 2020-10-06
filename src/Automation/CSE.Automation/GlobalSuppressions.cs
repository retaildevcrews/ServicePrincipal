// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Design", "CA1051:Do not declare visible instance fields", Justification = "Fields needed to be accessed by derived classes.", Scope = "member", Target = "CSE.Automation.Processors.DeltaProcessorBase._uniqueId")]
[assembly: SuppressMessage("Design", "CA1051:Do not declare visible instance fields", Justification = "Fields needed to be accessed by derived classes.", Scope = "member", Target = "~F:CSE.Automation.Processors.DeltaProcessorBase._configDAL")]
[assembly: SuppressMessage("Design", "CA1051:Do not declare visible instance fields", Justification = "Fields needed to be accessed by derived classes.", Scope = "member", Target = "~F:CSE.Automation.Processors.DeltaProcessorBase._config")]
[assembly: SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "This property is part of DTO.", Scope = "member", Target = "~P:CSE.Automation.Model.ProcessorConfiguration.SelectFields")]
