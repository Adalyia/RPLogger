namespace RPLogger;

/// <summary>
/// Class for storing some information about how to log a channel.
/// </summary>
internal class Channel(string name, string messageFormat, string timePrefixFormat, bool tellsChannel = false)
{
    public string Name { get; set; } = name;
    public string MessageFormat { get; set; } = messageFormat;
    public string TimePrefixFormat { get; set; } = timePrefixFormat;
    public bool TellsChannel = tellsChannel; // Whether this is a tells channel
}

