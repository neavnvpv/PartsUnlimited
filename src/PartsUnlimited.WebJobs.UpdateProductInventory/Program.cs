// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace PartsUnlimited.WebJobs.UpdateProductInventory
{
    public class Program
    {
        public static async Task<int> Main(string[] args)
        {
            var builder = new ConfigurationBuilder();
            builder.AddJsonFile("config.json", optional: true);
            var config = builder.Build();
            var webjobsConnectionString = config["Data:AzureWebJobsStorage:ConnectionString"];
            var dbConnectionString = config["Data:DefaultConnection:ConnectionString"];

            if (string.IsNullOrWhiteSpace(webjobsConnectionString))
            {
                Console.WriteLine("The configuration value for Azure Web Jobs Connection String is missing.");
                return 10;
            }

            if (string.IsNullOrWhiteSpace(dbConnectionString))
            {
                Console.WriteLine("The configuration value for Database Connection String is missing.");
                return 10;
            }

            var host = new HostBuilder()
                .ConfigureWebJobs(b =>
                {
                    b.AddAzureStorageQueues();
                    b.AddAzureStorageBlobs();
                })
                .ConfigureLogging((context, b) =>
                {
                    b.AddConsole();
                })
                .ConfigureServices(services =>
                {
                    services.AddSingleton<IConfiguration>(config);
                })
                .Build();

            using (host)
            {
                await host.RunAsync();
            }
            return 0;
        }
    }
}
