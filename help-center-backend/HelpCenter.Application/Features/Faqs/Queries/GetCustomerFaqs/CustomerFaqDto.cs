namespace HelpCenter.Application.Features.Faqs.Queries.GetCustomerFaqs;

public class CustomerFaqDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int? ProjectId { get; set; }
    public string? ProjectName { get; set; }
    public int? ModuleId { get; set; }
    public string? ModuleName { get; set; }
}
