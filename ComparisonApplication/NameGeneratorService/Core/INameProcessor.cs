using System.Collections.Generic;
using System.Threading.Tasks;

namespace NameGeneratorService.Core
{
    public interface INameProcessor
    {
        Task<IEnumerable<string>> GenerateNamesAsync(int count);
    }
}