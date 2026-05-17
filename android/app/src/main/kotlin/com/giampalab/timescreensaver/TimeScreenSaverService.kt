package com.giampalab.timescreensaver

import android.service.dreams.DreamService
import android.util.Log
import android.webkit.ConsoleMessage
import android.webkit.WebChromeClient
import android.webkit.WebResourceRequest
import android.webkit.WebResourceResponse
import android.webkit.WebView
import android.webkit.WebViewClient
import androidx.webkit.WebViewAssetLoader

class TimeScreenSaverService : DreamService() {

    private lateinit var webView: WebView

    override fun onAttachedToWindow() {
        super.onAttachedToWindow()
        isFullscreen = true
        isInteractive = false

        WebView.setWebContentsDebuggingEnabled(true)

        // Serve assets over https://appassets.androidplatform.net/ instead of file://
        // This is the correct way to load local assets in WebView — avoids all
        // file:// fetch restrictions that block Blazor WASM from loading _framework files
        val assetLoader = WebViewAssetLoader.Builder()
            .setDomain("appassets.androidplatform.net")
            .addPathHandler("/", WebViewAssetLoader.AssetsPathHandler(this))
            .build()

        webView = WebView(this)
        setContentView(webView)

        webView.settings.apply {
            javaScriptEnabled = true
            domStorageEnabled = true
            allowFileAccess = false  // not needed — assets served via https
        }

        webView.webViewClient = object : WebViewClient() {
            override fun shouldInterceptRequest(
                view: WebView,
                request: WebResourceRequest
            ): WebResourceResponse? = assetLoader.shouldInterceptRequest(request.url)
        }

        webView.webChromeClient = object : WebChromeClient() {
            override fun onConsoleMessage(msg: ConsoleMessage): Boolean {
                Log.d("TimeWebView", "${msg.messageLevel()} ${msg.message()} [${msg.sourceId()}:${msg.lineNumber()}]")
                return true
            }
        }

        webView.loadUrl("https://appassets.androidplatform.net/index.html")
    }

    override fun onDreamingStarted() { super.onDreamingStarted(); webView.onResume() }
    override fun onDreamingStopped() { super.onDreamingStopped(); webView.onPause() }
    override fun onDetachedFromWindow() { super.onDetachedFromWindow(); webView.destroy() }
}
