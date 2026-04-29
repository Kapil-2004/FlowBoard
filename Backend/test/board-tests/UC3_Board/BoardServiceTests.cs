using FlowBoard_Board.Data;
using FlowBoard_Board.DTOs;
using FlowBoard_Board.Models;
using FlowBoard_Board.Repositories;
using FlowBoard_Board.Services;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using Xunit;

namespace board_service.Tests.UC3_Board
{
    /// <summary>
    /// Unit tests for UC3: Board Management.
    /// Verifies board creation, retrieval, and updates using an in-memory database.
    /// </summary>
    public class BoardServiceTests : IDisposable
    {
        private readonly BoardDbContext _db;
        private readonly BoardServiceImpl _sut;
        private readonly Guid _testUserId = Guid.NewGuid();

        public BoardServiceTests()
        {
            var options = new DbContextOptionsBuilder<BoardDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _db = new BoardDbContext(options);
            var repo = new BoardRepository(_db);
            _sut = new BoardServiceImpl(repo);
        }

        public void Dispose() => _db.Dispose();

        [Fact]
        public async Task CreateBoard_ValidInput_SetsWorkspaceIdAndCreatorAsAdmin()
        {
            // Arrange
            var dto = new CreateBoardDto
            {
                WorkspaceId = 101,
                Name = "Project Alpha",
                Description = "A test board",
                Visibility = "PRIVATE",
                Background = "#000000"
            };

            // Act
            var result = await _sut.CreateBoardAsync(dto, _testUserId);

            // Assert
            result.Should().NotBeNull();
            result.WorkspaceId.Should().Be(101);
            result.Name.Should().Be("Project Alpha");
            result.CreatedById.Should().Be(_testUserId);
            
            // Verify membership
            result.Members.Should().ContainSingle(m => m.UserId == _testUserId && m.Role == "ADMIN");
        }

        [Fact]
        public async Task GetBoardsByWorkspace_FiltersByVisibility()
        {
            // Arrange - seed boards
            var workspaceId = 202;
            var publicBoard = new Board { WorkspaceId = workspaceId, Name = "Public", Visibility = "PUBLIC", CreatedById = Guid.NewGuid() };
            var privateBoard = new Board { WorkspaceId = workspaceId, Name = "Private", Visibility = "PRIVATE", CreatedById = Guid.NewGuid() };
            
            _db.Boards.AddRange(publicBoard, privateBoard);
            await _db.SaveChangesAsync();

            // Act
            var results = await _sut.GetBoardsByWorkspaceAsync(workspaceId, _testUserId);

            // Assert - only the public board should be visible to a stranger
            results.Should().HaveCount(1);
            results[0].Name.Should().Be("Public");
        }

        [Fact]
        public async Task UpdateBoard_OnlyAdminsCanModify()
        {
            // Arrange
            var board = new Board 
            { 
                BoardId = 1, 
                Name = "Old Name", 
                CreatedById = Guid.NewGuid(), // Someone else created it
                Visibility = "PUBLIC" 
            };
            _db.Boards.Add(board);
            await _db.SaveChangesAsync();

            var updateDto = new UpdateBoardDto { Name = "New Name" };

            // Act & Assert
            var act = async () => await _sut.UpdateBoardAsync(board.BoardId, updateDto, _testUserId);
            await act.Should().ThrowAsync<UnauthorizedAccessException>();
        }

        [Fact]
        public async Task CloseBoard_SetsIsClosedToTrue()
        {
            // Arrange
            var board = new Board 
            { 
                BoardId = 5, 
                Name = "Active Board", 
                CreatedById = _testUserId, 
                IsClosed = false 
            };
            _db.Boards.Add(board);
            await _db.SaveChangesAsync();

            // Act
            await _sut.CloseBoardAsync(board.BoardId, _testUserId);

            // Assert
            var closedBoard = await _db.Boards.FindAsync(5);
            closedBoard!.IsClosed.Should().BeTrue();
        }
    }
}
