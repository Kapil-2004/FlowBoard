using System.Collections.Generic;
using System.Threading.Tasks;
using FlowBoard_CardService.DTOs;

namespace FlowBoard_CardService.Services
{
    public interface ICardService
    {
        Task<CardDto> CreateCardAsync(CreateCardDto createDto);
        Task<CardDto> GetCardByIdAsync(int cardId);
        Task<List<CardDto>> GetCardsByListAsync(int listId);
        Task<List<CardDto>> GetCardsByBoardAsync(int boardId);
        Task<List<CardDto>> GetCardsByAssigneeAsync(int assigneeId);
        Task<CardDto> UpdateCardAsync(int cardId, UpdateCardDto updateDto);
        Task<CardDto> MoveCardAsync(int cardId, MoveCardDto moveDto);
        Task ReorderCardsAsync(int listId, ReorderCardsDto reorderDto);
        Task ArchiveCardAsync(int cardId);
        Task UnarchiveCardAsync(int cardId);
        Task DeleteCardAsync(int cardId);
        Task<CardDto> SetAssigneeAsync(int cardId, AssignCardDto assignDto);
        Task<List<CardDto>> GetOverdueCardsAsync();
    }
}
