using LCSoft.ApiKey.Attribute;
using Microsoft.AspNetCore.Mvc;

namespace LCSoft.ApiKey.Tests.AttributeTests
{
    public class ApiKeyAttributeTests
    {
        [Fact]
        public void Constructor_SetsCorrectServiceType()
        {
            // Act
            var attribute = new ApiKeyAttribute();

            // Assert
            Assert.IsAssignableFrom<ServiceFilterAttribute>(attribute);
            Assert.Equal(typeof(ApiKeyAuthorizationFilter), attribute.ServiceType);
        }
    }
}
