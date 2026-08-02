using System;
using System.Collections.Generic;
using System.Text;

namespace CoreMcp.Framework.Tools;

public interface IToolRegistry
{
    IReadOnlyCollection<IToolAdapter> GetAll();

    bool TryGetTool(string name, out IToolAdapter tool);

    IToolAdapter GetRequiredTool(string name);
}
