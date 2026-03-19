using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using EmployeeService;
using EmployeeService.Controller;
using EmployeeService.Model;

namespace InterviewConsole
{
    class Program
    {
        const int testId = 10;

        static void Main(string[] args)
        {
            DataTable dtEmployees = GetQueryResult("SELECT * FROM Employee");

            EmployeeController client = new EmployeeController();

            Console.WriteLine("=== ДЕРЕВО СОТРУДНИКОВ ===\n");

            var employee = client.GetEmployeeById(testId);

            if (employee != null)
            {
                PrintEmployeeTree(employee, 0);
            }
            else
            {
                Console.WriteLine("Root NULL");
            }

            Console.WriteLine($"\n=== UPDATE ID={testId} → Enable:{true} ===");
            // client.EnableEmployee(testId, employee.Enable ? 0 : 1);
            client.EnableEmployee(testId, 1);

            Console.WriteLine($"После Update: {employee.Enable}");
        }

        static void PrintEmployeeTree(Employee emp, int level)
        {
            string indent = new string(' ', level * 3) + (level > 0 ? "├── " : "");
            Console.WriteLine($"{indent}ID={emp.ID}: {emp.Name} (Enable={emp.Enable})");

            foreach (var sub in emp.Subordinates)
            {
                PrintEmployeeTree(sub, level + 1);
            }
        }
        
        private static DataTable GetQueryResult(string query)
        {
            var dt = new DataTable();

			using (var connection = new SqlConnection("Server=localhost,1433;Initial Catalog=Test;User ID=sa;Password=pass@word1; "))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
					command.CommandText = query;

                    using (var adapter = new SqlDataAdapter(command))
                    {
                        adapter.Fill(dt);
                    }
                }
            }

			return dt;
        }
    }
}
