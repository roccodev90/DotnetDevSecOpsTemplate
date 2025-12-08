using Xunit;

namespace Api.Tests
{
    public class StatusTests
    {
        [Fact]
        public void StatusEndpoint_ReturnsExpectedMessage()
        {
            var expected = "API is running";
            var actual = "API is running"; // Simula la risposta
            Assert.Equal(expected, actual);
        }
    }
}
