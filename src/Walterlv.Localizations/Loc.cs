using System;
using System.CodeDom.Compiler;

namespace Walterlv.Localizations
{
    [GeneratedCode("Walterlv.Localizations", "0.1.0")]
    public sealed class Loc : LocNode
    {
        public static readonly (LocNode Name, LocNode Version) App
            = ("Test", "0.1.0");

        public static readonly (
            LocNode Window,
            LocNode Dialog,
            LocNode Message,
            LocNode Title,
            LocNode User,
            LocNode Environment,
            LocNode Network,
            LocNode Content) Tip = ("", "", "", "", "", "", "", "");
    }
}
