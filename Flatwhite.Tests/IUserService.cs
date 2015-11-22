using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Flatwhite.Tests
{
    public interface IUserService
    {
        [OutputCache(Duration = 2000, StaleWhileRevalidate = 2000, VaryByParam = "userId", RevalidationKey = "User")]
        object GetById(Guid userId);

        [OutputCache(Duration = 2000, VaryByParam = "userId", RevalidationKey = "User")]
        Task<object> GetByIdAsync(Guid userId);

        [NoCache, TestHandleException]
        object GetByEmail(string email);

        [NoCache, TestHandleException]
        Task<object> GetByEmailAsync(string email);

        IEnumerable<object> GetRoles(Guid userId);

        [Revalidate("User")]
        void DisableUser(Guid userId);

        [OutputCache(Duration = 2000, CacheStoreId = 100)]
        object TestCustomStoreId();
    }
}
