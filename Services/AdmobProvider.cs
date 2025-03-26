using System;
using UnityEngine;
using GoogleMobileAds.Api;
using GoogleMobileAds.Ump.Api;

namespace RCore.Service
{
	public class AdMobProvider
	{
		private static AdMobProvider m_Instance;
		public static AdMobProvider Instance => m_Instance ??= new AdMobProvider();

		private static string AD_UNIT_INTERSTITIAL => Configuration.KeyValues["ADMOB_INTERSTITIAL"];
		private static string AD_UNIT_REWARDED => Configuration.KeyValues["ADMOB_REWARDED"];
		private static string AD_UNIT_BANNER => Configuration.KeyValues["ADMOB_BANNER"];

		public void Init()
		{
			// Create a ConsentRequestParameters object     
			var request = new ConsentRequestParameters();
			// Check the current consent information status
			ConsentInformation.Update(request, error =>
			{
				if (error != null)
				{
					// Handle the error.            
					Debug.LogError(error);
					InitAds();
					return;
				}

				ConsentForm.LoadAndShowConsentFormIfRequired(formError =>
				{
					if (formError != null)
					{
						// Consent gathering failed.
						InitAds();
						return;
					}

					// Consent has been gathered.            
					if (ConsentInformation.CanRequestAds())
						InitAds();
				});
			});
			void InitAds()
			{
				MobileAds.Initialize(_ =>
				{
					InitializeInterstitialAds();
					InitializeRewardedAds();
					InitializeBannerAds();
				});
			}
		}

#region Interstitial Ads

		private InterstitialAd m_interstitialAd;
		private int m_interstitialRetryAttempt;
		private bool m_interstitialInitialized;
		private Action m_onInterstitialAdCompleted;

		public void InitializeInterstitialAds()
		{
			m_interstitialInitialized = true;
			LoadInterstitial();
		}
		private void LoadInterstitial()
		{
			var request = new AdRequest();
			InterstitialAd.Load(AD_UNIT_INTERSTITIAL, request, (ad, error) =>
			{
				if (error != null || ad == null)
				{
					Debug.Log($"Interstitial ad failed to load: {error}");
					m_interstitialRetryAttempt++;
					// Use exponential delay with a cap (here, the delay is capped by 2^6 seconds).
					float retryDelay = (float)Math.Pow(2, Mathf.Min(6, m_interstitialRetryAttempt));
					TimerEventsGlobal.Instance.WaitForSeconds(retryDelay, (s) => LoadInterstitial());
					return;
				}
				m_interstitialRetryAttempt = 0;
				m_interstitialAd = ad;
				Debug.Log("Interstitial ad loaded.");

				m_interstitialAd.OnAdFullScreenContentOpened += InterstitialAd_OnAdFullScreenContentOpened;
				m_interstitialAd.OnAdFullScreenContentClosed += InterstitialAd_OnAdFullScreenContentClosed;
				m_interstitialAd.OnAdFullScreenContentFailed += InterstitialAd_OnAdFullScreenContentFailed;
			});
		}
		private void InterstitialAd_OnAdFullScreenContentClosed()
		{
			m_onInterstitialAdCompleted?.Invoke();
			m_onInterstitialAdCompleted = null;
			// Load the next interstitial ad.
			LoadInterstitial();
		}
		private void InterstitialAd_OnAdFullScreenContentFailed(AdError error)
		{
			Debug.Log($"Interstitial ad failed to show: {error}");
			LoadInterstitial();
		}
		private void InterstitialAd_OnAdFullScreenContentOpened()
		{
			Debug.Log(nameof(InterstitialAd_OnAdFullScreenContentOpened));
		}
		public void ShowInterstitial(Action callback = null)
		{
#if UNITY_EDITOR
			callback?.Invoke();
			return;
#endif
			if (IsInterstitialReady())
			{
				m_onInterstitialAdCompleted = callback;
				m_interstitialAd.Show();
			}
		}
		public void DestroyInterstitial()
		{
			if (m_interstitialAd != null)
			{
				m_interstitialAd.Destroy();
				m_interstitialAd = null;
			}
		}
		public bool IsInterstitialReady()
		{
#if UNITY_EDITOR
			return true;
#endif
			return m_interstitialInitialized && m_interstitialAd != null;
		}

#endregion

#region Rewarded Ads

		private RewardedAd m_rewardedAd;
		private int m_rewardedRetryAttempt;
		private bool m_rewardedInitialized;
		private Action<bool> m_onRewardedAdCompleted;

		public void InitializeRewardedAds()
		{
			m_rewardedInitialized = true;
			LoadRewardedAd();
		}
		private void LoadRewardedAd()
		{
			var request = new AdRequest();
			RewardedAd.Load(AD_UNIT_REWARDED, request, (ad, error) =>
			{
				if (error != null || ad == null)
				{
					Debug.Log($"Rewarded ad failed to load: {error}");
					m_rewardedRetryAttempt++;
					float retryDelay = (float)Math.Pow(2, Mathf.Min(6, m_rewardedRetryAttempt));
					TimerEventsGlobal.Instance.WaitForSeconds(retryDelay, (s) => LoadRewardedAd());
					return;
				}
				m_rewardedRetryAttempt = 0;
				m_rewardedAd = ad;
				Debug.Log("Rewarded ad loaded.");

				m_rewardedAd.OnAdFullScreenContentOpened += RewardedAd_OnAdFullScreenContentOpened;
				m_rewardedAd.OnAdFullScreenContentClosed += RewardedAd_OnAdFullScreenContentClosed;
				m_rewardedAd.OnAdFullScreenContentFailed += RewardedAd_OnAdFullScreenContentFailed;
			});
		}
		private void RewardedAd_OnAdFullScreenContentOpened()
		{
			Debug.Log(nameof(RewardedAd_OnAdFullScreenContentOpened));
		}
		private void RewardedAd_OnAdFullScreenContentClosed()
		{
			m_onRewardedAdCompleted?.Invoke(true);
			m_onRewardedAdCompleted = null;
			// Preload the next rewarded ad.
			LoadRewardedAd();
		}
		private void RewardedAd_OnAdFullScreenContentFailed(AdError error)
		{
			Debug.Log($"Rewarded ad failed to show: {error}");
			LoadRewardedAd();
			m_onRewardedAdCompleted?.Invoke(false);
			m_onRewardedAdCompleted = null;
		}
		public void ShowRewardedAd(Action<bool> callback = null)
		{
#if UNITY_EDITOR
			callback?.Invoke(true);
#endif
			if (IsRewardedVideoAvailable())
			{
				m_onRewardedAdCompleted = callback;
				m_rewardedAd.Show(null);
			}
			else
			{
				ShowMessage("Rewarded ads unavailable!");
			}
		}
		public void DestroyRewardedAd()
		{
			if (m_rewardedAd != null)
			{
				m_rewardedAd.Destroy();
				m_rewardedAd = null;
			}
		}
		public bool IsRewardedVideoAvailable()
		{
#if UNITY_EDITOR
			return true;
#endif
			return m_rewardedInitialized && m_rewardedAd != null;
		}

#endregion

#region Banner Ads

		private BannerView bannerView;
		private bool m_BannerLoaded;
		private bool m_BannerInitialized;

		public void InitializeBannerAds()
		{
			// Create a banner. Here we use the standard Banner size and position it at the bottom.
			var adSize = AdSize.Banner;
			bannerView = new BannerView(AD_UNIT_BANNER, adSize, AdPosition.Bottom);
			bannerView.OnBannerAdLoaded += Banner_OnBannerAdLoaded;
			bannerView.OnBannerAdLoadFailed += Banner_OnBannerAdLoadFailed;

			var request = new AdRequest();
			bannerView.LoadAd(request);
			m_BannerInitialized = true;
		}
		private void Banner_OnBannerAdLoaded()
		{
			m_BannerLoaded = true;
		}
		private void Banner_OnBannerAdLoadFailed(LoadAdError obj)
		{
			m_BannerLoaded = false;
		}
		public void DisplayBanner()
		{
			if (m_BannerInitialized && bannerView != null)
				bannerView.Show();
		}
		public void HideBanner()
		{
			if (m_BannerInitialized && bannerView != null)
				bannerView.Hide();
		}
		public void DestroyBanner()
		{
			if (m_BannerInitialized && bannerView != null)
			{
				bannerView.Destroy();
				bannerView = null;
			}
		}
		public bool IsBannerReady()
		{
			return m_BannerInitialized && m_BannerLoaded;
		}

#endregion

		public void ShowMessage(string msg)
		{
#if UNITY_ANDROID
			// Get the current Android activity.
			var currentActivity = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity");
			// Get the Toast class.
			AndroidJavaObject toastClass = new AndroidJavaClass("android.widget.Toast");
			// Create and show the Toast message.
			toastClass.CallStatic<AndroidJavaObject>("makeText", new object[]
				{
					currentActivity,
					msg,
					toastClass.GetStatic<int>("LENGTH_SHORT")
				}
			).Call("show", Array.Empty<object>());
#elif UNITY_IOS
			IOSControl.instance.ShowMessage(msg);
#else
			Debug.Log("ShowMessage: " + msg);
#endif
		}
	}
}