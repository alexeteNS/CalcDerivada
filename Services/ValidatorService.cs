using Interfaces;
using Models;

namespace Services
{
    public class ValidatorService : IValidatorService
    {
        private readonly IValidatorRepository _repo;
        public string ValidationError { get; private set; } = "";

        public ValidatorService(IValidatorRepository repo)
        {
            _repo = repo;
        }

        public bool Validate(DerivationInput input)
        {
            if (!_repo.IsValidFormat(input.Raw))
            {
                ValidationError = "Formato inválido. Usa solo: números, x, ^, +, -";
                return false;
            }
            if (!_repo.IsDerivable(input.Raw))
            {
                ValidationError = "La expresión no contiene términos derivables.";
                return false;
            }
            ValidationError = "";
            return true;
        }
    }
}
