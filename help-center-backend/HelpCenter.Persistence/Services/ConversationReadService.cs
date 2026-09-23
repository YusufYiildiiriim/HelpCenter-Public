using HelpCenter.Application.Interfaces;
using HelpCenter.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace HelpCenter.Persistence.Services;

public class ConversationReadService : IConversationReadService
{
    private readonly EfContext _efContext;

    public ConversationReadService(EfContext efContext)
    {
        _efContext = efContext;
    }

    public async Task<int> MarkMessagesAsReadAsync(int conversationId, bool readerIsAgent, CancellationToken cancellationToken = default)
    {
        var conversation = await _efContext.Conversations
            .Include(c => c.Messages)
            .ThenInclude(m => m.SenderParticipant)
            .FirstOrDefaultAsync(c => c.Id == conversationId, cancellationToken);

        if (conversation == null) return 0;

        var markedCount = conversation.MarkMessagesAsRead(readerIsAgent);

        if (markedCount > 0)
        {
            await _efContext.SaveChangesAsync(cancellationToken);
        }

        return markedCount;
    }
}
