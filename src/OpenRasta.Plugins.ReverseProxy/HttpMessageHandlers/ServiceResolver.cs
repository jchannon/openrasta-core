using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace OpenRasta.Plugins.ReverseProxy.HttpMessageHandlers
{
  public class ServiceResolver : IDisposable
  {
    readonly Func<string, Task<IPAddress[]>> _dnsResolver;
    readonly TimeSpan _refreshTime;
    readonly ConcurrentDictionary<string, IPAddress[]> _ipCache = new ConcurrentDictionary<string, IPAddress[]>();
    readonly CancellationTokenSource _stopRefresh;

    public ServiceResolver(Func<string,Task<IPAddress[]>> dnsResolver, TimeSpan refreshTime)
    {
      _dnsResolver = dnsResolver;
      _refreshTime = refreshTime;
      _stopRefresh = new CancellationTokenSource();
      
      Task.Run(Refresh, _stopRefresh.Token);
    }

    async Task Refresh()
    {
      while (!_stopRefresh.Token.IsCancellationRequested)
      {
        foreach (var domain in _ipCache.Keys.ToList())
        {
          try
          {
            await QueryDns(domain);
          }
          catch
          {
            _ipCache.TryRemove(domain, out _);
          }
        }
        await Task.Delay(_refreshTime, _stopRefresh.Token);
      }
    }

    public async Task<IPAddress> GetIPAddressAsync(string hostName)
    {
      var ips = await QueryDns(hostName);
      return ips[0];
    }

    public Task<IPAddress[]> Resolve(string hostName)
    {
      return _ipCache.TryGetValue(hostName, out var addresses) 
        ? Task.FromResult(addresses)
        : QueryDns(hostName);
    } 
    async Task<IPAddress[]> QueryDns(string hostName)
    {
      var ips = await _dnsResolver(hostName);
      _ipCache.AddOrUpdate(hostName, ips, (s, addresses) => ips);
      return ips;
    }

    public bool Contains(string hostName, IPAddress address)
    {
      return _ipCache.TryGetValue(hostName, out var values)
             && Array.Exists(values, a=>Equals(a, address));
    }

    public void Dispose()
    {
      _stopRefresh.Cancel();
    }
  }
}