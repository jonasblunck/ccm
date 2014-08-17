// Guids.cs
// MUST match guids.h
using System;

namespace JonasBlunck.ccmPack
{
    static class GuidList
    {
        public const string guidccmPackPkgString = "8dac8ba1-1cfe-4c5b-859c-c856180fe38a";
        public const string guidccmPackCmdSetString = "89532dfc-5c12-40a7-b3c1-bda094310496";
        public const string guidToolWindowPersistanceString = "7ab41d80-5e2c-46f8-8f6e-1a9d73285d7f";

        public static readonly Guid guidccmPackCmdSet = new Guid(guidccmPackCmdSetString);
    };
}