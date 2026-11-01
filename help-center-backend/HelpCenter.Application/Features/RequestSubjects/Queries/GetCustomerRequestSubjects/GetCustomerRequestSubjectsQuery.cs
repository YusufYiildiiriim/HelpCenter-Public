using MediatR;

namespace HelpCenter.Application.Features.RequestSubjects.Queries.GetCustomerRequestSubjects;

public record GetCustomerRequestSubjectsQuery(string Locale = HelpCenter.Domain.Common.SupportedLocales.Default) : IRequest<List<CustomerRequestSubjectDto>>;
