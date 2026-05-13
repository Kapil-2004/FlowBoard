using System;
using System.Threading.Tasks;
using FlowBoard_CardService.Data;
using FlowBoard_CardService.DTOs;
using FlowBoard_CardService.Models;
using FlowBoard_CardService.Repositories;
using FlowBoard_CardService.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FlowBoard_CardService.Tests
{
    public class CardServiceTests
    {
        private readonly CardDbContext _context;
        private readonly ICardRepository _repository;
        private readonly ICardService _service;

        public CardServiceTests()
        {
            var options = new DbContextOptionsBuilder<CardDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new CardDbContext(options);
            _repository = new CardRepository(_context);
            _service = new CardServiceImpl(_repository);
        }

        [Fact]
        public async Task CreateCard_ShouldAddCard_AndSetDefaults()
        {
            // Arrange
            var createDto = new CreateCardDto
            {
                BoardId = 1,
                ListId = 1,
                Title = "Test Card"
            };

            // Act
            var result = await _service.CreateCardAsync(createDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Test Card", result.Title);
            Assert.Equal("MEDIUM", result.Priority); // Default priority
            Assert.Equal("TO_DO", result.Status);    // Default status
            Assert.Equal(0, result.Position);

            var dbCard = await _context.Cards.FirstOrDefaultAsync(c => c.CardId == result.CardId);
            Assert.NotNull(dbCard);
        }

        [Fact]
        public async Task MoveCard_ShouldUpdateListIdAndPosition()
        {
            // Arrange
            var card1 = new Card { BoardId = 1, ListId = 1, Title = "Card 1", Position = 0 };
            _context.Cards.Add(card1);
            await _context.SaveChangesAsync();

            var moveDto = new MoveCardDto
            {
                TargetListId = 2,
                TargetPosition = 0
            };

            // Act
            var result = await _service.MoveCardAsync(card1.CardId, moveDto);

            // Assert
            Assert.Equal(2, result.ListId);
            Assert.Equal(0, result.Position); // It becomes 0 because target list is empty
        }

        [Fact]
        public async Task ArchiveCard_ShouldSetIsArchivedToTrue()
        {
            // Arrange
            var card = new Card { BoardId = 1, ListId = 1, Title = "Archive Me", IsArchived = false };
            _context.Cards.Add(card);
            await _context.SaveChangesAsync();

            // Act
            await _service.ArchiveCardAsync(card.CardId);

            // Assert
            var dbCard = await _context.Cards.FindAsync(card.CardId);
            Assert.True(dbCard!.IsArchived);
        }
    }
}
