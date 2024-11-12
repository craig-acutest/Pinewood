using Microsoft.EntityFrameworkCore;
using Dapper;

namespace Pinewood.Api.Data
{
    public class DatabaseReset
    {
        private readonly ApplicationDbContext _context;

        public DatabaseReset(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task ResetDatabaseAsync()
        {
            //// Delete the database
            //_context.Database.EnsureDeleted();

            //// Recreate the database
            //_context.Database.EnsureCreated();

            await DropAllTablesAsync();

            // Apply migrations if necessary
            await _context.Database.MigrateAsync();
        }

        public async Task DropAllForeignKeysAsync()
        {
            var foreignKeys = await _context.Database.GetDbConnection().QueryAsync<string>(
                @"SELECT 
            'ALTER TABLE [' + FK.TABLE_SCHEMA + '].[' + FK.TABLE_NAME + '] DROP CONSTRAINT [' + FK.CONSTRAINT_NAME + ']' 
          FROM 
            INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS RC
            JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS FK ON RC.CONSTRAINT_NAME = FK.CONSTRAINT_NAME");

            foreach (var fk in foreignKeys)
            {
                await _context.Database.ExecuteSqlRawAsync(fk);
            }
        }

        public async Task DropAllTablesAsync()
        {
            // Drop all foreign keys
            await DropAllForeignKeysAsync();

            // Get a list of all user tables
            var allTables = await _context.Database.GetDbConnection().QueryAsync<string>(
                "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' AND TABLE_SCHEMA = 'dbo'");

            foreach (var table in allTables)
            {
                // Drop each table if it exists
                await _context.Database.ExecuteSqlRawAsync($"DROP TABLE IF EXISTS [{table}]");
            }
        }
    }
}
