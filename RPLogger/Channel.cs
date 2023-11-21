namespace RPLogger;

/// <summary>
/// Class for storing some information about how to log a channel.
/// </summary>
internal class Channel
{
    public string Name { get; set; } = ""; // Channel name
    public string MessageFormat { get; set; } = "{0}: {1}"; // String format for messages
    public string TimePrefixFormat { get; set; } = "{0} "; // String format for time prefix
    public bool TellsChannel = false; // Whether this is a tells channel

    public Channel(string name, string messageFormat, string timePrefixFormat, bool tellsChannel = false)
    {
        Name = name;
        MessageFormat = messageFormat;
        TimePrefixFormat = timePrefixFormat;
        TellsChannel = tellsChannel;
    }
}

