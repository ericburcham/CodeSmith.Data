using System;

using CodeSmith.Data.Collections;

namespace CodeSmith.Data.Rules
{
    /// <summary>
    ///     A collection of rules.
    /// </summary>
    public class RuleCollection : ConcurrentDictionary<Type, RuleList>
    {
    }
}
