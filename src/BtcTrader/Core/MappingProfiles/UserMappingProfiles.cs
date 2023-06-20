using AutoMapper;
using BtcTrader.Contracts.Dtos.User;
using BtcTrader.Domain.User;

namespace BtcTrader.Core.MappingProfiles;

public class UserMappingProfiles : Profile
{
    public UserMappingProfiles()
    {
        CreateMap<AppUser, UserDto>();
    }
}

