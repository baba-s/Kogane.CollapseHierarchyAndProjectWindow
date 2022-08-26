using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine.SceneManagement;

namespace Kogane.Internal
{
    /// <summary>
    /// Hierarchy と Project ビューの開いている階層をすべて閉じるエディタ拡張
    /// </summary>
    internal static class CollapseHierarchyAndProjectWindow
    {
        /// <summary>
        /// Hierarchy の開いている階層をすべて閉じるメニュー
        /// </summary>
        [MenuItem( "GameObject/Kogane/Collapse All", false )]
        private static void CollapseHierarchy()
        {
            const BindingFlags nonPublicStatic   = BindingFlags.NonPublic | BindingFlags.Static;
            const BindingFlags nonPublicInstance = BindingFlags.NonPublic | BindingFlags.Instance;

            var assemblyPath                 = InternalEditorUtility.GetEditorAssemblyPath();
            var assembly                     = Assembly.LoadFrom( assemblyPath );
            var sceneHierarchyWindowType     = assembly.GetType( "UnityEditor.SceneHierarchyWindow" );
            var lastInteractedHierarchyField = sceneHierarchyWindowType.GetField( "s_LastInteractedHierarchy", nonPublicStatic );
            var hierarchy                    = lastInteractedHierarchyField.GetValue( sceneHierarchyWindowType );
            var hierarchyType                = hierarchy.GetType();
            var sceneHierarchyField          = hierarchyType.GetField( "m_SceneHierarchy", nonPublicInstance );
            var sceneView                    = sceneHierarchyField.GetValue( hierarchy );
            var sceneViewType                = sceneView.GetType();
            var treeViewReloadNeededField    = sceneViewType.GetField( "m_TreeViewReloadNeeded", nonPublicInstance );
            var treeViewStateField           = sceneViewType.GetField( "m_TreeViewState", nonPublicInstance );
            var treeState                    = treeViewStateField.GetValue( sceneView );
            var expandedIDsField             = treeState.GetType().GetField( "m_ExpandedIDs", nonPublicInstance );

            expandedIDsField.SetValue( treeState, new List<int>() );

            var openedScenes = new List<string>();

            for ( var i = 0; i < SceneManager.sceneCount; i++ )
            {
                openedScenes.Add( SceneManager.GetSceneAt( i ).name );
            }

            sceneViewType.InvokeMember
            (
                name: "SetScenesExpanded",
                invokeAttr: BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.NonPublic,
                binder: null,
                target: sceneView,
                args: new object[] { openedScenes }
            );

            treeViewReloadNeededField.SetValue( sceneView, true );

            sceneViewType.InvokeMember
            (
                name: "SyncIfNeeded",
                invokeAttr: BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.NonPublic,
                binder: null,
                target: sceneView,
                args: null
            );
        }

        /// <summary>
        /// Project ビューの開いている階層をすべて閉じるメニュー
        /// </summary>
        [MenuItem( "Assets/Kogane/Collapse All", false )]
        private static void CollapseProjectWindow()
        {
            var assemblyPath                      = InternalEditorUtility.GetEditorAssemblyPath();
            var assembly                          = Assembly.LoadFrom( assemblyPath );
            var projectBrowserType                = assembly.GetType( "UnityEditor.ProjectBrowser" );
            var lastInteractedProjectBrowserField = projectBrowserType.GetField( "s_LastInteractedProjectBrowser" );
            var projectBrowser                    = lastInteractedProjectBrowserField.GetValue( projectBrowserType );
            var assetTreeStateField               = projectBrowserType.GetField( "m_AssetTreeState", BindingFlags.Instance | BindingFlags.NonPublic );
            var treeState                         = assetTreeStateField.GetValue( projectBrowser );
            var expandedIDsField                  = treeState.GetType().GetProperty( "expandedIDs", BindingFlags.Instance | BindingFlags.Public );

            InternalEditorUtility.expandedProjectWindowItems = new int[ 0 ];

            expandedIDsField.SetValue( treeState, new List<int>(), null );

            projectBrowserType.InvokeMember
            (
                name: "OnProjectChanged",
                invokeAttr: BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.NonPublic,
                binder: null,
                target: projectBrowser,
                args: null
            );
        }
    }
}