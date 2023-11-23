namespace RPLogger;

/// <summary>
/// A class for storing information about a specific channel.
/// </summary>
/// <param name="name">The channel alias/display name</param>
/// <param name="messageFormat">The format for messages using {name} and {message} as placeholders</param>
/// <param name="timePrefixFormat">The format for the time prefix using {time} as a placeholder</param>
/// <param name="tellsChannel">Whether or not it's a tells/DM channel</param>
internal class Channel(string name, string messageFormat, string timePrefixFormat, bool tellsChannel = false)
{
    public string Name { get; set; } = name;
    public string MessageFormat { get; set; } = messageFormat;
    public string TimePrefixFormat { get; set; } = timePrefixFormat;
    public bool TellsChannel = tellsChannel; // Whether this is a tells channel
}

