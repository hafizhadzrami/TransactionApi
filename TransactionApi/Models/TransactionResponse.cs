namespace TransactionApi.Models
{
    public class TransactionResponse
    {
        public string ResultMessage { get; set; }
        public long Timestamp { get; set; }
        public string Signature { get; set; }
    }
}
