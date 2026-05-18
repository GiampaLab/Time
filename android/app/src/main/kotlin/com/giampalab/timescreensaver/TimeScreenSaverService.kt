package com.giampalab.timescreensaver

import android.content.Context
import android.content.pm.ActivityInfo
import android.service.dreams.DreamService
import android.util.Log
import android.webkit.ConsoleMessage
import android.webkit.MimeTypeMap
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

        window?.let { win ->
            val lp = win.attributes
            lp.screenOrientation = ActivityInfo.SCREEN_ORIENTATION_SENSOR_LANDSCAPE
            win.attributes = lp
        }

        WebView.setWebContentsDebuggingEnabled(true)

        val assetLoader = WebViewAssetLoader.Builder()
            .setDomain("appassets.androidplatform.net")
            .addPathHandler("/", WasmAssetsPathHandler(this))
            .build()

        webView = WebView(this)
        setContentView(webView)

        webView.settings.apply {
            javaScriptEnabled = true
            domStorageEnabled = true
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

        webView.loadUrl("https://appassets.androidplatform.net/")
    }

    override fun onDreamingStarted() { super.onDreamingStarted(); webView.onResume() }
    override fun onDreamingStopped() { super.onDreamingStopped(); webView.onPause() }
    override fun onDetachedFromWindow() { super.onDetachedFromWindow(); webView.destroy() }

    // Wraps AssetsPathHandler to set correct MIME types for Blazor WASM files
    private class WasmAssetsPathHandler(context: Context) :
        WebViewAssetLoader.PathHandler {

        private val delegate = WebViewAssetLoader.AssetsPathHandler(context)

        private val mimeOverrides = mapOf(
            "wasm" to "application/wasm",
            "dll"  to "application/octet-stream",
            "blat" to "application/octet-stream",
            "dat"  to "application/octet-stream"
        )

        override fun handle(path: String): WebResourceResponse? {
            val resolvedPath = if (path.isEmpty() || path == "/") "index.html" else path
            val response = delegate.handle(resolvedPath) ?: return null
            val ext = resolvedPath.substringAfterLast('.', "")
            val mime = mimeOverrides[ext]
                ?: MimeTypeMap.getSingleton().getMimeTypeFromExtension(ext)
                ?: "application/octet-stream"
            return WebResourceResponse(mime, response.encoding, response.data)
        }
    }
}
