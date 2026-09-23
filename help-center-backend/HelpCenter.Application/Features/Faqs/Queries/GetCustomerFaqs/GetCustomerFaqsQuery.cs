using MediatR;

namespace HelpCenter.Application.Features.Faqs.Queries.GetCustomerFaqs;

public record GetCustomerFaqsQuery(int? ProjectId = null, int? ModuleId = null) : IRequest<List<CustomerFaqDto>>;
