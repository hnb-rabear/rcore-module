using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace RCore
{
	[Serializable]
	public class CloudData
	{
		public string DeviceId;
		public string email;
		public string fbId;
		public string gpgsId;
		public int level;
		public string country;
		public int lastActive;
		public int activeTime;
		public int version;
		public string data;
	}

	public static class RemoteDataSaver
	{
		private const string BASE_URL = "https://api.zegostudio.com/data/";
		private static string PackageName = "com.ig.all.in.slime";
		public static void UploadProfileAsync(CloudData cloudData)
		{
			string jsonData = JsonUtility.ToJson(cloudData);
			string url = $"{BASE_URL}{PackageName}/update";
			UploadProfileAsync(url, jsonData);
		}
		private static async void UploadProfileAsync(string url, string jsonData)
		{
			using var request = new UnityWebRequest(url, "POST");
			byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
			request.uploadHandler = new UploadHandlerRaw(bodyRaw);
			request.downloadHandler = new DownloadHandlerBuffer();
			request.SetRequestHeader("Content-Type", "application/json");
			request.timeout = 8;
			var operation = request.SendWebRequest();
			while (!operation.isDone) await Task.Yield();
			if (request.result == UnityWebRequest.Result.Success)
				UnityEngine.Debug.Log($"POST Success: {request.downloadHandler.text}");
			else
				UnityEngine.Debug.LogError($"POST Error: {request.error}");
		}
		public static Task<CloudData> DownloadProfileAsync()
		{
			string deviceId = SystemInfo.deviceUniqueIdentifier;
			string url = $"{BASE_URL}{PackageName}/user-profile/{UnityWebRequest.EscapeURL(deviceId)}";
			return DownloadProfileAsync(url);
		}
		private static async Task<CloudData> DownloadProfileAsync(string url)
		{
			using var request = UnityWebRequest.Get(url);
			request.timeout = 8;
			var operation = request.SendWebRequest();
			while (!operation.isDone) await Task.Yield();
			if (request.result == UnityWebRequest.Result.Success)
				try
				{
					return JsonUtility.FromJson<CloudData>(request.downloadHandler.text);
				}
				catch (Exception ex)
				{
					UnityEngine.Debug.LogError($"[{DateTime.Now}]: {ex}");
					return null;
				}
			UnityEngine.Debug.LogError($"[{DateTime.Now}] GET Error: {request.error}");
			return null;
		}
		public static async Task<bool> DeleteProfileAsync()
		{
			string deviceId = SystemInfo.deviceUniqueIdentifier;
#if UNITY_IOS
			deviceId = Social.localUser.id;
#endif
			string url = $"{BASE_URL}{PackageName}/user-profile/{UnityWebRequest.EscapeURL(deviceId)}";
			return await DeleteProfileAsync(url);
		}
		private static async Task<bool> DeleteProfileAsync(string url)
		{
			using var request = UnityWebRequest.Delete(url);
			var operation = request.SendWebRequest();
			while (!operation.isDone) await Task.Yield();
			if (request.result == UnityWebRequest.Result.Success)
			{
				Debug.Log($"DELETE Success!");
				return true;
			}
			else
			{
				Debug.LogError($"DELETE Error: {request.error}");
				return false;
			}
		}
	}
}