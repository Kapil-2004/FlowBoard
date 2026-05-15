using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FlowBoard_CardService.Data;
using FlowBoard_CardService.Models;
using Microsoft.EntityFrameworkCore;

namespace FlowBoard_CardService.Repositories
{
    public class CardRepository : ICardRepository
    {
        private readonly CardDbContext _context;

        public CardRepository(CardDbContext context)
        {
            _context = context;
        }

        public async Task<Card> CreateCardAsync(Card card)
        {
            _context.Cards.Add(card);
            await _context.SaveChangesAsync();
            return card;
        }

        public async Task<Card?> FindByCardIdAsync(int cardId)
        {
            return await _context.Cards.FindAsync(cardId);
        }

        public async Task<List<Card>> FindByListIdAsync(int listId)
        {
            return await _context.Cards
                .Where(c => c.ListId == listId)
                .ToListAsync();
        }

        public async Task<List<Card>> FindByBoardIdAsync(int boardId)
        {
            return await _context.Cards
                .Where(c => c.BoardId == boardId)
                .ToListAsync();
        }

        public async Task<List<Card>> FindByAssigneeIdAsync(int assigneeId)
        {
            return await _context.Cards
                .Where(c => c.AssigneeId == assigneeId)
                .ToListAsync();
        }

        public async Task<List<Card>> FindByListIdOrderByPositionAsync(int listId)
        {
            return await _context.Cards
                .Where(c => c.ListId == listId && !c.IsArchived)
                .OrderBy(c => c.Position)
                .ToListAsync();
        }

        public async Task<List<Card>> FindByDueDateBeforeAsync(DateOnly date)
        {
            return await _context.Cards
                .Where(c => c.DueDate != null && c.DueDate < date && !c.IsArchived && c.Status != "DONE")
                .ToListAsync();
        }

        public async Task<List<Card>> FindByPriorityAsync(string priority)
        {
            return await _context.Cards
                .Where(c => c.Priority == priority)
                .ToListAsync();
        }

        public async Task<List<Card>> FindByStatusAsync(string status)
        {
            return await _context.Cards
                .Where(c => c.Status == status)
                .ToListAsync();
        }

        public async Task<int> CountByListIdAsync(int listId)
        {
            return await _context.Cards.CountAsync(c => c.ListId == listId && !c.IsArchived);
        }

        public async Task<int?> FindMaxPositionByListIdAsync(int listId)
        {
            return await _context.Cards
                .Where(c => c.ListId == listId)
                .MaxAsync(c => (int?)c.Position);
        }

        public async Task<Card> UpdateCardAsync(Card card)
        {
            _context.Cards.Update(card);
            await _context.SaveChangesAsync();
            return card;
        }

        public async Task DeleteCardAsync(int cardId)
        {
            var card = await _context.Cards.FindAsync(cardId);
            if (card != null)
            {
                _context.Cards.Remove(card);
                await _context.SaveChangesAsync();
            }
        }

        public async Task BatchUpdatePositionsAsync(List<Card> cards)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.Cards.UpdateRange(cards);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
