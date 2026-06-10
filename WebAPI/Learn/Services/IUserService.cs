using Learn.Models;
namespace Learn.Services
{
    public interface IUserService
    {
        public Task<User> Add(UserDTO userDTO);
        public Task<ICollection<User>> AddBulk(ICollection<UserDTO> userDtos);
        public Task<User?> GetById(int id);
        public Task<ICollection<User>> GetAll();
    }
}