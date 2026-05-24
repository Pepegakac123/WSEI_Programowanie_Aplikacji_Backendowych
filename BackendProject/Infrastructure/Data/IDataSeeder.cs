namespace Infrastructure.Data;

public interface IDataSeeder
{
    int Order { get; }
    Task SeedAsync();
}