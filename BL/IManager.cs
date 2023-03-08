using Home.Model;

namespace Home.BL;

public interface IManager
{
    Product? GetProduct(int id);
    IList<Product> GetProducts();
    IList<Product> GetProductsByCategory(int category);
}