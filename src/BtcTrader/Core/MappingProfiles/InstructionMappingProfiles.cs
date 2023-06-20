using AutoMapper;
using BtcTrader.Contracts.RequestModels.V1.Instructions;
using BtcTrader.Contracts.ResponseModels.V1.Instructions;
using BtcTrader.Domain.Instructions;
using BtcTrader.Domain.Instructions.Events;

namespace BtcTrader.Core.MappingProfiles;

public class InstructionMappingProfiles : Profile
{
    public InstructionMappingProfiles()
    {
        
        CreateMap<List<Instruction>, GetAllInstructionsResponseModel>()
            .ForMember(x=>x.TotalCount, 
                o => o.MapFrom(s=>s.Count))
            .ForMember(x=>x.Instructions, 
                o => o.MapFrom(s=>s));

        CreateMap<CreateInstructionRequestModelV1, Instruction>();
        CreateMap<Instruction, InstructionCreatedEvent>() .ForMember(x=>x.CreatedTime, 
            o => o.MapFrom(s=> DateTime.Now));
        CreateMap<Instruction, InstructionUpdatedEvent>()
            .ForMember(x=>x.UpdatedTime, 
            o => o.MapFrom(s=> DateTime.Now));
        
        CreateMap<Instruction, InstructionActivatedEvent>()
            .ForMember(x=>x.InstructionId, 
                o => o.MapFrom(s=> s.Id))
            .ForMember(x=>x.UpdatedTime, 
            o => o.MapFrom(s=> DateTime.Now));
        CreateMap<Instruction, InstructionDeletedEvent>()
            .ForMember(x=>x.InstructionId, 
                o => o.MapFrom(s=> s.Id))
            .ForMember(x=>x.UpdatedTime, 
                o => o.MapFrom(s=> DateTime.Now));
        CreateMap<Instruction, InstructionDeActivatedEvent>()
            .ForMember(x=>x.InstructionId, 
                o => o.MapFrom(s=> s.Id))
            .ForMember(x=>x.UpdatedTime, 
                o => o.MapFrom(s=> DateTime.Now));
        CreateMap<UpdateInstructionRequestModelV1, Instruction>()
            .ForMember(x=>x.InstructionDate,
                o =>
                {
                    o.PreCondition(s => s.InstructionDate != null);
                    o.MapFrom(s=>s.InstructionDate);
                })
            .ForMember(x=>x.CronExpression,
                o =>
                {
                    o.PreCondition(s => s.CronExpression != null);
                    o.MapFrom(s=>s.CronExpression);
                })
            .ForMember(x=>x.NotificationChannel, 
                o =>
                {
                    o.PreCondition(s => s.NotificationChannel != null && s.NotificationChannel.Any());
                    o.MapFrom(s=>s.NotificationChannel);
                })
            .ForMember(x=>x.Amount, 
            o => o.MapFrom(s=>s.Amount));
        
        CreateMap<List<InstructionAudit>, GetAllInstructionAuditsResponseModel>()
            .ForMember(x=>x.TotalCount, 
                o => o.MapFrom(s=>s.Count))
            .ForMember(x=>x.InstructionAudits, 
                o => o.MapFrom(s=>s));
    }
}