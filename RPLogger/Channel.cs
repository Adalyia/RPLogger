namespace RPLogger;

/// <summary>
/// Class for storing some information about how to log a channel.
/// </summary>
internal class Channel
{
    public string Name { get; set; } = ""; // Channel name
    public string MessageFormat { get; set; } = "{0}: {1}"; // Format for messages
    public bool TellsChannel = false; // Whether this is a tells channel

    public Channel(string name, string messageFormat, bool tellsChannel = false)
    {
        Name = name;
        MessageFormat = messageFormat;
        TellsChannel = tellsChannel;
    }
}

