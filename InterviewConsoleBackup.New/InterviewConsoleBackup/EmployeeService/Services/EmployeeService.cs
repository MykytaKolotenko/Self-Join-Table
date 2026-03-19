using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using EmployeeService.Model;
using EmployeeService.Models;

namespace EmployeeService.Service
{
    public class EmployeeService
    {
        private const string ConnectionString = "Server=localhost,1433;Initial Catalog=Test;User ID=sa;Password=pass@word1;";
        private readonly Dictionary<int, Employee> _employeeDictionary;

        public EmployeeService()
        {
            _employeeDictionary = LoadAllEmployeesById();
        }

        public Employee LoadEmployee(int identifier)
        {
            if (!_employeeDictionary.TryGetValue(identifier, out var rootEmployee))
                throw new NotFoundException($"Employee ID {identifier} not found");;

            BuildSubordinatesTree(rootEmployee, _employeeDictionary, new HashSet<int>());
            return rootEmployee;
        }

        public void UpdateEmployeeEnableStatus(int identifier, bool isEnabled)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {

                connection.Open();
                using var command = new SqlCommand(
                    "UPDATE Employee SET Enable = @isEnabled WHERE ID = @identifier",
                    connection);

                command.Parameters.Add("@identifier", SqlDbType.Int).Value   = identifier;
                command.Parameters.Add("@isEnabled", SqlDbType.Bit).Value    = isEnabled;

                int rowsAffected = command.ExecuteNonQuery();

                 if (rowsAffected == 0)
                    throw new NotFoundException($"Employee ID {identifier} not found");

                command.ExecuteNonQuery();

                _employeeDictionary[identifier].Enable = isEnabled;
            }
        }

        private Dictionary<int, Employee> LoadAllEmployeesById()
        {
            var dictionary = new Dictionary<int, Employee>();

            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                using var command = new SqlCommand(
                    "SELECT ID, Name, ManagerID, Enable FROM Employee",
                    connection);
                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    var identifier = Convert.ToInt32(reader["ID"]);
                    var employee = new Employee
                    {
                        ID = identifier,
                        Name = reader["Name"].ToString(),
                        ManagerID = reader["ManagerID"] == DBNull.Value
                            ? (int?)null
                            : Convert.ToInt32(reader["ManagerID"]),
                        Enable = Convert.ToBoolean(reader["Enable"]),
                        Subordinates = new List<Employee>()
                    };
                    dictionary[identifier] = employee;
                }
            }

            return dictionary;
        }

        private void BuildSubordinatesTree(Employee currentEmployee, IReadOnlyDictionary<int, Employee> employeesDictionary, HashSet<int> visitedIdentifiers)
        {
            if (!visitedIdentifiers.Add(currentEmployee.ID))
                return;

            var subordinateEmployees = employeesDictionary.Values
                .Where(employee =>
                    employee.ManagerID == currentEmployee.ID &&
                    employee.ID != currentEmployee.ID &&
                    !visitedIdentifiers.Contains(employee.ID))
                .ToList();

            foreach (var subordinate in subordinateEmployees)
            {
                currentEmployee.Subordinates.Add(subordinate);
                BuildSubordinatesTree(subordinate, employeesDictionary, visitedIdentifiers);
            }
        }

    }
}
