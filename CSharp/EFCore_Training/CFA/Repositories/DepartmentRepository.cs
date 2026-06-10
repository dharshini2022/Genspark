using CFA.Context;
using CFA.Models;
using Microsoft.EntityFrameworkCore;

namespace CFA.Repositories
{
    public class DepartmentRepository : IDepartmentRepository
    {
        private readonly AppDbContext _context;

        public DepartmentRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Department>> GetAll()
        {
            return await _context.Departments.ToListAsync();
        }

        public async Task<Department?> GetById(int id)
        {
            return await _context.Departments.SingleOrDefaultAsync(d => d.DepartmentId == id);
        }

        public async Task<Department> Add(Department department)
        {
            _context.Departments.Add(department);
            await _context.SaveChangesAsync();
            return department;
        }

        public async Task<Department?> Update(int id, Department department)
        {
            var existing = await _context.Departments.FindAsync(id);
            if (existing == null) throw new Exception("Department not found");

            existing.Name = department.Name;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<Department?> Delete(int id)
        {
            var existing = await _context.Departments.FindAsync(id);
            if (existing == null) throw new Exception("Department not found");

            _context.Departments.Remove(existing);
            await _context.SaveChangesAsync();
            return existing;
        }
    }
}
