using BtcTrader.Domain.Instructions;

namespace BtcTrader.Contracts.ResponseModels.V1.Instructions;

public class GetAllInstructionsResponseModel
{
    public List<Instruction> Instructions { get; set; }
    public int TotalCount { get; set; }
}