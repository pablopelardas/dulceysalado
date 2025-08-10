using SigmaProcessor.Models;
using SigmaProcessor.Models.Sigma;

namespace SigmaProcessor.Services.Sigma
{
    public interface ISigmaDataTransformer
    {
        List<ProductWithPricesAndStock> TransformToApiFormat(
            List<SigmaProduct> products,
            List<SigmaStock> stocks,
            SigmaProcessConfig config);
        
        PriceList MapPriceList(decimal price, int sigmaListId, SigmaProcessConfig config);
        EmpresaStock MapStock(SigmaStock stock, int empresaId);
    }
}