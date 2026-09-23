namespace HelpCenter.Application.Interfaces;

/// <summary>
/// Hides the "marking Conversation messages as read" operation from the Application
/// layer — the Include chain (Messages.SenderParticipant) and saving (SaveChanges) live in a
/// single place here. The handler just says "mark the messages in this conversation as read" and
/// does not care about HOW it is done (EF Include/ThenInclude, tracking).
/// </summary>
public interface IConversationReadService
{
    /// <summary>
    /// Marks unread messages in the specified conversation as read, where the reader is the other
    /// side, and saves. The return value is the number of messages marked (0 means the conversation
    /// does not exist or there were no messages to mark).
    /// </summary>
    Task<int> MarkMessagesAsReadAsync(int conversationId, bool readerIsAgent, CancellationToken cancellationToken = default);
}
