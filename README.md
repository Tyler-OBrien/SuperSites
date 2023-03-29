# SuperSites - Work in Progress 
Bundle up an entire directory, and distribute it across Cloudflare's range of products, including embedded in the Worker, KV, and R2! 

This uploads the necessary files & outputs a functional Cloudflare Worker, either using Hono, the web framework/router, or Vanilla, handling the fetch event ourselves with no extra packages.
## Why
* If you don't have assets over KV/Cloudflare Page's limit of 25 MB, you probably are better off just using Cloudflare Pages, or Worker Sites. 
* If you do have assets over KV's limit, or want to exclusively use R2, then this allows you to upload a static directory anywhere
* This also lets you (optionally) embed assets inside of the worker itself, base64 encoded. The ultimate unnecessary speed boost.
* Lastly, this optionally supports a "Manifest", keeping track of all old uploads, and removing unused files. We use this for avoiding uploading the same file twice, and also to keep assets around for a few versions (MaxManifestCount, default 3), in case your worker deployment goes wrong/worker propagation is slow and your old scripts are still around.
## How
* Download the executable (or build it), and run it once. It will generate a bundleconfig.json for you to configure. 
* Set up the storages / etc you want to use. You can have multiple of the same storage if you want different settings. See the configurations below, or the full configuration example below.
* Run the script with bundle (i.e ./CloudflareSuperSites bundle), and it will output a (hopefully) usable worker. Deploy the worker via Wrangler (wrangler publish)! If you want to set up CI/CD (not that I would recommend it, this may upload junk/a broken script): [check here!](https://github.com/Tyler-OBrien/personal_website/blob/master/.github/workflows/cloudflare_bundler_deploy.yml)
* You can use the clean up command (./CLoudflareSuperSites cleanup) with a manifest storage provider configured to clean up old files & manifests.
## Show


https://user-images.githubusercontent.com/94197210/228124997-843cd937-4fee-4232-ac46-a986c52e5b2b.mp4


## Configuration
### Base Configuration
| Name | Use | Environment Variable | Type
| ------------- | ------------------ | -------- | -------
| BundleDirectory | The Directory to bundle! Can be absolute or relative. Just be careful of JSON slash escaping  | BUNDLE_DIRECTORY | string
| OutputLocation | The Output location of the worker! Can be absolute or relative. Just be careful of JSON slash escaping. This includes the file name.   | OUTPUT_LOCATION | string
| Router | "Vanilla" or "Hono", outputs the worker using Hono, or completely vanilla Cloudflare Worker  | ROUTER | string
| Verbose | Verbose logging, or not.  | VERBOSE | bool
| DryRun | If true, does not upload anything to R2/KV/Any storage.  | VERBOSE | bool
| StorageConfigurations | Your Storage Configurations! See specific Configurations below! | | list of StorageConfiguration
| MaxManifestCount  | The max amount of manifests before we prune old ones and remove their assets! (Only if you run the cleanup command though!) | MAX_MANIFEST_COUNT |  int
| Etags | Should we return ETags on each request, and handle returning 304 not modified if-not-match? | USE_ETAGS | bool
| ManifestStorageConfiguration | The Storage to use for our Manifests! R2 is recommended. | | StorageConfiguration
### Base Storage Configuration - Properties all Storages have!
| Name | Use | Type
| ------------- | ------------------ | -------
| Type | The Type used to identify the storage! "Embedded", "R2", or "KV  |  string
| AllowedFileExtensions | File Extensions that are allowed in this configuration | List of strings
| DisallowFileExtensions | File Extensions that are not allowed in this configuration | List of strings
| IncludePaths | File paths that are  allowed in this configuration | List of strings
| ExcludePaths | File paths that are not allowed in this configuration | List of strings
| FileSizeLimit | Max file size in bytes | long
| CacheSeconds | Seconds to cache the item for. If Specified, it is cached in the CF Worker Cache API. Not supported for Embedded. | int

Notes:
* These must all match for the asset to be included. For example, if you specify to include the file path /cookies/, and only HTML file extensions are allowed, it must match both
* Include Paths based on the case-insentive Start of the relative path. For example, if your website is example.com, and you have a path at example.com/cookies/, IncludePaths /cookies/ would match any assets under that path
* File Extensions are also case-insensitive
### Embedded Storage Configuration - Assets that are base64 encoded and included in your worker js output
There are no unique embedded storage configuration options, just the base storage configuration options.
### KV Storage Configuration
| Name | Use | Environment Variable | Type
| ------------- | ------------------ | -------- | -------
| ApiToken | Cloudflare API Token, needs KV Edit Permissions  | CLOUDFLARE_API_TOKEN | string
| ACCOUNTID | Cloudflare Account Id  | ACCOUNTID | string
| NamespaceId | KV Namespace Id  | | string
| BindingName | Name of the KV Binding (in your wrangler.toml, or use the generated one)  | | string
### R2 Storage Configuration
| Name | Use | Environment Variable | Type
| ------------- | ------------------ | -------- | -------
| AccessKey | Cloudflare R2 S3 API Access Key | ACCESSKEY | string
| SecretKey | Cloudflare R2 S3 Secret Key  | SECRETKEY | string
| ACCOUNTID | Cloudflare Account Id  | ACCOUNTID | string
| BucketName | R2 Bucket Name | | string
| BindingName | Name of the KV Binding (in your wrangler.toml, or use the generated one)  | | string

Why the S3 API Keys? We use [Minio](https://github.com/minio/minio-dotnet) under the hood, there's no offical CF API for R2, and the unofficial one has an upload limit of ~100 MB (as does the entire API!). There are bits of code left for using the unofficial API, if you see them in the source.


Todo:

* Tests
* Maybe Tests with unstable_dev/the worker itself?
* Support for serving old assets (could force uploading all to a persistent data store, and re-creating routes based on the manifest)
* A Non-Full Worker mode, that outputs code that appends to your existing worker / otherwise lets you hook into it and use SuperSites alongside a normal worker (like you can use Cloudflare Worker Sites alongside a worker)


Full Configuration Example:

```json
{
   "BundleDirectory":"../public", # Relative or Exact Directory
   "OutputLocation":"worker/worker.js", # Relative or Exact File Path
   "Router":"Vanilla", # Vanilla or Hono
   "StorageConfigurations":[  # Repeat mutiple of the same type or exact same namespaceid/bucket name if you want.
      {
         "Type":"Embedded",
         "AllowedFileExtensions":[
            "html",
            "css",
            "js"
         ],
         "FileSizeLimit":200000
      },
      {
         "Type":"KV",
         "FileSizeLimit":10000000,
         "ApiToken":"...", # Use Environment Variables if your environment supports it. KV needs KV Edit Permissions
         "AccountId":"...",
         "NamespaceId":"...",
         "BindingName":"KV",
      },
      {
         "Type":"R2",
         "AccessKey":"...", # R2 S3 API Keys
         "SecretKey":"...",
         "AccountId":"...",
         "BucketName":"main-website",
         "BindingName":"R2",
         "CacheSeconds": 3600
      }
   ],
   "MaxManifestCount":3,
   "ManifestStorageConfiguration":{ # Not Required, if you don't care about leaving files for old deploys behind/leaving unused files in storage
      "Type":"R2",
      "BucketName":"main-website-manifest",
      "AccessKey":"...",
      "SecretKey":"...",
      "AccountId":"...",
      "BindingName":"R2",
   }
}
```


Example Site using Super Sites: https://tylerobrien.dev
