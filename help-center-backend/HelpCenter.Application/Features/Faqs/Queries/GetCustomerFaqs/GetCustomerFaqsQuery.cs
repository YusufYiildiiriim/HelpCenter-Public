using MediatR;

namespace HelpCenter.Application.Features.Faqs.Queries.GetCustomerFaqs;

public record GetCustomerFaqsQuery(Guid? ProjectId = null, Guid? ModuleId = null) : IRequest<List<CustomerFaqDto>>;
