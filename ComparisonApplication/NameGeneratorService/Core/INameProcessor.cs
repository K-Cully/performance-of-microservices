using System.Collections.Generic;

namespace NameGeneratorService.Core
{
    public interface INameProcessor
    {
        IEnumerable<string> GenerateNames(int count);
    }
}