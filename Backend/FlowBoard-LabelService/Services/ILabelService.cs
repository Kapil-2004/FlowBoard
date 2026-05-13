using System.Collections.Generic;
using System.Threading.Tasks;
using FlowBoard_LabelService.Models;

namespace FlowBoard_LabelService.Services
{
    public interface ILabelService
    {
        // Labels
        Task<Label> CreateLabel(Label label);
        Task<List<Label>> GetLabelsByBoard(int boardId);
        Task<Label> GetLabelById(int labelId);
        Task<Label> UpdateLabel(int labelId, Label label);
        Task<bool> DeleteLabel(int labelId);
        
        // Card-Label Association
        Task AddLabelToCard(int cardId, int labelId);
        Task RemoveLabelFromCard(int cardId, int labelId);
        Task<List<Label>> GetLabelsForCard(int cardId);

        // Checklists
        Task<Checklist> CreateChecklist(Checklist checklist);
        Task<Checklist> AddItem(int checklistId, ChecklistItem item);
        Task<ChecklistItem> ToggleItem(int itemId);
        Task<bool> DeleteChecklist(int checklistId);
        Task<List<Checklist>> GetChecklistsByCard(int cardId);
        Task<double> GetChecklistProgress(int cardId);
    }
}
