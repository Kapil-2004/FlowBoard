using System;
using System.Threading.Tasks;
using Xunit;
using Moq;
using FlowBoard_Workspace.Interfaces;
using FlowBoard_Workspace.Models;
using FlowBoard_Workspace.Services;
using FlowBoard_Workspace.DTOs;

namespace workspace_tests
{
    public class WorkspaceServiceTests
    {
        private readonly Mock<IWorkspaceRepository> _mockRepo;
        private readonly WorkspaceServiceImpl _service;

        public WorkspaceServiceTests()
        {
            _mockRepo = new Mock<IWorkspaceRepository>();
            _service = new WorkspaceServiceImpl(_mockRepo.Object);
        }

        [Fact]
        public async Task CreateWorkspaceAsync_ValidDto_CreatesWorkspaceAndAdminMember()
        {
            // Arrange
            var dto = new WorkspaceCreateDto { Name = "Test WS", Visibility = "PRIVATE" };
            Guid ownerId = Guid.NewGuid();

            _mockRepo.Setup(r => r.ExistsByNameAndOwnerIdAsync(dto.Name, ownerId))
                     .ReturnsAsync(false);

            _mockRepo.Setup(r => r.CreateWorkspaceAsync(It.IsAny<Workspace>()))
                     .ReturnsAsync((Workspace w) => { w.WorkspaceId = 100; return w; });

            _mockRepo.Setup(r => r.AddMemberAsync(It.IsAny<WorkspaceMember>()))
                     .ReturnsAsync((WorkspaceMember m) => m);

            // Act
            var result = await _service.CreateWorkspaceAsync(dto, ownerId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Test WS", result.Name);
            Assert.Equal(ownerId, result.OwnerId);

            _mockRepo.Verify(r => r.CreateWorkspaceAsync(It.IsAny<Workspace>()), Times.Once);
            _mockRepo.Verify(r => r.AddMemberAsync(It.Is<WorkspaceMember>(m => m.WorkspaceId == 100 && m.UserId == ownerId && m.Role == "ADMIN")), Times.Once);
        }

        [Fact]
        public async Task CreateWorkspaceAsync_DuplicateName_ThrowsException()
        {
            // Arrange
            var dto = new WorkspaceCreateDto { Name = "Existing WS" };
            Guid ownerId = Guid.NewGuid();

            _mockRepo.Setup(r => r.ExistsByNameAndOwnerIdAsync(dto.Name, ownerId))
                     .ReturnsAsync(true);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateWorkspaceAsync(dto, ownerId));
            
            _mockRepo.Verify(r => r.CreateWorkspaceAsync(It.IsAny<Workspace>()), Times.Never);
        }
    }
}
