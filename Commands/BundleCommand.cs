using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CloudflareWorkerBundler.Services.Bundler;
using CloudflareWorkerBundler.Services.Router;

namespace CloudflareWorkerBundler.Commands
{
    public class BundleCommand : Command
    {

        private readonly IFileBundler _fileBundler;




        public BundleCommand(IFileBundler fileBundler) : base("bundle", "Bundle Local Directory")
        {
            _fileBundler = fileBundler;
            var setDirectory = new Option<DirectoryInfo>("--directory")
            {
                Name = "--directory",
                Description = "Set Directory to use",
                IsRequired = false
            };


            AddOption(setDirectory);

            /*
            var setOutputFile = new Option<string>("--outputFile")
            {
                Name = "--outputFile",
                Description = "Set Output file name",
                IsRequired = false
            };


            AddOption(setOutputFile);
            */

            var setFileExtensionsToBundle = new Option<string>("--fileExtensions")
            {
                Name = "--fileExtensions",
                Description = "Set file extensions to include in bundle (default: just html, css, js). Comma seperated. Others will go into KV or R2. If you don't have your setup for KV/R2, then it will just produce a bundle with everything.",
                IsRequired = false
            };


            AddOption(setFileExtensionsToBundle);


            var bundleFileSizeLimit = new Option<long>("--BundleFileSizeLimit")
            {
                Name = "--BundleFileSizeLimit",
                Description = "Asset Max Size to be bundled in the worker. Any larger will be put in KV or R2, if specified. If KV or API, and an API Key, is not specified, this will go in the bundle anyway. This is in bytes, and defaults to 200 KB / 200000 ",
                IsRequired = false
            };


            AddOption(bundleFileSizeLimit);

            var KvNamespace = new Option<string>("--KvNameSpace")
            {
                Name = "--KvNameSpace",
                Description = "KV Namespace ID, to use for files that aren't included in bundle",
                IsRequired = false
            };


            AddOption(KvNamespace);


            var KVFileSizeLimit = new Option<long>("--KVFileSizeLimit")
            {
                Name = "--KVFileSizeLimit",
                Description = "KV File Size Limit. Defaults to 10 MB. KV currently supports a max of 25 MB, but at a cost. This is in bytes.",
                IsRequired = false
            };


            AddOption(KVFileSizeLimit);


            var R2BucketName = new Option<string>("--R2Bucket")
            {
                Name = "--R2Bucket",
                Description = "R2 Bucket Name, to use for files too big for KV",
                IsRequired = false
            };


            AddOption(R2BucketName);


            var APIToken = new Option<string>("--APIToken")
            {
                Name = "--APIToken",
                Description = "Cloudflare API Token, for uploading to KV or R2. Ensure the right permissions are set.",
                IsRequired = false
            };


            AddOption(APIToken);

            var accountId = new Option<string>("--Accountid")
            {
                Name = "--AccountId",
                Description = "Cloudflare Account ID, for uploading to KV or R2. Find under Workers Account-level tab on the right side. .",
                IsRequired = false
            };


            AddOption(accountId);


            this.SetHandler(ExecuteCommand, setDirectory, setFileExtensionsToBundle, bundleFileSizeLimit, KvNamespace, KVFileSizeLimit, R2BucketName, APIToken, accountId);
        }


        public async Task ExecuteCommand(DirectoryInfo directory, string fileExtensions, long bundleFileSizeLimit, string KVNamespace, long KVFileSizeLimit, string R2BucketName, string APIToken, string accountId)
        { 
            string outputFileName = "worker.js"; 
           IRouter router = null;
            if (directory == null)
            {
                directory = new DirectoryInfo(Environment.CurrentDirectory);
            }

            if (string.IsNullOrWhiteSpace(fileExtensions))
                fileExtensions = "css,js,html";
            if (bundleFileSizeLimit == default)
                bundleFileSizeLimit = 200000;
            if (KVFileSizeLimit == default)
                KVFileSizeLimit = 10000000;


            router = new HonoRouter();


            String[] fileExtensionsList = fileExtensions.Split(',');


            // Gather all files
            List<FileInfo> getFileInfo = new List<FileInfo>();

            foreach (var file in directory.GetFiles("*", SearchOption.AllDirectories))
            {
                if (file.Name.EndsWith(".ignore") == false)
                {
                    getFileInfo.Add(file);
                }
            }


            var outputFile = await _fileBundler.ProduceBundle(router, directory, getFileInfo, fileExtensionsList, bundleFileSizeLimit, KVNamespace, KVFileSizeLimit, R2BucketName, APIToken, accountId);

            await File.WriteAllTextAsync(outputFileName, outputFile);
        }



    }
}
