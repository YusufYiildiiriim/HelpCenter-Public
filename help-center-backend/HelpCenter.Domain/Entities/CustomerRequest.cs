using HelpCenter.Domain.Constants;
using HelpCenter.Domain.Enums;
using HelpCenter.Domain.Events;

namespace HelpCenter.Domain.Entities
{
    public class CustomerRequest : BaseEntity
    {
        public string TicketId { get; set; } = string.Empty;
        public int CustomerId { get; set; }
        public virtual Customer Customer { get; set; } = null!;
        public string Title { get; set; } = string.Empty;
        public int? RequestSubjectId { get; set; }
        public virtual RequestSubject? RequestSubject { get; set; }
        public int? ModuleId { get; set; }
        public virtual Module? Module { get; set; }
        public string Description { get; set; } = string.Empty;

        public RequestPriority Priority { get; set; } = RequestPriority.Low;
        public int? AssignedUserId { get; set; }
        public virtual User? AssignedUser { get; set; }

        public int? CurrentExpertId { get; set; }
        public virtual User? CurrentExpert { get; set; }

        public int? ConversationId { get; set; }
        public virtual Conversation? Conversation { get; set; }

        public virtual ICollection<CustomerRequestMessageDocument> Documents { get; set; } = new HashSet<CustomerRequestMessageDocument>();
        public virtual ICollection<CustomerRequestEvaluation> Actions { get; set; } = new HashSet<CustomerRequestEvaluation>();
        public virtual ICollection<RequestHistory> Histories { get; set; } = new HashSet<RequestHistory>();

        public int StatusId { get; set; }
        public virtual CustomerRequestStatus Status { get; set; } = null!;

        public static CustomerRequest Create(int customerId, int? moduleId, int? requestSubjectId, string title, RequestPriority priority, string customerFullName = "")
        {
            var request = new CustomerRequest
            {
                CustomerId = customerId,
                ModuleId = moduleId,
                RequestSubjectId = requestSubjectId,
                Title = title,
                Priority = priority,
                TicketId = "TKT-" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper()
            };
            request.SetStatus(RequestStatusConstants.WaitingForReply);
            request.RaiseDomainEvent(new RequestCreatedEvent(customerId, customerFullName, title));
            return request;
        }

        // Rich Domain Logic
        public void AssignAgent(int agentId, int actorAccountId, string? note = null)
        {
            string? oldAgent = AssignedUserId?.ToString();
            AssignedUserId = agentId;
            UpdatedAt = DateTime.UtcNow;

            AddHistory(actorAccountId, RequestActionType.Assigned, oldAgent, agentId.ToString(), note);
        }

        public void AssignExpert(int expertId, int actorAccountId, string expertEmail, string expertFullName, string agentFullName, string? note = null)
        {
            string? oldExpert = CurrentExpertId?.ToString();
            CurrentExpertId = expertId;
            int oldStatus = StatusId;
            StatusId = RequestStatusConstants.UnderTechnicalReview;
            UpdatedAt = DateTime.UtcNow;

            AddHistory(actorAccountId, RequestActionType.Assigned, oldExpert, expertId.ToString(), "Uzman Atandı");
            if (oldStatus != StatusId)
            {
                AddHistory(actorAccountId, RequestActionType.StatusChanged, oldStatus.ToString(), StatusId.ToString(), note);
            }

            RaiseDomainEvent(new ExpertAssignedEvent(expertEmail, expertFullName, agentFullName, Title));
        }

        public void Complete(int actorAccountId, string? note = null)
        {
            int oldStatus = StatusId;
            StatusId = RequestStatusConstants.Completed;
            UpdatedAt = DateTime.UtcNow;

            AddHistory(actorAccountId, RequestActionType.StatusChanged, oldStatus.ToString(), StatusId.ToString(), note ?? "Talep Tamamlandı");
        }

        public void ChangePriority(RequestPriority priority, int actorAccountId, string? note = null)
        {
            string oldPriority = Priority.ToString();
            Priority = priority;
            UpdatedAt = DateTime.UtcNow;

            AddHistory(actorAccountId, RequestActionType.PriorityUpdated, oldPriority, priority.ToString(), note);
        }

        public void SetStatus(int statusId, int? actorAccountId = null, string? note = null)
        {
            int oldStatus = StatusId;
            StatusId = statusId;
            UpdatedAt = DateTime.UtcNow;

            if (actorAccountId.HasValue && oldStatus != statusId)
            {
                AddHistory(actorAccountId.Value, RequestActionType.StatusChanged, oldStatus.ToString(), statusId.ToString(), note);
            }
        }

        public void AddHistory(int actorAccountId, RequestActionType action, string? oldValue = null, string? newValue = null, string? note = null)
        {
            Histories.Add(new RequestHistory
            {
                ActorAccountId = actorAccountId,
                Action = action,
                OldValue = oldValue,
                NewValue = newValue,
                Note = note,
                CreatedAt = DateTime.UtcNow
            });
        }
    }
}
