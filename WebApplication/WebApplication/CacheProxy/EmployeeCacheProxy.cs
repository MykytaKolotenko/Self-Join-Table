using System.Collections.Concurrent;
using WebApplication.Models;

namespace WebApplication.Services;

public class EmployeeCacheProxy : IEmployeeService
{
    private static readonly ConcurrentDictionary<int, Employee?> _cache = new ConcurrentDictionary<int, Employee?>();
    private static readonly SemaphoreSlim _cacheLock = new SemaphoreSlim(1, 1);

    private readonly IEmployeeService _innerService;

    public EmployeeCacheProxy(IEmployeeService innerService, ILogger<EmployeeCacheProxy> logger)
    {
        _innerService = innerService;
    }

    public async Task<Employee?> LoadEmployeeAsync(int identifier)
    {
        if (_cache.TryGetValue(identifier, out Employee? cached))
            return cached;

        await _cacheLock.WaitAsync();
        try
        {
            if (!_cache.TryGetValue(identifier, out cached))
            {
                cached = await _innerService.LoadEmployeeAsync(identifier);
                if (cached != null)
                    _cache[identifier] = cached;
            }
        }
        finally
        {
            _cacheLock.Release();
        }

        return cached;
    }

    public async Task UpdateEmployeeEnableStatusAsync(int identifier, bool isEnabled)
    {
        await _innerService.UpdateEmployeeEnableStatusAsync(identifier, isEnabled);
        await _cacheLock.WaitAsync();

        try
        {
            _cache.Clear();
        }
        finally
        {
            _cacheLock.Release();
        }
    }
}
