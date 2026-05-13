using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FlowBoard_LabelService.Data;
using FlowBoard_LabelService.Models;
using FlowBoard_LabelService.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FlowBoard_LabelService.Tests
{
    public class LabelServiceTests
    {
        private LabelDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<LabelDbContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                .Options;
            return new LabelDbContext(options);
        }

        [Fact]
        public async Task CreateLabel_ShouldAddLabel()
        {
            using var context = GetDbContext();
            var service = new LabelServiceImpl(context);
            var label = new Label { Name = "Urgent", Color = "#FF0000", BoardId = 1 };

            var result = await service.CreateLabel(label);

            Assert.Equal("Urgent", result.Name);
            Assert.Single(context.Labels);
        }

        [Fact]
        public async Task GetChecklistProgress_ShouldReturnCorrectPercentage()
        {
            using var context = GetDbContext();
            var service = new LabelServiceImpl(context);
            
            var checklist = new Checklist { CardId = 1, Title = "Tasks" };
            context.Checklists.Add(checklist);
            await context.SaveChangesAsync();

            context.ChecklistItems.AddRange(new List<ChecklistItem>
            {
                new ChecklistItem { ChecklistId = checklist.ChecklistId, Text = "Step 1", IsCompleted = true },
                new ChecklistItem { ChecklistId = checklist.ChecklistId, Text = "Step 2", IsCompleted = false }
            });
            await context.SaveChangesAsync();

            var progress = await service.GetChecklistProgress(1);

            Assert.Equal(50.0, progress);
        }
    }
}
