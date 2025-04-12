using System;
using UnityEngine;

namespace RCore.Data.JObject
{
	public interface IMonoBehaviourBinding
	{
		void Update();
		void OnApplicationPause(bool pause);
		void OnApplicationFocus(bool hasFocus);
		void OnApplicationQuit();
	}

	public partial class JObjectModelCollection : IMonoBehaviourBinding
	{
		public Action onInitialized;
		[SerializeField, Range(1, 10)] private int m_saveDelay = 3;
		[SerializeField] private bool m_saveOnPause = true;
		[SerializeField] private bool m_saveOnQuit = true;
		[NonSerialized] private bool m_initialized;
		[NonSerialized] private float m_saveCountdown;
		[NonSerialized] private float m_saveDelayCustom;
		[NonSerialized] private float m_lastSave;
		[NonSerialized] private bool m_enableAutoSave = true;
		[NonSerialized] private int m_pauseState = -1;
		public virtual void Init()
		{
			if (m_initialized)
				return;

			Load();
			PostLoad();
			m_initialized = true;
			onInitialized?.Invoke();
			DBLifecycle.Instance.Register(this);
		}
		public virtual void SaveData(bool now = false, float saveDelayCustom = 0)
		{
			if (!m_initialized)
				return;

			if (now)
			{
				// Do not allow multiple Save calls within a short period of time.
				if (Time.unscaledTime - m_lastSave < 0.2f)
					return;
				Save();
				m_saveDelayCustom = 0; // Reset save delay custom
				m_lastSave = Time.unscaledTime;
				return;
			}

			m_saveCountdown = m_saveDelay;
			if (saveDelayCustom > 0)
			{
				if (m_saveDelayCustom <= 0)
					m_saveDelayCustom = saveDelayCustom;
				else if (m_saveDelayCustom > saveDelayCustom)
					m_saveDelayCustom = saveDelayCustom;
				if (m_saveCountdown > m_saveDelayCustom)
					m_saveCountdown = m_saveDelayCustom;
			}
		}
		public virtual void EnableAutoSave(bool pValue)
		{
			m_enableAutoSave = pValue;
		}
		void IMonoBehaviourBinding.Update()
		{
			if (!m_initialized)
				return;

			OnUpdate(Time.deltaTime);

			//Save with a delay to prevent too many save calls in a short period of time
			if (m_saveCountdown > 0)
			{
				m_saveCountdown -= Time.deltaTime;
				if (m_saveCountdown <= 0)
					SaveData(true);
			}
		}
		void IMonoBehaviourBinding.OnApplicationPause(bool pause)
		{
			if (!m_initialized || m_pauseState == (pause ? 0 : 1))
				return;

			m_pauseState = pause ? 0 : 1;
			OnPause(pause);

			if (pause && m_saveOnPause && m_enableAutoSave)
				SaveData(true);
		}
		void IMonoBehaviourBinding.OnApplicationFocus(bool hasFocus)
		{
			bool pause = !hasFocus;
			if (!m_initialized || m_pauseState == (pause ? 0 : 1))
				return;

			m_pauseState = pause ? 0 : 1;
			OnPause(pause);

			if (pause && m_saveOnPause && m_enableAutoSave)
				SaveData(true);
		}
		void IMonoBehaviourBinding.OnApplicationQuit()
		{
			if (m_initialized && m_saveOnQuit && m_enableAutoSave)
				SaveData(true);
		}
	}
}