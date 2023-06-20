using BtcTrader.Contracts.Dtos.User;
using BtcTrader.Core.Services.User.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace BtcTrader.Controllers.V1;

public class AccountController : BaseApiController
{
    private readonly IUserService _userService;
    public AccountController(IUserService userService)
    {
        _userService = userService;
    }
    
    [HttpPost("register")] 
    public async Task<IActionResult> Register(RegisterDto registerDto)
    {
        var result = await _userService.Register(registerDto);
        return HandleResult(result);
    }
}