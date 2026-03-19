using System.Data;
using Microsoft.Data.SqlClient;
using WebApplication.Models;

namespace WebApplication.Services;

public class EmployeeService : IEmployeeService
{
    private const string CONNECTION_STRING = "DefaultConnection";

    private readonly string _connectionString;

    public EmployeeService(IConfiguration configuration, ILogger<EmployeeService> logger)
    {
        _connectionString = configuration.GetConnectionString(CONNECTION_STRING)
                            ?? throw new InvalidOperationException($"Connection string '${CONNECTION_STRING}' not found.");
    }

    public async Task<Employee?> LoadEmployeeAsync(int identifier)
    {
        Dictionary<int, Employee> allEmployeesById = await LoadAllEmployeesByIdAsync();

        if (!allEmployeesById.TryGetValue(identifier, out Employee? rootEmployee))
            throw new InvalidOperationException($"Employee with ID {identifier} not found.");

        BuildSubordinatesTree(rootEmployee, allEmployeesById, new HashSet<int>());
        return rootEmployee;
    }

    public async Task UpdateEmployeeEnableStatusAsync(int identifier, bool isEnabled)
    {
        await using SqlConnection connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        await using SqlCommand command = new SqlCommand(
            "UPDATE Employee SET Enable = @isEnabled WHERE ID = @id",
            connection);

        command.Parameters.Add("@id", SqlDbType.Int).Value = identifier;
        command.Parameters.Add("@isEnabled", SqlDbType.Bit).Value = isEnabled;

        int rowsAffected = await command.ExecuteNonQueryAsync();

        if (rowsAffected == 0)
            throw new InvalidOperationException($"Employee with ID {identifier} not found.");

        await command.ExecuteNonQueryAsync();
    }

    public async Task<Dictionary<int, Employee>> LoadAllEmployeesByIdAsync()
    {
        Dictionary<int, Employee> dictionary = new Dictionary<int, Employee>();

        await using SqlConnection connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        await using SqlCommand command = new SqlCommand(
            "SELECT ID, Name, ManagerID, Enable FROM Employee",
            connection);

        await using SqlDataReader? reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            int employeeId = reader.GetInt32("ID");
            Employee employee = new Employee
            {
                ID = employeeId,
                Name = reader.GetString("Name"),
                ManagerID = reader.IsDBNull("ManagerID") ? null : reader.GetInt32("ManagerID"),
                Enable = reader.GetBoolean("Enable")
            };
            dictionary[employeeId] = employee;
        }

        return dictionary;
    }

    public void BuildSubordinatesTree(
        Employee currentEmployee,
        IReadOnlyDictionary<int, Employee> employeesDictionary,
        HashSet<int> visitedIdentifiers)
    {
        if (!visitedIdentifiers.Add(currentEmployee.ID))
            return;

        List<Employee> subordinateEmployees = employeesDictionary.Values
            .Where(employee =>
                employee.ManagerID == currentEmployee.ID &&
                employee.ID != currentEmployee.ID &&
                !visitedIdentifiers.Contains(employee.ID))
            .ToList();

        foreach (Employee subordinateEmployee in subordinateEmployees)
        {
            currentEmployee.Subordinates.Add(subordinateEmployee);
            BuildSubordinatesTree(subordinateEmployee, employeesDictionary, visitedIdentifiers);
        }
    }
}
