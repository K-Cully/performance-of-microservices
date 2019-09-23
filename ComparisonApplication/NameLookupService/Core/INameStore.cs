using System.Threading.Tasks;

namespace NameLookupService.Core
{
    public interface INameStore
    {
        Task<string> GetNameAsync(int id);
    }
}