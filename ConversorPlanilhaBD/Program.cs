using Microsoft.Extensions.Configuration;

namespace ConversorPlanilhaBD
{
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            // Monta o leitor de arquivos de configuração 
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();


            //Extrai a string de conexão do appsettings
            string connectionString = configuration.GetConnectionString("PostgresConnection")
                ?? throw new InvalidOperationException("A string de conexão 'PostgresConnection' não foi encontrada no appsettings.json.");

            Application.Run(new FormImportacao(connectionString));
        }
    }
}