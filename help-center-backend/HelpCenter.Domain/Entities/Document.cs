namespace HelpCenter.Domain.Entities
{
    public class Document : BaseEntity
    {

        public string Path { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public int GuideId { get; set; }
        public virtual Guide Guide { get; set; } = null!;
    }
}
