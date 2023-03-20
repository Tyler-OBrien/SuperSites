using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudflareWorkerBundler.Services.Storage
{
    public interface IStorageCreatorService
    {
        public Task<List<IGenericStorage>> GetStorages();
    }
}
