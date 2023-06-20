using BtcTrader.Domain.Instructions;

namespace BtcTrader.Contracts.ResponseModels.V1.Instructions;

public class GetAllInstructionAuditsResponseModel
{
    public List<InstructionAudit> InstructionAudits { get; set; }
    public int TotalCount { get; set; }
}
