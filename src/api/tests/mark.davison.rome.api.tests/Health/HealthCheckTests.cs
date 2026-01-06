namespace mark.davison.rome.api.tests.Health;

public sealed class HealthCheckTests : ApiIntegrationTestBase
{
    [Test]
    public async Task HealthCheck_ReturnsHealthy()
    {
        var response = await GetRawAsync("/health/readiness");
        await Assert.That(response).IsEqualTo("Healthy");
        response = await GetRawAsync("/health/liveness");
        await Assert.That(response).IsEqualTo("Healthy");
        response = await GetRawAsync("/health/startup");
        await Assert.That(response).IsEqualTo("Healthy");
    }
}