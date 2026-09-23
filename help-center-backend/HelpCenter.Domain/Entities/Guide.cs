namespace HelpCenter.Domain.Entities
{
    public class Guide : BaseEntity
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public virtual ICollection<Document>? Documents { get; set; }

        public int? PreviousGuideId { get; set; }
        public virtual Guide? PreviousRequest { get; set; }
        public virtual Guide? NextRequest { get; set; }

        public string Module { get; set; } = string.Empty;
        public string? YoutubeUrl { get; set; }
        public bool IsPublic { get; set; } = false;

        public int? ProjectId { get; set; }
        public virtual Project? Project { get; set; }

        public void UpdateInfo(string title, string description, string module, string? youtubeUrl, int? previousGuideId, bool isActive, bool isPublic)
        {
            Title = title;
            Description = description;
            Module = module;
            YoutubeUrl = youtubeUrl;
            PreviousGuideId = previousGuideId;
            IsActive = isActive;
            IsPublic = isPublic;
            UpdatedAt = DateTime.UtcNow;
        }

        public void AddDocument(string path, string fileName)
        {
            Documents ??= new List<Document>();
            Documents.Add(new Document { Path = path, FileName = fileName });
            UpdatedAt = DateTime.UtcNow;
        }

        public void ClearDocuments()
        {
            if (Documents != null)
            {
                foreach (var doc in Documents.Where(d => !d.IsDeleted))
                {
                    doc.IsDeleted = true;
                }
            }
            UpdatedAt = DateTime.UtcNow;
        }

    }
}
