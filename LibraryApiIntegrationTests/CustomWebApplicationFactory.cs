using LibraryApi.Domain;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using LibraryApi.Services;

namespace LibraryApiIntegrationTests
{
    public class CustomWebApplicationFactory<TStartup>
    : WebApplicationFactory<TStartup> where TStartup : class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Remove the app's ApplicationDbContext registration.

                var employeeIdGeneratorDescriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(IGenerateEmployeeIds)
                    );

                if (employeeIdGeneratorDescriptor != null)
                {
                    services.Remove(employeeIdGeneratorDescriptor);
                }
                services.AddTransient<IGenerateEmployeeIds, TestingEmployeeIdGenerator>();


                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType ==
                        typeof(DbContextOptions<LibraryDataContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }
                // Add ApplicationDbContext using an in-memory database for testing.
                services.AddDbContext<LibraryDataContext>(options =>
                {
                    options.UseInMemoryDatabase("InMemoryDbForTesting");
                });

                // Build the service provider.
                var sp = services.BuildServiceProvider();
                // Create a scope to obtain a reference to the database
                // context (ApplicationDbContext).
                using (var scope = sp.CreateScope())
                {
                    var scopedServices = scope.ServiceProvider;
                    var db = scopedServices.GetRequiredService<LibraryDataContext>();
                    var logger = scopedServices
                        .GetRequiredService<ILogger<CustomWebApplicationFactory<TStartup>>>();
                    // Ensure the database is created.
                    db.Database.EnsureCreated();
                    try
                    {
                        // Seed the database with test data.
                        // Utilities.InitializeDbForTests(db);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "An error occurred seeding the " +
                            "database with test messages. Error: {Message}", ex.Message);
                    }
                }
            });
        }
    }
}