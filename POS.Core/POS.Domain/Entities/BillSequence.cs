namespace POS.Domain.Entities
{
    public class BillSequence
    {
        public int SequenceId { get; set; }
        public string Prefix { get; set; } = string.Empty;
        public int Year { get; set; }
        public int Month { get; set; }
    }
}
