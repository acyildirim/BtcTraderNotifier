using BtcTrader.Contracts.Dtos.User;

namespace BtcTrader.Core.Services.User.Abstractions;

public interface IUserService
{
    Task<Result<UserDto>> Register(RegisterDto registerDto);

}