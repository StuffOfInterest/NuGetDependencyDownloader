# NuGetDependencyDownloader
NuGet Dependency Downloader is a Windows form application which takes a NuGet package ID and optionally a version.  It then downloads that package and any depenencies that packages has.  It will process the dependencies for lower level dependencies until all have been downloaded.  By default this tool will only download the latest release version of each package but there is a checkbox to allow taking pre-release packages as well.

With the new trend in using smaller focused packages the depency list can be quite large.  As an example, when downloading "Microsoft.Asp.Net" version "5.2.3" it is necessary to pull down four packages to cover all dependencies.  With version "6.0.0-rc1-final" that list grows to 158 packages.  If you need to work with an offline package repository, either in a disconnected development environment or on a laptop on the go, manually walking this dependency list and downloading all necessary packages could take hours.  With this tool it only takes a few minutes.

There is currently an issue with the "Stop" button not firing right away when clicked.  Work is happening in a background thread so this should not be an issue, but it is.  If someone has a fix for this, a Pull Request would be most appreciated.

Hopefully this is of use to those of you working offline.

Delbert Matlock, aka Stuff Of Interest
