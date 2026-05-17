package com.giampalab.timescreensaver

import android.service.dreams.DreamService
import android.util.Log
import android.webkit.ConsoleMessage
import android.webkit.WebChromeClient
import android.webkit.WebView
import android.webkit.WebViewClient

class TimeScreenSaverService : DreamService() {

    private lateinit var webView: WebView

    override fun onAttachedToWindow() {
        super.onAttachedToWindow()
        isFullscreen = true
        isInteractive = false

        // Enable Chrome DevTools inspection via chrome://inspect on desktop
        WebView.setWebContentsDebuggingEnabled(true)

        webView = WebView(this)
        setContentView(webView)

        webView.settings.apply {
            javaScriptEnabled = true
            domStorageEnabled = true
            allowFileAccess = true
            // Required for Blazor WASM: allows fetch() to load _framework files
            // from file:// URLs (same origin as index.html)
            @Suppress("SetJavaScriptEnabled")
            allowFileAccessFromFileURLs = true
            allowUniversalAccessFromFileURLs = true
        }

        // Log JS console output to logcat (visible via: adb logcat -s TimeWebView)
        webView.webChromeClient = object : WebChromeClient() {
            override fun onConsoleMessage(msg: ConsoleMessage): Boolean {
                Log.d("TimeWebView", "${msg.messageLevel()} ${msg.message()} [${msg.sourceId()}:${msg.lineNumber()}]")
                return true
            }
        }

        webView.webViewClient = WebViewClient()
        webView.loadUrl("file:///android_asset/index.html")
    }

    override fun onDreamingStarted() { super.onDreamingStarted(); webView.onResume() }
    override fun onDreamingStopped() { super.onDreamingStopped(); webView.onPause() }
    override fun onDetachedFromWindow() { super.onDetachedFromWindow(); webView.destroy() }
}
