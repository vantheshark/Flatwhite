using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Flatwhite.Tests
{
    public interface IUserService
    {
        [OutputCache(Duration = 2, StaleWhileRevalidate = 2, VaryByParam = "userId", RevalidationKey = "User")]
        object GetById(Guid userId);

        [OutputCache(Duration = 2, VaryByParam = "userId", RevalidationKey = "User")]
        Task<object> GetByIdAsync(Guid userId);

        [NoCache, TestHandleException]
        object GetByEmail(string email);

        [NoCache, TestHandleException]
        Task<object> GetByEmailAsync(string email);

        [SwallowException, BadMethodFilter]
        IEnumerable<object> GetRoles(Guid userId);

        [Revalidate("User")]
        void DisableUser(Guid userId);

        [Revalidate("User")]
        [TestHandleException]
        Task DisableUserAsync(Guid userId);

        [OutputCache(Duration = 2, CacheStoreId = 100)]
        object TestCustomStoreId();
    }
}
