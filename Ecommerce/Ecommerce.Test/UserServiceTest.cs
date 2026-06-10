using AutoMapper;
using Ecommerce.BLL;
using Ecommerce.BLL.Mapper;
using Ecommerce.Contracts.Repositories;
using Ecommerce.Contracts.Services;
using Ecommerce.Models;
using Ecommerce.Models.DTOs;
using Moq;

namespace Ecommerce.Test
{
    public class UserServiceTest
    {
        private Mock<IUserRepository>     _mockUserRepo;
        private Mock<ICurrentUserService> _mockCurrentUser;
        private IMapper                   _mapper;
        private UserService               _userService;

        [SetUp]
        public void Setup()
        {
            _mockUserRepo    = new Mock<IUserRepository>();
            _mockCurrentUser = new Mock<ICurrentUserService>();

            // var config = new MapperConfiguration(cfg => cfg.AddProfile(new MappingProfile()));
            // _mapper = config.CreateMapper();
            _mapper = new Mock<IMapper>().Object;

            _userService = new UserService(_mockUserRepo.Object, _mapper, _mockCurrentUser.Object);
        }


        [Test]
        public async Task GetUserDetails_PassTest_ReturnsProfile()
        {
            // Arrange
            var user = new User { Id = 1, FullName = "Alice", Email = "alice@test.com", Role = UserRole.Customer, IsActive = true };
            _mockUserRepo.Setup(r => r.GetById(1)).ReturnsAsync(user);

            // Act
            var result = await _userService.GetUserDetails(1);

            // Assert
            Assert.That(result.FullName, Is.EqualTo("Alice"));
            Assert.That(result.Email,    Is.EqualTo("alice@test.com"));
        }

        [Test]
        public async Task GetUserDetails_FailTest_UserNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            _mockUserRepo.Setup(r => r.GetById(99)).ReturnsAsync((User?)null);

            // Act & Assert
            Assert.ThrowsAsync<KeyNotFoundException>(async () =>
                await _userService.GetUserDetails(99));
        }

        [Test]
        public async Task GetUserDetails_PassTest_AdminRole_ReturnsCorrectRole()
        {
            // Arrange
            var admin = new User { Id = 2, FullName = "Bob Admin", Email = "bob@test.com", Role = UserRole.Admin, IsActive = true };
            _mockUserRepo.Setup(r => r.GetById(2)).ReturnsAsync(admin);

            // Act
            var result = await _userService.GetUserDetails(2);

            // Assert
            Assert.That(result.Role, Is.EqualTo("Admin"));
        }

        [Test]
        public async Task UpdateProfile_PassTest_UpdatesFullName()
        {
            // Arrange
            var user = new User { Id = 3, FullName = "Carol Old", Email = "carol@test.com", Role = UserRole.Customer, IsActive = true };
            _mockCurrentUser.Setup(c => c.UserId).Returns(3);
            _mockUserRepo.Setup(r => r.GetById(3)).ReturnsAsync(user);
            _mockUserRepo.Setup(r => r.Update(3, It.IsAny<User>())).ReturnsAsync((int id, User u) => u);

            var request = new UserProfileRequest { FullName = "Carol New", Email = "carol@test.com" };

            // Act
            var result = await _userService.UpdateProfile(request);

            // Assert
            Assert.That(result.FullName, Is.EqualTo("Carol New"));
        }

        [Test]
        public async Task UpdateProfile_FailTest_UserNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            _mockCurrentUser.Setup(c => c.UserId).Returns(999);
            _mockUserRepo.Setup(r => r.GetById(999)).ReturnsAsync((User?)null);

            var request = new UserProfileRequest { FullName = "Ghost", Email = "ghost@test.com" };

            // Act & Assert
            Assert.ThrowsAsync<KeyNotFoundException>(async () =>
                await _userService.UpdateProfile(request));
        }

        [Test]
        public async Task UpdateProfile_FailTest_UpdateReturnsNull_ThrowsInvalidOperationException()
        {
            // Arrange
            var user = new User { Id = 4, FullName = "Dave", Email = "dave@test.com", Role = UserRole.Customer, IsActive = true };
            _mockCurrentUser.Setup(c => c.UserId).Returns(4);
            _mockUserRepo.Setup(r => r.GetById(4)).ReturnsAsync(user);
            _mockUserRepo.Setup(r => r.Update(4, It.IsAny<User>())).ReturnsAsync((User?)null);

            var request = new UserProfileRequest { FullName = "Dave New", Email = "dave@test.com" };

            // Act & Assert
            Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _userService.UpdateProfile(request));
        }

        // ─────────────────────────────────────────────────
        //  ListUsers
        // ─────────────────────────────────────────────────

        [Test]
        public async Task ListUsers_PassTest_ReturnsPaged()
        {
            // Arrange
            var users = new List<User>
            {
                new User { Id = 5, FullName = "Eve", Email = "eve@test.com", Role = UserRole.Customer, IsActive = true },
                new User { Id = 6, FullName = "Frank", Email = "frank@test.com", Role = UserRole.Vendor, IsActive = true }
            };
            _mockUserRepo.Setup(r => r.GetPagedUsers("", 1, 10)).ReturnsAsync((users, 2));

            var query = new PageRequest { PageNumber = 1, PageSize = 10 };

            // Act
            var result = await _userService.ListUsers(query);

            // Assert
            Assert.That(result.TotalCount, Is.EqualTo(2));
            Assert.That(result.Items.Count, Is.EqualTo(2));
        }

        [Test]
        public async Task ListUsers_PassTest_WithSearchTerm_FiltersResults()
        {
            // Arrange
            var users = new List<User>
            {
                new User { Id = 7, FullName = "Grace Match", Email = "grace@test.com", Role = UserRole.Customer, IsActive = true }
            };
            _mockUserRepo.Setup(r => r.GetPagedUsers("grace", 1, 5)).ReturnsAsync((users, 1));

            var query = new PageRequest { PageNumber = 1, PageSize = 5, SearchTerm = "grace" };

            // Act
            var result = await _userService.ListUsers(query);

            // Assert
            Assert.That(result.TotalCount, Is.EqualTo(1));
            Assert.That(result.Items.First().FullName, Is.EqualTo("Grace Match"));
        }

        [Test]
        public async Task ListUsers_PassTest_EmptyResult_ReturnZeroCount()
        {
            // Arrange
            _mockUserRepo.Setup(r => r.GetPagedUsers("nobody", 1, 10))
                         .ReturnsAsync((new List<User>(), 0));

            var query = new PageRequest { PageNumber = 1, PageSize = 10, SearchTerm = "nobody" };

            // Act
            var result = await _userService.ListUsers(query);

            // Assert
            Assert.That(result.TotalCount, Is.EqualTo(0));
            Assert.That(result.Items, Is.Empty);
        }

        // ─────────────────────────────────────────────────
        //  ChangeRole
        // ─────────────────────────────────────────────────

        [Test]
        public async Task ChangeRole_PassTest_ChangesRoleToVendor()
        {
            // Arrange
            var user = new User { Id = 8, FullName = "Hank", Email = "hank@test.com", Role = UserRole.Customer, IsActive = true };
            _mockUserRepo.Setup(r => r.GetById(8)).ReturnsAsync(user);
            _mockUserRepo.Setup(r => r.Update(8, It.IsAny<User>())).ReturnsAsync((int id, User u) => u);

            var request = new ChangeRoleRequest { UserId = 8, NewRole = UserRole.Vendor };

            // Act
            var result = await _userService.ChangeRole(request);

            // Assert
            Assert.That(result.Role, Is.EqualTo("Vendor"));
        }

        [Test]
        public async Task ChangeRole_FailTest_UserNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            _mockUserRepo.Setup(r => r.GetById(999)).ReturnsAsync((User?)null);

            var request = new ChangeRoleRequest { UserId = 999, NewRole = UserRole.Admin };

            // Act & Assert
            Assert.ThrowsAsync<KeyNotFoundException>(async () =>
                await _userService.ChangeRole(request));
        }

        [Test]
        public async Task ChangeRole_PassTest_ChangesRoleToAdmin()
        {
            // Arrange
            var user = new User { Id = 9, FullName = "Ivy", Email = "ivy@test.com", Role = UserRole.Customer, IsActive = true };
            _mockUserRepo.Setup(r => r.GetById(9)).ReturnsAsync(user);
            _mockUserRepo.Setup(r => r.Update(9, It.IsAny<User>())).ReturnsAsync((int id, User u) => u);

            var request = new ChangeRoleRequest { UserId = 9, NewRole = UserRole.Admin };

            // Act
            var result = await _userService.ChangeRole(request);

            // Assert
            Assert.That(result.Role, Is.EqualTo("Admin"));
        }

        // ─────────────────────────────────────────────────
        //  ChangePassword
        // ─────────────────────────────────────────────────

        [Test]
        public async Task ChangePassword_PassTest_ValidOldPassword_ReturnsTrue()
        {
            // Arrange
            string oldPassword = "OldPass@1";
            var user = new User
            {
                Id = 10, FullName = "Jack", Email = "jack@test.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(oldPassword),
                Role = UserRole.Customer, IsActive = true
            };
            _mockCurrentUser.Setup(c => c.UserId).Returns(10);
            _mockUserRepo.Setup(r => r.GetById(10)).ReturnsAsync(user);
            _mockUserRepo.Setup(r => r.ChangePassword(10, It.IsAny<string>())).ReturnsAsync(true);

            var request = new ChangePasswordRequest { OldPassword = oldPassword, NewPassword = "NewPass@1" };

            // Act
            var result = await _userService.ChangePassword(request);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public async Task ChangePassword_FailTest_WrongOldPassword_ThrowsUnauthorized()
        {
            // Arrange
            var user = new User
            {
                Id = 11, FullName = "Karen", Email = "karen@test.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("CorrectPass@1"),
                Role = UserRole.Customer, IsActive = true
            };
            _mockCurrentUser.Setup(c => c.UserId).Returns(11);
            _mockUserRepo.Setup(r => r.GetById(11)).ReturnsAsync(user);

            var request = new ChangePasswordRequest { OldPassword = "WrongPass@1", NewPassword = "NewPass@1" };

            // Act & Assert
            Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
                await _userService.ChangePassword(request));
        }

        [Test]
        public async Task ChangePassword_FailTest_UserNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            _mockCurrentUser.Setup(c => c.UserId).Returns(999);
            _mockUserRepo.Setup(r => r.GetById(999)).ReturnsAsync((User?)null);

            var request = new ChangePasswordRequest { OldPassword = "any", NewPassword = "new" };

            // Act & Assert
            Assert.ThrowsAsync<KeyNotFoundException>(async () =>
                await _userService.ChangePassword(request));
        }

        // ─────────────────────────────────────────────────
        //  ToggleAccountStatus
        // ─────────────────────────────────────────────────

        [Test]
        public async Task ToggleAccountStatus_PassTest_ActiveToInactive_ReturnsTrue()
        {
            // Arrange
            var user = new User { Id = 12, FullName = "Leo", Email = "leo@test.com", Role = UserRole.Customer, IsActive = true };
            _mockCurrentUser.Setup(c => c.UserId).Returns(12);
            _mockUserRepo.Setup(r => r.GetById(12)).ReturnsAsync(user);
            _mockUserRepo.Setup(r => r.Update(12, It.IsAny<User>())).ReturnsAsync((int id, User u) => u);

            // Act
            var result = await _userService.ToggleAccountStatus();

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public async Task ToggleAccountStatus_FailTest_UserNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            _mockCurrentUser.Setup(c => c.UserId).Returns(999);
            _mockUserRepo.Setup(r => r.GetById(999)).ReturnsAsync((User?)null);

            // Act & Assert
            Assert.ThrowsAsync<KeyNotFoundException>(async () =>
                await _userService.ToggleAccountStatus());
        }

        [Test]
        public async Task ToggleAccountStatus_PassTest_InactiveToActive_ReturnsTrue()
        {
            // Arrange – user starts inactive
            var user = new User { Id = 13, FullName = "Mia", Email = "mia@test.com", Role = UserRole.Customer, IsActive = false };
            _mockCurrentUser.Setup(c => c.UserId).Returns(13);
            _mockUserRepo.Setup(r => r.GetById(13)).ReturnsAsync(user);
            _mockUserRepo.Setup(r => r.Update(13, It.IsAny<User>())).ReturnsAsync((int id, User u) => u);

            // Act
            var result = await _userService.ToggleAccountStatus();

            // Assert
            Assert.That(result, Is.True);
        }

        // ─────────────────────────────────────────────────
        //  RevokeAdmin
        // ─────────────────────────────────────────────────

        [Test]
        public async Task RevokeAdmin_PassTest_DeactivatesAdmin()
        {
            // Arrange
            var admin = new User { Id = 14, FullName = "Ned Admin", Email = "ned@test.com", Role = UserRole.Admin, IsActive = true };
            _mockUserRepo.Setup(r => r.GetById(14)).ReturnsAsync(admin);
            _mockUserRepo.Setup(r => r.Update(14, It.IsAny<User>())).ReturnsAsync((int id, User u) => u);

            // Act
            var result = await _userService.RevokeAdmin(14);

            // Assert
            Assert.That(result.IsActive, Is.False);
        }

        [Test]
        public async Task RevokeAdmin_FailTest_AdminNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            _mockUserRepo.Setup(r => r.GetById(999)).ReturnsAsync((User?)null);

            // Act & Assert
            Assert.ThrowsAsync<KeyNotFoundException>(async () =>
                await _userService.RevokeAdmin(999));
        }

        [Test]
        public async Task RevokeAdmin_PassTest_ReturnsCorrectEmail()
        {
            // Arrange
            var admin = new User { Id = 15, FullName = "Olivia Admin", Email = "olivia@test.com", Role = UserRole.Admin, IsActive = true };
            _mockUserRepo.Setup(r => r.GetById(15)).ReturnsAsync(admin);
            _mockUserRepo.Setup(r => r.Update(15, It.IsAny<User>())).ReturnsAsync((int id, User u) => u);

            // Act
            var result = await _userService.RevokeAdmin(15);

            // Assert
            Assert.That(result.Email, Is.EqualTo("olivia@test.com"));
        }

        // ─────────────────────────────────────────────────
        //  AddUserAddress
        // ─────────────────────────────────────────────────

        [Test]
        public async Task AddUserAddress_PassTest_ReturnsAddedAddress()
        {
            // Arrange
            _mockCurrentUser.Setup(c => c.UserId).Returns(16);
            var addressReq = new AddAddressRequest
            {
                RecipientName = "Paul", Phone = "9876543210",
                Line1 = "1 Main St", City = "Chennai",
                State = "Tamil Nadu", PostalCode = "600001", Country = "India"
            };

            _mockUserRepo
                .Setup(r => r.AddUserAddress(It.IsAny<UserAddress>()))
                .ReturnsAsync((UserAddress a) => a);

            // Act
            var result = await _userService.AddUserAddress(addressReq);

            // Assert
            Assert.That(result.City, Is.EqualTo("Chennai"));
        }

        [Test]
        public async Task AddUserAddress_PassTest_UsesCurrentUserId()
        {
            // Arrange
            _mockCurrentUser.Setup(c => c.UserId).Returns(17);
            int capturedUserId = 0;

            _mockUserRepo
                .Setup(r => r.AddUserAddress(It.IsAny<UserAddress>()))
                .Callback<UserAddress>(a => capturedUserId = a.UserId)
                .ReturnsAsync((UserAddress a) => a);

            var addressReq = new AddAddressRequest
            {
                RecipientName = "Quinn", Phone = "1234567890",
                Line1 = "5 Lake Rd", City = "Mumbai",
                State = "Maharashtra", PostalCode = "400001", Country = "India"
            };

            // Act
            await _userService.AddUserAddress(addressReq);

            // Assert – the UserId was injected from ICurrentUserService
            Assert.That(capturedUserId, Is.EqualTo(17));
        }

        [Test]
        public async Task AddUserAddress_PassTest_MapsAllFields()
        {
            // Arrange
            _mockCurrentUser.Setup(c => c.UserId).Returns(18);
            _mockUserRepo
                .Setup(r => r.AddUserAddress(It.IsAny<UserAddress>()))
                .ReturnsAsync((UserAddress a) => a);

            var addressReq = new AddAddressRequest
            {
                RecipientName = "Rachel", Phone = "5555555555",
                Line1 = "99 Park Ave", Line2 = "Suite 10",
                Landmark = "Near City Park", City = "Pune",
                State = "Maharashtra", PostalCode = "411001",
                Country = "India", Label = "Home"
            };

            // Act
            var result = await _userService.AddUserAddress(addressReq);

            // Assert
            Assert.That(result.Line1, Is.EqualTo("99 Park Ave"));
            Assert.That(result.Label, Is.EqualTo("Home"));
        }

        // ─────────────────────────────────────────────────
        //  GetAllUserAddress
        // ─────────────────────────────────────────────────

        [Test]
        public async Task GetAllUserAddress_PassTest_ReturnsAddresses()
        {
            // Arrange
            _mockCurrentUser.Setup(c => c.UserId).Returns(19);
            var addresses = new List<UserAddress>
            {
                new UserAddress { Id = 1, UserId = 19, City = "Delhi", RecipientName = "Sam", Phone = "1111111111", Line1 = "1 A", State = "DL", PostalCode = "110001" },
                new UserAddress { Id = 2, UserId = 19, City = "Pune",  RecipientName = "Sam", Phone = "2222222222", Line1 = "2 B", State = "MH", PostalCode = "411001" }
            };
            _mockUserRepo.Setup(r => r.GetAllAddressByUserId(19)).ReturnsAsync(addresses);

            // Act
            var result = await _userService.GetAllUserAddress();

            // Assert
            Assert.That(result.Count, Is.EqualTo(2));
        }

        [Test]
        public async Task GetAllUserAddress_PassTest_EmptyList_ReturnsEmpty()
        {
            // Arrange
            _mockCurrentUser.Setup(c => c.UserId).Returns(20);
            _mockUserRepo.Setup(r => r.GetAllAddressByUserId(20)).ReturnsAsync(new List<UserAddress>());

            // Act
            var result = await _userService.GetAllUserAddress();

            // Assert
            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task GetAllUserAddress_PassTest_ReturnsMappedCityNames()
        {
            // Arrange
            _mockCurrentUser.Setup(c => c.UserId).Returns(21);
            var addresses = new List<UserAddress>
            {
                new UserAddress { Id = 3, UserId = 21, City = "Bangalore", RecipientName = "Tom", Phone = "3333333333", Line1 = "3 C", State = "KA", PostalCode = "560001" }
            };
            _mockUserRepo.Setup(r => r.GetAllAddressByUserId(21)).ReturnsAsync(addresses);

            // Act
            var result = await _userService.GetAllUserAddress();

            // Assert
            Assert.That(result.First().City, Is.EqualTo("Bangalore"));
        }
    }
}
