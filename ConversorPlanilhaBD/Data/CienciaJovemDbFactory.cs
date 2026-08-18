using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConversorPlanilhaBD.Data
{
    //Essa classse serve para o EF Core ler appsettings.json para criar o banco
    public class CienciaJovemDbFactory : IDesignTimeDbContextFactory<CienciaJovemDb>
    {
        public CienciaJovemDb CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            string connectionString = configuration.GetConnectionString("PostgresConnection")
                ?? throw new InvalidOperationException("String de conexão não encontrada no appsettings.json para design-time.");

            var optionsBuilder = new DbContextOptionsBuilder<CienciaJovemDb>();
            optionsBuilder.UseNpgsql(connectionString);

            return new CienciaJovemDb(optionsBuilder.Options);
        }
    }
}
