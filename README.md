# CloudflareSuperSites - Work in Progress 
Bundle up an entire directory, and distribute it across Cloudflare's range of products, including embedded in the Worker, KV, and R2! 

This uploads the necessary files & outputs a functional Cloudflare Worker, either using Hono, the web framework/router, or Vanilla, just processing the normal fetch event ourselves.
## Why
* If you don't have assets over KV/Cloudflare Page's limit of 25 MB, you probably are better off just using Cloudflare Pages, or Worker Sites. 
* If you do have assets over KV's limit, or want to exclusively use R2, or KV's pricing is too expensive, then this allows you to upload a static directory anywhere
* This also lets you (optionally) embed assets inside of the worker itself, base64 encoded. The ultimate unnecessary speed boost.
* Lastly, this optionally supports a "Manifest", keeping track of all old uploads, and removing unused files. We use this for avoiding uploading the same file twice, and also to keep assets around for a few versions (MaxManifestCount, default 3), in case your worker deployment goes wrong/worker propagation is slow and your old scripts are still around.
## How
* Download the executable (or build it), and run it once. It will generate a bundleconfig.json for you to configure
* Set up the storages / etc you want to use. You can have multiple of the same storage if you want different settings
* Run it again, and it will output a usable worker. Deploy the worker! If you want to set up CI/CD (not that I would recommend it, this may upload junk/a broken script): [check here!](https://github.com/Tyler-OBrien/personal_website/blob/master/.github/workflows/cloudflare_bundler_deploy.yml)
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
| ApiToken | Cloudflare API Token  | CLOUDFLARE_API_TOKEN | string
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
