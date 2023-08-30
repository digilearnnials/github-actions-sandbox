using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;

namespace Digi.Utilities.Deploy
{
    public static class GameBuilderCI
    {
        private const string ProjectPathArg = "projectPath";
        private const string BuildTargetArg = "buildTarget";
        private const string CustomBuildPathArg = "customBuildPath";
        private const string CustomBuildNameArg = "customBuildName";
        private const string ScriptingBackendArg = "scriptingBackend";


        private static Dictionary<string, string> GetValidatedOptions ()
        {            
            const string defaultCustomBuildName = "TestBuild";
            const string defaultScriptingBackendName = "IL2CPP";

            string buildTarget;
            
            Dictionary<string, string> validatedOptions = ParseCommandLineArguments();

            if (!validatedOptions.TryGetValue(ProjectPathArg, out string _))
            {
                Console.WriteLine($"Missing argument -{ProjectPathArg}");
                EditorApplication.Exit(110);
            }

            if (!validatedOptions.TryGetValue(BuildTargetArg, out buildTarget))
            {
                Console.WriteLine($"Missing argument -{BuildTargetArg}");
                EditorApplication.Exit(120);
            }

            if (!Enum.IsDefined(typeof(BuildTarget), buildTarget ?? string.Empty))
            {
                Console.WriteLine($"Invalid build target provided ({buildTarget})");
                EditorApplication.Exit(121);
            }

            if (!validatedOptions.TryGetValue(CustomBuildPathArg, out string _))
            {
                Console.WriteLine($"Missing argument -{CustomBuildPathArg}");
                EditorApplication.Exit(130);
            }
            
            if (!validatedOptions.TryGetValue(CustomBuildNameArg, out string customBuildName) || string.IsNullOrEmpty(customBuildName))
            {
                Console.WriteLine($"Missing or invalid argument -{CustomBuildNameArg}; defaulting to {defaultCustomBuildName}.");
                validatedOptions.Add(CustomBuildNameArg, defaultCustomBuildName);
            }

            if (!validatedOptions.TryGetValue(ScriptingBackendArg, out string _))
            {
                Console.WriteLine($"Missing argument -{ScriptingBackendArg}; defaulting to {defaultScriptingBackendName}.");
                validatedOptions.Add(ScriptingBackendArg, defaultScriptingBackendName);
            }

            return validatedOptions;
        }

        private static ScriptingImplementation GetScriptingImplementation (string scriptingBackendName)
        {
            return scriptingBackendName switch
            {
                "IL2CPP" => ScriptingImplementation.IL2CPP,
                "Mono" => ScriptingImplementation.Mono2x,
                _ => ScriptingImplementation.IL2CPP
            };
        } 

        private static Dictionary<string, string> ParseCommandLineArguments ()
        {
            Dictionary<string, string> providedArguments = new Dictionary<string, string>();
            
            string[] args = Environment.GetCommandLineArgs();

            Console.WriteLine($"{Environment.NewLine}" +
                              $"###########################{Environment.NewLine}" +
                              $"#    Parsing settings     #{Environment.NewLine}" +
                              $"###########################{Environment.NewLine}" +
                              $"{Environment.NewLine}");

            for (int current = 0, next = 1; current < args.Length; current++, next++)
            {                
                if (!args[current].StartsWith("-")) 
                    continue;
                
                string flag = args[current].TrimStart('-');

                bool flagHasValue = next < args.Length && !args[next].StartsWith("-");
                string value = flagHasValue ? args[next].TrimStart('-') : "";
                string displayValue = "\"" + value + "\"";

                Console.WriteLine($"Found flag \"{flag}\" with value {displayValue}.");
                providedArguments.Add(flag, value);
            }

            return providedArguments;
        }

        private static void ExecuteBuild (BuildTarget buildTarget, string filePath)
        {
            string[] scenes = EditorBuildSettings.scenes.Where(scene => scene.enabled).Select(s => s.path).ToArray();
            
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions()
            {
                scenes = scenes,
                target = buildTarget,
                locationPathName = filePath
            };

            BuildSummary buildSummary = BuildPipeline.BuildPlayer(buildPlayerOptions).summary;
            
            ReportSummary(buildSummary);
            ExitWithResult(buildSummary.result);
        }

        private static void ReportSummary (BuildSummary summary)
        {
            Console.WriteLine($"{Environment.NewLine}" +
                              $"###########################{Environment.NewLine}" +
                              $"#      Build results      #{Environment.NewLine}" +
                              $"###########################{Environment.NewLine}" +
                              $"{Environment.NewLine}" +
                              $"Duration: {summary.totalTime.ToString()}{Environment.NewLine}" +
                              $"Warnings: {summary.totalWarnings.ToString()}{Environment.NewLine}" +
                              $"Errors: {summary.totalErrors.ToString()}{Environment.NewLine}" +
                              $"Size: {summary.totalSize.ToString()} bytes{Environment.NewLine}" +
                              $"{Environment.NewLine}");
        }

        private static void ExitWithResult (BuildResult result)
        {
            switch (result)
            {
                case BuildResult.Succeeded:
                    Console.WriteLine("Build succeeded!");
                    EditorApplication.Exit(0);
                    break;
                
                case BuildResult.Failed:
                    Console.WriteLine("Build failed!");
                    EditorApplication.Exit(101);
                    break;
                
                case BuildResult.Cancelled:
                    Console.WriteLine("Build cancelled!");
                    EditorApplication.Exit(102);
                    break;
                
                case BuildResult.Unknown:
                default:
                    Console.WriteLine("Build result is unknown!");
                    EditorApplication.Exit(103);
                    break;
            }
        }

        public static void Build ()
        {
            Dictionary<string, string> options = GetValidatedOptions();
            BuildTarget buildTarget = (BuildTarget)Enum.Parse(typeof(BuildTarget), options[BuildTargetArg]);
            BuildTargetGroup buildTargetGroup = BuildPipeline.GetBuildTargetGroup(buildTarget);
            
            PlayerSettings.SetScriptingBackend(buildTargetGroup, GetScriptingImplementation(options[ScriptingBackendArg]));

            Console.WriteLine("Scripting Backend: " + PlayerSettings.GetScriptingBackend(buildTargetGroup).ToString());

            ExecuteBuild(buildTarget, options[CustomBuildPathArg]);
        }
    }
}