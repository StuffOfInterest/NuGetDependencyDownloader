using System;
using System.Collections.Generic;
using System.Linq;
using NuGet.Common;
using NuGet.Configuration;

namespace NuGetDependencyDownloader
{ 
    public class MachineWideSettings : IMachineWideSettings
    {
        private readonly Lazy<IEnumerable<ISettings>> _settings;

        public MachineWideSettings()
        {
            var baseDirectory = NuGetEnvironment.GetFolderPath(NuGetFolderPath.MachineWideConfigDirectory);
            _settings = new Lazy<IEnumerable<ISettings>>(
                () => new List<ISettings> {global::NuGet.Configuration.Settings.LoadMachineWideSettings(baseDirectory)});
        }

        public IEnumerable<ISettings> Settings => _settings.Value;

        ISettings IMachineWideSettings.Settings => _settings.Value.FirstOrDefault();
    }
}