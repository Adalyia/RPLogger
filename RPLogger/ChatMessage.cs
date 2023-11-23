using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPLogger;

/// <summary>
/// Class for storing some information about a specific chat entry.
/// </summary>
/// <param name="fullSenderName"></param>
/// <param name="senderId"></param>
/// <param name="sender"></param>
/// <param name="message"></param>
internal class ChatMessage (string fullSenderName, uint senderId, SeString sender, SeString message)
{
    public string FullSenderName { get; set; } = fullSenderName;
    public uint SenderId { get; set; } = senderId;
    public SeString Sender { get; set; } = sender;
    public SeString Message { get; set; } = message;
}

