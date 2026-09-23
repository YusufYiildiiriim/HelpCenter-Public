namespace HelpCenter.Application.Exceptions;

public class BaseException : Exception
{
    public string Code { get; }
    public int StatusCode { get; }

    public BaseException(string message, string code = "Error", int statusCode = 400) : base(message)
    {
        Code = code;
        StatusCode = statusCode;
    }
}

public class NotFoundException : BaseException
{
    public NotFoundException(string message, string code = "NotFound") : base(message, code, 404) { }
}

public class BadRequestException : BaseException
{
    public BadRequestException(string message, string code = "BadRequest") : base(message, code, 400) { }
}

public class UnauthorizedException : BaseException
{
    public UnauthorizedException(string message, string code = "Unauthorized") : base(message, code, 401) { }
}

public class ForbiddenException : BaseException
{
    public ForbiddenException(string message, string code = "Forbidden") : base(message, code, 403) { }
}

public class ConflictException : BaseException
{
    public ConflictException(string message, string code = "Conflict") : base(message, code, 409) { }
}

public class InternalServerErrorException : BaseException
{
    public InternalServerErrorException(string message, string code = "InternalError") : base(message, code, 500) { }
}

public class ValidationException : BaseException
{
    public List<string>? Errors { get; }
    public ValidationException(string message, string code = "ValidationError") : base(message, code, 400) { }
    public ValidationException(string message, List<string> errors, string code = "ValidationError") : base(message, code, 400)
    {
        Errors = errors;
    }
}

// -------------------------------------------------------------------------
// Specialized Error Types (Specialized Business Exceptions)
// -------------------------------------------------------------------------

public class UserNotFoundException : NotFoundException
{
    public UserNotFoundException(string message = "Kullanıcı hesabı bulunamadı.")
        : base(message, "UserNotFound") { }
}

public class CustomerNotFoundException : NotFoundException
{
    public CustomerNotFoundException(string message = "Müşteri hesabı bulunamadı.")
        : base(message, "CustomerNotFound") { }
}

public class EmailNotFoundException : NotFoundException
{
    public EmailNotFoundException(string message = "Hesaba tanımlı bir e-posta adresi bulunamadı.")
        : base(message, "EmailNotFound") { }
}

public class AccountInactiveException : BadRequestException
{
    public AccountInactiveException(string message = "Hesabınız aktif değil.", string code = "AccountInactive")
        : base(message, code) { }
}

public class UserInactiveException : AccountInactiveException
{
    public UserInactiveException(string message = "Kullanıcı hesabınız aktif değil.")
        : base(message, "UserInactive") { }
}

public class CustomerInactiveException : AccountInactiveException
{
    public CustomerInactiveException(string message = "Müşteri hesabınız aktif değil. Lütfen yöneticinizle iletişime geçin.")
        : base(message, "CustomerInactive") { }
}

public class InvalidCredentialsException : UnauthorizedException
{
    public InvalidCredentialsException(string message = "Hatalı kullanıcı adı/e-posta veya şifre.")
        : base(message, "InvalidCredentials") { }
}

public class UserHasNoRoleException : ForbiddenException
{
    public UserHasNoRoleException(string message = "Kullanıcıya atanmış bir rol bulunamadı. Lütfen yöneticinizle iletişime geçin.")
        : base(message, "UserHasNoRole") { }
}

public class InvalidResetCodeException : ValidationException
{
    public InvalidResetCodeException(string message = "Girdiğiniz doğrulama kodu hatalı. Lütfen kontrol edip tekrar deneyin.")
        : base(message, "InvalidResetCode") { }
}

public class ResetCodeExpiredException : ValidationException
{
    public ResetCodeExpiredException(string message = "Doğrulama kodu geçersiz veya süresi dolmuş. Lütfen yeni bir kod talep edin.")
        : base(message, "ResetCodeExpired") { }
}

public class InvalidIdentifierException : ValidationException
{
    public InvalidIdentifierException(string message = "E-posta adresi veya kullanıcı adı girilmelidir.")
        : base(message, "InvalidIdentifier") { }
}

public class EmailAlreadyExistsException : ConflictException
{
    public EmailAlreadyExistsException(string message = "Bu e-posta adresi zaten kullanımda.")
        : base(message, "EmailAlreadyExists") { }
}

public class UsernameAlreadyExistsException : ConflictException
{
    public UsernameAlreadyExistsException(string message = "Bu kullanıcı adı zaten kullanımda.")
        : base(message, "UsernameAlreadyExists") { }
}

// -------------------------------------------------------------------------
// Company Exceptions
// -------------------------------------------------------------------------

public class CompanyNotFoundException : NotFoundException
{
    public CompanyNotFoundException(string message = "Firma bulunamadı.")
        : base(message, "CompanyNotFound") { }
}

public class CompanyNameAlreadyExistsException : ConflictException
{
    public CompanyNameAlreadyExistsException(string message = "Bu isimde bir firma zaten mevcut.")
        : base(message, "CompanyNameAlreadyExists") { }
}

public class CompanyHasActiveRequestsException : ConflictException
{
    public CompanyHasActiveRequestsException(string message = "Bu firmaya ait destek talepleri olduğu için firma silinemez.")
        : base(message, "CompanyHasActiveRequests") { }
}

// -------------------------------------------------------------------------
// FAQ Exceptions
// -------------------------------------------------------------------------

public class FaqNotFoundException : NotFoundException
{
    public FaqNotFoundException(string message = "SSS bulunamadı.")
        : base(message, "FaqNotFound") { }
}

public class FaqTitleAlreadyExistsException : ConflictException
{
    public FaqTitleAlreadyExistsException(string message = "Bu başlıkta bir SSS zaten mevcut.")
        : base(message, "FaqTitleAlreadyExists") { }
}

// -------------------------------------------------------------------------
// Guide Exceptions
// -------------------------------------------------------------------------

public class GuideNotFoundException : NotFoundException
{
    public GuideNotFoundException(string message = "Rehber bulunamadı.")
        : base(message, "GuideNotFound") { }
}

public class GuideTitleAlreadyExistsException : ConflictException
{
    public GuideTitleAlreadyExistsException(string message = "Bu başlıkta bir kullanım kılavuzu zaten mevcut.")
        : base(message, "GuideTitleAlreadyExists") { }
}

// -------------------------------------------------------------------------
// Module Exceptions
// -------------------------------------------------------------------------

public class ModuleNotFoundException : NotFoundException
{
    public ModuleNotFoundException(string message = "Modül bulunamadı.")
        : base(message, "ModuleNotFound") { }
}

public class ModuleNotAssignedToProjectException : BadRequestException
{
    public ModuleNotAssignedToProjectException(string message = "Seçilen modül bu projeye atanmamıştır.")
        : base(message, "ModuleNotAssignedToProject") { }
}

public class ModuleNameAlreadyExistsException : ConflictException
{
    public ModuleNameAlreadyExistsException(string message = "Bu isimde bir modül zaten mevcut.")
        : base(message, "ModuleNameAlreadyExists") { }
}

public class ModuleLockedException : ConflictException
{
    public ModuleLockedException(string message = "Bu modül sistem tarafından kilitlenmiştir ve güncellenemez.")
        : base(message, "ModuleLocked") { }
}

public class ModuleExpertNotFoundException : NotFoundException
{
    public ModuleExpertNotFoundException(string message = "Uzman kaydı bulunamadı.")
        : base(message, "ModuleExpertNotFound") { }
}

// -------------------------------------------------------------------------
// Organization Exceptions
// -------------------------------------------------------------------------

public class OrganizationInfoNotFoundException : NotFoundException
{
    public OrganizationInfoNotFoundException(string message = "Kurum bilgisi bulunamadı.")
        : base(message, "OrganizationInfoNotFound") { }
}

// -------------------------------------------------------------------------
// Project Exceptions
// -------------------------------------------------------------------------

public class ProjectNotFoundException : NotFoundException
{
    public ProjectNotFoundException(string message = "Proje bulunamadı.")
        : base(message, "ProjectNotFound") { }
}

public class ProjectNameAlreadyExistsException : ConflictException
{
    public ProjectNameAlreadyExistsException(string message = "Bu isimde bir proje zaten mevcut.")
        : base(message, "ProjectNameAlreadyExists") { }
}

public class ProjectHasActiveCompaniesException : ConflictException
{
    public ProjectHasActiveCompaniesException(string message = "Bu projeye bağlı firmalar olduğu için işlem yapılamaz. Önce firmaların proje bağlantısını kaldırın.")
        : base(message, "ProjectHasActiveCompanies") { }
}

public class ProjectAssignmentNotFoundException : NotFoundException
{
    public ProjectAssignmentNotFoundException(string message = "Proje ataması bulunamadı.")
        : base(message, "ProjectAssignmentNotFound") { }
}

// -------------------------------------------------------------------------
// RequestSubject Exceptions
// -------------------------------------------------------------------------

public class RequestSubjectNotFoundException : NotFoundException
{
    public RequestSubjectNotFoundException(string message = "Talep konusu bulunamadı.")
        : base(message, "RequestSubjectNotFound") { }
}

public class RequestSubjectNameAlreadyExistsException : ConflictException
{
    public RequestSubjectNameAlreadyExistsException(string message = "Bu isimde bir talep konusu zaten mevcut.")
        : base(message, "RequestSubjectNameAlreadyExists") { }
}

public class RequestSubjectLockedException : ConflictException
{
    public RequestSubjectLockedException(string message = "Bu talep konusu sistem tarafından kilitlenmiştir ve güncellenemez/silinemez.")
        : base(message, "RequestSubjectLocked") { }
}

// -------------------------------------------------------------------------
// Request Exceptions
// -------------------------------------------------------------------------

public class RequestNotFoundException : NotFoundException
{
    public RequestNotFoundException(string message = "Talep bulunamadı.")
        : base(message, "RequestNotFound") { }
}

public class RequestCompletedException : ConflictException
{
    public RequestCompletedException(string message = "Tamamlanmış bir talebe işlem yapılamaz.")
        : base(message, "RequestCompleted") { }
}

public class RequestNotBelongToCustomerException : NotFoundException
{
    public RequestNotBelongToCustomerException(string message = "Talep bulunamadı veya size ait değil.")
        : base(message, "RequestNotBelongToCustomer") { }
}

public class AgentNotFoundException : NotFoundException
{
    public AgentNotFoundException(string message = "Temsilci bulunamadı.")
        : base(message, "AgentNotFound") { }
}

public class ExpertNotFoundException : NotFoundException
{
    public ExpertNotFoundException(string message = "Uzman bulunamadı.")
        : base(message, "ExpertNotFound") { }
}

public class ModuleNotAllowedForCompanyException : ForbiddenException
{
    public ModuleNotAllowedForCompanyException(string message = "Seçilen modül firmanıza veya projenize tanımlı değildir.")
        : base(message, "ModuleNotAllowedForCompany") { }
}

// -------------------------------------------------------------------------
// Role Exceptions
// -------------------------------------------------------------------------

public class RoleNotFoundException : NotFoundException
{
    public RoleNotFoundException(string message = "Rol bulunamadı.")
        : base(message, "RoleNotFound") { }
}

public class RoleNameAlreadyExistsException : ConflictException
{
    public RoleNameAlreadyExistsException(string message = "Bu isimde bir rol zaten mevcut.")
        : base(message, "RoleNameAlreadyExists") { }
}

public class RoleAssignedToUsersException : ConflictException
{
    public RoleAssignedToUsersException(string message = "Bu rol bir veya daha fazla kullanıcıya atanmış olduğu için silinemez.")
        : base(message, "RoleAssignedToUsers") { }
}

// -------------------------------------------------------------------------
// Statistics & Report Exceptions
// -------------------------------------------------------------------------

public class DashboardAccessForbiddenException : ForbiddenException
{
    public DashboardAccessForbiddenException(string message = "İstatistik verilerini görüntüleme yetkiniz bulunmamaktadır.")
        : base(message, "DashboardAccessForbidden") { }
}

public class ReportsAccessForbiddenException : ForbiddenException
{
    public ReportsAccessForbiddenException(string message = "Rapor verilerini görüntüleme yetkiniz bulunmamaktadır.")
        : base(message, "ReportsAccessForbidden") { }
}
