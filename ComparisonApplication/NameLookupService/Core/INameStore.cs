using System.Threading.Tasks;

namespace NameLookupService.Core
{
    public interface INameStore
    {
        Task<string> GetName(int id);
    }
}