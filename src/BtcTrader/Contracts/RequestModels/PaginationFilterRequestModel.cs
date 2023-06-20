namespace BtcTrader.Contracts.RequestModels;

public class PaginationFilterRequestModel
{
    public int Page { get; set; }
    public int Size { get; set; }
        
    public Dictionary<string,string>? FilterBy { get; set; }

    public PaginationFilterRequestModel()
    {
        this.Page = 1;
        this.Size = 100;
    }
    public PaginationFilterRequestModel(int pageNumber, int pageSize)
    {
        this.Page = pageNumber < 1 ? 1 : pageNumber;
        this.Size = pageSize > 100 ? 100 : pageSize;
    }
}