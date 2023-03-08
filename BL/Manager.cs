using Home.Model;

namespace Home.BL;

public class Manager : IManager
{
    private readonly Repository.Repository _repository;
    public Manager(Repository.Repository repository)
    {
        _repository = repository;
    }
    
    public Product? GetProduct(int id)
    {
        return _repository.GetProduct(id);
    }
    
    public IList<Product> GetProducts()
    {
        return _repository.GetProducts();
    }

    public IList<Product> GetProductsByCategory(int category)
    {
        return _repository.GetProductsByCategory(category);
    }
}