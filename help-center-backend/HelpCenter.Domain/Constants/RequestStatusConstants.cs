namespace HelpCenter.Domain.Constants
{
    public static class RequestStatusConstants
    {
        public static readonly Guid WaitingForReply = Guid.Parse("10000000-0000-4000-8000-000000000001");
        public static readonly Guid Replied = Guid.Parse("10000000-0000-4000-8000-000000000002");
        public static readonly Guid Completed = Guid.Parse("10000000-0000-4000-8000-000000000003");
        public static readonly Guid UnderTechnicalReview = Guid.Parse("10000000-0000-4000-8000-000000000004");
    }
}
