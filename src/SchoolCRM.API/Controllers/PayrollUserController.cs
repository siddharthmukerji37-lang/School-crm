using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolCRM.Application.DTOs;
using SchoolCRM.Application.DTOs.Payroll;
using SchoolCRM.Application.Interfaces.Services;
using SchoolCRM.Shared.Models;

namespace SchoolCRM.API.Controllers;

[ApiController]
[Route("api/payroll")]
[Authorize]
public class PayrollUserController : ControllerBase
{
    private readonly IPayrollService _payrollService;

    public PayrollUserController(IPayrollService payrollService)
    {
        _payrollService = payrollService;
    }

    [HttpGet("my-profile")]
    public async Task<ActionResult<ApiResponse<SalaryProfileDto>>> GetMySalaryProfile()
    {
        var result = await _payrollService.GetMySalaryProfileAsync();
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("my-payrolls")]
    public async Task<ActionResult<ApiResponse<List<PayrollDto>>>> GetMyPayrolls()
    {
        var result = await _payrollService.GetMyPayrollsAsync();
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("my-payrolls/{payrollId}/payslip")]
    public async Task<ActionResult<ApiResponse<PayslipDto>>> GetMyPayslip(Guid payrollId)
    {
        var result = await _payrollService.GetMyPayslipAsync(payrollId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("my-payslips")]
    public async Task<ActionResult<ApiResponse<List<PayslipDto>>>> GetMyPayslips()
    {
        var result = await _payrollService.GetMyPayslipsAsync();
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
