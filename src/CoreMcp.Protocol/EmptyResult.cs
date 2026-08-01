using System;
using System.Collections.Generic;
using System.Text;

namespace CoreMcp.Protocol;

public sealed class EmptyResult : IMcpResult
{
    public static readonly EmptyResult Instance = new();

    private EmptyResult()
    {
    }
}
