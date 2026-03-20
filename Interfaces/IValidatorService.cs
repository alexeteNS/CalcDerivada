using Models;

namespace Interfaces
{
    public interface IValidatorService
    {
        bool Validate(DerivationInput input);
        string ValidationError { get; }
    }
}
