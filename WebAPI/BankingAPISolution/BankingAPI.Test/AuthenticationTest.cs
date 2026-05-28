using BankingAPI.Contexts;
using BankingAPI.Controllers;
using BankingAPI.Interfaces;
using BankingAPI.Models;
using BankingAPI.Models.DTOs;
using BankingAPI.Repositories;
using BankingAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;


namespace BankingAPI.Test
{
    public class AuthenticationTest
    {
        IAuthenticationService authenticationService;
        IRepository<int, Customer> customerRepository;
        IRepository<string, Account> accountRepository;
        IRepository<string, User> userRepository;
        [SetUp]
        public async Task SetupAsync()
        {
            var options = new DbContextOptionsBuilder<BankingContext>().UseInMemoryDatabase("BankingDb").Options;
            BankingContext bankingContext = new BankingContext(options);
            customerRepository = new Repository<int, Customer>(bankingContext);
            accountRepository = new Repository<string, Account>(bankingContext);
            userRepository = new Repository<string, User>(bankingContext);
            var inMemorySettings = new Dictionary<string, string>
            {
                { "JWT:Key", "BankingAPI_JWT_SECRET_KEY_2026_SUPER_LONG_32_PLUS_CHARS" },
                { "JWT:Issuer", "G3 Server" },
                { "JWT:ExpiryInMinutes", "60" }
            };
            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            ITokenService tokenService = new TokenService(configuration);

            authenticationService = new CustomerService(accountRepository, userRepository, customerRepository, tokenService);
            await authenticationService.Register(new RegisterUserRequest
            {
                Username = "john_doe",
                Name = "John Doe",
                Email = "john@gamil.com",
                DateOfBirth = new DateTime(2000, 1, 1),
                Phone = "1234567890",
                Status = "Active"

            });
        }

        [Test]
        public async Task LoginAsync()
        {
            //Arrange
            LoginRequest request = new LoginRequest
            {
                Username = "john_doe",
                Password = "john1234"
            };
            Mock<ILogger<AuthenticationController>> mockLogger = new Mock<ILogger<AuthenticationController>>();
            //Act
            var result = await new AuthenticationController(authenticationService, mockLogger.Object).CustomerLogin(request);
            Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
        }
    }
}