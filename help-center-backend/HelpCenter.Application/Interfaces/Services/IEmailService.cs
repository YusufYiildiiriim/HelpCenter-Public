namespace HelpCenter.Application.Interfaces;

public interface IEmailService
{
    Task SendNewRequestNotificationAsync(string toEmail, string agentName, string customerName, string requestTitle, CancellationToken cancellationToken = default);
    Task SendExpertAssignedNotificationAsync(string toEmail, string expertName, string agentName, string requestTitle, CancellationToken cancellationToken = default);
    Task SendPasswordResetCodeAsync(string toEmail, string userName, string code, CancellationToken cancellationToken = default);
}

public interface IEmailDispatcher
{
    void EnqueueAgentNotifications(string customerName, string requestTitle, IEnumerable<(string Email, string FullName)> agents);
    void EnqueueExpertNotification(string expertEmail, string expertName, string agentName, string requestTitle);
}
