using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FlowBoard_CardService.DTOs;
using FlowBoard_CardService.Models;
using FlowBoard_CardService.Repositories;

namespace FlowBoard_CardService.Services
{
    public class CardServiceImpl : ICardService
    {
        private readonly ICardRepository _repository;

        public CardServiceImpl(ICardRepository repository)
        {
            _repository = repository;
        }

        private static CardDto MapToDto(Card card)
        {
            return new CardDto
            {
                CardId = card.CardId,
                ListId = card.ListId,
                BoardId = card.BoardId,
                Title = card.Title,
                Description = card.Description,
                Position = card.Position,
                Priority = card.Priority,
                Status = card.Status,
                DueDate = card.DueDate,
                StartDate = card.StartDate,
                AssigneeId = card.AssigneeId,
                CreatedById = card.CreatedById,
                IsArchived = card.IsArchived,
                CoverColor = card.CoverColor,
                CreatedAt = card.CreatedAt,
                UpdatedAt = card.UpdatedAt
            };
        }

        public async Task<CardDto> CreateCardAsync(CreateCardDto createDto)
        {
            var maxPosition = await _repository.FindMaxPositionByListIdAsync(createDto.ListId);
            var newPosition = maxPosition.HasValue ? maxPosition.Value + 1 : 0;

            var card = new Card
            {
                ListId = createDto.ListId,
                BoardId = createDto.BoardId,
                Title = createDto.Title,
                Description = createDto.Description,
                Priority = createDto.Priority ?? "MEDIUM",
                Status = createDto.Status ?? "TO_DO",
                DueDate = createDto.DueDate,
                StartDate = createDto.StartDate,
                AssigneeId = createDto.AssigneeId,
                CoverColor = createDto.CoverColor,
                Position = newPosition,
                IsArchived = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var created = await _repository.CreateCardAsync(card);
            return MapToDto(created);
        }

        public async Task<CardDto> GetCardByIdAsync(int cardId)
        {
            var card = await _repository.FindByCardIdAsync(cardId) ?? throw new Exception("Card not found");
            return MapToDto(card);
        }

        public async Task<List<CardDto>> GetCardsByListAsync(int listId)
        {
            var cards = await _repository.FindByListIdOrderByPositionAsync(listId);
            return cards.Select(MapToDto).ToList();
        }

        public async Task<List<CardDto>> GetCardsByBoardAsync(int boardId)
        {
            var cards = await _repository.FindByBoardIdAsync(boardId);
            return cards.Where(c => !c.IsArchived).OrderBy(c => c.Position).Select(MapToDto).ToList();
        }

        public async Task<List<CardDto>> GetCardsByAssigneeAsync(int assigneeId)
        {
            var cards = await _repository.FindByAssigneeIdAsync(assigneeId);
            return cards.Where(c => !c.IsArchived).Select(MapToDto).ToList();
        }

        public async Task<CardDto> UpdateCardAsync(int cardId, UpdateCardDto updateDto)
        {
            var card = await _repository.FindByCardIdAsync(cardId) ?? throw new Exception("Card not found");

            if (updateDto.Title != null) card.Title = updateDto.Title;
            if (updateDto.Description != null) card.Description = updateDto.Description;
            if (updateDto.Priority != null) card.Priority = updateDto.Priority;
            if (updateDto.Status != null) card.Status = updateDto.Status;
            if (updateDto.DueDate != null) card.DueDate = updateDto.DueDate;
            if (updateDto.StartDate != null) card.StartDate = updateDto.StartDate;
            if (updateDto.CoverColor != null) card.CoverColor = updateDto.CoverColor;

            card.UpdatedAt = DateTime.UtcNow;

            var updated = await _repository.UpdateCardAsync(card);
            return MapToDto(updated);
        }

        public async Task<CardDto> MoveCardAsync(int cardId, MoveCardDto moveDto)
        {
            var card = await _repository.FindByCardIdAsync(cardId) ?? throw new Exception("Card not found");

            // Moving to another list
            if (card.ListId != moveDto.TargetListId)
            {
                // Remove from old list - we won't compact old list for simplicity, or we could.
                card.ListId = moveDto.TargetListId;
            }

            // Find max position in target list if position is beyond bounds, else just put it at end.
            // For robust dragging, UI usually passes exact reordered array via ReorderCardsAsync.
            // If MoveCard is single action, we append.
            var maxPosition = await _repository.FindMaxPositionByListIdAsync(moveDto.TargetListId);
            card.Position = maxPosition.HasValue ? maxPosition.Value + 1 : 0;
            
            // If target position is specified differently, handled by Reorder.

            card.UpdatedAt = DateTime.UtcNow;
            var updated = await _repository.UpdateCardAsync(card);
            return MapToDto(updated);
        }

        public async Task ReorderCardsAsync(int listId, ReorderCardsDto reorderDto)
        {
            var cards = await _repository.FindByListIdAsync(listId);
            var cardDict = cards.ToDictionary(c => c.CardId);

            var toUpdate = new List<Card>();
            for (int i = 0; i < reorderDto.CardIds.Count; i++)
            {
                var id = reorderDto.CardIds[i];
                if (cardDict.TryGetValue(id, out var card))
                {
                    if (card.Position != i)
                    {
                        card.Position = i;
                        card.UpdatedAt = DateTime.UtcNow;
                        toUpdate.Add(card);
                    }
                }
            }

            if (toUpdate.Any())
            {
                await _repository.BatchUpdatePositionsAsync(toUpdate);
            }
        }

        public async Task ArchiveCardAsync(int cardId)
        {
            var card = await _repository.FindByCardIdAsync(cardId) ?? throw new Exception("Card not found");
            card.IsArchived = true;
            card.UpdatedAt = DateTime.UtcNow;
            await _repository.UpdateCardAsync(card);
        }

        public async Task UnarchiveCardAsync(int cardId)
        {
            var card = await _repository.FindByCardIdAsync(cardId) ?? throw new Exception("Card not found");
            card.IsArchived = false;
            card.UpdatedAt = DateTime.UtcNow;
            await _repository.UpdateCardAsync(card);
        }

        public async Task DeleteCardAsync(int cardId)
        {
            await _repository.DeleteCardAsync(cardId);
        }

        public async Task<CardDto> SetAssigneeAsync(int cardId, AssignCardDto assignDto)
        {
            var card = await _repository.FindByCardIdAsync(cardId) ?? throw new Exception("Card not found");
            card.AssigneeId = assignDto.AssigneeId;
            card.UpdatedAt = DateTime.UtcNow;
            var updated = await _repository.UpdateCardAsync(card);
            return MapToDto(updated);
        }

        public async Task<List<CardDto>> GetOverdueCardsAsync()
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var cards = await _repository.FindByDueDateBeforeAsync(today);
            return cards.Select(MapToDto).ToList();
        }
    }
}
