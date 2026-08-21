using SchoolCRM.Application.DTOs;
using SchoolCRM.Application.DTOs.Payroll;
using SchoolCRM.Shared.Models;

namespace SchoolCRM.Application.Interfaces.Services;

public interface IPayrollService
{
    Task<ApiResponse<PayrollSettingDto>> GetPayrollSettingsAsync();
    Task<ApiResponse<PayrollSettingDto>> SavePayrollSettingsAsync(CreatePayrollSettingDto dto);

    Task<ApiResponse<SalaryProfileDto>> GetSalaryProfileAsync(string userId);
    Task<ApiResponse<SalaryProfileDto>> GetMySalaryProfileAsync();
    Task<ApiResponse<List<SalaryProfileDto>>> GetAllSalaryProfilesAsync();
    Task<ApiResponse<SalaryProfileDto>> CreateSalaryProfileAsync(CreateSalaryProfileDto dto);
    Task<ApiResponse<SalaryProfileDto>> UpdateSalaryProfileAsync(Guid id, CreateSalaryProfileDto dto);

    Task<ApiResponse<List<SalaryComponentDto>>> GetSalaryComponentsAsync(Guid profileId);
    Task<ApiResponse<SalaryComponentDto>> AddSalaryComponentAsync(Guid profileId, CreateSalaryComponentDto dto);
    Task<ApiResponse<SalaryComponentDto>> UpdateSalaryComponentAsync(Guid id, CreateSalaryComponentDto dto);
    Task<ApiResponse> DeleteSalaryComponentAsync(Guid id);

    Task<ApiResponse<List<PayrollDto>>> GenerateMonthlyPayrollAsync(GeneratePayrollDto dto);
    Task<ApiResponse<PayrollDto>> GetPayrollAsync(Guid id);
    Task<ApiResponse<List<PayrollDto>>> GetPayrollsAsync(int month, int year);
    Task<ApiResponse<PayrollDto>> ApprovePayrollAsync(Guid id);
    Task<ApiResponse<PayrollDto>> MarkPaidAsync(Guid id);
    Task<ApiResponse<PayslipDto>> GeneratePayslipAsync(Guid payrollId);
    Task<ApiResponse<List<PayrollDto>>> GetMyPayrollsAsync();
    Task<ApiResponse<PayslipDto>> GetMyPayslipAsync(Guid payrollId);
    Task<ApiResponse<List<PayslipDto>>> GetMyPayslipsAsync();
    Task<ApiResponse<PayrollReportDto>> GetPayrollReportAsync(int month, int year);
}
