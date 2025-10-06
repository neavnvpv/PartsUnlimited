// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PartsUnlimited.Models;

namespace PartsUnlimited.UnitTests;

public class TestBase : IDisposable
{
    protected readonly WebApplicationFactory<Program> Factory;
    protected readonly IServiceScope Scope;
    protected readonly PartsUnlimitedContext Context;

    public TestBase()
    {
        Factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");
                builder.ConfigureServices(services =>
                {
                    var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<PartsUnlimitedContext>));
                    if (descriptor != null)
                        services.Remove(descriptor);

                    services.AddDbContext<PartsUnlimitedContext>(options =>
                        options.UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}"));
                });
            });

        Scope = Factory.Services.CreateScope();
        Context = Scope.ServiceProvider.GetRequiredService<PartsUnlimitedContext>();
    }

    public void Dispose()
    {
        Context?.Dispose();
        Scope?.Dispose();
        Factory?.Dispose();
        GC.SuppressFinalize(this);
    }
}