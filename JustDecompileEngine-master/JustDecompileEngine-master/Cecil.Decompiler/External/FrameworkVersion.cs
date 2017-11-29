﻿using JustDecompile.SmartAssembly.Attributes;

namespace Telerik.JustDecompiler.External
{
    [DoNotPrune]
    [DoNotObfuscateType]
    public enum FrameworkVersion
    {
        Unknown,
        v1_0,
        v1_1,
        v2_0,
        v3_0,
        v3_5,
        v4_0,
        v4_5,
        v4_5_1,
        v4_5_2,
        v4_6,
        v4_6_1,
        v4_6_2,
        v4_7,
        WinRT,
        Silverlight,
        WindowsPhone,
        WindowsCE,
        NetPortableV4_6,
        NetCoreV4_5,
        NetCoreV4_5_1,
        NetCoreV5_0
    }
}