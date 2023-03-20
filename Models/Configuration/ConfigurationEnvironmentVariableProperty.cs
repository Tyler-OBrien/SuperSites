﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudflareWorkerBundler.Models.Configuration
{

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class ConfigurationEnvironmentVariableProperty : Attribute
    {
        /// <summary>
        /// Handles resolving this property from Environment variables
        /// </summary>
        /// <param name="environmentVariable"></param>
        public ConfigurationEnvironmentVariableProperty(string environmentVariable)
        {
            EnvironmentVariable = environmentVariable;
        }

        public string EnvironmentVariable { get; set; }

    }
}
