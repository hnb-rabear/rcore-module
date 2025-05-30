﻿/***
 * Author HNB-RaBear - 2019
 **/

using RCore.Audio;
using RCore.Editor.Tool;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace RCore.Editor
{
	public static class RMenu
	{
		// CTRL + ALT + j => Configuration
		// CTRL + ALT + k => Scenes Navigator
		// CTRL + ALT + l => Asset Shortcuts
		// CTRL + ALT + n => JObject Database
		// CTRL + ALT + m => KeyValue Database
		// CTRL + ALT + / => Tools Collection
		// SHIFT + 1 => Save Assets

		public const int GROUP_1 = 0;
		public const int GROUP_2 = 20;
		public const int GROUP_3 = 40;
		public const int GROUP_4 = 60;
		public const int GROUP_5 = 80;
		public const int GROUP_6 = 100;

		public const string GAMEOBJECT_R = "GameObject/RCore/";
		public const string GAMEOBJECT_R_CREATE = "GameObject/RCore/Create/";
		public const string GAMEOBJECT_R_UI = "GameObject/RCore/UI/";

		public const string R_ASSETS = "Assets/RCore/";
		public const string R_TOOLS = "RCore/Tools/";
		public const string R_EXPLORER = "RCore/Explorer/";

		private const string ALT = "&";
		private const string SHIFT = "#";
		private const string CTRL = "%";

		[MenuItem("RCore/Configuration " + CTRL + "_" + ALT + "_j", priority = GROUP_1 + 1)]
		private static void OpenEnvSetting()
		{
			Selection.activeObject = Configuration.Instance;
		}

		[MenuItem("RCore/Copy name of Build", priority = GROUP_1 + 2)]
		private static void CopyBuildName()
		{
			string buildName = EditorHelper.GetBuildName();
			GUIUtility.systemCopyBuffer = buildName;
			UnityEngine.Debug.Log("Copied name of Build: " + buildName);
		}

		//==========================================================

		[MenuItem(R_ASSETS + "Save Assets")]
		[MenuItem("RCore/Save Assets " + SHIFT + "_1", priority = GROUP_2 + 1)]
		private static void SaveAssets()
		{
			var objs = Selection.objects;
			if (objs != null)
				foreach (var obj in objs)
					EditorUtility.SetDirty(obj);

			AssetDatabase.SaveAssets();
		}

		[MenuItem("RCore/Scenes Navigator " + CTRL + "_" + ALT + "_k", priority = GROUP_2 + 2)]
		private static void OpenScenesNavigator()
		{
			ScenesNavigatorWindow.ShowWindow();
		}

		[MenuItem("RCore/Asset Shortcuts " + CTRL + "_" + ALT + "_l", priority = GROUP_2 + 2)]
		private static void OpenAssetShortcuts()
		{
			AssetShortcutsWindow.ShowWindow();
		}
		
		//==========================================================

		[MenuItem("RCore/Clear PlayerPrefs", priority = GROUP_4 + 1)]
		private static void ClearPlayerPrefs()
		{
			if (EditorHelper.ConfirmPopup("Clear PlayerPrefs"))
				PlayerPrefs.DeleteAll();
		}

		//==========================================================

		[MenuItem(R_EXPLORER + "DataPath Folder", false, GROUP_5 + 1)]
		private static void OpenDataPathFolder()
		{
			string path = Application.dataPath;
			var psi = new ProcessStartInfo(path);
			Process.Start(psi);
		}

		[MenuItem(R_EXPLORER + "StreamingAssets Folder", false, GROUP_5 + 2)]
		private static void OpenStreamingAssetsFolder()
		{
			string path = Application.streamingAssetsPath;
			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
				AssetDatabase.Refresh();
			}
			var psi = new ProcessStartInfo(path);
			Process.Start(psi);
		}

		[MenuItem(R_EXPLORER + "PersistentData Folder", false, GROUP_5 + 3)]
		private static void OpenPersistentDataFolder()
		{
			string path = Application.persistentDataPath;
			if (!Directory.Exists(path))
				Directory.CreateDirectory(path);
			var psi = new ProcessStartInfo(path);
			Process.Start(psi);
		}

		[MenuItem(R_EXPLORER + "UnityEditor Folder", false, GROUP_5 + 4)]
		private static void OpenUnityEditorFolder()
		{
			string path = EditorApplication.applicationPath.Substring(0, EditorApplication.applicationPath.LastIndexOf("/"));
			var psi = new ProcessStartInfo(path);
			Process.Start(psi);
		}

		[MenuItem(R_EXPLORER + "Editor Icon Dictionary", false, GROUP_5 + 5)]
		public static void OpenEditorIconsWindow()
		{
			EditorIconsWindow.ShowWindow();
		}

		//==========================================================

		[MenuItem(R_TOOLS + "Tools Collection " + CTRL + "_" + ALT + "_/", priority = GROUP_6 + 1)]
		private static void OpenToolsCollectionWindow()
		{
			ToolsCollectionWindow.ShowWindow();
		}

		[MenuItem(R_TOOLS + "Screenshot Taker", priority = GROUP_6 + 2)]
		public static void OpenScreenshotTaker()
		{
			ScreenshotTaker.ShowWindow();
		}

		[MenuItem(R_TOOLS + "Find Component Reference", priority = GROUP_6 + 3)]
		public static void OpenFindComponentReferenceWindow()
		{
			FindComponentReferenceWindow.ShowWindow();
		}

		[MenuItem(R_TOOLS + "Find Objects", priority = GROUP_6 + 4)]
		public static void OpenObjectsFinderWindow()
		{
			ObjectsFinderWindow.ShowWindow();
		}

		[MenuItem(R_TOOLS + "Find And Replace Assets", priority = GROUP_6 + 5)]
		public static void OpenSearchAndReplaceAssetWindow()
		{
			FindAndReplaceAssetWindow.ShowWindow();
		}

		[MenuItem(R_TOOLS + "Datetime Picker", priority = GROUP_6 + 6)]
		public static void OpenDateTimePickerWindow()
		{
			DateTimePickerWindow.ShowWindow(DateTime.Now);
		}
		
		//==============================================

		[MenuItem(GAMEOBJECT_R + "Group GameObjects", priority = GROUP_1 + 1)]
		private static void GroupGameObjects()
		{
			var objs = Selection.gameObjects;
			Transform parent = null;
			if (objs.Length > 1)
			{
				for (int i = 0; i < objs.Length; i++)
					parent = objs[i].transform.parent;

				var group = new GameObject();
				group.transform.SetParent(parent);
				for (int i = 0; i < objs.Length; i++)
					objs[i].transform.SetParent(group.transform);
				Selection.activeObject = group;
			}
		}

		[MenuItem(GAMEOBJECT_R + "Ungroup GameObjects", priority = GROUP_1 + 2)]
		private static void UngroupGameObjects()
		{
			var objs = Selection.gameObjects;
			if (objs.Length > 1)
			{
				for (int i = 0; i < objs.Length; i++)
					objs[i].transform.SetParent(null);
			}
		}

		[MenuItem(GAMEOBJECT_R + "Reorder SpriteRenderers")]
		public static void ReorderSortingOfSpriteRenderers()
		{
			foreach (var target in Selection.gameObjects)
			{
				ComponentHelper.ReorderSortingOfSpriteRenderers(target.GetComponentsInChildren<SpriteRenderer>(true));
				EditorUtility.SetDirty(target);
			}
		}

		[MenuItem(GAMEOBJECT_R_UI + "Perfect Image pixels per unit multiplier (W)", priority = GROUP_3 + 1)]
		public static void PerfectRatioImagesByWidth()
		{
			RUtil.PerfectRatioImagesByWidth(Selection.gameObjects);
		}

		[MenuItem(GAMEOBJECT_R_UI + "Perfect Image pixels per unit multiplier (H)", priority = GROUP_3 + 2)]
		public static void PerfectRatioImagesByHeight()
		{
			RUtil.PerfectRatioImagesByHeight(Selection.gameObjects);
		}

		[MenuItem(GAMEOBJECT_R_UI + "Perfect Image Size", priority = GROUP_3 + 3)]
		public static void SetImagesPerfectRatio()
		{
			foreach (var target in Selection.gameObjects)
			{
				var images = target.GetComponentsInChildren<UnityEngine.UI.Image>(true);
				foreach (var image in images)
					image.PerfectRatio();
			}
		}

		[MenuItem(GAMEOBJECT_R_UI + "Adjust Anchors To Corners", priority = GROUP_3 + 4)]
		public static void AdjustAnchorsToCorners()
		{
			foreach (var target in Selection.gameObjects)
			{
				var rectTransform = target.transform as RectTransform;
				rectTransform.AdjustAnchorsToCorners();
			}
		}

		[MenuItem(GAMEOBJECT_R_UI + "Adjust Anchors To Pivot", priority = GROUP_3 + 5)]
		public static void AdjustAnchorsToPivot()
		{
			foreach (var target in Selection.gameObjects)
			{
				var rectTransform = target.transform as RectTransform;
				rectTransform.AdjustAnchorsToPivot();
			}
		}

		[MenuItem(GAMEOBJECT_R_UI + "Replace Text By TextMeshProUGUI")]
		public static void ReplaceTextsByTextTMP()
		{
			EditorHelper.ReplaceTextsByTextTMP(Selection.gameObjects);
		}

		[MenuItem(GAMEOBJECT_R_CREATE + "AudioManager", priority = GROUP_2 + 1)]
		public static void AddAudioManager()
		{
			var gameObject = new GameObject("AudioManager");
			gameObject.AddComponent<AudioManager>();
		}

		//==============================================

		[MenuItem(R_ASSETS + "Refresh Assets in folder")]
		private static void RefreshAssetsInSelectedFolder()
		{
			EditorHelper.RefreshAssetsInSelectedFolder("t:GameObject t:ScriptableObject");
		}

		[MenuItem(R_ASSETS + "Export Selected Folders to Unity Package")]
		private static void ExportSelectedFoldersToUnityPackage()
		{
			EditorHelper.ExportSelectedFoldersToUnityPackage();
		}
	}
}