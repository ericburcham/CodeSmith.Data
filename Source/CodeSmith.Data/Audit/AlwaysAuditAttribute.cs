﻿using System;

namespace CodeSmith.Data.Audit
{
    /// <summary>
    ///     Indicates that a field in an audited class should always be included in the audit data even if it hasn't been
    ///     changed.
    /// </summary>
    /// <seealso cref="AuditAttribute" />
    /// <seealso cref="NotAuditedAttribute" />
    /// <seealso cref="AuditManager" />
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class AlwaysAuditAttribute : Attribute
    {
    }
}
