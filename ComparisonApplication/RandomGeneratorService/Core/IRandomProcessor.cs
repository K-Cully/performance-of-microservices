using System.Threading.Tasks;

namespace RandomGeneratorService.Core
{
    public interface IRandomProcessor
    {
        Task<int> GetRandomNumber();
    }
}