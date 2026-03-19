using Microsoft.AspNetCore.Mvc;
using WebApplication.Models;
using WebApplication.Services;

namespace WebApplication.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmployeeController : ControllerBase
{
    private readonly IEmployeeService _service;

    public EmployeeController(IEmployeeService service)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Employee>> GetEmployeeById(int id)
    {
        try
        {
            Employee? employee = await _service.LoadEmployeeAsync(id);
            if (employee == null)
                return NotFound();
            return Ok(employee);
        }
        catch (InvalidOperationException ex)
            when (ex.Message.Contains("Employee") && ex.Message.Contains("not found"))
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPost("{id}/enable")]
    public async Task<IActionResult> EnableEmployee(int id, [FromBody] RequestDTO.EnableRequest request)
    {
        try
        {
            await _service.UpdateEmployeeEnableStatusAsync(id, request.Enable);
            return NoContent();
        }
        catch (InvalidOperationException ex)
            when (ex.Message.Contains("Employee") && ex.Message.Contains("not found"))
        {
            return NotFound(ex.Message);
        }
    }
}
