using RCore.Data.JObject;

namespace RCore.Example.Data.JObject
{
	public class AchievementModel : JObjectModel<AchievementData>
	{
		public override void Init() { }
		public override void OnPause(bool pause, int utcNowTimestamp, int offlineSeconds) { }
		public override void OnPostLoad(int utcNowTimestamp, int offlineSeconds) { }
		public override void OnUpdate(float deltaTime) { }
		public override void OnPreSave(int utcNowTimestamp) { }
		public override void OnRemoteConfigFetched() { }
	}
}