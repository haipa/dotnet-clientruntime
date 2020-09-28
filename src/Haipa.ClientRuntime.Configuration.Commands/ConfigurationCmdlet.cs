﻿using System.Management.Automation;

namespace Haipa.ClientRuntime.Configuration
{
    public abstract class ConfigurationCmdlet : PSCmdlet
    {
        [Parameter(
            ValueFromPipelineByPropertyName = true)]
        [ValidateNotNullOrEmpty]
        public string Configuration { get; set; }


        protected ConfigStoreSettings GetStoreSettings()
        {
            if (!(SessionState.PSVariable.GetValue("HaipaStoreLocationSettings", null) is ConfigStoreSettings s))
                SessionState.PSVariable.Set("HaipaStoreLocationSettings", s = new ConfigStoreSettings());

            return s;
        }

        protected string GetConfigurationName()
        {
            return Configuration ?? GetStoreSettings().DefaultConfigurationName;
        }


        protected ConfigStoresReader GetStoresReader()
        {
            return new ConfigStoresReader(new PowershellEnvironment(SessionState), GetConfigurationName());
        }

        protected ConfigStoresWriter GetStoresWriter()
        {
            var locationSettings = GetStoreSettings();
            var environment = new PowershellEnvironment(SessionState);
            var configurationName = GetConfigurationName();

            var writerSettings = new ConfigStoresWriterSettings
            {
                ConfigurationName = configurationName,
                ClientsStore = ConfigStore.GetStore(locationSettings.Clients, environment, configurationName),
                DefaultsStore = ConfigStore.GetStore(locationSettings.Defaults, environment, configurationName),
                EndpointsStore = ConfigStore.GetStore(locationSettings.Endpoints, environment, configurationName)
            };
            

            return new ConfigStoresWriter(writerSettings);
        }

    }
}