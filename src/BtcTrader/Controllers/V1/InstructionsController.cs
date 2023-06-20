using BtcTrader.Contracts.RequestModels;
using BtcTrader.Contracts.RequestModels.V1.Instructions;
using BtcTrader.Core.Services.Instructions.Abstractions;
using BtcTrader.Infrastructure.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace BtcTrader.Controllers.V1
{
    [ApiVersion("1")]
    public class InstructionsController : BaseApiController
    {
        private readonly IInstructionsService _instructionsService;

        public InstructionsController(IInstructionsService instructionsService)
        {
            _instructionsService = instructionsService;
        }

        [MapToApiVersion("1")]
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery]PaginationFilterRequestModel filterRequestModel)
        {
            if (Request.QueryString.Value != null)
                filterRequestModel.FilterBy = QueryHelper.GenerateFilterFromQueryString(Request.QueryString.Value);
            var result = await _instructionsService.GetAllInstruction(filterRequestModel);
            return HandleResult(result);
        }
        [MapToApiVersion("1")]
        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetByIdAsync(Guid id)
        {
            var result = await _instructionsService.GetByIdAsync(id);
            return HandleResult(result);
        }
        [MapToApiVersion("1")]
        [HttpPost]
        public async Task<IActionResult> CreateInstructionAsync([FromBody] CreateInstructionRequestModelV1? requestModel)
        {
            var result = await _instructionsService.InsertInstructionAsync(requestModel!);
            return HandleResult(result);
        }
        
        [MapToApiVersion("1")]
        [HttpPatch]
        [Route("{id}")]
        public async Task<IActionResult> UpdateInstructionAsync([FromBody] UpdateInstructionRequestModelV1 requestModel, Guid id)
        {
            var result = await _instructionsService.UpdateInstructionAsync(requestModel, id);
            return HandleResult(result);
        }
        [MapToApiVersion("1")]
        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> DeleteInstructionAsync(Guid id, Guid userId)
        {
            var result = await _instructionsService.DeleteInstructionAsync(id,userId);
            return HandleResult(result);
        }
        
        [MapToApiVersion("1")]
        [HttpPatch]
        [Route("{id}/activate")]
        public async Task<IActionResult> ActivateInstructionAsync(Guid id,[FromHeader] Guid userId)
        {
            var result = await _instructionsService.ActivateInstructionAsync(id,userId);
            return HandleResult(result);
        }
        [MapToApiVersion("1")]
        [HttpPatch]
        [Route("{id}/deactivate")]
        public async Task<IActionResult> DeActivateInstructionAsync(Guid id, [FromHeader] Guid userId)
        {
            var result = await _instructionsService.DeActivateInstructionAsync(id,userId);
            return HandleResult(result);
        }
        [MapToApiVersion("1")]
        [HttpGet]
        [Route("audits")]
        public async Task<IActionResult> GetAllInstructionAuditAsync([FromQuery]PaginationFilterRequestModel filterRequestModel)
        {
            if (Request.QueryString.Value != null)
                filterRequestModel.FilterBy = QueryHelper.GenerateFilterFromQueryString(Request.QueryString.Value);
            var result = await _instructionsService.GetAllInstructionAudit(filterRequestModel);
            return HandleResult(result);
        }
        [MapToApiVersion("1")]
        [HttpGet]
        [Route("audits/{id}")]
        public async Task<IActionResult> GetInstructionAuditByIdAsync(Guid id)
        {
            var result = await _instructionsService.GetInstructionAuditByIdAsync(id);
            return HandleResult(result);
        }
    }
}

