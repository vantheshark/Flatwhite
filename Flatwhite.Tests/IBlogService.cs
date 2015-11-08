using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Flatwhite.Tests
{
    [OutputCache(Duration = 50000)]
    public interface IBlogService
    {
        object GetById(Guid postId);
        
        Task<object> GetByIdAsync(Guid userId);

        [NoCache] // This should be applied if EnableInterfaceInterceptors
        IEnumerable<object> GetComments(Guid postId, int count);
    }

    public class BlogService : IBlogService
    {
        [OutputCache(Duration = 2000)]
        public virtual object GetById(Guid postId)
        {
            InvokeCount++;
            return new {};
        }

        public async Task<object> GetByIdAsync(Guid userId)
        {
            await Task.Delay(1000);
            InvokeCount++;
            return new { };
        }

        [OutputCache(Duration = 2000)]  // This should be applied if EnableClassInterceptors
        public virtual IEnumerable<object> GetComments(Guid postId, int count)
        {
            InvokeCount++;
            yield break;
        }

        public int InvokeCount { get; private set; }
    }
}