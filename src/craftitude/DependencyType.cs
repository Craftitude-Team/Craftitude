using System;

namespace Craftitude
{
    [Flags]
    public enum DependencyType : byte
    {
        Suggestion = 1,
        Prerequirement = 2,
        Requirement = 4,
        Incompatibility = 8,
        Inclusion = 16
    }
}