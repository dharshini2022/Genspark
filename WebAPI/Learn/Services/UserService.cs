using Learn.Models;
using Learn.Repositories;
namespace Learn.Services
{
    public class UserService : IUserService
    {
        private readonly IRepository<int, User> _userRepository;

        public UserService(IRepository<int, User> userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<User> Add(UserDTO userDTO)
        {
            var user = MapToUser(userDTO);
            return await _userRepository.Create(user);
        }

        public async Task<ICollection<User>> GetAll()
        {
            return await _userRepository.GetAll();
        }

        public async Task<User?> GetById(int id)
        {
            return await _userRepository.GetById(id);
        }

        public async Task<ICollection<User>> AddBulk(ICollection<UserDTO> userDTOs)
        {
            var users = userDTOs.Select(MapToUser).ToList();
            return await _userRepository.CreateBulk(users);
        }

        private User MapToUser(UserDTO userDTO)
        {
            return new User
            {
                Name = userDTO.Name,
                Email = userDTO.Email,
                Phone = userDTO.Phone,
                Age = userDTO.Age
            };
        }
    }
}