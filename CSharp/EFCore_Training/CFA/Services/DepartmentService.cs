using CFA.Models;
using CFA.Repositories;

namespace CFA.Services
{
    public class DepartmentService : IDepartmentService
    {
        private readonly IDepartmentRepository _repository;

        public DepartmentService(IDepartmentRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<Department>> GetAllDepartments()
        {
            return await _repository.GetAll();
        }

        public async Task<Department?> GetDepartmentById(int id)
        {
            return await _repository.GetById(id);
        }

        public async Task<Department> CreateDepartment(DepartmentDTO department)
        {
            if (string.IsNullOrWhiteSpace(department.Name))
            {
                throw new ArgumentException("Department name cannot be empty.");
            }
            return await _repository.Add(new Department(){Name = department.Name});
        }

        public async Task<Department?> UpdateDepartment(int id, DepartmentDTO department)
        {
            if (id != department.DepartmentId)
            {
                throw new ArgumentException("ID mismatch.");
            }
            if (string.IsNullOrWhiteSpace(department.Name))
            {
                throw new ArgumentException("Department name cannot be empty.");
            }
            return await _repository.Update(id,new Department(){Name = department.Name});
        }

        public async Task<Department?> DeleteDepartment(int id)
        {
            return await _repository.Delete(id);
        }
    }
}
