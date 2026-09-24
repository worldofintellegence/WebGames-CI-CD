# 📖 Complete Guide: How to Implement Ads in Unity WebGL

This document provides step-by-step instructions on implementing, configuring, and testing **Interstitial** and **Rewarded** ads in your Unity WebGL games using **Google H5 Games Ads (Ad Placement API)** and the custom **GameBridge** system.

---

## 📑 Table of Contents
1. [Architecture Overview](#1-architecture-overview)
2. [Quick Setup in Unity](#2-quick-setup-in-unity)
3. [Calling Ads from C# Scripts](#3-calling-ads-from-c-scripts)
4. [In-Editor Testing (No WebGL Build Needed)](#4-in-editor-testing-no-webgl-build-needed)
5. [Configuring the WebGL Template](#5-configuring-the-webgl-template)
6. [Connecting Real Google Ads (AdSense / H5 Games)](#6-connecting-real-google-ads-adsense--h5-games)
7. [Controlling Ads from Website JavaScript (`game-bridge.js`)](#7-controlling-ads-from-website-javascript-game-bridgejs)
8. [Best Practices & Troubleshooting](#8-best-practices--troubleshooting)
9. [Multi-Game Template & `game-config.json` System](#9-multi-game-template--game-configjson-system)

---

## 1. Architecture Overview

Here is how data flows when an ad is requested:

```
┌────────────────────────────────────────────────────────┐
│                      UNITY (C#)                        │
│  AdManager.Instance.ShowRewarded("extra_life", ...)    │
└──────────────────────────┬─────────────────────────────┘
                           │ [DllImport("__Internal")]
                           ▼
┌────────────────────────────────────────────────────────┐
│              UNITY WEBGL PLUGIN (.jslib)               │
│  Assets/Plugins/WebGL/AdBridge.jslib                   │
└──────────────────────────┬─────────────────────────────┘
                           │ calls window.GameBridge
                           ▼
┌────────────────────────────────────────────────────────┐
│             JAVASCRIPT BRIDGE (game-bridge.js)         │
│  TemplateData/game-bridge.js                           │
│  - Triggers Google 'adBreak()' OR Testing Modal        │
└──────────────────────────┬─────────────────────────────┘
                           │ unityInstance.SendMessage()
                           ▼
┌────────────────────────────────────────────────────────┐
│                      UNITY (C#)                        │
│  AdManager.cs: OnRewardedSuccess() -> Grants Reward    │
└────────────────────────────────────────────────────────┘
```

---

## 2. Quick Setup in Unity

All necessary files are already installed in your project:
- **`Assets/Scripts/Ads/AdManager.cs`**: Singleton manager.
- **`Assets/Scripts/Ads/AdDemoUI.cs`**: In-game test GUI.
- **`Assets/Plugins/WebGL/AdBridge.jslib`**: WebGL native bridge.
- **`Assets/WebGLTemplates/AdSupportedTemplate/`**: Custom WebGL template.

### To setup in a new scene:
1. Open your scene in Unity.
2. In the top Unity menu, click:
   **`Tools` > `WebGL Ads` > `Add AdManager & Demo UI to Current Scene`**
   *(Or simply create an empty GameObject named `[AdManager]` and attach `AdManager.cs`)*.
3. Save the scene (`Ctrl + S` or `Cmd + S`).

---

## 3. Calling Ads from C# Scripts

### A. Interstitial Ad (Level Complete / Game Over)
Interstitial ads are full-screen breaks shown between levels or when the player loses.

```csharp
using UnityEngine;
using CIEL.WebGames.Ads;

public class LevelController : MonoBehaviour
{
    public void OnLevelComplete()
    {
        // Request an interstitial ad
        AdManager.Instance.ShowInterstitial("level_finished", () =>
        {
            // Callback: Runs after ad closes
            Debug.Log("Ad closed! Loading next level...");
            LoadNextLevel();
        });
    }

    private void LoadNextLevel()
    {
        // Load your next level here
    }
}
```

### B. Rewarded Ad (Watch Video for In-Game Item / Coins / Revive)
Rewarded ads grant in-game benefits only when the user finishes watching the ad.

```csharp
using UnityEngine;
using CIEL.WebGames.Ads;

public class ShopOrRewardUI : MonoBehaviour
{
    private int playerCoins = 0;

    public void OnWatchAdForCoinsClicked()
    {
        AdManager.Instance.ShowRewarded("free_coins_reward",
            onRewardSuccess: () =>
            {
                // User watched the full ad -> Grant reward
                playerCoins += 50;
                Debug.Log($"Success! Coins rewarded. New balance: {playerCoins}");
            },
            onRewardFailed: () =>
            {
                // User skipped the ad or ad failed to load
                Debug.Log("Reward was not granted (ad skipped or failed).");
            }
        );
    }
}
```

### C. Listening to Global Events (Optional)
You can also subscribe to `AdManager` events anywhere in your game:

```csharp
using CIEL.WebGames.Ads;
using UnityEngine;

public class SoundAndGameObserver : MonoBehaviour
{
    private void OnEnable()
    {
        AdManager.Instance.OnAdOpenedEvent += HandleAdOpened;
        AdManager.Instance.OnAdClosedEvent += HandleAdClosed;
        AdManager.Instance.OnRewardedSuccessEvent += HandleReward;
    }

    private void OnDisable()
    {
        if (AdManager.Instance != null)
        {
            AdManager.Instance.OnAdOpenedEvent -= HandleAdOpened;
            AdManager.Instance.OnAdClosedEvent -= HandleAdClosed;
            AdManager.Instance.OnRewardedSuccessEvent -= HandleReward;
        }
    }

    private void HandleAdOpened(string placement)
    {
        Debug.Log($"Ad started for placement: {placement}");
    }

    private void HandleAdClosed(string placement)
    {
        Debug.Log($"Ad ended for placement: {placement}");
    }

    private void HandleReward(string placement)
    {
        Debug.Log($"Reward unlocked for: {placement}");
    }
}
```

> [!NOTE]
> **Automatic Time & Sound Management**:
> By default, `AdManager` automatically sets `Time.timeScale = 0` (pauses the game) and `AudioListener.pause = true` (mutes all audio) when an ad opens, and automatically restores them when the ad closes.

---

## 4. In-Editor Testing (No WebGL Build Needed)

You do **not** have to export a WebGL build every time you want to test ad triggers and rewards!

1. Press **Play** in the Unity Editor.
2. In the top-left corner, you will see the **CIEL WebGL Ads Controller** overlay (powered by `AdDemoUI.cs`).
3. Click **"▶ Show Interstitial Ad"** or **"🎁 Show Rewarded Ad"**.
4. The system will:
   - Pause game time and mute game audio.
   - Run a simulated ad timer in the console (`[AdManager (Editor Simulation)]`).
   - Resume game time and grant `+50 Coins`.

---

## 5. Configuring the WebGL Template

To ensure your WebGL build uses this custom template:

1. Open **`Edit` > `Project Settings` > `Player`**.
2. Select the **WebGL tab** (HTML5 logo).
3. Expand the **Resolution and Presentation** section.
4. Select **`PROJECT:AdSupportedTemplate`**.
   *(Alternatively, click `Tools > WebGL Ads > Apply AdSupported Template` in the Unity top menu).*

When you build the project via **`File` > `Build Settings` > `Build`**, Unity will output your game with:
- The responsive dark layout.
- The Google Ads integration.
- `TemplateData/game-bridge.js`.
- The interactive in-browser ad preview modal.

---

## 6. Connecting Real Google Ads (AdSense / H5 Games)

Google provides the **Ad Placement API (H5 Games Ads)** for browser games.

### Step 1: Add Your Publisher ID in `index.html`
Open [`Assets/WebGLTemplates/AdSupportedTemplate/index.html`](file:///Volumes/AS_Drive/CIEL%20Tech%20%28PK%29/CIEL-WebGames/Assets/WebGLTemplates/AdSupportedTemplate/index.html) and locate the script tag around line 14:

```html
<script async
  data-ad-client="ca-pub-XXXXXXXXXXXXXXXX"   <!-- Replace with your Google Publisher ID -->
  data-ad-channel="0000000000"
  data-ad-frequency-hint="30s"
  data-adbreak-test="on"                    <!-- Keep "on" for testing; remove or set "off" for production -->
  crossorigin="anonymous"
  src="https://pagead2.googlesyndication.com/pagead/js/adsbygoogle.js">
</script>
```

### Step 2: Switch Off Test Mode in `game-bridge.js`
Open [`Assets/WebGLTemplates/AdSupportedTemplate/TemplateData/game-bridge.js`](file:///Volumes/AS_Drive/CIEL%20Tech%20%28PK%29/CIEL-WebGames/Assets/WebGLTemplates/AdSupportedTemplate/TemplateData/game-bridge.js) and update the config:

```javascript
config: {
  // Set to false to serve real Google ads via adBreak()
  // Set to true to show the local interactive preview modal
  testMode: false,
  googlePublisherId: "ca-pub-XXXXXXXXXXXXXXXX",
  interstitialDuration: 4,
  rewardedDuration: 5,
  debugLogs: true
},
```

---

## 7. Controlling Ads from Website JavaScript (`game-bridge.js`)

If you are embedding the game into an existing gaming portal, website, or React/Vue app, you can communicate with the game directly from the browser:

### A. Calling Ads or Sending Data into Unity:
```javascript
// Show an interstitial ad
window.GameBridge.showInterstitial("portal_break");

// Show a rewarded ad
window.GameBridge.showRewarded("double_reward");

// Send any custom data to a C# method:
// Syntax: sendToGame(gameObjectName, methodName, parameter)
window.GameBridge.sendToGame("Player", "AddGems", 25);

// Pause or Resume the game from the website
window.GameBridge.pauseGame();
window.GameBridge.resumeGame();
```

### B. Listening to Events from Unity:
```javascript
// Game finished loading
window.addEventListener("unity:ready", function(e) {
  console.log("Game WebGL loaded and ready!");
});

// Rewarded ad completed (player earned reward)
window.addEventListener("unity:rewardGranted", function(e) {
  console.log("Reward placement:", e.detail.placement);
  // Update website database or user profile wallet here
});

// Ad started
window.addEventListener("unity:adOpened", function(e) {
  console.log("Ad opened for:", e.detail.placement);
});

// Ad closed
window.addEventListener("unity:adClosed", function(e) {
  console.log("Ad closed for:", e.detail.placement);
});
```

---

## 8. Best Practices & Troubleshooting

| Issue | Cause | Solution |
| :--- | :--- | :--- |
| **No ads show up on localhost** | Google Ads does not serve real ads on `localhost` or unapproved domains. | Keep `testMode: true` in `game-bridge.js` during local development to use the interactive preview modal. |
| **Audio keeps playing during ad** | Custom audio sources ignore `AudioListener.pause`. | In `AdManager.cs`, `autoMuteAudio` is enabled by default. If using custom FMOD or external audio, subscribe to `OnAdOpenedEvent` to mute manually. |
| **AdBlocker blocked the ads** | User has an ad blocker extension enabled. | Call `AdManager.Instance.IsAdBlockerDetected()` in C# to notify the user or offer alternative rewards. |
| **Touch inputs pass through to game on mobile** | Canvas clicks during ad overlay. | `AdManager` pauses `Time.timeScale = 0` and the template modal overlay has `z-index: 9999` with backdrop shield. |

---

## 9. Multi-Game Template & `game-config.json` System

This repository is designed as a **Universal WebGL Game Base Template** on your GitHub `main` branch. Whenever you or your team create a new mini-game, you do not need to rewrite ads or player settings from scratch.

### The `game-config.json` File
Located at the root of the repository: [`game-config.json`](file:///Volumes/AS_Drive/CIEL%20Tech%20%28PK%29/CIEL-WebGames/game-config.json)

```json
{
  "gameId": "ciel-minigame-template",
  "productName": "CIEL Web Game",
  "companyName": "CIEL Tech",
  "version": "1.0.0",
  "description": "Mini-game description...",
  "category": "Arcade",
  "tags": ["webgl", "html5", "unity", "arcade"],
  "icon": "Assets/Icons/app-icon.png",
  "screenshots": [
    "Assets/Screenshots/screenshot1.png",
    "Assets/Screenshots/screenshot2.png"
  ],
  "playerSettings": {
    "defaultScreenWidth": 960,
    "defaultScreenHeight": 600,
    "runInBackground": true,
    "webGLTemplate": "PROJECT:AdSupportedTemplate"
  },
  "adsConfig": {
    "testMode": true,
    "publisherId": "ca-pub-0000000000000000",
    "interstitialDuration": 4,
    "rewardedDuration": 5
  }
}
```

### Visual Editor Window in Unity
In Unity, go to the top menu:
**`Tools` > `WebGL Game Config` > `Open Config Manager Window`**

This window provides 4 tabs:
1. **Game Identity**: Set Product Name, Company, Version, Category, and Description.
2. **Assets & Media**: Drag & drop your App Icon (`512x512`) and configure screenshots.
3. **Player Settings**: Set canvas resolution (e.g. 960x600 or 1280x720) and WebGL template.
4. **Ads & Monetization**: Toggle Test Mode and set your Google Publisher ID.

Click **"⚡ Apply to PlayerSettings"** to instantly push all JSON settings into Unity's internal PlayerSettings and Icon settings.

### Creating a New Mini-Game from this Template
1. Branch off `main` (or clone the template repo).
2. Create your gameplay in Unity.
3. Replace the icon in `Assets/Icons/app-icon.png` and screenshots in `Assets/Screenshots/`.
4. Open `Tools > WebGL Game Config > Open Config Manager Window`, update your game's name/description, and click **Apply to PlayerSettings**.
5. Build WebGL — your game is ready with custom branding, resolution, and Google Ads out of the box!

---

### 🚀 You are all set!
Your Unity WebGL game is now fully equipped to play ads on websites, communicate two-way with the browser, and monetize seamlessly.
