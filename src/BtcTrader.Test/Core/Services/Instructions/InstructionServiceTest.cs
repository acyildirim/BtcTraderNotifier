using System.Linq.Expressions;
using AutoMapper;
using BtcTrader.Contracts.RequestModels;
using BtcTrader.Contracts.RequestModels.V1.Instructions;
using BtcTrader.Core.MappingProfiles;
using BtcTrader.Core.Services.Instructions.Implementations;
using BtcTrader.Data.Repositories.Interfaces;
using BtcTrader.Domain.Instructions;
using BtcTrader.Infrastructure.Services.Abstractions;
using BtcTrader.Test.Builders.Instructions;
using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;

namespace BtcTrader.Test.Core.Services.Instructions;

public class InstructionServiceTest
{
    private readonly Mock<IRepository<Instruction>> _instructionRepository = new();
    private readonly Mock<IRepository<InstructionAudit>> _instructionAuditRepository = new();
    private readonly Mock<ILogger<InstructionsService>> _logger = new();
    private readonly Mock<IQueryService<Instruction>> _instructionQueryService = new();
    private readonly Mock<IQueryService<InstructionAudit>> _auditQueryService = new();
    private readonly Mock<IBus> _bus = new();

    [Fact]
    public void GetAllInstructionAsync_Success_AccordingToPaginationFilterValue()
    {
        //Arrange
        var paginationFilterRequestModel = new PaginationFilterRequestModel(1, 1);
        var instructions = InstructionBuilder.MockInstruction().Generate(5).AsQueryable();
        _instructionQueryService.Setup(x =>
            x.GetExpression(It.IsAny<Dictionary<string, string>>())).Returns((Expression<Func<Instruction, bool>>)null);
        _instructionRepository.Setup(x => x.AsQueryable()).Returns(instructions);
        var mapper = new MapperConfiguration(cfg =>
            cfg.AddProfiles(new List<Profile>()
                {
                    new InstructionMappingProfiles(), new InstructionEventsMappingProfile(), new UserMappingProfiles()
                })).CreateMapper();
        var _instructionService =
            new InstructionsService(mapper, _logger.Object, _instructionQueryService.Object,
                _bus.Object, _auditQueryService.Object, _instructionRepository.Object,
                _instructionAuditRepository.Object);

        //Act
        var actualResponse = _instructionService.GetAllInstruction(paginationFilterRequestModel).Result.Value
            .Instructions;

        //Assert
        Assert.NotEmpty(actualResponse);
        Assert.Equal(paginationFilterRequestModel.Size, actualResponse.Count);
    }

    [Fact]
    public void GetAllInstructionAuditsAsync_Success_AccordingToPaginationFilterValue()
    {
        //Arrange
        var paginationFilterRequestModel = new PaginationFilterRequestModel(1, 1);
        var instructionsAudits = InstructionAuditBuilder.MockInstructionAudit().Generate(5).AsQueryable();
        _auditQueryService.Setup(x =>
            x.GetExpression(It.IsAny<Dictionary<string, string>>())).Returns((Expression<Func<InstructionAudit, bool>>)null);
        _instructionAuditRepository.Setup(x => x.AsQueryable()).Returns(instructionsAudits);
        var mapper = new MapperConfiguration(cfg =>
            cfg.AddProfiles(new List<Profile>()
            {
                new InstructionMappingProfiles(), new InstructionEventsMappingProfile(), new UserMappingProfiles()
            })).CreateMapper();
        var _instructionService =
            new InstructionsService(mapper, _logger.Object, _instructionQueryService.Object,
                _bus.Object, _auditQueryService.Object, _instructionRepository.Object,
                _instructionAuditRepository.Object);

        //Act
        var actualResponse = _instructionService.GetAllInstructionAudit(paginationFilterRequestModel).Result.Value
            .InstructionAudits;

        //Assert
        Assert.NotEmpty(actualResponse);
        Assert.Equal(paginationFilterRequestModel.Size, actualResponse.Count);
    }
    
    [Fact]
    public void InsertInstructionAsync_Failed_AmountLessThan100()
    {
        //Arrange
        var mapper = new MapperConfiguration(cfg =>
            cfg.AddProfiles(new List<Profile>()
            {
                new InstructionMappingProfiles(), new InstructionEventsMappingProfile(), new UserMappingProfiles()
            })).CreateMapper();
        var createInstructionRequestModelV1 = new CreateInstructionRequestModelV1
        {
            UserId = Guid.NewGuid(),
            InstructionType = "Bitcoin",
            CronExpression = "*/50 * * * * *",
            Amount = 80,
            NotificationChannel = new List<string>()
            {
                "Email"
            },
            InstructionDate = DateTime.Parse("2023-05-01T13:21:09.565Z")
            
        };
        var stringWriter = new StringWriter();
        Console.SetOut(stringWriter);
       
        var _instructionService =
            new InstructionsService(mapper, _logger.Object, _instructionQueryService.Object,
                _bus.Object, _auditQueryService.Object, _instructionRepository.Object,
                _instructionAuditRepository.Object);

        //Act
        var validationResult = createInstructionRequestModelV1.ValidateRequest();
        var actualResponse = _instructionService.InsertInstructionAsync(createInstructionRequestModelV1).Result;
        Console.WriteLine($"Error Occured While Validating Request Model {createInstructionRequestModelV1.GetType().FullName} : {validationResult}");
        var actualLog = stringWriter.ToString().Split("\n")[0];
        var expectedResult = $"Error Occured While Validating Request Model " +
                             $"BtcTrader.Contracts.RequestModels.V1.Instructions.CreateInstructionRequestModelV1 :" +
                             $" 'Amount' must be greater than or equal to '100'.";
        //Assert
        Assert.Equal(expectedResult, actualLog);
        Assert.False(actualResponse.IsSuccess);
    }
    [Fact]
    public void InsertInstructionAsync_Failed_DateIsInvalid()
    {
        //Arrange
        var mapper = new MapperConfiguration(cfg =>
            cfg.AddProfiles(new List<Profile>()
            {
                new InstructionMappingProfiles(), new InstructionEventsMappingProfile(), new UserMappingProfiles()
            })).CreateMapper();
        var createInstructionRequestModelV1 = new CreateInstructionRequestModelV1
        {
            UserId = Guid.NewGuid(),
            InstructionType = "Bitcoin",
            CronExpression = "*/50 * * * * *",
            Amount = 120,
            NotificationChannel = new List<string>()
            {
                "Email"
            },
            InstructionDate = DateTime.Parse("2023-05-29T13:21:09.565Z")
            
        };
        var stringWriter = new StringWriter();
        Console.SetOut(stringWriter);
       
        var _instructionService =
            new InstructionsService(mapper, _logger.Object, _instructionQueryService.Object,
                _bus.Object, _auditQueryService.Object, _instructionRepository.Object,
                _instructionAuditRepository.Object);

        //Act
        var validationResult = createInstructionRequestModelV1.ValidateRequest();
        var actualResponse = _instructionService.InsertInstructionAsync(createInstructionRequestModelV1).Result;
        Console.WriteLine($"Error Occured While Validating Request Model {createInstructionRequestModelV1.GetType().FullName} : {validationResult}");
        var actualLog = stringWriter.ToString().Split("\n")[0];
        var expectedResult = $"Error Occured While Validating Request Model " +
                             $"BtcTrader.Contracts.RequestModels.V1.Instructions.CreateInstructionRequestModelV1 :" +
                             $" You can only give an instructions within first 28 day of month";
        //Assert
        Assert.Equal(expectedResult, actualLog);
        Assert.False(actualResponse.IsSuccess);
    }
    [Fact]
    public void InsertInstructionAsync_Failed_UserAlreadyHaveActiveInstruction()
    {
        //Arrange
        var mapper = new MapperConfiguration(cfg =>
            cfg.AddProfiles(new List<Profile>()
            {
                new InstructionMappingProfiles(), new InstructionEventsMappingProfile(), new UserMappingProfiles()
            })).CreateMapper();
        var instruction = Task.Run(()=> InstructionBuilder.MockInstruction().Generate(1)[0]);
        var createInstructionRequestModelV1 = new CreateInstructionRequestModelV1
        {
            UserId = instruction.Result.UserId,
            InstructionType = "Bitcoin",
            CronExpression = "*/50 * * * * *",
            Amount = 120,
            NotificationChannel = new List<string>()
            {
                "Email"
            },
            InstructionDate = DateTime.Parse("2023-05-25T13:21:09.565Z")
            
        };
        var stringWriter = new StringWriter();
        Console.SetOut(stringWriter);
        _instructionRepository.Setup(x =>
            x.FindOneAsync(It.IsAny<Expression<Func<Instruction, bool>>>())).Returns(instruction);

        var _instructionService =
            new InstructionsService(mapper, _logger.Object, _instructionQueryService.Object,
                _bus.Object, _auditQueryService.Object, _instructionRepository.Object,
                _instructionAuditRepository.Object);

        //Act
        var actualResponse = _instructionService.InsertInstructionAsync(createInstructionRequestModelV1).Result;
        Console.WriteLine($"There is already active instruction on related user.");
        var actualLog = stringWriter.ToString().Split("\n")[0];
        var expectedMessage = $"There is already active instruction on related user.";
        //Assert
        Assert.Equal(expectedMessage, actualLog);
        Assert.Equal(expectedMessage,actualResponse.Error);
        Assert.False(actualResponse.IsSuccess);
    }
    [Fact]
    public void InsertInstructionAsync_Successfully()
    {
        //Arrange
        var mapper = new MapperConfiguration(cfg =>
            cfg.AddProfiles(new List<Profile>()
            {
                new InstructionMappingProfiles(), new InstructionEventsMappingProfile(), new UserMappingProfiles()
            })).CreateMapper();
        var instructionList = new List<Instruction>();
        var instruction = InstructionBuilder.MockInstruction().Generate(1)[0];
        var createInstructionRequestModelV1 = new CreateInstructionRequestModelV1
        {
            UserId = Guid.NewGuid(),
            InstructionType = "Bitcoin",
            CronExpression = "*/50 * * * * *",
            Amount = 120,
            NotificationChannel = new List<string>()
            {
                "Email"
            },
            InstructionDate = DateTime.Parse("2023-05-25T13:21:09.565Z")
            
        };
        Task<bool> FunctionInsertInstruction(Instruction instruction)
        {
            instructionList.Add(instruction);
            return Task.FromResult(true);
        }
        _instructionRepository.Setup(x => x.InsertAsync(It.IsAny<Instruction>()))
            .Returns(() => FunctionInsertInstruction(instruction));
        var _instructionService =
            new InstructionsService(mapper, _logger.Object, _instructionQueryService.Object,
                _bus.Object, _auditQueryService.Object, _instructionRepository.Object,
                _instructionAuditRepository.Object);
        //Act
        var actualResponse = _instructionService.InsertInstructionAsync(createInstructionRequestModelV1).Result;
       
        //Assert
        _instructionRepository.Verify(x=>x.InsertAsync(It.IsAny<Instruction>()), Times.Once);
        Assert.True(actualResponse.IsSuccess);
        Assert.Equal(1, instructionList.Count);
    }
    
    [Fact]
    public void InsertInstructionAsync_Failed_DbError()
    {
        //Arrange
        var mapper = new MapperConfiguration(cfg =>
            cfg.AddProfiles(new List<Profile>()
            {
                new InstructionMappingProfiles(), new InstructionEventsMappingProfile(), new UserMappingProfiles()
            })).CreateMapper();
        var instructionList = new List<Instruction>();
        var instruction = InstructionBuilder.MockInstruction().Generate(1)[0];
        var createInstructionRequestModelV1 = new CreateInstructionRequestModelV1
        {
            UserId = Guid.NewGuid(),
            InstructionType = "Bitcoin",
            CronExpression = "*/50 * * * * *",
            Amount = 120,
            NotificationChannel = new List<string>()
            {
                "Email"
            },
            InstructionDate = DateTime.Parse("2023-05-25T13:21:09.565Z")
            
        };
        var stringWriter = new StringWriter();
        Console.SetOut(stringWriter);
        Task<bool> FunctionInsertInstruction(Instruction instruction)
        {
            return Task.FromResult(false);
        }
        _instructionRepository.Setup(x => x.InsertAsync(It.IsAny<Instruction>()))
            .Returns(() => FunctionInsertInstruction(instruction));
        var _instructionService =
            new InstructionsService(mapper, _logger.Object, _instructionQueryService.Object,
                _bus.Object, _auditQueryService.Object, _instructionRepository.Object,
                _instructionAuditRepository.Object);
        //Act
        var actualResponse = _instructionService.InsertInstructionAsync(createInstructionRequestModelV1).Result;
        //Assert
        _instructionRepository.Verify(x=>x.InsertAsync(It.IsAny<Instruction>()), Times.Once);
        Console.WriteLine($"Error Occured While Creating {instruction.GetType().FullName} with user info : {createInstructionRequestModelV1.UserId}");
        var actualLog = stringWriter.ToString().Split("\n")[0];
        var expectedLog = $"Error Occured While Creating " +
                              $"BtcTrader.Domain.Instructions.Instruction " +
                              $"with user info : {createInstructionRequestModelV1.UserId}";
        var expectedMessage = $"Error Occured While Creating " +
                          $"BtcTrader.Domain.Instructions.Instruction";
        //Assert
        Assert.Equal(expectedLog, actualLog);
        Assert.Equal(expectedMessage,actualResponse.Error);
        Assert.False(actualResponse.IsSuccess);
    }
    
    [Fact]
    public void GetByIdAsync_Successfully()
    {
        //Arrange
        var instruction = InstructionBuilder.MockInstruction().Generate(1)[0];
        _instructionRepository.Setup(x => x.GetByIDAsync(It.IsAny<Guid>())).Returns(Task.FromResult(instruction));
        var mapper = new MapperConfiguration(cfg =>
            cfg.AddProfiles(new List<Profile>()
            {
                new InstructionMappingProfiles(), new InstructionEventsMappingProfile(), new UserMappingProfiles()
            })).CreateMapper();
        var _instructionService =
            new InstructionsService(mapper, _logger.Object, _instructionQueryService.Object,
                _bus.Object, _auditQueryService.Object, _instructionRepository.Object,
                _instructionAuditRepository.Object);

        //Act
        var actualResponse = _instructionService.GetByIdAsync(Guid.NewGuid()).Result.Value;

        //Assert
        Assert.NotNull(actualResponse);
        Assert.Equal(instruction, actualResponse);
    }
    [Fact]
    public void GetByIdAsync_ReturnNull_WhenInstructionNotExist()
    {
        //Arrange
        _instructionRepository.Setup(x => x.GetByIDAsync(It.IsAny<Guid>())).Returns(Task.FromResult<Instruction>(null));
        var mapper = new MapperConfiguration(cfg =>
            cfg.AddProfiles(new List<Profile>()
            {
                new InstructionMappingProfiles(), new InstructionEventsMappingProfile(), new UserMappingProfiles()
            })).CreateMapper();
        var _instructionService =
            new InstructionsService(mapper, _logger.Object, _instructionQueryService.Object,
                _bus.Object, _auditQueryService.Object, _instructionRepository.Object,
                _instructionAuditRepository.Object);
        var guid = Guid.NewGuid();
        var stringWriter = new StringWriter();
        Console.SetOut(stringWriter);
        //Act
        var actualResponse = _instructionService.GetByIdAsync(guid).Result;
        Console.WriteLine($"Instruction could not found with related id : {guid}");
        var actualLog = stringWriter.ToString().Split("\n")[0];
        var expectedLog = $"Instruction could not found with related id : {guid}";
        var expectedMessage = "Instruction could not found with related id";
        //Assert
        Assert.Null(actualResponse.Value);
        Assert.Equal(expectedLog, actualLog);
        Assert.Equal(expectedMessage,actualResponse.Error);
        Assert.False(actualResponse.IsSuccess);
    }
    
    [Fact]
    public void ActivateInstructionAsync_Failed_InstructionIsNull()
    {
        var instruction = InstructionBuilder.MockInstruction().Generate(1)[0];
        _instructionRepository.Setup(x => x.GetByIDAsync(It.IsAny<Guid>())).Returns(Task.FromResult<Instruction>(null));
        var mapper = new MapperConfiguration(cfg =>
            cfg.AddProfiles(new List<Profile>()
            {
                new InstructionMappingProfiles(), new InstructionEventsMappingProfile(), new UserMappingProfiles()
            })).CreateMapper();
        var _instructionService =
            new InstructionsService(mapper, _logger.Object, _instructionQueryService.Object,
                _bus.Object, _auditQueryService.Object, _instructionRepository.Object,
                _instructionAuditRepository.Object);
        var guid = Guid.NewGuid();
        var stringWriter = new StringWriter();
        Console.SetOut(stringWriter);
        //Act
        var actualResponse = _instructionService.ActivateInstructionAsync(guid,instruction.UserId).Result;
        Console.WriteLine($"Instruction could not found with related id : {guid}");
        var actualLog = stringWriter.ToString().Split("\n")[0];
        var expectedLog = $"Instruction could not found with related id : {guid}";
        var expectedMessage = "Instruction could not found with related id";
        //Assert
        Assert.Equal(expectedLog, actualLog);
        Assert.Equal(expectedMessage,actualResponse.Error);
        Assert.False(actualResponse.IsSuccess);
    }
    [Fact]
    public void ActivateInstructionAsync_Failed_UserIsNotEligible()
    {
        var instruction = InstructionBuilder.MockInstruction().Generate(1)[0];
        instruction.IsActive = false;
        _instructionRepository.Setup(x => x.GetByIDAsync(It.IsAny<Guid>())).Returns(Task.FromResult<Instruction>(instruction));
        var mapper = new MapperConfiguration(cfg =>
            cfg.AddProfiles(new List<Profile>()
            {
                new InstructionMappingProfiles(), new InstructionEventsMappingProfile(), new UserMappingProfiles()
            })).CreateMapper();
        var _instructionService =
            new InstructionsService(mapper, _logger.Object, _instructionQueryService.Object,
                _bus.Object, _auditQueryService.Object, _instructionRepository.Object,
                _instructionAuditRepository.Object);
        var guid = Guid.NewGuid();
        var stringWriter = new StringWriter();
        Console.SetOut(stringWriter);
        var newGuid = Guid.NewGuid();
        //Act
        var actualResponse = _instructionService.ActivateInstructionAsync(guid,newGuid).Result;
        var expectedLog = $"User {newGuid} is not eligible to activate instruction with id {instruction.Id}";
        Console.WriteLine(expectedLog);
        var actualLog = stringWriter.ToString().Split("\n")[0];
        var expectedMessage = "User is not eligible to activate instruction";
        //Assert
        Assert.Equal(expectedLog, actualLog);
        Assert.Equal(expectedMessage,actualResponse.Error);
        Assert.False(actualResponse.IsSuccess);
    }
    [Fact]
    public void ActivateInstructionAsync_Failed_InstructionAlreadyActive()
    {
        var instruction = InstructionBuilder.MockInstruction().Generate(1)[0];
        instruction.IsActive = true;
        _instructionRepository.Setup(x => x.GetByIDAsync(It.IsAny<Guid>())).Returns(Task.FromResult<Instruction>(instruction));
        var mapper = new MapperConfiguration(cfg =>
            cfg.AddProfiles(new List<Profile>()
            {
                new InstructionMappingProfiles(), new InstructionEventsMappingProfile(), new UserMappingProfiles()
            })).CreateMapper();
        var _instructionService =
            new InstructionsService(mapper, _logger.Object, _instructionQueryService.Object,
                _bus.Object, _auditQueryService.Object, _instructionRepository.Object,
                _instructionAuditRepository.Object);
        var guid = Guid.NewGuid();
        var stringWriter = new StringWriter();
        Console.SetOut(stringWriter);
        //Act
        var actualResponse = _instructionService.ActivateInstructionAsync(guid,instruction.UserId).Result;
        var expectedLog = $"Instruction which is already active cannot be activated with related id : {instruction.Id}";
        Console.WriteLine(expectedLog);
        var actualLog = stringWriter.ToString().Split("\n")[0];
        var expectedMessage = "Instruction which is already active cannot be activated with related id";
        //Assert
        Assert.Equal(expectedLog, actualLog);
        Assert.Equal(expectedMessage,actualResponse.Error);
        Assert.False(actualResponse.IsSuccess);
    }
    [Fact]
    public void ActivateInstructionAsync_Failed_DbError()
    {
        var instruction = InstructionBuilder.MockInstruction().Generate(1)[0];
        instruction.IsActive = false;
        _instructionRepository.Setup(x => x.GetByIDAsync(It.IsAny<Guid>())).Returns(Task.FromResult(instruction));
        _instructionRepository.Setup(x => x.Update(It.IsAny<Instruction>())).Returns(false);
        var mapper = new MapperConfiguration(cfg =>
            cfg.AddProfiles(new List<Profile>()
            {
                new InstructionMappingProfiles(), new InstructionEventsMappingProfile(), new UserMappingProfiles()
            })).CreateMapper();
        var _instructionService =
            new InstructionsService(mapper, _logger.Object, _instructionQueryService.Object,
                _bus.Object, _auditQueryService.Object, _instructionRepository.Object,
                _instructionAuditRepository.Object);
        var guid = Guid.NewGuid();
        var stringWriter = new StringWriter();
        Console.SetOut(stringWriter);
        //Act
        var actualResponse = _instructionService.ActivateInstructionAsync(guid,instruction.UserId).Result;
        var expectedLog = $"Error Occured While activating {instruction.GetType().FullName} with user info : {instruction.UserId}";
        Console.WriteLine(expectedLog);
        var actualLog = stringWriter.ToString().Split("\n")[0];
        var expectedMessage = "Failed to activate the instruction";
        //Assert
        Assert.Equal(expectedLog, actualLog);
        Assert.Equal(expectedMessage,actualResponse.Error);
        Assert.False(actualResponse.IsSuccess);
    }
    [Fact]
    public void ActivateInstructionAsync_Successfully()
    {
        var instruction = InstructionBuilder.MockInstruction().Generate(1)[0];
        instruction.IsActive = false;
        _instructionRepository.Setup(x => x.GetByIDAsync(It.IsAny<Guid>())).Returns(Task.FromResult(instruction));
        _instructionRepository.Setup(x => x.Update(It.IsAny<Instruction>())).Returns(true);
        var mapper = new MapperConfiguration(cfg =>
            cfg.AddProfiles(new List<Profile>()
            {
                new InstructionMappingProfiles(), new InstructionEventsMappingProfile(), new UserMappingProfiles()
            })).CreateMapper();
        var _instructionService =
            new InstructionsService(mapper, _logger.Object, _instructionQueryService.Object,
                _bus.Object, _auditQueryService.Object, _instructionRepository.Object,
                _instructionAuditRepository.Object);
        var guid = Guid.NewGuid();

        //Act
        var actualResponse = _instructionService.ActivateInstructionAsync(guid,instruction.UserId).Result;
        //Assert
        Assert.True(actualResponse.IsSuccess);
    }
    [Fact]
    public void DeActivateInstructionAsync_Failed_InstructionIsNull()
    {
        var instruction = InstructionBuilder.MockInstruction().Generate(1)[0];
        _instructionRepository.Setup(x => x.GetByIDAsync(It.IsAny<Guid>())).Returns(Task.FromResult<Instruction>(null));
        var mapper = new MapperConfiguration(cfg =>
            cfg.AddProfiles(new List<Profile>()
            {
                new InstructionMappingProfiles(), new InstructionEventsMappingProfile(), new UserMappingProfiles()
            })).CreateMapper();
        var _instructionService =
            new InstructionsService(mapper, _logger.Object, _instructionQueryService.Object,
                _bus.Object, _auditQueryService.Object, _instructionRepository.Object,
                _instructionAuditRepository.Object);
        var guid = Guid.NewGuid();
        var stringWriter = new StringWriter();
        Console.SetOut(stringWriter);
        //Act
        var actualResponse = _instructionService.DeActivateInstructionAsync(guid,instruction.UserId).Result;
        Console.WriteLine($"Instruction could not found with related id : {guid}");
        var actualLog = stringWriter.ToString().Split("\n")[0];
        var expectedLog = $"Instruction could not found with related id : {guid}";
        var expectedMessage = "Instruction could not found with related id";
        //Assert
        Assert.Equal(expectedLog, actualLog);
        Assert.Equal(expectedMessage,actualResponse.Error);
        Assert.False(actualResponse.IsSuccess);
    }
    [Fact]
    public void DeActivateInstructionAsync_Failed_UserIsNotEligible()
    {
        var instruction = InstructionBuilder.MockInstruction().Generate(1)[0];
        instruction.IsActive = true;
        _instructionRepository.Setup(x => x.GetByIDAsync(It.IsAny<Guid>())).Returns(Task.FromResult<Instruction>(instruction));
        var mapper = new MapperConfiguration(cfg =>
            cfg.AddProfiles(new List<Profile>()
            {
                new InstructionMappingProfiles(), new InstructionEventsMappingProfile(), new UserMappingProfiles()
            })).CreateMapper();
        var _instructionService =
            new InstructionsService(mapper, _logger.Object, _instructionQueryService.Object,
                _bus.Object, _auditQueryService.Object, _instructionRepository.Object,
                _instructionAuditRepository.Object);
        var guid = Guid.NewGuid();
        var stringWriter = new StringWriter();
        Console.SetOut(stringWriter);
        var newGuid = Guid.NewGuid();
        //Act
        var actualResponse = _instructionService.DeActivateInstructionAsync(guid,newGuid).Result;
        var expectedLog = $"User {newGuid} is not eligible to deactivate instruction with id {instruction.Id}";
        Console.WriteLine(expectedLog);
        var actualLog = stringWriter.ToString().Split("\n")[0];
        var expectedMessage = "User is not eligible to deactivate instruction";
        //Assert
        Assert.Equal(expectedLog, actualLog);
        Assert.Equal(expectedMessage,actualResponse.Error);
        Assert.False(actualResponse.IsSuccess);
    }
    [Fact]
    public void DeActivateInstructionAsync_Failed_InstructionAlreadyDeActived()
    {
        var instruction = InstructionBuilder.MockInstruction().Generate(1)[0];
        instruction.IsActive = false;
        _instructionRepository.Setup(x => x.GetByIDAsync(It.IsAny<Guid>())).Returns(Task.FromResult<Instruction>(instruction));
        var mapper = new MapperConfiguration(cfg =>
            cfg.AddProfiles(new List<Profile>()
            {
                new InstructionMappingProfiles(), new InstructionEventsMappingProfile(), new UserMappingProfiles()
            })).CreateMapper();
        var _instructionService =
            new InstructionsService(mapper, _logger.Object, _instructionQueryService.Object,
                _bus.Object, _auditQueryService.Object, _instructionRepository.Object,
                _instructionAuditRepository.Object);
        var guid = Guid.NewGuid();
        var stringWriter = new StringWriter();
        Console.SetOut(stringWriter);
        //Act
        var actualResponse = _instructionService.DeActivateInstructionAsync(guid,instruction.UserId).Result;
        var expectedLog = $"Instruction which is already deactive cannot be deactivated with related id : {instruction.Id}";
        Console.WriteLine(expectedLog);
        var actualLog = stringWriter.ToString().Split("\n")[0];
        var expectedMessage = "Instruction which is already deactive cannot be deactivated with related id";
        //Assert
        Assert.Equal(expectedLog, actualLog);
        Assert.Equal(expectedMessage,actualResponse.Error);
        Assert.False(actualResponse.IsSuccess);
    }
    [Fact]
    public void DeActivateInstructionAsync_Failed_DbError()
    {
        var instruction = InstructionBuilder.MockInstruction().Generate(1)[0];
        instruction.IsActive = true;
        _instructionRepository.Setup(x => x.GetByIDAsync(It.IsAny<Guid>())).Returns(Task.FromResult(instruction));
        _instructionRepository.Setup(x => x.Update(It.IsAny<Instruction>())).Returns(false);
        var mapper = new MapperConfiguration(cfg =>
            cfg.AddProfiles(new List<Profile>()
            {
                new InstructionMappingProfiles(), new InstructionEventsMappingProfile(), new UserMappingProfiles()
            })).CreateMapper();
        var _instructionService =
            new InstructionsService(mapper, _logger.Object, _instructionQueryService.Object,
                _bus.Object, _auditQueryService.Object, _instructionRepository.Object,
                _instructionAuditRepository.Object);
        var guid = Guid.NewGuid();
        var stringWriter = new StringWriter();
        Console.SetOut(stringWriter);
        //Act
        var actualResponse = _instructionService.DeActivateInstructionAsync(guid,instruction.UserId).Result;
        var expectedLog = $"Error Occured While deactivating {instruction.GetType().FullName} with user info : {instruction.UserId}";
        Console.WriteLine(expectedLog);
        var actualLog = stringWriter.ToString().Split("\n")[0];
        var expectedMessage = "Failed to activate the instruction";
        //Assert
        Assert.Equal(expectedLog, actualLog);
        Assert.Equal(expectedMessage,actualResponse.Error);
        Assert.False(actualResponse.IsSuccess);
    }
    [Fact]
    public void DeActivateInstructionAsync_Successfully()
    {
        var instruction = InstructionBuilder.MockInstruction().Generate(1)[0];
        instruction.IsActive = true;
        _instructionRepository.Setup(x => x.GetByIDAsync(It.IsAny<Guid>())).Returns(Task.FromResult(instruction));
        _instructionRepository.Setup(x => x.Update(It.IsAny<Instruction>())).Returns(true);
        var mapper = new MapperConfiguration(cfg =>
            cfg.AddProfiles(new List<Profile>()
            {
                new InstructionMappingProfiles(), new InstructionEventsMappingProfile(), new UserMappingProfiles()
            })).CreateMapper();
        var _instructionService =
            new InstructionsService(mapper, _logger.Object, _instructionQueryService.Object,
                _bus.Object, _auditQueryService.Object, _instructionRepository.Object,
                _instructionAuditRepository.Object);
        var guid = Guid.NewGuid();

        //Act
        var actualResponse = _instructionService.DeActivateInstructionAsync(guid,instruction.UserId).Result;
        //Assert
        Assert.True(actualResponse.IsSuccess);
    }
    [Fact]
    public void DeleteInstructionAsync_Failed_InstructionIsNull()
    {
        var instruction = InstructionBuilder.MockInstruction().Generate(1)[0];
        _instructionRepository.Setup(x => x.GetByIDAsync(It.IsAny<Guid>())).Returns(Task.FromResult<Instruction>(null));
        var mapper = new MapperConfiguration(cfg =>
            cfg.AddProfiles(new List<Profile>()
            {
                new InstructionMappingProfiles(), new InstructionEventsMappingProfile(), new UserMappingProfiles()
            })).CreateMapper();
        var _instructionService =
            new InstructionsService(mapper, _logger.Object, _instructionQueryService.Object,
                _bus.Object, _auditQueryService.Object, _instructionRepository.Object,
                _instructionAuditRepository.Object);
        var guid = Guid.NewGuid();
        var stringWriter = new StringWriter();
        Console.SetOut(stringWriter);
        //Act
        var actualResponse = _instructionService.DeleteInstructionAsync(guid,instruction.UserId).Result;
        Console.WriteLine($"Instruction could not found with related id : {guid}");
        var actualLog = stringWriter.ToString().Split("\n")[0];
        var expectedLog = $"Instruction could not found with related id : {guid}";
        var expectedMessage = "Instruction could not found with related id";
        //Assert
        Assert.Equal(expectedLog, actualLog);
        Assert.Equal(expectedMessage,actualResponse.Error);
        Assert.False(actualResponse.IsSuccess);
    }
    [Fact]
    public void DeleteInstructionAsync_Failed_UserIsNotEligible()
    {
        var instruction = InstructionBuilder.MockInstruction().Generate(1)[0];
        _instructionRepository.Setup(x => x.GetByIDAsync(It.IsAny<Guid>())).Returns(Task.FromResult<Instruction>(instruction));
        var mapper = new MapperConfiguration(cfg =>
            cfg.AddProfiles(new List<Profile>()
            {
                new InstructionMappingProfiles(), new InstructionEventsMappingProfile(), new UserMappingProfiles()
            })).CreateMapper();
        var _instructionService =
            new InstructionsService(mapper, _logger.Object, _instructionQueryService.Object,
                _bus.Object, _auditQueryService.Object, _instructionRepository.Object,
                _instructionAuditRepository.Object);
        var guid = Guid.NewGuid();
        var stringWriter = new StringWriter();
        Console.SetOut(stringWriter);
        var newGuid = Guid.NewGuid();
        //Act
        var actualResponse = _instructionService.DeleteInstructionAsync(guid,newGuid).Result;
        var expectedLog = $"User {newGuid} is not eligible to delete instruction with id {instruction.Id}";
        Console.WriteLine(expectedLog);
        var actualLog = stringWriter.ToString().Split("\n")[0];
        var expectedMessage = "User is not eligible to delete instruction";
        //Assert
        Assert.Equal(expectedLog, actualLog);
        Assert.Equal(expectedMessage,actualResponse.Error);
        Assert.False(actualResponse.IsSuccess);
    }
    [Fact]
    public void DeleteInstructionAsync_Failed_DbError()
    {
        var instruction = InstructionBuilder.MockInstruction().Generate(1)[0];
        _instructionRepository.Setup(x => x.GetByIDAsync(It.IsAny<Guid>())).Returns(Task.FromResult(instruction));
        _instructionRepository.Setup(x => x.Remove(It.IsAny<Instruction>())).Returns(false);
        var mapper = new MapperConfiguration(cfg =>
            cfg.AddProfiles(new List<Profile>()
            {
                new InstructionMappingProfiles(), new InstructionEventsMappingProfile(), new UserMappingProfiles()
            })).CreateMapper();
        var _instructionService =
            new InstructionsService(mapper, _logger.Object, _instructionQueryService.Object,
                _bus.Object, _auditQueryService.Object, _instructionRepository.Object,
                _instructionAuditRepository.Object);
        var guid = Guid.NewGuid();
        var stringWriter = new StringWriter();
        Console.SetOut(stringWriter);
        //Act
        var actualResponse = _instructionService.DeleteInstructionAsync(guid,instruction.UserId).Result;
        var expectedLog = $"Error Occured While deleting {instruction.GetType().FullName} with user info : {instruction.UserId}";
        Console.WriteLine(expectedLog);
        var actualLog = stringWriter.ToString().Split("\n")[0];
        var expectedMessage = "Failed to delete the instruction";
        //Assert
        Assert.Equal(expectedLog, actualLog);
        Assert.Equal(expectedMessage,actualResponse.Error);
        Assert.False(actualResponse.IsSuccess);
    }
    [Fact]
    public void DeleteInstructionAsync_Successfully()
    {
        var instruction = InstructionBuilder.MockInstruction().Generate(1)[0];
        _instructionRepository.Setup(x => x.GetByIDAsync(It.IsAny<Guid>())).Returns(Task.FromResult(instruction));
        _instructionRepository.Setup(x => x.Remove(It.IsAny<Instruction>())).Returns(true);
        var mapper = new MapperConfiguration(cfg =>
            cfg.AddProfiles(new List<Profile>()
            {
                new InstructionMappingProfiles(), new InstructionEventsMappingProfile(), new UserMappingProfiles()
            })).CreateMapper();
        var _instructionService =
            new InstructionsService(mapper, _logger.Object, _instructionQueryService.Object,
                _bus.Object, _auditQueryService.Object, _instructionRepository.Object,
                _instructionAuditRepository.Object);
        var guid = Guid.NewGuid();

        //Act
        var actualResponse = _instructionService.DeleteInstructionAsync(guid,instruction.UserId).Result;
        //Assert
        Assert.True(actualResponse.IsSuccess);
    }
    [Fact]
    public void UpdateInstructionAsync_Failed_AmountLessThan100()
    {
        //Arrange
        var instruction = InstructionBuilder.MockInstruction().Generate(1)[0];
        _instructionRepository.Setup(x => x.GetByIDAsync(It.IsAny<Guid>())).Returns(Task.FromResult(instruction));
        var mapper = new MapperConfiguration(cfg =>
            cfg.AddProfiles(new List<Profile>()
            {
                new InstructionMappingProfiles(), new InstructionEventsMappingProfile(), new UserMappingProfiles()
            })).CreateMapper();
        var updateInstructionRequestModelV1 = new UpdateInstructionRequestModelV1()
        {
            UserId = Guid.NewGuid(),
            Amount = 80,
        };
        var stringWriter = new StringWriter();
        Console.SetOut(stringWriter);
       
        var _instructionService =
            new InstructionsService(mapper, _logger.Object, _instructionQueryService.Object,
                _bus.Object, _auditQueryService.Object, _instructionRepository.Object,
                _instructionAuditRepository.Object);

        //Act
        var validationResult = updateInstructionRequestModelV1.ValidateRequest();
        var actualResponse = _instructionService.UpdateInstructionAsync(updateInstructionRequestModelV1,instruction.Id).Result;
        Console.WriteLine($"Error Occured While Validating Request Model {updateInstructionRequestModelV1.GetType().FullName} : {validationResult}");
        var actualLog = stringWriter.ToString().Split("\n")[0];
        var expectedLog = $"Error Occured While Validating Request Model " +
                             $"BtcTrader.Contracts.RequestModels.V1.Instructions.UpdateInstructionRequestModelV1 :" +
                             $" 'Amount' must be greater than or equal to '100'.";
        //Assert
        Assert.Equal(expectedLog, actualLog);
        Assert.Equal(expectedLog, actualResponse.Error);
        Assert.False(actualResponse.IsSuccess);
    }
    [Fact]
    public void UpdateInstructionAsync_Failed_InstructionIsNull()
    {
        //Arrange
        var instruction = InstructionBuilder.MockInstruction().Generate(1)[0];
        _instructionRepository.Setup(x => x.GetByIDAsync(It.IsAny<Guid>())).Returns(Task.FromResult<Instruction>(null));
        var mapper = new MapperConfiguration(cfg =>
            cfg.AddProfiles(new List<Profile>()
            {
                new InstructionMappingProfiles(), new InstructionEventsMappingProfile(), new UserMappingProfiles()
            })).CreateMapper();
        var updateInstructionRequestModelV1 = new UpdateInstructionRequestModelV1()
        {
            UserId = instruction.UserId,
            Amount = 120,
        };
        var stringWriter = new StringWriter();
        Console.SetOut(stringWriter);
       
        var _instructionService =
            new InstructionsService(mapper, _logger.Object, _instructionQueryService.Object,
                _bus.Object, _auditQueryService.Object, _instructionRepository.Object,
                _instructionAuditRepository.Object);

        //Act
        var actualResponse = _instructionService.UpdateInstructionAsync(updateInstructionRequestModelV1,instruction.Id).Result;
        Console.WriteLine($"Instruction could not found with related id : {instruction.Id}");
       
        var actualLog = stringWriter.ToString().Split("\n")[0];
        var expectedLog = $"Instruction could not found with related id : {instruction.Id}";
        var expectedMessage = "Instruction could not found with related id";
        //Assert
        Assert.Equal(expectedLog, actualLog);
        Assert.Equal(expectedMessage, actualResponse.Error);
        Assert.False(actualResponse.IsSuccess);
    }
    [Fact]
    public void UpdateInstructionAsync_Failed_UserIsNotEligibleToUpdate()
    {
        //Arrange
        var instruction = InstructionBuilder.MockInstruction().Generate(1)[0];
        _instructionRepository.Setup(x => x.GetByIDAsync(It.IsAny<Guid>())).Returns(Task.FromResult<Instruction>(instruction));
        var mapper = new MapperConfiguration(cfg =>
            cfg.AddProfiles(new List<Profile>()
            {
                new InstructionMappingProfiles(), new InstructionEventsMappingProfile(), new UserMappingProfiles()
            })).CreateMapper();
        var updateInstructionRequestModelV1 = new UpdateInstructionRequestModelV1()
        {
            UserId = Guid.NewGuid(),
            Amount = 120,
        };
        var stringWriter = new StringWriter();
        Console.SetOut(stringWriter);
       
        var _instructionService =
            new InstructionsService(mapper, _logger.Object, _instructionQueryService.Object,
                _bus.Object, _auditQueryService.Object, _instructionRepository.Object,
                _instructionAuditRepository.Object);

        //Act
        var actualResponse = _instructionService.UpdateInstructionAsync(updateInstructionRequestModelV1,instruction.Id).Result;
        Console.WriteLine($"User {updateInstructionRequestModelV1.UserId} is not eligible to update instruction with id {instruction.Id}");
       
        var actualLog = stringWriter.ToString().Split("\n")[0];
        var expectedLog = $"User {updateInstructionRequestModelV1.UserId} is not eligible to update instruction with id {instruction.Id}";
        var expectedMessage = "User is not eligible to update instruction";
        //Assert
        Assert.Equal(expectedLog, actualLog);
        Assert.Equal(expectedMessage, actualResponse.Error);
        Assert.False(actualResponse.IsSuccess);
    }
    [Fact]
    public void UpdateInstructionAsync_Failed_DbError()
    {
        //Arrange
        var instruction = InstructionBuilder.MockInstruction().Generate(1)[0];
        _instructionRepository.Setup(x => x.GetByIDAsync(It.IsAny<Guid>())).Returns(Task.FromResult<Instruction>(instruction));
        _instructionRepository.Setup(x => x.Update(It.IsAny<Instruction>())).Returns(false);
        var mapper = new MapperConfiguration(cfg =>
            cfg.AddProfiles(new List<Profile>()
            {
                new InstructionMappingProfiles(), new InstructionEventsMappingProfile(), new UserMappingProfiles()
            })).CreateMapper();
        var updateInstructionRequestModelV1 = new UpdateInstructionRequestModelV1()
        {
            UserId = instruction.UserId,
            Amount = 120,
        };
        var stringWriter = new StringWriter();
        Console.SetOut(stringWriter);
       
        var _instructionService =
            new InstructionsService(mapper, _logger.Object, _instructionQueryService.Object,
                _bus.Object, _auditQueryService.Object, _instructionRepository.Object,
                _instructionAuditRepository.Object);

        //Act
        var actualResponse = _instructionService.UpdateInstructionAsync(updateInstructionRequestModelV1,instruction.Id).Result;
        Console.WriteLine($"Error Occured While Updating {instruction.GetType().FullName} with user info : {updateInstructionRequestModelV1.UserId}");
       
        var actualLog = stringWriter.ToString().Split("\n")[0];
        var expectedLog = $"Error Occured While Updating {instruction.GetType().FullName} with user info : {updateInstructionRequestModelV1.UserId}";
        var expectedMessage = $"Error Occured While Updating {instruction.GetType().FullName}";
        //Assert
        Assert.Equal(expectedLog, actualLog);
        Assert.Equal(expectedMessage, actualResponse.Error);
        Assert.False(actualResponse.IsSuccess);
    }
    [Fact]
    public void UpdateInstructionAsync_Successfully()
    {
        //Arrange
        var instruction = InstructionBuilder.MockInstruction().Generate(1)[0];
        _instructionRepository.Setup(x => x.GetByIDAsync(It.IsAny<Guid>())).Returns(Task.FromResult<Instruction>(instruction));
        _instructionRepository.Setup(x => x.Update(It.IsAny<Instruction>())).Returns(true);
        var mapper = new MapperConfiguration(cfg =>
            cfg.AddProfiles(new List<Profile>()
            {
                new InstructionMappingProfiles(), new InstructionEventsMappingProfile(), new UserMappingProfiles()
            })).CreateMapper();
        var updateInstructionRequestModelV1 = new UpdateInstructionRequestModelV1()
        {
            UserId = instruction.UserId,
            Amount = 120,
        };
       
        var _instructionService =
            new InstructionsService(mapper, _logger.Object, _instructionQueryService.Object,
                _bus.Object, _auditQueryService.Object, _instructionRepository.Object,
                _instructionAuditRepository.Object);

        //Act
        var actualResponse = _instructionService.UpdateInstructionAsync(updateInstructionRequestModelV1,instruction.Id).Result;
        //Assert
        Assert.True(actualResponse.IsSuccess);
    }
    [Fact]
    public void GetInstructionAuditByIdAsync_Successfully()
    {
        //Arrange
        var instructionAudit = InstructionAuditBuilder.MockInstructionAudit().Generate(1)[0];
        _instructionAuditRepository.Setup(x => x.GetByIDAsync(It.IsAny<Guid>())).Returns(Task.FromResult(instructionAudit));
        var mapper = new MapperConfiguration(cfg =>
            cfg.AddProfiles(new List<Profile>()
            {
                new InstructionMappingProfiles(), new InstructionEventsMappingProfile(), new UserMappingProfiles()
            })).CreateMapper();
        var _instructionService =
            new InstructionsService(mapper, _logger.Object, _instructionQueryService.Object,
                _bus.Object, _auditQueryService.Object, _instructionRepository.Object,
                _instructionAuditRepository.Object);

        //Act
        var actualResponse = _instructionService.GetInstructionAuditByIdAsync(Guid.NewGuid()).Result.Value;

        //Assert
        Assert.NotNull(actualResponse);
        Assert.Equal(instructionAudit, actualResponse);
    }
    [Fact]
    public void GetInstructionAuditByIdAsync_ReturnNull_WhenInstructionNotExist()
    {
        //Arrange
        _instructionAuditRepository.Setup(x => x.GetByIDAsync(It.IsAny<Guid>())).Returns(Task.FromResult<InstructionAudit>(null));
        var mapper = new MapperConfiguration(cfg =>
            cfg.AddProfiles(new List<Profile>()
            {
                new InstructionMappingProfiles(), new InstructionEventsMappingProfile(), new UserMappingProfiles()
            })).CreateMapper();
        var _instructionService =
            new InstructionsService(mapper, _logger.Object, _instructionQueryService.Object,
                _bus.Object, _auditQueryService.Object, _instructionRepository.Object,
                _instructionAuditRepository.Object);
        var guid = Guid.NewGuid();
        var stringWriter = new StringWriter();
        Console.SetOut(stringWriter);
        //Act
        var actualResponse = _instructionService.GetInstructionAuditByIdAsync(guid).Result;
        Console.WriteLine($"Instruction audit could not found with related id : {guid}");
        var actualLog = stringWriter.ToString().Split("\n")[0];
        var expectedLog = $"Instruction audit could not found with related id : {guid}";
        var expectedMessage = "Instruction audit could not found with related id";
        //Assert
        Assert.Null(actualResponse.Value);
        Assert.Equal(expectedLog, actualLog);
        Assert.Equal(expectedMessage,actualResponse.Error);
        Assert.False(actualResponse.IsSuccess);
    }
}

