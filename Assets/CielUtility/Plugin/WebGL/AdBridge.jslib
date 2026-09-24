mergeInto(LibraryManager.library, {
  JS_InitAdBridge: function(gameObjectNamePtr) {
    var gameObjectName = UTF8ToString(gameObjectNamePtr);
    if (typeof window !== 'undefined' && window.UnityAdBridge) {
      window.UnityAdBridge.init(gameObjectName);
    } else {
      console.log('[AdBridge.jslib] UnityAdBridge not found on window yet. Target object: ' + gameObjectName);
    }
  },

  JS_ShowInterstitial: function(placementPtr) {
    var placement = UTF8ToString(placementPtr);
    if (typeof window !== 'undefined' && window.UnityAdBridge) {
      window.UnityAdBridge.showInterstitial(placement);
    } else {
      console.warn('[AdBridge.jslib] UnityAdBridge is not defined on window.');
    }
  },

  JS_ShowRewarded: function(placementPtr) {
    var placement = UTF8ToString(placementPtr);
    if (typeof window !== 'undefined' && window.UnityAdBridge) {
      window.UnityAdBridge.showRewarded(placement);
    } else {
      console.warn('[AdBridge.jslib] UnityAdBridge is not defined on window.');
    }
  },

  JS_IsAdBlockerDetected: function() {
    if (typeof window !== 'undefined' && window.UnityAdBridge && window.UnityAdBridge.isAdBlockerActive) {
      return window.UnityAdBridge.isAdBlockerActive() ? 1 : 0;
    }
    return 0;
  }
});
