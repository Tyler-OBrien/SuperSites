using CloudflareWorkerBundler.Services.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudflareWorkerBundler.Services.Router
{
    public interface IRouterCreatorService
    {
        public Task<IRouter> GetRouter();

    }
}
