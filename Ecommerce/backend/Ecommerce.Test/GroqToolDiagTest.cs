using NUnit.Framework;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Threading.Tasks;
using System.Net.Http;
using System;
using Ecommerce.BLL;

namespace Ecommerce.Test
{
    [TestFixture]
    public class GroqToolDiagTest
    {
        [Test]
        public void TestExecutionSettingsCompilation()
        {
            var settings = new OpenAIPromptExecutionSettings
            {
                ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
            };
            Assert.That(settings, Is.Not.Null);
            Console.WriteLine("[Diag Compile] OpenAIPromptExecutionSettings successfully compiled.");
        }

        [Test]
        public void TestChatbotPlugins_GetProductDetailsRegistration()
        {
            var plugins = new ChatbotPlugins(null!, null!, null!, null!, null!, null!, null!, null!, null!, null!, null!, null!);
            var kernel = Kernel.CreateBuilder().Build();
            var plugin = kernel.Plugins.AddFromObject(plugins, "ECommercePlugins");
            
            Assert.That(plugin.Contains("GetProductDetails"), Is.True);
            Assert.That(plugin.Contains("SearchProducts"), Is.True);

            var function = plugin["GetProductDetails"];
            Assert.That(function, Is.Not.Null);
            Assert.That(function.Description, Does.Contain("Retrieve detailed specifications"));
            
            var parameters = function.Metadata.Parameters;
            Assert.That(parameters, Has.Count.EqualTo(1));
            Assert.That(parameters[0].Name, Is.EqualTo("productId"));
            Assert.That(parameters[0].ParameterType, Is.EqualTo(typeof(int)));
        }
    }
}
