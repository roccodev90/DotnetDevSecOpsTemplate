using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using IndustrialSecureApi.Infrastructure;
using IndustrialSecureApi;

namespace IndustrialSecureApi.Tests.Integration.TestHelpers;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Rimuovi il DbContext reale
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Aggiungi DbContext in-memory per i test
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseInMemoryDatabase("TestDatabase");
            });

            // Assicurati che Authorization sia configurato (necessario per UseAuthorization)
            services.AddAuthorization();
        });
    }
}