using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Flatwhite.Tests
{
    
    public interface IUserService
    {
        [OutputCache(Duration = 1000, VaryByParam = "userId")]
        object GetById(Guid userId);

        


        [NoCache]
        object GetByEmail(string email);

        IEnumerable<object> GetRoles(Guid userId);

        
    }
}
