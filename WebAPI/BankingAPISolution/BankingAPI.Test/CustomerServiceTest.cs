using BankingAPI.Contexts;
using BankingAPI.Interfaces;
using BankingAPI.Models;
using BankingAPI.Models.DTOs;
using BankingAPI.Repositories;
using BankingAPI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingApiTest
{
    public class CustomerServiceTest
    {
        
        ICustomerInteract customerInteract;
        IRepository<int, Customer> customerRepository;
        IRepository<string, Account> accountRepository;
        IRepository<string, User> userRepository;
       [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<BankingContext>()
                .UseInMemoryDatabase("BankingDb")
                .Options;

            BankingContext bankingContext = new BankingContext(options);

            customerRepository = new Repository<int, Customer>(bankingContext);
            accountRepository = new Repository<string, Account>(bankingContext);
            userRepository = new Repository<string, User>(bankingContext);

            var inMemorySettings = new Dictionary<string, string?>
            {
                { "JWT:Key", "ThisismySuperSecretKeyOn22052026" },
                { "JWT:Issuer", "BankingAPI" },
                { "JWT:ExpiryInMinutes", "60" }
            };

            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            ITokenService tokenService = new TokenService(configuration);

            customerInteract = new CustomerService(
                accountRepository,
                userRepository,
                customerRepository,
                tokenService
            );
        }
            


        [Test]
        public async Task OpenAccountPassTest()
        {
            Customer customer = new Customer()
            {
                Name = "sample",
                Email = "sample@gmail.com",
                Phone = "1234567890",
                Status = "Active",
                DateOfBirth = new DateTime(2004,1,1) 
            };

            await customerRepository.Create(customer);
            CreateAccountRequest request = new CreateAccountRequest
            {
                Balance = 100,
                AccountType = "Saving Account",
                CustomerId = 104,
            };
            var result = await customerInteract.OpensAccount(request);
            Assert.That(result.AccountNumber,Is.Not.Null);
        }

        [TearDown]
        public void TearDown()
        {
            var options = new DbContextOptionsBuilder<BankingContext>().UseInMemoryDatabase("BankingDb").Options;
            BankingContext bankingContext = new BankingContext(options);
            bankingContext.Database.EnsureDeleted();
        }

    }
}