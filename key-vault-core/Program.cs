using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;

namespace key_vault_core
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

     
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, config) =>
                {
                    var versionPrefix = context.HostingEnvironment.EnvironmentName;
                    var settings = config.Build();
                    var keyVaultUrl = settings.GetValue<string>("keyVaultUrl");
                    var keyVaultEndpoint = new Uri(keyVaultUrl);

                    config.AddAzureKeyVault(
                        keyVaultEndpoint,
                        new DefaultAzureCredential(),   new AzureKeyVaultConfigurationOptions  { ReloadInterval =new TimeSpan(1,0,0)});
                })
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });

        //https://docs.microsoft.com/en-us/dotnet/api/azure.identity.defaultazurecredential?view=azure-dotnet
        //DefaultAzureCredential
        //Provides a default TokenCredential authentication flow for applications that will be deployed to Azure. 
        //The following credential types if enabled will be tried, in order:

        //EnvironmentCredential
        //ManagedIdentityCredential
        //SharedTokenCacheCredential
        //VisualStudioCredential
        //VisualStudioCodeCredential
        //AzureCliCredential
        //AzurePowerShellCredential
        //InteractiveBrowserCredential
    }

}
