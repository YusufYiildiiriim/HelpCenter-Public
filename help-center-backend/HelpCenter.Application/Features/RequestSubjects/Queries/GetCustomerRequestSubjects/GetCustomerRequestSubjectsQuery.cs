using MediatR;

namespace HelpCenter.Application.Features.RequestSubjects.Queries.GetCustomerRequestSubjects;

public record GetCustomerRequestSubjectsQuery() : IRequest<List<CustomerRequestSubjectDto>>;
