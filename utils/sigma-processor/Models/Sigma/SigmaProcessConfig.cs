namespace SigmaProcessor.Models.Sigma
{
    public class SigmaProcessConfig
    {
        public bool Enabled { get; set; } = false;
        public string ProductsFileName { get; set; } = "Productos.xml";
        public string StockFileName { get; set; } = "Stock.xml";
        public string ClientsFileName { get; set; } = "Clientes.xml";
        public int EmpresaId { get; set; } = 1;
        
        public Dictionary<string, int> PriceListMapping { get; set; } = new Dictionary<string, int>
        {
            { "2", 1 },  // SIGMA Lista 2 → API Lista 1
            { "4", 5 }   // SIGMA Lista 4 → API Lista 5
        };
        
        public CategoryMappingConfig CategoryMapping { get; set; } = new CategoryMappingConfig();
    }

    public class CategoryMappingConfig
    {
        public bool UseGroupAsCategory { get; set; } = true;  // fgrupo → categoria_id
        public bool UseRubroAsGroup2 { get; set; } = true;   // frubro → grupo2
    }
}