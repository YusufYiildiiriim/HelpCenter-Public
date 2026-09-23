using AutoMapper;
using HelpCenter.Application.Features.Statuses.Queries.GetAllStatuses;
using HelpCenter.Domain.Entities;

namespace HelpCenter.Application.Features.Statuses.Mappings;

public class StatusProfile : Profile
{
    public StatusProfile()
    {
        CreateMap<CustomerRequestStatus, StatusDto>();
    }
}
