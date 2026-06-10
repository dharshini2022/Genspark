using CFA.Models;

namespace CFA.Repositories
{
    public interface IDepartmentRepository
    {
        Task<IEnumerable<Department>> GetAll();
        Task<Department?> GetById(int id);
        Task<Department> Add(Department department);
        Task<Department?> Update(int id, Department department);
        Task<Department?> Delete(int id);
    }
}
