using HelpCenter.Application.Interfaces;

namespace HelpCenter.Infrastructure.Services.Email;

public sealed class EmailDispatcher : IEmailDispatcher
{
    private readonly EmailQueue _queue;

    public EmailDispatcher(EmailQueue queue)
    {
        _queue = queue;
    }

    public void EnqueueAgentNotifications(string customerName, string requestTitle, IEnumerable<(string Email, string FullName)> agents)
    {
        foreach (var (email, fullName) in agents)
            _queue.Enqueue(new EmailJob(EmailJobType.NewRequest, email, fullName, requestTitle, customerName));
    }

    public void EnqueueExpertNotification(string expertEmail, string expertName, string agentName, string requestTitle)
    {
        _queue.Enqueue(new EmailJob(EmailJobType.ExpertAssigned, expertEmail, expertName, requestTitle, agentName));
    }
}
