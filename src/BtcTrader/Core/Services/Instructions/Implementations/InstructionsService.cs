using AutoMapper;
using BtcTrader.Contracts.RequestModels;
using BtcTrader.Contracts.RequestModels.V1.Instructions;
using BtcTrader.Contracts.ResponseModels.V1.Instructions;
using BtcTrader.Core.Services.Instructions.Abstractions;
using BtcTrader.Data.Repositories.Interfaces;
using BtcTrader.Domain.Instructions;
using BtcTrader.Domain.Instructions.Events;
using BtcTrader.Infrastructure.Services.Abstractions;
using MassTransit;

namespace BtcTrader.Core.Services.Instructions.Implementations;

public class InstructionsService : IInstructionsService
{
    private readonly IRepository<Instruction> _instructionRepository;
    private readonly IRepository<InstructionAudit> _instructionAuditRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<InstructionsService> _logger;
    private readonly IQueryService<Instruction> _instructionQueryService;
    private readonly IQueryService<InstructionAudit> _auditQueryService;
    private readonly IBus _bus;


    public InstructionsService(IMapper mapper, 
        ILogger<InstructionsService> logger,
        IQueryService<Instruction> instructionQueryService, 
        IBus bus, IQueryService<InstructionAudit> auditQueryService, 
        IRepository<Instruction> instructionRepository,
        IRepository<InstructionAudit> instructionAuditRepository)
    {
        _mapper = mapper;
        _logger = logger;
        _instructionQueryService = instructionQueryService;
        _bus = bus;
        _auditQueryService = auditQueryService;
        _instructionRepository = instructionRepository;
        _instructionAuditRepository = instructionAuditRepository;
    }

    public Task<Result<GetAllInstructionsResponseModel>> GetAllInstruction(PaginationFilterRequestModel requestModel)
    {
        var response = new GetAllInstructionsResponseModel();
        var queryFilters = _instructionQueryService.GetExpression(requestModel.FilterBy);
        var skipCount = (requestModel.Page - 1) * requestModel.Size;
        var instructions = queryFilters != null ? _instructionRepository.FilterBy(queryFilters)
            .OrderByDescending(x=>x.InstructionDate)
            .Skip(skipCount)
            .Take(requestModel.Size).ToList() :  _instructionRepository.AsQueryable()
            .OrderByDescending(x => x.InstructionDate)
            .Skip(skipCount)
            .Take(requestModel.Size)
            .ToList();
        
        _mapper.Map( instructions, response);
        return Task.FromResult(Result<GetAllInstructionsResponseModel>.Success(response));

    }

    public async Task<Result<bool>> InsertInstructionAsync(CreateInstructionRequestModelV1 requestModelV1)
    {
        var validationResult = requestModelV1.ValidateRequest();
        if (!validationResult.IsValid)
        {
            var validationErrors = string.Join(",", validationResult.Errors);
            _logger.LogInformation($"Error Occured While Validating Request Model {requestModelV1.GetType().FullName} : {validationErrors}");
            return Result<bool>.Failure($"Error Occured While Validating Request Model {requestModelV1.GetType().FullName} : {validationErrors}");
        }
        if (await IsActiveInstructionExistOnUser(requestModelV1))
        {
            _logger.LogInformation("There is already active instruction on related user.");
            return Result<bool>.Failure("There is already active instruction on related user.");
        }
        
        var instruction = new Instruction();
        instruction = _mapper.Map(requestModelV1, instruction);
        var isSuccess = await _instructionRepository.InsertAsync(instruction);
        
        if (!isSuccess)
        {
            _logger.LogInformation($"Error Occured While Creating {instruction.GetType().FullName} with user info : {requestModelV1.UserId}");
            return Result<bool>.Failure($"Error Occured While Creating {instruction.GetType().FullName}");
        }

        var instructionCreatedEvent = new InstructionCreatedEvent();
        _mapper.Map(instruction, instructionCreatedEvent);
        _bus.Publish<IInstructionCreatedEvent>(instructionCreatedEvent);
        return Result<bool>.Success(isSuccess);
    }
   
    public async Task<Result<bool>> UpdateInstructionAsync(UpdateInstructionRequestModelV1 requestModelV1, Guid id)
    {
        var validationResult = requestModelV1.ValidateRequest();
        if (!validationResult.IsValid)
        {
            var validationErrors = string.Join(",", validationResult.Errors);
            _logger.LogInformation($"Error Occured While Validating Request Model {requestModelV1.GetType().FullName} : {validationErrors}");
            return Result<bool>.Failure($"Error Occured While Validating Request Model {requestModelV1.GetType().FullName} : {validationErrors}");
        }

        var instruction = await _instructionRepository.GetByIDAsync(id);
        if (instruction is null)
        {
            _logger.LogInformation($"Instruction could not found with related id : {id}");
            return Result<bool>.Failure("Instruction could not found with related id");
        }

        if (!IsUserEligibleOnInstruction(requestModelV1.UserId,instruction.UserId))
        {
            _logger.LogInformation($"User {requestModelV1.UserId} is not eligible to update instruction with id {instruction.Id}");
            return Result<bool>.Failure("User is not eligible to update instruction");
        }
        var instructionUpdatedEvent = new InstructionUpdatedEvent
        {
            OldAmount = instruction.Amount
        };

        _mapper.Map(requestModelV1, instruction);
        var isSuccess = _instructionRepository.Update(instruction);
        if (!isSuccess)
        {
            _logger.LogInformation($"Error Occured While Updating {instruction.GetType().FullName} with user info : {requestModelV1.UserId}");
            return Result<bool>.Failure($"Error Occured While Updating {instruction.GetType().FullName}");
        }
        _mapper.Map(instruction, instructionUpdatedEvent);
        instructionUpdatedEvent.NewAmount = requestModelV1.Amount != null ? requestModelV1.Amount : null;
        _bus.Publish<IInstructionUpdatedEvent>(instructionUpdatedEvent);
        return Result<bool>.Success(isSuccess);
    }

    public async Task<Result<bool>> DeleteInstructionAsync(Guid id,Guid userId)
    {
        var instruction = await _instructionRepository.GetByIDAsync(id);
        if (instruction is null)
        {
            _logger.LogInformation($"Instruction could not found with related id : {id}");
            return Result<bool>.Failure("Instruction could not found with related id");
        }

        if (!IsUserEligibleOnInstruction(userId,instruction.UserId))
        {
            _logger.LogInformation($"User {userId} is not eligible to delete instruction with id {instruction.Id}");
            return Result<bool>.Failure($"User is not eligible to delete instruction");
        }
        
        var isSuccess = _instructionRepository.Remove(instruction);
        if (!isSuccess)
        {
            _logger.LogInformation($"Error Occured While deleting {instruction.GetType().FullName} with user info : {id}");
             return Result<bool>.Failure("Failed to delete the instruction");
        }

        var instructionDeletedEvent = new InstructionDeletedEvent();
        _mapper.Map(instruction, instructionDeletedEvent);

        _bus.Publish<IInstructionDeletedEvent>(instructionDeletedEvent);
        return Result<bool>.Success(isSuccess);
    }

    public async Task<Result<Instruction>> GetByIdAsync(Guid id)
    {
        var instruction = await _instructionRepository.GetByIDAsync(id);
        if (instruction != null) return Result<Instruction>.Success(instruction);
        _logger.LogInformation($"Instruction could not found with related id : {id}");
        return Result<Instruction>.Failure("Instruction could not found with related id");
    }

    public async Task<Result<bool>> ActivateInstructionAsync(Guid id, Guid userId)
    {
        var instruction = await _instructionRepository.GetByIDAsync(id);
        if (instruction is null)
        {
            _logger.LogInformation($"Instruction could not found with related id : {id}");
            return Result<bool>.Failure("Instruction could not found with related id");
        }

        if (!IsUserEligibleOnInstruction(userId,instruction.UserId))
        {
            _logger.LogInformation($"User {userId} is not eligible to activate instruction with id {instruction.Id}");
            return Result<bool>.Failure($"User is not eligible to activate instruction");
        }

        if (instruction.IsActive)
        {
            _logger.LogInformation($"Instruction which is already active cannot be activated with related id : {id}");
            return Result<bool>.Failure("Instruction which is already active cannot be activated with related id");
        }

        instruction.IsActive = true;
        var isSuccess = _instructionRepository.Update(instruction);
        if (!isSuccess)
        {
            _logger.LogInformation($"Error Occured While activating {instruction.GetType().FullName} with user info : {id}");
            return Result<bool>.Failure("Failed to activate the instruction");
        }

        var instructionActivatedEvent = new InstructionActivatedEvent();
        _mapper.Map(instruction, instructionActivatedEvent);
        _bus.Publish<IInstructionActivatedEvent>(instructionActivatedEvent);
        return Result<bool>.Success(isSuccess);
    }

    public async Task<Result<bool>> DeActivateInstructionAsync(Guid id, Guid userId)
    {
        var instruction = await _instructionRepository.GetByIDAsync(id);
        if (instruction is null)
        {
            _logger.LogInformation($"Instruction could not found with related id : {id}");
            return Result<bool>.Failure("Instruction could not found with related id");
        }

        if (!IsUserEligibleOnInstruction(userId,instruction.UserId))
        {
            _logger.LogInformation($"User {userId} is not eligible to deactivate instruction with id {instruction.Id}");
            return Result<bool>.Failure($"User is not eligible to deactivate instruction");
        }

        if (!instruction.IsActive)
        {
            _logger.LogInformation($"Instruction which is already deactive cannot be deactivated with related id : {id}");
            return Result<bool>.Failure($"Instruction which is already deactive cannot be deactivated with related id");
        }

        instruction.IsActive = false;
        var isSuccess = _instructionRepository.Update(instruction);
        if (!isSuccess)
        {
            _logger.LogInformation($"Error Occured While deactivating {instruction.GetType().FullName} with user info : {id}");
            return Result<bool>.Failure("Failed to activate the instruction");
        }

        var instructionDeActivatedEvent = new InstructionDeActivatedEvent();
        _mapper.Map(instruction, instructionDeActivatedEvent);

        _bus.Publish<InstructionDeActivatedEvent>(instructionDeActivatedEvent);
        return Result<bool>.Success(isSuccess);
    }

    public Task<Result<GetAllInstructionAuditsResponseModel>> GetAllInstructionAudit(PaginationFilterRequestModel filterRequestModel)
    {
        var response = new GetAllInstructionAuditsResponseModel();
        var queryFilters = _auditQueryService.GetExpression(filterRequestModel.FilterBy);
       
        var skipCount = (filterRequestModel.Page - 1) * filterRequestModel.Size;
        var instructionsAudits = queryFilters != null ?   _instructionAuditRepository.FilterBy(queryFilters)
            .OrderByDescending(x=>x.InstructionDate)
            .Skip(skipCount)
            .Take(filterRequestModel.Size).ToList() : _instructionAuditRepository.AsQueryable()
            .OrderByDescending(x => x.InstructionDate)
            .Skip(skipCount)
            .Take(filterRequestModel.Size)
            .ToList();
        _mapper.Map(instructionsAudits, response);
        return Task.FromResult(Result<GetAllInstructionAuditsResponseModel>.Success(response));
    }

    public async Task<Result<InstructionAudit>> GetInstructionAuditByIdAsync(Guid id)
    {
        var instruction = await _instructionAuditRepository.GetByIDAsync(id);
        if (instruction is not null) return Result<InstructionAudit>.Success(instruction);
        _logger.LogInformation($"Instruction audit could not found with related id : {id}");
        return Result<InstructionAudit>.Failure("Instruction audit could not found with related id");
    }


    private async Task<bool> IsActiveInstructionExistOnUser(CreateInstructionRequestModelV1 requestModelV1)
    {
        var instruction = await _instructionRepository.FindOneAsync(x => x.UserId == requestModelV1.UserId && x.IsActive);
        return instruction is not null;
    }
   
   
    private bool IsUserEligibleOnInstruction(Guid userId, Guid instructionUserId)
    {
        return instructionUserId == userId;
    }
}