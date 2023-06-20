using AutoMapper;
using BtcTrader.Contracts.RequestModels.V1.Instructions;
using BtcTrader.Core.Services.Instructions.Common;
using BtcTrader.Domain.Instructions;
using BtcTrader.Domain.Instructions.Events;

namespace BtcTrader.Core.MappingProfiles;

public class InstructionEventsMappingProfile : Profile
{
    public InstructionEventsMappingProfile()
    {
        CreateMap<InstructionCreatedEvent, InstructionAudit>()
            .ForMember(x => x.InstructionId,
                o => o.MapFrom(s => s.Id))
            .ForMember(x => x.Id,
                o => o.MapFrom(s => Guid.NewGuid()))
            .ForMember(x => x.IsActive,
                o => o.MapFrom(s => true))
            .ForMember(x => x.OldAmount,
                o => o.MapFrom(s => s.Amount))
            .ForMember(x => x.Message,
                o => o.MapFrom(s => string.Format(EventAuditMessages.INSTRUCTION_CREATED_BY_USER,
                    s.Id,
                    s.UserId,
                    s.CreatedTime)));
        CreateMap<InstructionUpdatedEvent, InstructionAudit>()
            .ForMember(x => x.InstructionId,
                o => o.MapFrom(s => s.Id))
            .ForMember(x => x.Id,
                o => o.MapFrom(s => Guid.NewGuid()))
            .ForMember(x => x.IsActive,
                o => o.MapFrom(s => true))
            .ForMember(x => x.OldAmount,
                o => o.MapFrom(s => s.OldAmount))
            .ForMember(x => x.NewAmount,
                o => o.MapFrom(s => s.NewAmount))
            .ForMember(x => x.Message,
                o => o.MapFrom(s => string.Format(EventAuditMessages.INSTRUCTION_UPDATED_BY_USER,
                    s.Id,
                    s.UserId,
                    s.UpdatedTime)));
        CreateMap<InstructionActivatedEvent, InstructionAudit>()
            .ForMember(x => x.Id,
                o => o.MapFrom(s => Guid.NewGuid()))
            .ForMember(x => x.IsActive,
                o => o.MapFrom(s => true))
            .ForMember(x => x.Message,
                o => o.MapFrom(s => string.Format(EventAuditMessages.INSTRUCTION_ACTIVATED_BY_USER,
                    s.InstructionId,
                    s.UserId,
                    s.UpdatedTime)));
        CreateMap<InstructionDeActivatedEvent, InstructionAudit>()
            .ForMember(x => x.Id,
                o => o.MapFrom(s => Guid.NewGuid()))
            .ForMember(x => x.IsActive,
                o => o.MapFrom(s => false))
            .ForMember(x => x.Message,
                o => o.MapFrom(s => string.Format(EventAuditMessages.INSTRUCTION_DEACTIVATED_BY_USER,
                    s.InstructionId,
                    s.UserId,
                    s.UpdatedTime)));
        CreateMap<InstructionDeletedEvent, InstructionAudit>()
            .ForMember(x => x.Id,
                o => o.MapFrom(s => Guid.NewGuid()))
            .ForMember(x => x.IsActive,
                o => o.MapFrom(s => false))
            .ForMember(x => x.Message,
                o => o.MapFrom(s => string.Format(EventAuditMessages.INSTRUCTION_DELETED_BY_USER,
                    s.InstructionId,
                    s.UserId,
                    s.UpdatedTime)));

    }
}