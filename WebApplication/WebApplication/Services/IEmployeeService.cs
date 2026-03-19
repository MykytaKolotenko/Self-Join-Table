using WebApplication.Models;

namespace WebApplication.Services;

public interface IEmployeeService
{
    Task<Employee?> LoadEmployeeAsync(int identifier);
    Task UpdateEmployeeEnableStatusAsync(int identifier, bool isEnabled);
}
