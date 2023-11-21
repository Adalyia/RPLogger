using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPLogger;

internal class Channel
{
    public string Name { get; set; } = "";
    public string MessageFormat { get; set; } = "{0}: {1}";
    public bool TellsChannel = false;

    public Channel(string name, string messageFormat, bool tellsChannel = false)
    {
        this.Name = name;
        this.MessageFormat = messageFormat;
        this.TellsChannel = tellsChannel;
    }
}

