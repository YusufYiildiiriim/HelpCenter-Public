using MediatR;

namespace HelpCenter.Application.Features.RequestSubjects.Queries.GetCustomerRequestSubjects;

public record GetCustomerRequestSubjectsQuery(string Locale = HelpCenter.Domain.Common.SupportedLocales.Default, Guid CustomerId = default, Guid CompanyId = default) : IRequest<List<CustomerRequestSubjectDto>>;
