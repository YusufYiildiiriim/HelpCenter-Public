namespace HelpCenter.Application.Features.RequestSubjects.Queries.GetCustomerRequestSubjects;

public class CustomerRequestSubjectDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string RequestedLocale { get; set; } = HelpCenter.Domain.Common.SupportedLocales.Default;
    public string? ContentLocale { get; set; }
}
