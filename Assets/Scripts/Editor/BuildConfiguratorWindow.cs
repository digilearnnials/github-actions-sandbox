using System;
using UnityEngine;
using UnityEditor;

namespace Digi.EditorTools
{
    public class BuildConfiguratorWindow : EditorWindow
    {
        private static ProductType productType;
        private static EnvironmentType environmentType;
        private static ServerType serverType;


        void Awake ()
        {
            LoadCurrentSettings();
        }

        void OnGUI ()
        {
            Rect rect = new Rect();
            
            productType = (ProductType)GetOptionIndex(ref rect, "Product Type", productType);
            environmentType = (EnvironmentType)GetOptionIndex(ref rect, "Environment Type", environmentType);
            serverType = (ServerType)GetOptionIndex(ref rect, "Server Type", serverType);

            CheckSettingsUpdate();
        }


        private static void LoadCurrentSettings ()
        {
            productType = BuildConfigurator.GetCurrentProductType();
            environmentType = BuildConfigurator.GetCurrentEnvironmentType();
            serverType = BuildConfigurator.GetCurrentServerType();
        }
        
        private static void ApplyNewSettings ()
        {
            BuildConfigurator.SetProductType(productType);
            BuildConfigurator.SetEnvironmentType(environmentType);
            BuildConfigurator.SetServerType(serverType);
        }

        private static GUIContent[] GetCategoryContents<T> () where T : Enum
        {
            GUIContent[] contents;

            string[] labels = Enum.GetNames(typeof(T));
            Type enumType = typeof(T);

            contents = new GUIContent[labels.Length];

            if (enumType == typeof(ProductType))
            {
                for (int i = 0; i < labels.Length; i++)
                    contents[i] = new GUIContent(labels[i], GetProductOptionTooltip((ProductType)i));
            }
            else if (enumType == typeof(EnvironmentType))
            {
                for (int i = 0; i < labels.Length; i++)
                    contents[i] = new GUIContent(labels[i], GetEnvironmentOptionTooltip((EnvironmentType)i));
            }
            else if (enumType == typeof(ServerType))
            {
                for (int i = 0; i < labels.Length; i++)
                    contents[i] = new GUIContent(labels[i], GetServerOptionTooltip((ServerType)i));
            }

            return contents;
        }

        private static string GetProductOptionTooltip (ProductType productType)
        {
            string tooltip = string.Empty;

            switch (productType)
            {
                case ProductType.Client:
                    
                    tooltip = "Option to build the standard game for end-users (for either Windows or WebGL). " +
                              "All added scenes (except the one for the standalone wardrobe) are set enabled. " +
                              "Removes the Dedicated Sever subtarget (if originally set).";
                    
                    break;

                case ProductType.Server:
                    
                    tooltip = "Option to build the dedicated server (headless) that should be uploaded to the hosting service. " +
                              "Only the gameplay scene is set enabled (to allow automatic start up in headless mode). " +
                              "Target platform is changed to Dedicated Server.";
                    
                    break;
            }

            return tooltip;
        }

        private static string GetEnvironmentOptionTooltip (EnvironmentType environmentType)
        {
            string tooltip = string.Empty;

            switch (environmentType)
            {
                case EnvironmentType.Development:
                    
                    tooltip = "Option to build using the DEVELOPMENT macro. This allows to establish connection to development servers and matches. " +
                              "Players can only connect to development servers, and can only be matched with other development users. ";
                    
                    break;
                
                case EnvironmentType.Staging:
                    
                    tooltip = "Option to build using the STAGING macro. This allows to establish connection to staging servers and matches. " +
                              "Players can only connect to staging servers, and can only be matched with other staging users. ";
                    
                    break;

                case EnvironmentType.Production:
                    
                    tooltip = "Option to build using the PRODUCTION macro. This allows to establish connection to production servers and matches. " +
                              "Players can only connect to production servers, and can only be matched with other production users. ";

                    break;
            }

            return tooltip;
        }

        private static string GetServerOptionTooltip (ServerType serverType)
        {
            string tooltip = string.Empty;

            switch (serverType)
            {
                case ServerType.Hosted:
                    
                    tooltip = "Option to build without using the NON_HOSTED_SERVER_TEST macro. This is used to connect to the server orchestration service. " +
                              "Clients automatically request servers to the orchestration provider, and servers only run correctly while hosted. ";
                    
                    break;

                case ServerType.Local:
                    
                    tooltip = "Option to build using the NON_HOSTED_SERVER_TEST macro. This is used to manually start a match as server or client (for local testing). " +
                              "Clients can choose if they want to start as client or server when they launch a match, and servers can run in a local machine. ";
                    
                    break;
            }

            return tooltip;
        }

        private int GetOptionIndex<T> (ref Rect rect, string label, T selected) where T : Enum
        {
            const int columsCount = 1;
            const float padding = 30f;
            const float headerSpacing = 20f;
            const float optionHeight = 25f;

            GUIStyle headerStyle = new GUIStyle()
            {
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                normal = new GUIStyleState() { textColor = Color.white }
            };
            
            GUIContent[] contents = GetCategoryContents<T>();
            int selectedIndex = (int)(selected as object);
            
            rect = new Rect(padding, rect.position.y + rect.height + padding, position.width - padding * 2f, headerSpacing);

            GUI.Label(rect, label, headerStyle);

            rect = new Rect(rect.x, rect.y + headerSpacing, rect.width, optionHeight * contents.Length);

            return GUI.SelectionGrid(rect, selectedIndex, contents, columsCount);
        }

        private void CheckSettingsUpdate ()
        {
            const float confirmButtonWidth = 200f;
            const float confirmButtonHeight = 30f;
            
            Color previousColor = GUI.backgroundColor;
            Rect rect = new Rect(position.width / 2f - confirmButtonWidth / 2f, 
                                 position.height - confirmButtonHeight - 20f, 
                                 confirmButtonWidth, 
                                 confirmButtonHeight);

            GUI.backgroundColor = Color.green;

            if (GUI.Button(rect, "Confirm Changes"))
            {
                ApplyNewSettings();
                Close();
            }

            GUI.backgroundColor = previousColor;
        }
    }
}