using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FlowBoard_LabelService.Data;
using FlowBoard_LabelService.Models;
using Microsoft.EntityFrameworkCore;

namespace FlowBoard_LabelService.Services
{
    public class LabelServiceImpl : ILabelService
    {
        private readonly LabelDbContext _context;

        public LabelServiceImpl(LabelDbContext context)
        {
            _context = context;
        }

        public async Task<Label> CreateLabel(Label label)
        {
            _context.Labels.Add(label);
            await _context.SaveChangesAsync();
            return label;
        }

        public async Task<List<Label>> GetLabelsByBoard(int boardId)
        {
            return await _context.Labels.Where(l => l.BoardId == boardId).ToListAsync();
        }

        public async Task<Label> GetLabelById(int labelId)
        {
            return await _context.Labels.FindAsync(labelId);
        }

        public async Task<Label> UpdateLabel(int labelId, Label label)
        {
            var existing = await _context.Labels.FindAsync(labelId);
            if (existing == null) return null;

            existing.Name = label.Name;
            existing.Color = label.Color;
            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteLabel(int labelId)
        {
            var label = await _context.Labels.FindAsync(labelId);
            if (label == null) return false;

            _context.Labels.Remove(label);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task AddLabelToCard(int cardId, int labelId)
        {
            var exists = await _context.CardLabels.AnyAsync(cl => cl.CardId == cardId && cl.LabelId == labelId);
            if (!exists)
            {
                _context.CardLabels.Add(new CardLabel { CardId = cardId, LabelId = labelId });
                await _context.SaveChangesAsync();
            }
        }

        public async Task RemoveLabelFromCard(int cardId, int labelId)
        {
            var association = await _context.CardLabels.FirstOrDefaultAsync(cl => cl.CardId == cardId && cl.LabelId == labelId);
            if (association != null)
            {
                _context.CardLabels.Remove(association);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Label>> GetLabelsForCard(int cardId)
        {
            return await _context.CardLabels
                .Where(cl => cl.CardId == cardId)
                .Select(cl => cl.Label)
                .ToListAsync();
        }

        public async Task<Checklist> CreateChecklist(Checklist checklist)
        {
            _context.Checklists.Add(checklist);
            await _context.SaveChangesAsync();
            return checklist;
        }

        public async Task<Checklist> AddItem(int checklistId, ChecklistItem item)
        {
            var checklist = await _context.Checklists.Include(c => c.Items).FirstOrDefaultAsync(c => c.ChecklistId == checklistId);
            if (checklist == null) return null;

            item.ChecklistId = checklistId;
            _context.ChecklistItems.Add(item);
            await _context.SaveChangesAsync();
            return checklist;
        }

        public async Task<ChecklistItem> ToggleItem(int itemId)
        {
            var item = await _context.ChecklistItems.FindAsync(itemId);
            if (item == null) return null;

            item.IsCompleted = !item.IsCompleted;
            await _context.SaveChangesAsync();
            return item;
        }

        public async Task<bool> DeleteChecklist(int checklistId)
        {
            var checklist = await _context.Checklists.FindAsync(checklistId);
            if (checklist == null) return false;

            _context.Checklists.Remove(checklist);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Checklist>> GetChecklistsByCard(int cardId)
        {
            return await _context.Checklists
                .Include(c => c.Items)
                .Where(c => c.CardId == cardId)
                .OrderBy(c => c.Position)
                .ToListAsync();
        }

        public async Task<double> GetChecklistProgress(int cardId)
        {
            var items = await _context.Checklists
                .Where(c => c.CardId == cardId)
                .SelectMany(c => c.Items)
                .ToListAsync();

            if (!items.Any()) return 0;

            int completed = items.Count(i => i.IsCompleted);
            return (double)completed / items.Count * 100;
        }
    }
}
