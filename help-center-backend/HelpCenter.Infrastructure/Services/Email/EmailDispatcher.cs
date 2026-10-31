using HelpCenter.Application.Interfaces;

namespace HelpCenter.Infrastructure.Services.Email;

public sealed class EmailDispatcher : IEmailDispatcher
{
    private readonly EmailQueue _queue;

    public EmailDispatcher(EmailQueue queue)
    {
        _queue = queue;
    }

    public void EnqueueAgentNotifications(string customerName, string requestTitle, IEnumerable<(string Email, string FullName, string? Locale)> agents)
    {
        foreach (var (email, fullName, locale) in agents)
            _queue.Enqueue(new EmailJob(EmailJobType.NewRequest, email, fullName, requestTitle, customerName,
                HelpCenter.Domain.Common.SupportedLocales.Normalize(locale) ?? HelpCenter.Domain.Common.SupportedLocales.Default));
    }

    public void EnqueueExpertNotification(string expertEmail, string expertName, string agentName, string requestTitle, string? locale = null)
    {
        _queue.Enqueue(new EmailJob(EmailJobType.ExpertAssigned, expertEmail, expertName, requestTitle, agentName,
            HelpCenter.Domain.Common.SupportedLocales.Normalize(locale) ?? HelpCenter.Domain.Common.SupportedLocales.Default));
    }
}
