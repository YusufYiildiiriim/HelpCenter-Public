using HelpCenter.Application.Common.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;

namespace HelpCenter.WebApi.Binders;

/// <summary>
/// Converts the <see cref="IFormFile"/>s coming from a multipart form into the Application layer's
/// framework-agnostic <see cref="FileUpload"/> type. This way commands
/// continue to be bound directly with [FromForm] without being tied to ASP.NET.
/// Supports both a single <c>FileUpload</c> and <c>List&lt;FileUpload&gt;</c> properties.
/// </summary>
public sealed class FileUploadModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        ArgumentNullException.ThrowIfNull(bindingContext);

        var request = bindingContext.HttpContext.Request;
        if (!request.HasFormContentType)
        {
            bindingContext.Result = ModelBindingResult.Success(null);
            return Task.CompletedTask;
        }

        var fieldName = bindingContext.FieldName;
        var files = request.Form.Files.GetFiles(fieldName);

        var isCollection = bindingContext.ModelType != typeof(FileUpload);

        if (files.Count == 0)
        {
            // No file was sent: leaving null in a collection preserves the existing behavior
            // (handlers check "Files != null && Count > 0").
            bindingContext.Result = ModelBindingResult.Success(null);
            return Task.CompletedTask;
        }

        if (isCollection)
        {
            bindingContext.Result = ModelBindingResult.Success(files.Select(ToFileUpload).ToList());
        }
        else
        {
            bindingContext.Result = ModelBindingResult.Success(ToFileUpload(files[0]));
        }

        return Task.CompletedTask;
    }

    private static FileUpload ToFileUpload(IFormFile file) => file.ToFileUpload();
}

/// <summary>
/// For places in the presentation layer that need manual conversion (e.g. an IFormFile field
/// on the controller's own request DTO).
/// </summary>
public static class FormFileExtensions
{
    public static FileUpload ToFileUpload(this IFormFile file) =>
        new(file.FileName, file.Length, file.ContentType, file.OpenReadStream);
}

/// <summary>
/// Activates <see cref="FileUploadModelBinder"/> whenever it sees
/// <see cref="FileUpload"/> or <c>List&lt;FileUpload&gt;</c> types.
/// </summary>
public sealed class FileUploadModelBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var modelType = context.Metadata.ModelType;

        var isFileUpload = modelType == typeof(FileUpload);
        var isFileUploadCollection =
            modelType.IsGenericType
            && typeof(IEnumerable<FileUpload>).IsAssignableFrom(modelType);

        if (isFileUpload || isFileUploadCollection)
        {
            return new BinderTypeModelBinder(typeof(FileUploadModelBinder));
        }

        return null;
    }
}
