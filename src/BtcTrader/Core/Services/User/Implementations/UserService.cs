using AutoMapper;
using BtcTrader.Contracts.Dtos.User;
using BtcTrader.Core.Services.User.Abstractions;
using BtcTrader.Domain.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BtcTrader.Core.Services.User.Implementations;

public class UserService : IUserService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IMapper _mapper;

    public UserService(
        UserManager<AppUser> userManager,
        IMapper mapper)
    {
        _userManager = userManager;
        _mapper = mapper;
    }

    public async Task<Result<UserDto>> Register(RegisterDto registerDto)
    {
        var userDto = new UserDto();
        if (await _userManager.Users.AnyAsync(x => x.Email == registerDto.Email))
        {
            return Result<UserDto>.Failure("Email taken");
        }

        if (await _userManager.Users.AnyAsync(x => x.UserName == registerDto.Username))
        {
            return Result<UserDto>.Failure("Username taken");
        }

        var user = new AppUser
        {
            Email = registerDto.Email,
            UserName = registerDto.Username
        };

        var result = await _userManager.CreateAsync(user, registerDto.Password);
        if (!result.Succeeded) return Result<UserDto>.Failure("User Creation Failed");
        _mapper.Map(user, userDto);
        return Result<UserDto>.Success(userDto);

    }
   
}