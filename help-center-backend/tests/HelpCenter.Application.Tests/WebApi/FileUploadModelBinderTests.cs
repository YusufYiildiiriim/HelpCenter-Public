using System.IO;
using System.Text;
using FluentAssertions;
using HelpCenter.Application.Common.Models;
using HelpCenter.WebApi.Binders;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using NSubstitute;
using Xunit;

namespace HelpCenter.Application.Tests.WebApi;

public class FileUploadModelBinderTests
{
    [Fact]
    public async Task BindModelAsync_should_bind_single_file_to_FileUpload()
    {
        var binder = new FileUploadModelBinder();

        var formFile = Substitute.For<IFormFile>();
        formFile.Name.Returns("File");
        formFile.FileName.Returns("sample.pdf");
        formFile.Length.Returns(1024);
        formFile.ContentType.Returns("application/pdf");
        formFile.OpenReadStream().Returns(new MemoryStream(Encoding.UTF8.GetBytes("sample content")));

        var fileCollection = new FormFileCollection { formFile };
        var formCollection = new FormCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>(), fileCollection);

        var httpContext = new DefaultHttpContext();
        httpContext.Request.ContentType = "multipart/form-data; boundary=----boundary";
        httpContext.Request.Form = formCollection;

        var bindingContext = new DefaultModelBindingContext
        {
            ActionContext = new Microsoft.AspNetCore.Mvc.ActionContext { HttpContext = httpContext },
            FieldName = "File",
            ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(typeof(FileUpload))
        };

        await binder.BindModelAsync(bindingContext);

        bindingContext.Result.IsModelSet.Should().BeTrue();
        var model = bindingContext.Result.Model as FileUpload;
        model.Should().NotBeNull();
        model!.FileName.Should().Be("sample.pdf");
        model.ContentType.Should().Be("application/pdf");
        model.Length.Should().Be(1024);
    }
}
