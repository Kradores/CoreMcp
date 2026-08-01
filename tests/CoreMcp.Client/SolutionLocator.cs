using System;
using System.Collections.Generic;
using System.Text;

namespace CoreMcp.Client;

public static class SolutionLocator
{
    public static DirectoryInfo FindSolutionRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);

        while (dir != null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "CoreMcp.slnx")))
                return dir;

            dir = dir.Parent;
        }

        throw new InvalidOperationException(
            "Could not locate solution root.");
    }
}
