using FluentAssertions;
using HelpCenter.Application.Common.Extensions;
using HelpCenter.Application.Common.Models;
using Xunit;

namespace HelpCenter.Application.Tests.Common;

public class CommonComponentsTests
{
    [Fact]
    public void ToPaginatedResponse_should_calculate_pagination_metadata_correctly()
    {
        var items = Enumerable.Range(1, 25).Select(i => $"Item {i}").ToList();

        var result = items.ToPaginatedResponse(pageNumber: 2, pageSize: 10);

        result.TotalCount.Should().Be(25);
        result.TotalPages.Should().Be(3);
        result.CurrentPage.Should().Be(2);
        result.PageSize.Should().Be(10);
        result.Items.Should().HaveCount(10);
        result.Items.First().Should().Be("Item 11");
        result.Items.Last().Should().Be("Item 20");
    }

    [Fact]
    public void ToPaginatedResponse_should_handle_edge_cases()
    {
        var items = new List<int>();

        var result = items.ToPaginatedResponse(pageNumber: 0, pageSize: -1);

        result.CurrentPage.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.TotalCount.Should().Be(0);
        result.TotalPages.Should().Be(0);
        result.Items.Should().BeEmpty();
    }

    [Fact]
    public void ToPaginatedResponse_with_pageSize_zero_should_return_all_items_unpaged()
    {
        // Regression test for b83c58c: pageSize=0 is the "no limit" contract used by
        // dropdown/export callers (e.g. AdminModuleService/AdminRoleService.getAll()).
        var items = Enumerable.Range(1, 37).ToList();

        var result = items.ToPaginatedResponse(pageNumber: 1, pageSize: 0);

        result.Items.Should().HaveCount(37);
        result.TotalCount.Should().Be(37);
        result.TotalPages.Should().Be(1);
        result.CurrentPage.Should().Be(1);
        result.PageSize.Should().Be(37);
    }

    [Fact]
    public void ApiResponse_SuccessResult_should_populate_success_and_data()
    {
        var resp = ApiResponse<string>.SuccessResult("sample data", "Operation succeeded");

        resp.Success.Should().BeTrue();
        resp.Data.Should().Be("sample data");
        resp.Message.Should().Be("Operation succeeded");
        resp.Error.Should().BeNull();
    }

    [Fact]
    public void ApiResponse_Fail_should_populate_error_details()
    {
        var details = new[] { "Field 1 error", "Field 2 error" };
        var resp = ApiResponse.Fail("Operation failed", "ValidationError", details);

        resp.Success.Should().BeFalse();
        resp.Message.Should().Be("Operation failed");
        resp.Error.Should().NotBeNull();
        resp.Error!.Code.Should().Be("ValidationError");
        resp.Error.Message.Should().Be("Operation failed");
        resp.Error.Details.Should().BeEquivalentTo(details);
    }
}
