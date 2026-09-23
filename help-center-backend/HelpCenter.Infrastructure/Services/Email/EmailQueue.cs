using System.Threading.Channels;

namespace HelpCenter.Infrastructure.Services.Email;

public enum EmailJobType { NewRequest, ExpertAssigned }

public record EmailJob(
    EmailJobType Type,
    string ToEmail,
    string RecipientName,
    string RequestTitle,
    string SenderName = ""
);

public sealed class EmailQueue
{
    private readonly Channel<EmailJob> _channel = Channel.CreateUnbounded<EmailJob>(
        new UnboundedChannelOptions { SingleReader = true }
    );

    public ChannelReader<EmailJob> Reader => _channel.Reader;

    public void Enqueue(EmailJob job) => _channel.Writer.TryWrite(job);
}
