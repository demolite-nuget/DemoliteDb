using System.IO;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tools.DotNet;

class Build : NukeBuild
{
	/// Support plugins are available for:
	///   - JetBrains ReSharper        https://nuke.build/resharper
	///   - JetBrains Rider            https://nuke.build/rider
	///   - Microsoft VisualStudio     https://nuke.build/visualstudio
	///   - Microsoft VSCode           https://nuke.build/vscode
	public static int Main() => Execute<Build>(x => x.Pack);

	[Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
	readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

	[Solution(GenerateProjects = true)] 
	readonly Solution Solution;
	
	static AbsolutePath ReleaseDirectory => RootDirectory / .. / .. / "release";
	
	Target Clean => t => t
		.Before(Restore)
		.Executes(() =>
			{
				if (Directory.Exists(ReleaseDirectory))
					Directory.Delete(ReleaseDirectory, true);

				Directory.CreateDirectory(ReleaseDirectory);
				return DotNetTasks.DotNetClean(s => s.SetProject(Solution));
			}
		);

	Target Restore => t => t
		.DependsOn(Clean)
		.Executes(() => DotNetTasks.DotNetRestore(s => s.SetProjectFile(Solution)));

	Target Compile => t => t
		.DependsOn(Restore)
		.Executes(() =>
			DotNetTasks.DotNetBuild(s => s.SetProjectFile(Solution).SetConfiguration(Configuration))
		);

	Target Test => t => t
		.DependsOn(Compile)
		.Executes(() => DotNetTasks.DotNetTest(s => s.SetProjectFile(Solution).SetConfiguration(Configuration)));

	Target Pack => t => t
		.DependsOn(Test)
		.Executes(() =>
			{
				DotNetTasks.DotNetPack(s
					=> s
						.SetProject(Solution.Demolite_Db)
						.SetConfiguration(Configuration)
						.SetOutputDirectory(ReleaseDirectory)
				);
			}
		);

}
