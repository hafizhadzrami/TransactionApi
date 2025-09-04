namespace TransactionApi.Models
{
    public class TransactionRequest
    {
        public string PartnerKey { get; set; }
        public string PartnerRefNo { get; set; }
        public string PartnerPassword { get; set; }
        public decimal TotalAmount { get; set; }
        public List<TransactionItem> Items { get; set; }
        public long Timestamp { get; set; }
        public string Sig { get; set; }
    }

    public class TransactionItem
    {
        public string PartnerItemRef { get; set; }
        public string Name { get; set; }
        public int Qty { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
