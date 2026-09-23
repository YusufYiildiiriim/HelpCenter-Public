using FluentAssertions;
using HelpCenter.Application.Exceptions;
using HelpCenter.Application.Features.RequestSubjects.Commands.CreateRequestSubject;
using HelpCenter.Application.Features.RequestSubjects.Commands.DeleteRequestSubject;
using HelpCenter.Application.Features.RequestSubjects.Commands.UpdateRequestSubject;
using HelpCenter.Application.Features.RequestSubjects.Queries.GetRequestSubjects;
using HelpCenter.Application.Interfaces;
using HelpCenter.Application.Tests.Common;
using HelpCenter.Domain.Entities;
using Xunit;

namespace HelpCenter.Application.Tests.Features.RequestSubjects;

public class RequestSubjectHandlerTests : HandlerTestBase
{
    [Fact]
    public async Task CreateRequestSubject_should_create_subject()
    {
        var handler = new CreateRequestSubjectCommandHandler(Uow, new CreateRequestSubjectRules(Uow));
        var command = new CreateRequestSubjectCommand
        {
            Name = "General Inquiry",
            Description = "General questions",
            IsActive = true
        };

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().BeTrue();
        Db.RequestSubjects.Should().ContainSingle(s => s.Name == "General Inquiry");
    }

    [Fact]
    public async Task CreateRequestSubject_should_throw_ConflictException_on_duplicate_name()
    {
        Db.RequestSubjects.Add(new RequestSubject { Name = "General Inquiry", CreatedAt = DateTime.UtcNow });
        await Db.SaveChangesAsync();

        var handler = new CreateRequestSubjectCommandHandler(Uow, new CreateRequestSubjectRules(Uow));
        var command = new CreateRequestSubjectCommand { Name = "General Inquiry" };

        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<RequestSubjectNameAlreadyExistsException>();
    }

    [Fact]
    public async Task UpdateRequestSubject_should_update_subject()
    {
        var subject = new RequestSubject { Name = "Old Subject", Description = "Old", IsActive = true, CreatedAt = DateTime.UtcNow };
        Db.RequestSubjects.Add(subject);
        await Db.SaveChangesAsync();
        Db.ChangeTracker.Clear();

        var handler = new UpdateRequestSubjectCommandHandler(Uow, new UpdateRequestSubjectRules(Uow));
        var command = new UpdateRequestSubjectCommand
        {
            Id = subject.Id,
            Name = "Updated Subject",
            Description = "Updated",
            IsActive = false
        };

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().BeTrue();
        var updated = Db.RequestSubjects.Find(subject.Id);
        updated!.Name.Should().Be("Updated Subject");
        updated.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteRequestSubject_should_soft_delete_subject()
    {
        var subject = new RequestSubject { Name = "To Delete", CreatedAt = DateTime.UtcNow };
        Db.RequestSubjects.Add(subject);
        await Db.SaveChangesAsync();
        Db.ChangeTracker.Clear();

        var handler = new DeleteRequestSubjectCommandHandler(Uow);
        var result = await handler.Handle(new DeleteRequestSubjectCommand(subject.Id), CancellationToken.None);

        result.Should().BeTrue();
        Db.RequestSubjects.Find(subject.Id)!.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateRequestSubject_should_throw_RequestSubjectLockedException_when_subject_is_locked()
    {
        var subject = new RequestSubject { Name = "Locked Subject", Description = "Old", IsActive = true, CreatedAt = DateTime.UtcNow, IsLocked = true };
        Db.RequestSubjects.Add(subject);
        await Db.SaveChangesAsync();
        Db.ChangeTracker.Clear();

        var handler = new UpdateRequestSubjectCommandHandler(Uow, new UpdateRequestSubjectRules(Uow));
        var command = new UpdateRequestSubjectCommand
        {
            Id = subject.Id,
            Name = "Should Not Update",
            Description = "Should Not Update",
            IsActive = false
        };

        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<RequestSubjectLockedException>();
        Db.RequestSubjects.Find(subject.Id)!.Name.Should().Be("Locked Subject");
    }

    [Fact]
    public async Task DeleteRequestSubject_should_throw_RequestSubjectLockedException_when_subject_is_locked()
    {
        var subject = new RequestSubject { Name = "Locked Subject", CreatedAt = DateTime.UtcNow, IsLocked = true };
        Db.RequestSubjects.Add(subject);
        await Db.SaveChangesAsync();
        Db.ChangeTracker.Clear();

        var handler = new DeleteRequestSubjectCommandHandler(Uow);

        Func<Task> act = async () => await handler.Handle(new DeleteRequestSubjectCommand(subject.Id), CancellationToken.None);

        await act.Should().ThrowAsync<RequestSubjectLockedException>();
        Db.RequestSubjects.Find(subject.Id)!.IsDeleted.Should().BeFalse();
    }
}
