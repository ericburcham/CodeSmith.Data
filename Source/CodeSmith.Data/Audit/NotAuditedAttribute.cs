﻿using System;

namespace CodeSmith.Data.Audit
{
    /// <summary>
    ///     Indicates that a field in an audited class should not be included in the audit log.
    /// </summary>
    /// <remarks>
    ///     Use the NotAuditedAttribute attribute to prevent a field from being included in the audit.
    /// </remarks>
    /// <seealso cref="AuditAttribute" />
    /// <seealso cref="AlwaysAuditAttribute" />
    /// <seealso cref="AuditManager" />
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class NotAuditedAttribute : Attribute
    {
    }
}
