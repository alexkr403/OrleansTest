using System.Net;
using System.Threading.Tasks;
using NUnit.Framework;
using Orleans.Configuration;
using Orleans.Hosting;

namespace OrleansTests;

[TestFixture]
public class DemoFixture
{
    private const string ClusterId = "TestCluster";
    private const string ServiceId = "ServiceIdName";

    [Test]
    public async Task Hanging()
    {
        var host1 = CreateHost(11111, 30001, null);
        var host2 = CreateHost(11112, 30002, 11111);

        await host1.StartAsync();
        await host2.StartAsync();

        // it hangs
        await DisposeHostsAsync(host1, host2);

        // it works
        //await DisposeHostsAsync(host2, host1);
    }

    private static async Task DisposeHostsAsync(ISiloHost first, ISiloHost second)
    {
        await first.StopAsync();
        await first.DisposeAsync();

        await second.StopAsync();
        await second.DisposeAsync();
    }

    private static ISiloHost CreateHost(int siloPort, int gatewayPort, int? primarySiloPort)
    {
        var siloHostBuilder = new SiloHostBuilder()
            .Configure<ClusterOptions>(
                options =>
                {
                    options.ClusterId = ClusterId;
                    options.ServiceId = ServiceId;
                })
            .ConfigureEndpoints(siloPort, gatewayPort);

        IPEndPoint primarySilo = default;
        if (primarySiloPort.HasValue)
        {
            primarySilo = new IPEndPoint(IPAddress.Loopback, primarySiloPort.Value);
        }

        siloHostBuilder.UseLocalhostClustering(siloPort, gatewayPort, primarySilo, ServiceId, ClusterId);

        return siloHostBuilder.Build();
    }
}