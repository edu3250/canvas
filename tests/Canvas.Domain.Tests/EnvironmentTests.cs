using Xunit;

namespace Canvas.Domain.Tests;

public class EnvironmentTests
{
    [Fact]
    public void RunsOnNetTen() => Assert.Equal(10, Environment.Version.Major);
}
