using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Ecommerce.BLL;
using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;
using Stripe;

namespace Ecommerce.Test
{
    [TestFixture]
    public class StripeServiceTest
    {
        private Mock<IConfiguration> _mockConfig;

        [SetUp]
        public void Setup()
        {
            _mockConfig = new Mock<IConfiguration>();
        }

        [Test]
        public void Constructor_ShouldThrowException_WhenSecretKeyMissing()
        {
           
            _mockConfig.Setup(c => c["Stripe:SecretKey"]).Returns((string?)null);

            Assert.Throws<InvalidOperationException>(() => new StripeService(_mockConfig.Object));
        }
    }
}
