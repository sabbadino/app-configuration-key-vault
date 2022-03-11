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

        // KEY VAULT ONLY
        //public static IHostBuilder CreateHostBuilder(string[] args) =>
        //    Host.CreateDefaultBuilder(args)
        //        .ConfigureAppConfiguration((context, config) =>
        //        {
        //            var versionPrefix = context.HostingEnvironment.EnvironmentName;
        //            var settings = config.Build();
        //            var keyVaultUrl = settings.GetValue<string>("keyVaultUrl");
        //            var keyVaultEndpoint = new Uri(keyVaultUrl);

        //            config.AddAzureKeyVault(
        //                keyVaultEndpoint,
        //                // new DefaultAzureCredential(),   new AzureKeyVaultConfigurationOptions  { ReloadInterval =new TimeSpan(1,0,0)});
        //                new DefaultAzureCredential(), new PrefixKeyVaultSecretManager(versionPrefix));
        //        })
        //        .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                    webBuilder.ConfigureAppConfiguration((hostingContext, config) =>
                        {
#if !DEBUG
                            var settings = config.Build();

                            config.AddAzureAppConfiguration(options =>
                            {

                              //  options.Connect(cnstring)

                                options.Connect(new Uri(cnstring), new ManagedIdentityCredential())

                                    .ConfigureKeyVault(kv =>
                                    {
                                        kv.SetCredential(new DefaultAzureCredential());
                                    })
                                    .ConfigureRefresh(refresh =>
                                    {
                                        refresh.Register("SettingsGroup:Sentinel", refreshAll: true)
                                               .SetCacheExpiration(TimeSpan.FromSeconds(10));
                                    })
                                    .UseFeatureFlags(op =>
                                    {
                                        op.Select("feature*"); // to filter on fetaure flags
                                    })

//                                    .Select("SettingsGroup:Key1", LabelFilter.Null)
                                //.Select(KeyFilter.Any, LabelFilter.Null)
                                //.Select(KeyFilter.Any, "DEvelopmente);
                                ;

                            },optional:false);
#endif
                        })
                        .UseStartup<Startup>());
    }
    public class PrefixKeyVaultSecretManager : KeyVaultSecretManager
    {
        private readonly string _prefix;

        public PrefixKeyVaultSecretManager(string prefix)
        {
            _prefix = $"{prefix}-";
        }

        public override bool Load(SecretProperties secret)
        {
            return secret.Name.StartsWith(_prefix);
        }

        public override string GetKey(KeyVaultSecret secret)
        {
            return secret.Name
                .Substring(_prefix.Length)
                .Replace("--", ConfigurationPath.KeyDelimiter);
        }
    }
}
