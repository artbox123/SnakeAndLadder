using GoogleMobileAds.Api;
using GoogleMobileAds.Common;
using System;
using UnityEngine;
using UnityEngine.Advertisements;

namespace ArtboxGames
{
    public class AdsManager : GameManager, IUnityAdsInitializationListener, IUnityAdsLoadListener, IUnityAdsShowListener
    {
        public static AdsManager Instance;

        public bool testAd = false;

        private AppOpenAd appOpenAd;
        private bool appOpenLoaded = false;

        private BannerView _bannerView;
        private InterstitialAd _interstitialAd;
        private RewardedAd _rewardedAd;

        private bool showAppOpen = true;

        public bool IsAdAvailable
        {
            get
            {
                return appOpenAd != null
                       && appOpenLoaded;
            }
        }

        private string gameId = "2868584";
        private string interstitialAdUnitId_Unity = "Android_Interstitial";
        private string rewardAdUnitId_Unity = "Android_Rewarded";

        //private string appID = "ca-app-pub-6163322720080156~5133711337";
        private string bannerUnitId = "ca-app-pub-6163322720080156/4534945538";
        private string openAdUnitId = "ca-app-pub-6163322720080156/2607120181";
        private string interstitialAdUnitId = "ca-app-pub-6163322720080156/5656455510";
        private string rewardAdUnitId = "ca-app-pub-6163322720080156/9212557148";

        private bool giveReward = false;

        void Awake()
        {
            if (Instance)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
        }

        // Start is called before the first frame update
        public void Start()
        {
            if (testAd)
            {
                openAdUnitId = "ca-app-pub-3940256099942544/9257395921";
                bannerUnitId = "ca-app-pub-3940256099942544/6300978111";
                interstitialAdUnitId = "ca-app-pub-3940256099942544/1033173712";
                rewardAdUnitId = "ca-app-pub-3940256099942544/5224354917";
            }

            Advertisement.Initialize(gameId, false, this);

            // Listen to application foreground and background events.
            AppStateEventNotifier.AppStateChanged += OnAppStateChanged;

            RequestConfiguration requestConfiguration = new RequestConfiguration
            {
                TagForUnderAgeOfConsent = TagForUnderAgeOfConsent.True
            };
            MobileAds.SetRequestConfiguration(requestConfiguration);

            // Initialize the Google Mobile Ads SDK.
            MobileAds.Initialize((InitializationStatus initStatus) =>
            {
                // This callback is called once the MobileAds SDK is initialized.
                Debug.Log("Ads init sucess");

                LoadAppOpenAd();
                LoadBannerAd();
                LoadInterstitialAd();
                LoadRewardedAd();
            });
        }

        public void LoadAppOpenAd()
        {
            // Clean up the old ad before loading a new one.
            if (appOpenAd != null)
            {
                appOpenAd.Destroy();
                appOpenAd = null;
            }

            Debug.Log("Loading the app open ad.");

            // Create our request used to load the ad.
            var adRequest = new AdRequest();

            // send the request to load the ad.
            AppOpenAd.Load(openAdUnitId, adRequest,
                (AppOpenAd ad, LoadAdError error) =>
                {
                // if error is not null, the load request failed.
                if (error != null || ad == null)
                    {
                        Debug.LogError("app open ad failed to load an ad " +
                                       "with error : " + error);
                        return;
                    }

                    Debug.Log("App open ad loaded with response : "
                              + ad.GetResponseInfo());

                    appOpenLoaded = true;
                    appOpenAd = ad;
                    RegisterEventHandlers(ad);

                    if (showAppOpen)
                    {
                        showAppOpen = false;
                        ShowAppOpenAd();
                    }
                });
        }

        private void RegisterEventHandlers(AppOpenAd ad)
        {
            // Raised when the ad is estimated to have earned money.
            ad.OnAdPaid += (AdValue adValue) =>
            {
                Debug.Log(String.Format("App open ad paid {0} {1}.",
                    adValue.Value,
                    adValue.CurrencyCode));
            };
            // Raised when an impression is recorded for an ad.
            ad.OnAdImpressionRecorded += () =>
            {
                Debug.Log("App open ad recorded an impression.");
            };
            // Raised when a click is recorded for an ad.
            ad.OnAdClicked += () =>
            {
                Debug.Log("App open ad was clicked.");
            };
            // Raised when an ad opened full screen content.
            ad.OnAdFullScreenContentOpened += () =>
            {
                Debug.Log("App open ad full screen content opened.");
            };
            // Raised when the ad closed full screen content.
            ad.OnAdFullScreenContentClosed += () =>
            {
                Debug.Log("App open ad full screen content closed.");
                LoadAppOpenAd();
            };
            // Raised when the ad failed to open full screen content.
            ad.OnAdFullScreenContentFailed += (AdError error) =>
            {
                Debug.LogError("App open ad failed to open full screen content " +
                               "with error : " + error);
            };
        }

        private void OnAppStateChanged(AppState state)
        {
            //Debug.Log("App State changed to : " + state);

            // if the app is Foregrounded and the ad is available, show it.
            if (state == AppState.Foreground)
            {
                if (IsAdAvailable)
                {
                    ShowAppOpenAd();
                }
            }
        }

        public void ShowAppOpenAd()
        {
            if (appOpenAd != null && appOpenAd.CanShowAd())
            {
                appOpenAd.Show();
                appOpenLoaded = false;
            }
            else
            {
                Debug.LogError("App open ad is not ready yet.");
                LoadAppOpenAd();
            }
        }

        public void CreateBannerView()
        {
            // If we already have a banner, destroy the old one.
            if (_bannerView != null)
            {
                DestroyBannerView();
            }

            // Create a 320x50 banner at top of the screen
            _bannerView = new BannerView(bannerUnitId, AdSize.Banner, AdPosition.Top);
        }

        public void LoadBannerAd()
        {
            // create an instance of a banner view first.
            if (_bannerView == null)
            {
                CreateBannerView();
            }

            // create our request used to load the ad.
            var adRequest = new AdRequest();

            // send the request to load the ad.
            Debug.Log("Loading banner ad.");
            ListenToAdEvents();
            _bannerView.LoadAd(adRequest);
        }

        private void ListenToAdEvents()
        {
            // Raised when an ad is loaded into the banner view.
            _bannerView.OnBannerAdLoaded += () =>
            {
                Debug.Log("Banner view loaded an ad with response : "
                    + _bannerView.GetResponseInfo());
            };
            // Raised when an ad fails to load into the banner view.
            _bannerView.OnBannerAdLoadFailed += (LoadAdError error) =>
            {
                Debug.LogError("Banner view failed to load an ad with error : "
                    + error);
            };
            // Raised when the ad is estimated to have earned money.
            _bannerView.OnAdPaid += (AdValue adValue) =>
            {
                Debug.Log(String.Format("Banner view paid {0} {1}.",
                    adValue.Value,
                    adValue.CurrencyCode));
            };
            // Raised when an impression is recorded for an ad.
            _bannerView.OnAdImpressionRecorded += () =>
            {
                Debug.Log("Banner view recorded an impression.");
            };
            // Raised when a click is recorded for an ad.
            _bannerView.OnAdClicked += () =>
            {
                Debug.Log("Banner view was clicked.");
            };
            // Raised when an ad opened full screen content.
            _bannerView.OnAdFullScreenContentOpened += () =>
            {
                Debug.Log("Banner view full screen content opened.");
            };
            // Raised when the ad closed full screen content.
            _bannerView.OnAdFullScreenContentClosed += () =>
            {
                Debug.Log("Banner view full screen content closed.");
            };
        }

        public void DestroyBannerView()
        {
            if (_bannerView != null)
            {
                //Debug.Log("Destroying banner view.");
                _bannerView.Destroy();
                _bannerView = null;
            }
        }

        public void HideBannerView()
        {
            if (_bannerView != null)
            {
                _bannerView.Hide();
            }
        }

        public void ShowBannerView()
        {
            if (_bannerView != null)
            {
                //Debug.Log("Destroying banner view.");
                _bannerView.Show();
            }
        }

        public void ChangeBannerView(AdPosition adPosition)
        {
            if (_bannerView != null)
            {
                //Debug.Log("Showing banner view.");
                _bannerView.SetPosition(adPosition);
            }
        }

        public void LoadInterstitialAd()
        {
            // Clean up the old ad before loading a new one.
            if (_interstitialAd != null)
            {
                _interstitialAd.Destroy();
                _interstitialAd = null;
            }

            Debug.Log("Loading the interstitial ad.");

            // create our request used to load the ad.
            var adRequest = new AdRequest();

            // send the request to load the ad.
            InterstitialAd.Load(interstitialAdUnitId, adRequest,
                (InterstitialAd ad, LoadAdError error) =>
                {
                // if error is not null, the load request failed.
                if (error != null || ad == null)
                    {
                        Debug.LogError("interstitial ad failed to load an ad " +
                                       "with error : " + error);
                        return;
                    }

                    Debug.Log("Interstitial ad loaded with response : "
                              + ad.GetResponseInfo());

                    _interstitialAd = ad;
                    RegisterEventHandlers(ad);
                });
        }

        private void RegisterEventHandlers(InterstitialAd interstitialAd)
        {
            // Raised when the ad is estimated to have earned money.
            interstitialAd.OnAdPaid += (AdValue adValue) =>
            {
                Debug.Log(String.Format("Interstitial ad paid {0} {1}.",
                    adValue.Value,
                    adValue.CurrencyCode));
            };
            // Raised when an impression is recorded for an ad.
            interstitialAd.OnAdImpressionRecorded += () =>
            {
                Debug.Log("Interstitial ad recorded an impression.");
            };
            // Raised when a click is recorded for an ad.
            interstitialAd.OnAdClicked += () =>
            {
                Debug.Log("Interstitial ad was clicked.");
            };
            // Raised when an ad opened full screen content.
            interstitialAd.OnAdFullScreenContentOpened += () =>
            {
                Debug.Log("Interstitial ad full screen content opened.");
            };
            // Raised when the ad closed full screen content.
            interstitialAd.OnAdFullScreenContentClosed += () =>
            {
                Debug.Log("Interstitial ad full screen content closed.");
                LoadInterstitialAd();
            };
            // Raised when the ad failed to open full screen content.
            interstitialAd.OnAdFullScreenContentFailed += (AdError error) =>
            {
                Debug.LogError("Interstitial ad failed to open full screen content " +
                               "with error : " + error);
            };
        }

        public void ShowInterstitialAd()
        {
            if (_interstitialAd != null && _interstitialAd.CanShowAd())
            {
                //Debug.Log("Showing interstitial ad.");
                _interstitialAd.Show();
            }
            else
            {
                //Debug.LogError("Interstitial ad is not ready yet.");
                LoadInterstitialAd();
                ShowUnityInterstitialAd();
            }
        }

        public void LoadRewardedAd()
        {
            // Clean up the old ad before loading a new one.
            if (_rewardedAd != null)
            {
                _rewardedAd.Destroy();
                _rewardedAd = null;
            }

            Debug.Log("Loading the rewarded ad.");

            // create our request used to load the ad.
            var adRequest = new AdRequest();

            // send the request to load the ad.
            RewardedAd.Load(rewardAdUnitId, adRequest,
                (RewardedAd ad, LoadAdError error) =>
                {
                // if error is not null, the load request failed.
                if (error != null || ad == null)
                    {
                        Debug.LogError("Rewarded ad failed to load an ad " +
                                       "with error : " + error);
                        return;
                    }

                    Debug.Log("Rewarded ad loaded with response : "
                              + ad.GetResponseInfo());

                    _rewardedAd = ad;
                    RegisterEventHandlers(ad);
                });
        }

        public bool ShowRewardedAd()
        {
            //const string rewardMsg = "Rewarded ad rewarded the user. Type: {0}, amount: {1}.";

            if (_rewardedAd != null && _rewardedAd.CanShowAd())
            {
                _rewardedAd.Show((Reward reward) =>
                {
                    // TODO: Reward the user.
                    //Debug.Log(String.Format(rewardMsg, reward.Type, reward.Amount));

                    giveReward = true;
                });
                return true;
            }
            else
            {
                LoadRewardedAd();
                return ShowUnityRewardedVideo(); ;
            }
        }

        private void RegisterEventHandlers(RewardedAd ad)
        {
            // Raised when the ad is estimated to have earned money.
            ad.OnAdPaid += (AdValue adValue) =>
            {
                Debug.Log(String.Format("Rewarded ad paid {0} {1}.",
                    adValue.Value,
                    adValue.CurrencyCode));
            };
            // Raised when an impression is recorded for an ad.
            ad.OnAdImpressionRecorded += () =>
            {
                Debug.Log("Rewarded ad recorded an impression.");
            };
            // Raised when a click is recorded for an ad.
            ad.OnAdClicked += () =>
            {
                Debug.Log("Rewarded ad was clicked.");
            };
            // Raised when an ad opened full screen content.
            ad.OnAdFullScreenContentOpened += () =>
            {
                Debug.Log("Rewarded ad full screen content opened.");
            };
            // Raised when the ad closed full screen content.
            ad.OnAdFullScreenContentClosed += () =>
            {
                Debug.Log("Rewarded ad full screen content closed.");
            };
            // Raised when the ad failed to open full screen content.
            ad.OnAdFullScreenContentFailed += (AdError error) =>
            {
                Debug.LogError("Rewarded ad failed to open full screen content " +
                               "with error : " + error);
            };
        }

        public void OnInitializationComplete()
        {
            Debug.Log("=== OnInitializationComplete");

            LoadUnityInterstitialAd();
            LoadUnityRewardedAds();
        }

        public void LoadUnityInterstitialAd()
        {
            // IMPORTANT! Only load content AFTER initialization (in this example, initialization is handled in a different script).
            Advertisement.Load(interstitialAdUnitId_Unity, this);
        }

        // Show the loaded content in the Ad Unit: 
        public void ShowUnityInterstitialAd()
        {
            // Note that if the ad content wasn't previously loaded, this method will fail
            Advertisement.Show(interstitialAdUnitId_Unity, this);
        }

        public void LoadUnityRewardedAds()
        {
            Advertisement.Load(rewardAdUnitId_Unity, this);
        }

        public bool ShowUnityRewardedVideo()
        {
            Advertisement.Show(rewardAdUnitId_Unity, this);
            return true;
        }

        public void OnUnityAdsAdLoaded(string placementId)
        {
            Debug.Log("OnUnityAdsAdLoaded : " + placementId);
        }

        public void OnUnityAdsFailedToLoad(string placementId, UnityAdsLoadError error, string message)
        {
            Debug.Log("OnUnityAdsFailedToLoad : " + placementId);
        }

        public void OnUnityAdsShowFailure(string placementId, UnityAdsShowError error, string message)
        {
            Debug.Log("OnUnityAdsShowFailure : " + placementId);
        }

        public void OnUnityAdsShowStart(string placementId)
        {
            Debug.Log("OnUnityAdsShowStart : " + placementId);
        }

        public void OnUnityAdsShowClick(string placementId)
        {
            Debug.Log("OnUnityAdsShowClick : " + placementId);
        }

        public void OnUnityAdsShowComplete(string placementId, UnityAdsShowCompletionState showCompletionState)
        {
            Debug.Log("OnUnityAdsShowComplete : " + placementId);

            if (placementId.Equals(interstitialAdUnitId_Unity))
            {
                LoadUnityInterstitialAd();
            }
            else if (placementId.Equals(rewardAdUnitId_Unity))
            {
                giveReward = true;
                LoadUnityRewardedAds();
            }
        }

        public void OnInitializationFailed(UnityAdsInitializationError error, string message)
        {
            Debug.Log("OnInitializationFailed : " + message);
        }

        private void Update()
        {
            if (giveReward)
            {
                giveReward = false;
                PlayerInfo.Instance.UpdateCoins(CoinAction.Add, 1000);
            }
        }
    }
}