using Dalamud.Game.Text.SeStringHandling;
using System;

namespace RPLogger;

/// <summary>
/// Class for storing some information about a specific chat entry.
/// </summary>
/// <param name="fullSenderName">Full corrected name of the message sender</param>
/// <param name="sender">Sender</param>
/// <param name="message">Message</param>
/// <param name="timestamp">Message timestamp</param>
internal class ChatMessage (string fullSenderName, SeString sender, SeString message, DateTimeOffset timestamp)
{
    public string FullSenderName { get; set; } = fullSenderName;
    public SeString Sender { get; set; } = sender;
    public SeString Message { get; set; } = message;
    public DateTimeOffset Timestamp { get; set; } = timestamp;
}

