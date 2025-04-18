using System;
using UnityEngine;

namespace RCore.Data.JObject
{
	public class SessionModel : JObjectModel<SessionData>
	{
		public float secondsTillNextDay;
		public float secondsTillNextWeek;

		public override void Init() { }
		public override void OnPostLoad(int utcNowTimestamp, int offlineSeconds)
		{
			if (data.firstActive == 0)
				data.firstActive = utcNowTimestamp;
			var lastActive = TimeHelper.UnixTimestampToDateTime(data.lastActive).ToLocalTime();
			var now = TimeHelper.UnixTimestampToDateTime(utcNowTimestamp).ToLocalTime();
			if ((now - lastActive).TotalDays > 1)
				data.daysStreak = 0; //Reset days streak
			if (lastActive.Date != now.Date)
			{
				data.days++;
				data.daysStreak++;
				data.SessionsDaily = 0; // Reset daily sessions count
			}
			if (lastActive.Year != now.Year || TimeHelper.GetCurrentWeekNumber(lastActive) != TimeHelper.GetCurrentWeekNumber(now))
				data.SessionsWeekly = 0; // Reset weekly sessions count
			if (lastActive.Year != now.Year || lastActive.Month != now.Month)
				data.SessionsMonthly = 0; // Reset monthly sessions count
			data.SessionsTotal++;
			data.SessionsDaily++;
			data.SessionsWeekly++;
			data.SessionsMonthly++;
			secondsTillNextDay = (float)(now.Date.AddDays(1) - now).TotalSeconds;
		}
		public override void OnPause(bool pause, int utcNowTimestamp, int offlineSeconds)
		{
			if (!pause)
			{
				CheckNewDay();
				data.lastActive = utcNowTimestamp;
			}
		}
		public override void OnUpdate(float deltaTime)
		{
			data.activeTime += deltaTime;

			if (secondsTillNextDay > 0)
			{
				secondsTillNextDay -= deltaTime;
				if (secondsTillNextDay <= 0)
					CheckNewDay();
			}
			if (secondsTillNextWeek > 0)
				secondsTillNextWeek -= deltaTime;
		}
		public override void OnPreSave(int utcNowTimestamp)
		{
			data.lastActive = utcNowTimestamp;
#if UNITY_ANDROID
			string curVersion = $"{Application.version}.{RUtil.GetVersionCode()}";
#else
			string curVersion = Application.version;
#endif
			if (string.IsNullOrEmpty(data.installVersion))
				data.installVersion = curVersion;
			data.updateVersion = curVersion;
		}
		private void CheckNewDay()
		{
			var lastActive = TimeHelper.UnixTimestampToDateTime(data.lastActive).ToLocalTime();
			var now = TimeHelper.GetNow(false);
			if ((now - lastActive).TotalDays > 1)
				data.daysStreak = 0; //Reset days streak
			if (lastActive.Year != now.Year || TimeHelper.GetCurrentWeekNumber(lastActive) != TimeHelper.GetCurrentWeekNumber(now))
				data.SessionsWeekly = 1; // Reset weekly sessions count
			if (lastActive.Year != now.Year || lastActive.Month != now.Month)
				data.SessionsMonthly = 1; // Reset monthly sessions count
			if (lastActive.Date != now.Date)
				AddOneDay();
			secondsTillNextDay = (float)(now.Date.AddDays(1) - now).TotalSeconds;
			secondsTillNextWeek = (float)TimeHelper.GetSecondsTillDayOfWeek(DayOfWeek.Monday, now);
		}
		public void AddOneDay()
		{
			data.days++;
			data.daysStreak++;
			data.SessionsDaily = 1; // Reset daily sessions count
			DispatchEvent(new NewDayStartedEvent());
		}
		public virtual int GetOfflineSeconds()
		{
			int offlineSeconds = 0;
			if (data.lastActive > 0)
			{
				int utcNowTimestamp = TimeHelper.GetNowTimestamp(true);
				offlineSeconds = utcNowTimestamp - data.lastActive;
			}
			return offlineSeconds;
		}
	}
}