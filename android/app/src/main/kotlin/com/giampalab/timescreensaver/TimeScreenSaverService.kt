package com.giampalab.timescreensaver

import android.service.dreams.DreamService
import android.webkit.WebView
import android.webkit.WebViewClient

class TimeScreenSaverService : DreamService() {

    private lateinit var webView: WebView

    override fun onAttachedToWindow() {
        super.onAttachedToWindow()
        isFullscreen = true
        isInteractive = false

        webView = WebView(this)
        setContentView(webView)

        webView.settings.apply {
            javaScriptEnabled = true
            domStorageEnabled = true
            allowFileAccess = true
        }

        webView.webViewClient = WebViewClient()
        webView.loadUrl("file:///android_asset/index.html")
    }

    override fun onDreamingStarted() {
        super.onDreamingStarted()
        webView.onResume()
    }

    override fun onDreamingStopped() {
        super.onDreamingStopped()
        webView.onPause()
    }

    override fun onDetachedFromWindow() {
        super.onDetachedFromWindow()
        webView.destroy()
    }
}
