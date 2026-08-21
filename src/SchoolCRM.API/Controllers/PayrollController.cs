using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolCRM.Application.DTOs.Payroll;
using SchoolCRM.Application.Interfaces.Services;
using SchoolCRM.Shared.Models;

namespace SchoolCRM.API.Controllers;

[ApiController]
[Route("api/admin/payroll")]
[Authorize(Roles = "SuperAdmin,SchoolAdmin,Accountant")]
public class PayrollController : ControllerBase
{
    private readonly IPayrollService _payrollService;

    public PayrollController(IPayrollService payrollService)
    {
        _payrollService = payrollService;
    }

    [HttpGet("settings")]
    public async Task<ActionResult<ApiResponse<PayrollSettingDto>>> GetSettings()
    {
        var result = await _payrollService.GetPayrollSettingsAsync();
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("settings")]
    public async Task<ActionResult<ApiResponse<PayrollSettingDto>>> SaveSettings([FromBody] CreatePayrollSettingDto dto)
    {
        var result = await _payrollService.SavePayrollSettingsAsync(dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("salary-profiles")]
    public async Task<ActionResult<ApiResponse<List<SalaryProfileDto>>>> GetAllSalaryProfiles()
    {
        var result = await _payrollService.GetAllSalaryProfilesAsync();
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("salary-profiles/{userId}")]
    public async Task<ActionResult<ApiResponse<SalaryProfileDto>>> GetSalaryProfile(string userId)
    {
        var result = await _payrollService.GetSalaryProfileAsync(userId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("salary-profiles")]
    public async Task<ActionResult<ApiResponse<SalaryProfileDto>>> CreateSalaryProfile([FromBody] CreateSalaryProfileDto dto)
    {
        var result = await _payrollService.CreateSalaryProfileAsync(dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("salary-profiles/{id}")]
    public async Task<ActionResult<ApiResponse<SalaryProfileDto>>> UpdateSalaryProfile(Guid id, [FromBody] CreateSalaryProfileDto dto)
    {
        var result = await _payrollService.UpdateSalaryProfileAsync(id, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("salary-profiles/{profileId}/components")]
    public async Task<ActionResult<ApiResponse<List<SalaryComponentDto>>>> GetComponents(Guid profileId)
    {
        var result = await _payrollService.GetSalaryComponentsAsync(profileId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("salary-profiles/{profileId}/components")]
    public async Task<ActionResult<ApiResponse<SalaryComponentDto>>> AddComponent(Guid profileId, [FromBody] CreateSalaryComponentDto dto)
    {
        var result = await _payrollService.AddSalaryComponentAsync(profileId, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("components/{id}")]
    public async Task<ActionResult<ApiResponse<SalaryComponentDto>>> UpdateComponent(Guid id, [FromBody] CreateSalaryComponentDto dto)
    {
        var result = await _payrollService.UpdateSalaryComponentAsync(id, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("components/{id}")]
    public async Task<ActionResult<ApiResponse>> DeleteComponent(Guid id)
    {
        var result = await _payrollService.DeleteSalaryComponentAsync(id);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("generate")]
    public async Task<ActionResult<ApiResponse<List<PayrollDto>>>> GeneratePayroll([FromBody] GeneratePayrollDto dto)
    {
        var result = await _payrollService.GenerateMonthlyPayrollAsync(dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("payrolls")]
    public async Task<ActionResult<ApiResponse<List<PayrollDto>>>> GetPayrolls([FromQuery] int month, [FromQuery] int year)
    {
        var result = await _payrollService.GetPayrollsAsync(month, year);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("payrolls/{id}")]
    public async Task<ActionResult<ApiResponse<PayrollDto>>> GetPayroll(Guid id)
    {
        var result = await _payrollService.GetPayrollAsync(id);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("payrolls/{id}/approve")]
    public async Task<ActionResult<ApiResponse<PayrollDto>>> ApprovePayroll(Guid id)
    {
        var result = await _payrollService.ApprovePayrollAsync(id);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("payrolls/{id}/mark-paid")]
    public async Task<ActionResult<ApiResponse<PayrollDto>>> MarkPaid(Guid id)
    {
        var result = await _payrollService.MarkPaidAsync(id);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("payrolls/{id}/generate-payslip")]
    public async Task<ActionResult<ApiResponse<PayslipDto>>> GeneratePayslip(Guid id)
    {
        var result = await _payrollService.GeneratePayslipAsync(id);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("report")]
    public async Task<ActionResult<ApiResponse<PayrollReportDto>>> GetReport([FromQuery] int month, [FromQuery] int year)
    {
        var result = await _payrollService.GetPayrollReportAsync(month, year);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
