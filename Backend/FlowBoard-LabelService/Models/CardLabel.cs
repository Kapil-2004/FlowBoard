namespace FlowBoard_LabelService.Models
{
    public class CardLabel
    {
        public int CardId { get; set; }
        public int LabelId { get; set; }

        // Navigation
        public Label Label { get; set; }
    }
}
