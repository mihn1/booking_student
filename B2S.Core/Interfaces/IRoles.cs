using B2S.Core.Entities;
using System.Threading.Tasks;

namespace B2S.Core.Interfaces
{
    public interface IRoles
    {
        Task UpdateRoles(User appUser, User currentUserLogin);
    }
}
