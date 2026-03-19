using System.ServiceModel;
using EmployeeService.Controller.Interface;
using EmployeeService.Model;

namespace EmployeeService.Controller
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select Service1.svc or Service1.svc.cs at the Solution Explorer and start debugging.
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public class EmployeeController : IEmployeeController
    {
        private readonly Service.EmployeeService _service = new Service.EmployeeService();

        public Employee GetEmployeeById(int id)
        {
            return _service.LoadEmployee(id);
        }

        public void EnableEmployee(int id, int enable)
        {
            _service.UpdateEmployeeEnableStatus(id, enable == 1);
        }
    }
}
