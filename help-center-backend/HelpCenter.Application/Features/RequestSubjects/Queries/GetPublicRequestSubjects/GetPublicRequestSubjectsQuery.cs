using MediatR;

namespace HelpCenter.Application.Features.RequestSubjects.Queries.GetPublicRequestSubjects;

public record GetPublicRequestSubjectsQuery() : IRequest<List<PublicRequestSubjectDto>>;
