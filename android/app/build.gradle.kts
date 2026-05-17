plugins {
    id("com.android.application")
    id("org.jetbrains.kotlin.android")
}

android {
    namespace = "com.giampalab.timescreensaver"
    compileSdk = 34

    defaultConfig {
        applicationId = "com.giampalab.timescreensaver"
        minSdk = 26
        targetSdk = 34
        versionCode = 1
        versionName = "1.0"
    }

    buildTypes {
        release {
            isMinifyEnabled = false
        }
    }

    aaptOptions {
        ignoreAssetsPattern = "*.gz:*.br"
    }

    compileOptions {
        sourceCompatibility = JavaVersion.VERSION_1_8
        targetCompatibility = JavaVersion.VERSION_1_8
    }

    kotlinOptions {
        jvmTarget = "1.8"
    }
}

// No extra dependencies — WebView ships with Android
