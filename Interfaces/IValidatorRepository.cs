using Models;

namespace Interfaces
{
    public interface IValidatorRepository
    {
        bool IsValidFormat(string input);
        bool IsDerivable(string input);
    }
}
