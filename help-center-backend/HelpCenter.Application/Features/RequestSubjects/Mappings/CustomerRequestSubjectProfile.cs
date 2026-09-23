using AutoMapper;
using HelpCenter.Application.Features.RequestSubjects.Queries;
using HelpCenter.Application.Features.RequestSubjects.Queries.GetCustomerRequestSubjects;
using HelpCenter.Domain.Entities;

namespace HelpCenter.Application.Features.RequestSubjects.Mappings;

public class CustomerRequestSubjectProfile : AutoMapper.Profile
{
    public CustomerRequestSubjectProfile()
    {
        CreateMap<RequestSubject, CustomerRequestSubjectDto>();
    }
}
