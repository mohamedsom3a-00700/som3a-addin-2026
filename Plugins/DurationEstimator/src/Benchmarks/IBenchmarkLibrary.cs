namespace Som3a.DurationEstimator.Benchmarks;

public interface IBenchmarkLibrary
{
    ProductivityRate GetById(string id);
    IEnumerable<ProductivityRate> GetByTradeCategory(string tradeCategoryId);
    IEnumerable<ProductivityRate> Search(string query);
    ProductivityRate Add(ProductivityRate rate);
    ProductivityRate Update(ProductivityRate rate);
    void Delete(string id);
    void ImportBuiltIn();
}
