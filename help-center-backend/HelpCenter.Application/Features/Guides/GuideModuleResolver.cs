using HelpCenter.Application.Exceptions;
using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Entities;

namespace HelpCenter.Application.Features.Guides;

public static class GuideModuleResolver
{
    public static async Task<Module> ResolveAsync(IUnitOfWork unitOfWork, int? id, string name, CancellationToken ct)
    {
        var matches = (await unitOfWork.Repository<Module>().FindAsync(
            m => id.HasValue ? m.Id == id.Value : m.Name == name, ct)).ToList();
        if (matches.Count != 1) throw new ModuleNotFoundException();
        return matches[0];
    }
}
