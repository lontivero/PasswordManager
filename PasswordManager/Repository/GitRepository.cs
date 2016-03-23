using System;
using System.Configuration;
using System.Diagnostics;

namespace PasswordManager.Repository
{
	class GitWrapper
	{
		private readonly string _path;
		private readonly string _gitPath;
		private readonly string _gitRepoUrl;
		private string _gitUser;
		private string _gitEmail;

		public GitWrapper(string path)
		{
			_path = path;
			_gitPath = ConfigurationManager.AppSettings["git-path"];
			_gitRepoUrl = ConfigurationManager.AppSettings["git-remote-url"];
			_gitUser = ConfigurationManager.AppSettings["git-user-name"];
			_gitEmail = ConfigurationManager.AppSettings["git-user-email"];

		}

		public void Init()
		{
			var script = new[] {
				"git init --quiet",
				"git config user.name \"" + _gitUser + "\"",
				"git config user.email \"" + _gitEmail + "\"",
				"git remote add origin " + _gitRepoUrl
			};

			RunGitScript(script);
		}

		public void Add(string message)
		{
			var script = new[] {
				"git add .",
				"git commit -m \"" + message + "\"",
				"git push -u origin master"
			};

			RunGitScript(script);
		}

		private void RunGitScript(string[] script)
		{
			foreach (var s in script)
			{
				ExecuteGitWith(s);
			}
		}
		private void ExecuteGitWith(string arguments)
		{
			var startInfo = new ProcessStartInfo(_gitPath);
			if (!startInfo.EnvironmentVariables.ContainsKey("GIT_SSH"))
				startInfo.EnvironmentVariables.Add("GIT_SSH", "C:\\Program Files (x86)\\PuTTY\\plink.exe");
			startInfo.Arguments = arguments.Substring(4);
			startInfo.CreateNoWindow = true;
			startInfo.UseShellExecute = false;
			startInfo.RedirectStandardError = true;
			startInfo.RedirectStandardOutput = true;
			startInfo.WorkingDirectory = _path;
			
			var exit = false;
			var output = string.Empty;
			using(var process = Process.Start(startInfo))
			{
				do
				{
					if (!process.HasExited)
					{
						process.Refresh();
						if (!process.Responding)
						{

						}
					}
				}while(!process.WaitForExit(5*1000));
				if(process.ExitCode != 0)
				{
					var error = process.StandardError.ReadToEnd();
					Console.WriteLine(error);
				}
			}
		}
	}
}
