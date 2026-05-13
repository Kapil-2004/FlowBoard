using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FlowBoard_CardService.Models;

namespace FlowBoard_CardService.Repositories
{
    public interface ICardRepository
    {
        Task<Card> CreateCardAsync(Card card);
        Task<Card?> FindByCardIdAsync(int cardId);
        Task<List<Card>> FindByListIdAsync(int listId);
        Task<List<Card>> FindByBoardIdAsync(int boardId);
        Task<List<Card>> FindByAssigneeIdAsync(int assigneeId);
        Task<List<Card>> FindByListIdOrderByPositionAsync(int listId);
        Task<List<Card>> FindByDueDateBeforeAsync(DateOnly date);
        Task<List<Card>> FindByPriorityAsync(string priority);
        Task<List<Card>> FindByStatusAsync(string status);
        Task<int> CountByListIdAsync(int listId);
        Task<int?> FindMaxPositionByListIdAsync(int listId);
        Task<Card> UpdateCardAsync(Card card);
        Task DeleteCardAsync(int cardId);
        Task BatchUpdatePositionsAsync(List<Card> cards);
    }
}
