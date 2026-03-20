using Interfaces;
using Models;

namespace Services
{
    public class DetectorService : IDetectorService
    {
        private readonly IDetectorRepository _repo;

        public DetectorService(IDetectorRepository repo)
        {
            _repo = repo;
        }

        public DerivationType Detect(DerivationInput input)
        {
            return _repo.DetectType(input);
        }
    }
}
