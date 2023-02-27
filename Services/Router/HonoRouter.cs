﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudflareWorkerBundler.Services.Router
{
    public class HonoRouter : IRouter
    {

        public const string HONO_HEADER =
            @"
import { Hono } from 'hono'
const app = new Hono()";

        public const string HONO_FOOTER = @"export default app";

        public string EnvironmentVariableInsideRequest
        {
            get => "c.env.";
        }


        public void Begin(StringBuilder stringBuilder, bool fullWorker)
        {
            if (fullWorker)
            {
                stringBuilder.AppendLine(HONO_HEADER.Trim());
            }

        }

        public void AddRoute(StringBuilder stringBuilder, string relativePath, string fileHash, string responseCode)
        {
            stringBuilder.AppendLine(
                $"app.on(['GET', 'HEAD'],'/{relativePath}', async (c) => {{ {responseCode}  }})");

        }

        public void Add404Route(StringBuilder stringBuilder, string responseCode)
        {
            stringBuilder.AppendLine(
                $"app.on(['GET', 'HEAD'],'*', async (c) => {{ {responseCode} }})");
        }

        public void End(StringBuilder stringBuilder, bool fullWorker)
        {
            if (fullWorker)
            {
                stringBuilder.AppendLine(HONO_FOOTER.Trim());
            }
        }
    }
}
