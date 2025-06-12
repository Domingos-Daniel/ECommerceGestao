using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Data;

namespace ECommerceGestao.Data
{
    public static class DatabaseExtensions
    {
        public static async Task UpdateDatabaseSchemaAsync(IServiceProvider serviceProvider, ILogger logger)
        {
            try
            {
                using var scope = serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                
                // Obtém a conexão com o banco de dados
                var connection = dbContext.Database.GetDbConnection() as SqliteConnection;
                
                if (connection.State != ConnectionState.Open)
                {
                    await connection.OpenAsync();
                }
                
                // Verificamos se a coluna "IdentityNumber" já existe na tabela AspNetUsers
                bool columnExists = false;
                
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "PRAGMA table_info(AspNetUsers)";
                    
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            string columnName = reader.GetString(1);
                            if (columnName.Equals("IdentityNumber", StringComparison.OrdinalIgnoreCase))
                            {
                                columnExists = true;
                                break;
                            }
                        }
                    }
                }
                
                // Se a coluna não existir, adicionamos ela
                if (!columnExists)
                {
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "ALTER TABLE AspNetUsers ADD COLUMN IdentityNumber TEXT DEFAULT ''";
                        await command.ExecuteNonQueryAsync();
                        logger.LogInformation("Coluna IdentityNumber adicionada com sucesso à tabela AspNetUsers");
                    }
                }
                
                if (connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
                
                logger.LogInformation("Atualização do esquema do banco de dados concluída com sucesso");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro ao atualizar o esquema do banco de dados");
                throw;
            }
        }
    }
}
