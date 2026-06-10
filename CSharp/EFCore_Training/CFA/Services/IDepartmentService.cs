using CFA.Models;

namespace CFA.Services
{
    public interface IDepartmentService
    {
        Task<IEnumerable<Department>> GetAllDepartments();
        Task<Department?> GetDepartmentById(int id);
        Task<Department> CreateDepartment(DepartmentDTO department);
        Task<Department?> UpdateDepartment(int id, DepartmentDTO department);
        Task<Department?> DeleteDepartment(int id);
    }
}
