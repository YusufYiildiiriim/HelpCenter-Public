using HelpCenter.Application.Exceptions;
using HelpCenter.Domain.Entities;

namespace HelpCenter.Application.Features.Faqs.Commands.DeleteFaq;

/// <summary>
/// Rules belonging only to the delete FAQ slice.
/// </summary>
public static class DeleteFaqRules
{
    /// <summary>
    /// Verifies the FAQ record exists in the system.
    /// </summary>
    public static void FaqShouldExist(FAQ? faq)
    {
        if (faq == null || faq.IsDeleted)
        {
            throw new FaqNotFoundException();
        }
    }
}
