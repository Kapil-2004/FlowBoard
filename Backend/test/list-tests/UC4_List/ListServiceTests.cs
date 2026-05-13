using FlowBoard_ListService.Data;
using FlowBoard_ListService.DTOs;
using FlowBoard_ListService.Models;
using FlowBoard_ListService.Repositories;
using FlowBoard_ListService.Services;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using Xunit;

namespace list_service.Tests.UC4_List
{
    /// <summary>
    /// Unit tests for UC4: List/Column-Service.
    /// Uses an EF Core in-memory database so no real PostgreSQL instance is needed.
    /// Each test gets a fresh DB via a unique name to ensure full isolation.
    /// </summary>
    public class ListServiceTests : IDisposable
    {
        private readonly ListDbContext  _db;
        private readonly ListServiceImpl _sut;   // System Under Test

        public ListServiceTests()
        {
            var options = new DbContextOptionsBuilder<ListDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _db  = new ListDbContext(options);
            var repo = new ListRepository(_db);
            _sut = new ListServiceImpl(repo);
        }

        public void Dispose() => _db.Dispose();

        // ────────────────────────────────────────────────────────────────────────
        // Create
        // ────────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task CreateList_FirstList_GetsPositionZero()
        {
            // Arrange
            var dto = new CreateListDto { BoardId = 1, Name = "To Do", Color = "#6366F1" };

            // Act
            var result = await _sut.CreateListAsync(dto);

            // Assert
            result.Should().NotBeNull();
            result.Position.Should().Be(0);
            result.Name.Should().Be("To Do");
            result.BoardId.Should().Be(1);
        }

        [Fact]
        public async Task CreateList_SecondList_GetsPositionOne()
        {
            // Arrange – seed one list at position 0
            var first = new TaskList { BoardId = 2, Name = "To Do", Position = 0 };
            _db.TaskLists.Add(first);
            await _db.SaveChangesAsync();

            var dto = new CreateListDto { BoardId = 2, Name = "In Progress" };

            // Act
            var result = await _sut.CreateListAsync(dto);

            // Assert
            result.Position.Should().Be(1);
        }

        // ────────────────────────────────────────────────────────────────────────
        // Read
        // ────────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task GetListsByBoard_ReturnsOnlyActiveListsInOrder()
        {
            // Arrange – mix of active and archived lists
            _db.TaskLists.AddRange(
                new TaskList { BoardId = 3, Name = "Done",        Position = 2, IsArchived = false },
                new TaskList { BoardId = 3, Name = "Archived Col", Position = 0, IsArchived = true },
                new TaskList { BoardId = 3, Name = "To Do",       Position = 0, IsArchived = false },
                new TaskList { BoardId = 3, Name = "In Progress", Position = 1, IsArchived = false }
            );
            await _db.SaveChangesAsync();

            // Act
            var results = await _sut.GetListsByBoardAsync(3);

            // Assert – only 3 active, sorted by Position
            results.Should().HaveCount(3);
            results[0].Name.Should().Be("To Do");
            results[1].Name.Should().Be("In Progress");
            results[2].Name.Should().Be("Done");
        }

        [Fact]
        public async Task GetArchivedLists_ReturnsOnlyArchivedLists()
        {
            // Arrange
            _db.TaskLists.AddRange(
                new TaskList { BoardId = 4, Name = "Active",   Position = 0, IsArchived = false },
                new TaskList { BoardId = 4, Name = "Archived", Position = 1, IsArchived = true }
            );
            await _db.SaveChangesAsync();

            // Act
            var results = await _sut.GetArchivedListsAsync(4);

            // Assert
            results.Should().HaveCount(1);
            results[0].Name.Should().Be("Archived");
        }

        // ────────────────────────────────────────────────────────────────────────
        // Update
        // ────────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task UpdateList_ChangesNameAndColor()
        {
            // Arrange
            var list = new TaskList { BoardId = 5, Name = "Old Name", Position = 0, Color = "#000000" };
            _db.TaskLists.Add(list);
            await _db.SaveChangesAsync();

            // Act
            var result = await _sut.UpdateListAsync(list.ListId,
                new UpdateListDto { Name = "New Name", Color = "#FF5733" });

            // Assert
            result.Name.Should().Be("New Name");
            result.Color.Should().Be("#FF5733");
        }

        [Fact]
        public async Task UpdateList_UnknownId_ThrowsKeyNotFound()
        {
            // Act & Assert
            var act = async () => await _sut.UpdateListAsync(999, new UpdateListDto { Name = "X" });
            await act.Should().ThrowAsync<KeyNotFoundException>();
        }

        // ────────────────────────────────────────────────────────────────────────
        // Reorder
        // ────────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task ReorderLists_UpdatesPositionsAtomically()
        {
            // Arrange – three lists on board 6
            var l1 = new TaskList { BoardId = 6, Name = "A", Position = 0 };
            var l2 = new TaskList { BoardId = 6, Name = "B", Position = 1 };
            var l3 = new TaskList { BoardId = 6, Name = "C", Position = 2 };
            _db.TaskLists.AddRange(l1, l2, l3);
            await _db.SaveChangesAsync();

            // Act – reverse the order: C, B, A
            await _sut.ReorderListsAsync(6, new List<int> { l3.ListId, l2.ListId, l1.ListId });

            // Assert
            var updated = await _sut.GetListsByBoardAsync(6);
            updated[0].Name.Should().Be("C");
            updated[1].Name.Should().Be("B");
            updated[2].Name.Should().Be("A");
        }

        [Fact]
        public async Task ReorderLists_ForeignIdInPayload_ThrowsInvalidOperation()
        {
            // Arrange – one list on board 7
            var l = new TaskList { BoardId = 7, Name = "Solo", Position = 0 };
            _db.TaskLists.Add(l);
            await _db.SaveChangesAsync();

            // Act & Assert – supply an ID that doesn't belong to board 7
            var act = async () => await _sut.ReorderListsAsync(7, new List<int> { l.ListId, 9999 });
            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        // ────────────────────────────────────────────────────────────────────────
        // Archive / Unarchive
        // ────────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task ArchiveList_SetsIsArchivedTrue()
        {
            // Arrange
            var list = new TaskList { BoardId = 8, Name = "Sprint 1", Position = 0, IsArchived = false };
            _db.TaskLists.Add(list);
            await _db.SaveChangesAsync();

            // Act
            await _sut.ArchiveListAsync(list.ListId);

            // Assert
            var stored = await _db.TaskLists.FindAsync(list.ListId);
            stored!.IsArchived.Should().BeTrue();
        }

        [Fact]
        public async Task UnarchiveList_SetsIsArchivedFalse()
        {
            // Arrange
            var list = new TaskList { BoardId = 9, Name = "Old Sprint", Position = 0, IsArchived = true };
            _db.TaskLists.Add(list);
            await _db.SaveChangesAsync();

            // Act
            await _sut.UnarchiveListAsync(list.ListId);

            // Assert
            var stored = await _db.TaskLists.FindAsync(list.ListId);
            stored!.IsArchived.Should().BeFalse();
        }

        // ────────────────────────────────────────────────────────────────────────
        // Move
        // ────────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task MoveList_ChangesBoard_AndAppendsAtEnd()
        {
            // Arrange – target board 12 already has two lists
            _db.TaskLists.AddRange(
                new TaskList { BoardId = 12, Name = "Existing-A", Position = 0 },
                new TaskList { BoardId = 12, Name = "Existing-B", Position = 1 }
            );
            // The list to move lives on board 11
            var source = new TaskList { BoardId = 11, Name = "Mover", Position = 0 };
            _db.TaskLists.Add(source);
            await _db.SaveChangesAsync();

            // Act
            var moved = await _sut.MoveListAsync(source.ListId, targetBoardId: 12);

            // Assert
            moved.BoardId.Should().Be(12);
            moved.Position.Should().Be(2); // appended after positions 0 and 1
        }

        // ────────────────────────────────────────────────────────────────────────
        // Delete
        // ────────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task DeleteList_RemovesFromDatabase()
        {
            // Arrange
            var list = new TaskList { BoardId = 10, Name = "Temp", Position = 0 };
            _db.TaskLists.Add(list);
            await _db.SaveChangesAsync();

            // Act
            await _sut.DeleteListAsync(list.ListId);

            // Assert
            var stored = await _db.TaskLists.FindAsync(list.ListId);
            stored.Should().BeNull();
        }

        [Fact]
        public async Task DeleteList_UnknownId_ThrowsKeyNotFound()
        {
            var act = async () => await _sut.DeleteListAsync(999);
            await act.Should().ThrowAsync<KeyNotFoundException>();
        }
    }
}
