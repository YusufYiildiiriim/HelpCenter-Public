using HelpCenter.Application.Features.Requests.Queries.GetAdminRequests;
using HelpCenter.Application.Interfaces;
using HelpCenter.WebApi.Controllers.Admin;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;

namespace HelpCenter.Application.Tests.WebApi;

public class RequestCsvPriorityTests
{
    [Theory]
    [InlineData("Low", "Düşük")]
    [InlineData("Medium", "Orta")]
    [InlineData("High", "Yüksek")]
    [InlineData("Critical", "Kritik")]
    public async Task Export_should_write_Turkish_priority_without_changing_filter(string priority, string label)
    {
        var filter = new GetAdminRequestsQuery();
        var export = Substitute.For<IRequestCsvExportService>();
        export.StreamAsync(filter, Arg.Any<CancellationToken>()).Returns(Rows(priority));
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var controller = new AdminRequestsController(Substitute.For<IMediator>(), export)
        {
            ControllerContext = new ControllerContext { HttpContext = context }
        };

        await controller.ExportCsv(filter, CancellationToken.None);

        context.Response.Body.Position = 0;
        var csv = await new StreamReader(context.Response.Body).ReadToEndAsync();
        Assert.Contains("Öncelik", csv);
        Assert.Contains($"\"{label}\"", csv);
        Assert.DoesNotContain($"\"{priority}\"", csv);
        _ = export.Received(1).StreamAsync(filter, Arg.Any<CancellationToken>());
    }

    private static async IAsyncEnumerable<RequestExportRowDto> Rows(string priority)
    {
        await Task.CompletedTask;
        yield return new RequestExportRowDto { TicketId = "QA-1", PriorityName = priority };
    }
}
