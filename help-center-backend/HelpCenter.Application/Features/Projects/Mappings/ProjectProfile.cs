using AutoMapper;
using HelpCenter.Application.Features.Projects.Commands.CreateProject;
using HelpCenter.Application.Features.Projects.Commands.UpdateProject;
using HelpCenter.Application.Features.Projects.Queries.GetProjects;
using HelpCenter.Application.Features.Projects.Queries.GetPublicProjects;
using HelpCenter.Application.Features.Projects.Queries.GetUsersByProjectId;
using HelpCenter.Domain.Entities;

namespace HelpCenter.Application.Features.Projects.Mappings;

public class ProjectProfile : AutoMapper.Profile
{
    public ProjectProfile()
    {
        CreateMap<Project, ProjectDto>()
            .ForMember(dest => dest.CompanyCount, opt => opt.MapFrom(src => src.Companies.Count))
            .ForMember(dest => dest.UserCount, opt => opt.MapFrom(src => src.UserProjects.Count))
            .ForMember(dest => dest.ModuleIds, opt => opt.MapFrom(src => src.ProjectModules.Select(x => x.ModuleId).ToList()));
        CreateMap<UserProject, ProjectUserDto>()
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.User != null ? src.User.FirstName + " " + src.User.LastName : string.Empty))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User != null ? src.User.Email : string.Empty));
        CreateMap<CreateProjectCommand, Project>();
        CreateMap<UpdateProjectCommand, Project>()
            .ForMember(dest => dest.RowVersion, opt => opt.Ignore());
    }
}
