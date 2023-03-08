using Fermyon.Spin.Sdk;
using Home.Model;

namespace Home.Repository;

public class Repository
{
    private readonly string _connectionString;
    public Repository()
    {
        _connectionString = @"user=postgres password=PasswordIP2! dbname=postgres host=127.0.0.1 port=1433";
    }

    public Product? GetProduct(int id)
    {
        var result = PostgresOutbound.Query(_connectionString, "SELECT * FROM products where \"Id\" = $1", id);
        return result.Rows.Select(row => new Product()
        {
            Id = (int) (row[0].Value() ?? 0), Name = ((string) row[1].Value()!)!, Description = (string) row[2].Value()!,
            Price = (int) row[3].Value()!
        }).SingleOrDefault();
    }

    public List<Product> GetProducts()
    {
        var result = PostgresOutbound.Query(_connectionString, "SELECT * FROM products");
        return result.Rows.Select(row => new Product()
        {
            Id = (int) (row[0].Value() ?? 0), Name = ((string) row[1].Value()!)!, Description = (string) row[2].Value()!,
            Price = (int) row[3].Value()!
        }).ToList();
    }

    public IList<Product> GetProductsByCategory(int category)
    {
        var result = PostgresOutbound.Query(_connectionString, "SELECT * FROM products where \"Category_id\" = $1", category);
        return result.Rows.Select(row => new Product()
        {
            Id = (int) (row[0].Value() ?? 0), Name = ((string) row[1].Value()!)!,
            Description = (string) row[2].Value()!,
            Price = (int) row[3].Value()!
        }).ToList();
    }
}