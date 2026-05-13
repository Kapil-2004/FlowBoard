using FlowBoard_Comment_AttachmentService.Models;
using FlowBoard_Comment_AttachmentService.Repositories;
using FlowBoard_Comment_AttachmentService.Services;
using Moq;
using System.Threading.Tasks;
using Xunit;
using MassTransit;

namespace FlowBoard_CommentService.Tests
{
    public class CommentServiceTests
    {
        private readonly Mock<ICommentRepository> _mockRepo;
        private readonly Mock<IPublishEndpoint> _mockPublish;
        private readonly CommentServiceImpl _service;

        public CommentServiceTests()
        {
            _mockRepo = new Mock<ICommentRepository>();
            _mockPublish = new Mock<IPublishEndpoint>();
            _service = new CommentServiceImpl(_mockRepo.Object, _mockPublish.Object);
        }

        [Fact]
        public async Task AddComment_ShouldReturnCreatedComment()
        {
            // Arrange
            var comment = new Comment { Content = "Test Comment", CardId = 1, AuthorId = 1 };
            _mockRepo.Setup(r => r.AddAsync(It.IsAny<Comment>())).ReturnsAsync(comment);

            // Act
            var result = await _service.AddCommentAsync(comment);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Test Comment", result.Content);
            _mockRepo.Verify(r => r.AddAsync(It.IsAny<Comment>()), Times.Once);
        }

        [Fact]
        public async Task DeleteComment_ShouldInvokeRepoDelete()
        {
            // Act
            await _service.DeleteCommentAsync(1);

            // Assert
            _mockRepo.Verify(r => r.DeleteByCommentIdAsync(1), Times.Once);
        }
    }
}
