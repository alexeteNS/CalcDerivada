using Models;

namespace Interfaces
{
    public interface IDetectorRepository
    {
        DerivationType DetectType(DerivationInput input);
    }
}
