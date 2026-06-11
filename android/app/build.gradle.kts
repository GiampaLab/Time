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

    buildFeatures {
        // Needed for BuildConfig.DEBUG, used to keep WebView debugging out of release.
        buildConfig = true
    }

    androidResources {
        ignoreAssetsPatterns += listOf("*.gz", "*.br")
    }

    compileOptions {
        sourceCompatibility = JavaVersion.VERSION_1_8
        targetCompatibility = JavaVersion.VERSION_1_8
    }

    kotlinOptions {
        jvmTarget = "1.8"
    }
}

dependencies {
    implementation("androidx.webkit:webkit:1.11.0")
}
