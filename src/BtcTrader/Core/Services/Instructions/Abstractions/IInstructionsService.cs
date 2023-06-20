using BtcTrader.Contracts.RequestModels;
using BtcTrader.Contracts.RequestModels.V1.Instructions;
using BtcTrader.Contracts.ResponseModels.V1.Instructions;
using BtcTrader.Domain.Instructions;

namespace BtcTrader.Core.Services.Instructions.Abstractions;

public interface IInstructionsService
{
    Task<Result<GetAllInstructionsResponseModel>> GetAllInstruction(PaginationFilterRequestModel requestModel);
    Task<Result<bool>> InsertInstructionAsync(CreateInstructionRequestModelV1 requestModelV1);
    Task<Result<bool>> UpdateInstructionAsync(UpdateInstructionRequestModelV1 requestModelV1, Guid id);
    Task<Result<bool>> DeleteInstructionAsync(Guid id,Guid userId);
    Task<Result<Instruction>> GetByIdAsync(Guid id);
    Task<Result<bool>> ActivateInstructionAsync(Guid id, Guid userId);
    Task<Result<bool>> DeActivateInstructionAsync(Guid id, Guid userId);
    Task<Result<GetAllInstructionAuditsResponseModel>> GetAllInstructionAudit(PaginationFilterRequestModel filterRequestModel);
    Task<Result<InstructionAudit>> GetInstructionAuditByIdAsync(Guid id);
}