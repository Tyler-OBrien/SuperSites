﻿namespace CloudflareSuperSites.Models.Manifest
{
    public class BundlerManifest
    {
        public Deployment CreateNewDeployment()
        {
            var newDeployment = new Deployment();
            Deployments.Add(newDeployment);
            newDeployment.Files = new List<ManifestFile>();
            newDeployment.DeploymentTimeUTC = DateTime.UtcNow;
            newDeployment.ID = Guid.NewGuid().ToString("N");
            LastDeployUTC = DateTime.UtcNow;
            LastDeployVersion = newDeployment.ID;
            return newDeployment;
        }
        public DateTime LastDeployUTC { get; set; }

        public string LastDeployVersion { get; set; }

        public string ManifestVersion { get; set; }

        public List<Deployment> Deployments { get; set; }
    }
}
