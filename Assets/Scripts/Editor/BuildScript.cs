using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;

namespace Digi.EditorTools
{
    public static class BuildScript
    {
        private static readonly string Eol = Environment.NewLine;
        private static readonly string[] Secrets = { "androidKeystorePass", "androidKeyaliasName", "androidKeyaliasPass" };


        private static Dictionary<string, string> GetValidatedOptions ()
        {
            ParseCommandLineArguments(out Dictionary<string, string> validatedOptions);

            if (!validatedOptions.TryGetValue("projectPath", out string _))
            {
                Console.WriteLine("Missing argument -projectPath");
                EditorApplication.Exit(110);
            }

            if (!validatedOptions.TryGetValue("buildTarget", out string buildTarget))
            {
                Console.WriteLine("Missing argument -buildTarget");
                EditorApplication.Exit(120);
            }

            if (!Enum.IsDefined(typeof(BuildTarget), buildTarget ?? string.Empty))
                EditorApplication.Exit(121);

            if (!validatedOptions.TryGetValue("customBuildPath", out string _))
            {
                Console.WriteLine("Missing argument -customBuildPath");
                EditorApplication.Exit(130);
            }

            if (!validatedOptions.TryGetValue("productType", out string _))
            {
                Console.WriteLine("Missing argument -productType");
                EditorApplication.Exit(140);
            }
            
            if (!validatedOptions.TryGetValue("environmentType", out string _))
            {
                Console.WriteLine("Missing argument -environmentType");
                EditorApplication.Exit(141);
            }
            
            if (!validatedOptions.TryGetValue("serverType", out string _))
            {
                Console.WriteLine("Missing argument -serverType");
                EditorApplication.Exit(142);
            }

            const string defaultCustomBuildName = "TestBuild";
            
            if (!validatedOptions.TryGetValue("customBuildName", out string customBuildName))
            {
                Console.WriteLine($"Missing argument -customBuildName, defaulting to {defaultCustomBuildName}.");
                validatedOptions.Add("customBuildName", defaultCustomBuildName);
            }
            else if (customBuildName == "")
            {
                Console.WriteLine($"Invalid argument -customBuildName, defaulting to {defaultCustomBuildName}.");
                validatedOptions.Add("customBuildName", defaultCustomBuildName);
            }

            return validatedOptions;
        }

        private static void ConfigureVersion (Dictionary<string, string> options)
        {
            PlayerSettings.bundleVersion = options["buildVersion"];
            PlayerSettings.macOS.buildNumber = options["buildVersion"];
            PlayerSettings.Android.bundleVersionCode = int.Parse(options["androidVersionCode"]);
        }

        private static void ConfigureSplashScreen (bool useSplash = true)
        {
            if (useSplash)
                PlayerSettings.SplashScreen.showUnityLogo = false;
            else
                PlayerSettings.SplashScreen.show = false;
        }

        private static void ConfigureBuildTarget (BuildTarget buildTarget, Dictionary<string, string> options)
        {
            switch (buildTarget)
            {
                case BuildTarget.Android:
                    EditorUserBuildSettings.buildAppBundle = options["customBuildPath"].EndsWith(".aab");
                    
                    if (options.TryGetValue("androidKeystoreName", out string keystoreName) && !string.IsNullOrEmpty(keystoreName))
                    {
                        PlayerSettings.Android.useCustomKeystore = true;
                        PlayerSettings.Android.keystoreName = keystoreName;
                    }
                    
                    if (options.TryGetValue("androidKeystorePass", out string keystorePass) && !string.IsNullOrEmpty(keystorePass))
                        PlayerSettings.Android.keystorePass = keystorePass;
                    
                    if (options.TryGetValue("androidKeyaliasName", out string keyaliasName) && !string.IsNullOrEmpty(keyaliasName))
                        PlayerSettings.Android.keyaliasName = keyaliasName;
                    
                    if (options.TryGetValue("androidKeyaliasPass", out string keyaliasPass) && !string.IsNullOrEmpty(keyaliasPass))
                        PlayerSettings.Android.keyaliasPass = keyaliasPass;
                    
                    if (options.TryGetValue("androidTargetSdkVersion", out string androidTargetSdkVersion) && !string.IsNullOrEmpty(androidTargetSdkVersion))
                    {
                        AndroidSdkVersions targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
                        
                        try
                        {
                            targetSdkVersion = (AndroidSdkVersions)Enum.Parse(typeof(AndroidSdkVersions), androidTargetSdkVersion);
                        }
                        catch
                        {
                            UnityEngine.Debug.Log("Failed to parse androidTargetSdkVersion! Fallback to AndroidApiLevelAuto");
                        }

                        PlayerSettings.Android.targetSdkVersion = targetSdkVersion;
                    }
                    break;
                    
                case BuildTarget.StandaloneOSX:
                    PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone, ScriptingImplementation.Mono2x);
                    break;
            }
        }

        private static void ConfigureProductOptions (Dictionary<string, string> options)
        {
            ProductType productType = options["productType"] switch
            {
                "Client" => ProductType.Client,
                "Server" => ProductType.Server,
                _ => ProductType.Server
            };

            EnvironmentType environmentType = options["environmentType"] switch
            {
                "Development" => EnvironmentType.Development,
                "Staging" => EnvironmentType.Staging,
                "Production" => EnvironmentType.Production,
                _ => EnvironmentType.Development
            };
            
            ServerType serverType = options["serverType"] switch
            {
                "Hosted" => ServerType.Hosted,
                "Local" => ServerType.Local,
                _ => ServerType.Hosted
            };

            BuildConfigurator.SetProductType(productType);
            BuildConfigurator.SetEnvironmentType(environmentType);
            BuildConfigurator.SetServerType(serverType);
        }

        private static void ParseCommandLineArguments (out Dictionary<string, string> providedArguments)
        {
            providedArguments = new Dictionary<string, string>();
            
            string[] args = Environment.GetCommandLineArgs();

            Console.WriteLine(
                $"{Eol}" +
                $"###########################{Eol}" +
                $"#    Parsing settings     #{Eol}" +
                $"###########################{Eol}" +
                $"{Eol}"
            );

            for (int current = 0, next = 1; current < args.Length; current++, next++)
            {                
                if (!args[current].StartsWith("-")) 
                    continue;
                
                string flag = args[current].TrimStart('-');

                bool flagHasValue = next < args.Length && !args[next].StartsWith("-");
                string value = flagHasValue ? args[next].TrimStart('-') : "";
                bool secret = Secrets.Contains(flag);
                string displayValue = secret ? "*HIDDEN*" : "\"" + value + "\"";

                Console.WriteLine($"Found flag \"{flag}\" with value {displayValue}.");
                providedArguments.Add(flag, value);
            }
        }

        private static void ExecuteBuild (BuildTarget buildTarget, StandaloneBuildSubtarget standaloneBuildSubtarget, string filePath)
        {
            string[] scenes = EditorBuildSettings.scenes.Where(scene => scene.enabled).Select(s => s.path).ToArray();
            
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions()
            {
                scenes = scenes,
                target = buildTarget,
                subtarget = (int)standaloneBuildSubtarget,
                locationPathName = filePath,
            };

            BuildSummary buildSummary = BuildPipeline.BuildPlayer(buildPlayerOptions).summary;
            
            ReportSummary(buildSummary);
            ExitWithResult(buildSummary.result);
        }

        private static void ReportSummary (BuildSummary summary)
        {
            Console.WriteLine(
                $"{Eol}" +
                $"###########################{Eol}" +
                $"#      Build results      #{Eol}" +
                $"###########################{Eol}" +
                $"{Eol}" +
                $"Duration: {summary.totalTime.ToString()}{Eol}" +
                $"Warnings: {summary.totalWarnings.ToString()}{Eol}" +
                $"Errors: {summary.totalErrors.ToString()}{Eol}" +
                $"Size: {summary.totalSize.ToString()} bytes{Eol}" +
                $"{Eol}"
            );
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
            BuildTarget buildTarget = (BuildTarget)Enum.Parse(typeof(BuildTarget), options["buildTarget"]);
            StandaloneBuildSubtarget standaloneBuildSubtarget = (options["productType"] == "Server")? StandaloneBuildSubtarget.Server : StandaloneBuildSubtarget.Player;

            ConfigureVersion(options);
            ConfigureSplashScreen();
            ConfigureBuildTarget(buildTarget, options);
            ConfigureProductOptions(options);

            ExecuteBuild(buildTarget, standaloneBuildSubtarget, options["customBuildPath"]);
        }
    }
}