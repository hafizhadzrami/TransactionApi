namespace TransactionApi.Models;

public class ItemDetail
{
    public string? Partneritemref { get; set; }  // M, not null/empty
    public string? Name { get; set; }            // M, not null/empty
    public int? Qty { get; set; }                // M, <= 5
    public long? Unitprice { get; set; }         // M, cents, positive
}
