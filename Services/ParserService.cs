using Interfaces;
using Models;

namespace Services
{
    public class ParserService : IParserService
    {
        private readonly IParserRepository _repo;

        public ParserService(IParserRepository repo)
        {
            _repo = repo;
        }

        public ParsedDerivation Parse(DerivationInput input, DerivationType type)
        {
            return _repo.BuildParsed(input, type);
        }
    }
}
