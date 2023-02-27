using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudflareWorkerBundler.Services.Router
{
    public interface IRouter
    {
        public string EnvironmentVariableInsideRequest { get; }
        public void Begin(StringBuilder stringBuilder, bool fullWorker);
        public void AddRoute(StringBuilder stringBuilder, string relativePath, string fileHash, string responseCode);
        public void End(StringBuilder stringBuilder, bool fullWorker);
    }
}
