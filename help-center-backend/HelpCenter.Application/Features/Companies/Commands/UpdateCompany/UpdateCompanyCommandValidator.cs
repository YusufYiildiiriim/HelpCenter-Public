using FluentValidation;

namespace HelpCenter.Application.Features.Companies.Commands.UpdateCompany;

public class UpdateCompanyCommandValidator : AbstractValidator<UpdateCompanyCommand>
{
    public UpdateCompanyCommandValidator()
    {
        RuleFor(x => x.PublicId).NotEmpty().WithMessage("Geçersiz firma ID.");
        RuleFor(x => x.Name).NotEmpty().WithMessage("Firma adı boş bırakılamaz.");
        RuleFor(x => x.Address).NotEmpty().WithMessage("Adres boş bırakılamaz.");
        RuleFor(x => x.Phone).NotEmpty().WithMessage("Telefon boş bırakılamaz.");
        RuleFor(x => x.Mail)
            .NotEmpty().WithMessage("Firma e-posta adresi boş bırakılamaz.")
            .EmailAddress().WithMessage("Geçerli bir e-posta adresi giriniz.");
        RuleFor(x => x.ContactPersonName).NotEmpty().WithMessage("Yetkili adı boş bırakılamaz.");
        RuleFor(x => x.ContactPersonSurname).NotEmpty().WithMessage("Yetkili soyadı boş bırakılamaz.");
        RuleFor(x => x.ContactPersonEmail)
            .NotEmpty().WithMessage("Yetkili e-posta adresi boş bırakılamaz.")
            .EmailAddress().WithMessage("Geçerli bir yetkili e-posta adresi giriniz.");
        RuleFor(x => x.BranchCount)
            .GreaterThanOrEqualTo(0).WithMessage("Şube sayısı negatif olamaz.");
        RuleFor(x => x.ProjectId)
            .GreaterThan(0).WithMessage("Geçersiz proje ID.")
            .When(x => x.ProjectId.HasValue);
        RuleForEach(x => x.ModuleIds)
            .GreaterThan(0).WithMessage("Geçersiz modül ID.")
            .When(x => x.ModuleIds != null);
        RuleFor(x => x.Password)
            .MinimumLength(6).WithMessage("Şifre en az 6 karakter olmalıdır.")
            .When(x => !string.IsNullOrEmpty(x.Password));
    }
}
