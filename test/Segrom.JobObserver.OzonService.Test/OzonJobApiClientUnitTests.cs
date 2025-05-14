using Newtonsoft.Json;
using Segrom.JobObserver.OzonService.OzonJobApiClient;
using Segrom.JobObserver.OzonService.TestUtils;
using Xunit.Abstractions;

namespace Segrom.JobObserver.OzonService.Test;

public class OzonJobApiClientUnitTests(ITestOutputHelper output)
{
    [Fact]
    public async Task TestApi()
    {
        var httpClient = new HttpClient();
        var logger = new XUnitLogger<ApiClient>(output);
        var client = new ApiClient(httpClient, logger);
        var exception = await Record.ExceptionAsync(async () =>
        {
            var result = await client.GetVacanciesAsync(
                100, 1, CancellationToken.None);
            output.WriteLine($"Result: {JsonConvert.SerializeObject(result)}");
        });
        Assert.Null(exception);
    }
}