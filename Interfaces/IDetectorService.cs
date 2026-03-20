using Models;

namespace Interfaces
{
    public interface IDetectorService
    {
        DerivationType Detect(DerivationInput input);
    }
}
