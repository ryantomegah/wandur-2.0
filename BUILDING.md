# Building and Installing the Wandur App

This guide provides step-by-step instructions for building and installing the Wandur AR shopping app on your device.

## Prerequisites

### For All Platforms
- Unity Hub and Unity 2022.3 LTS or newer
- Git (for cloning the repository)

### For Android Builds
- Android SDK (installed via Unity Hub)
- Android device with Developer Options and USB debugging enabled
- USB cable to connect your device

### For iOS Builds
- Mac computer with Xcode 14 or newer
- Apple Developer account
- iPhone or iPad running iOS 14 or newer
- USB cable to connect your device

## Setup Instructions

### 1. Install Unity Hub and Unity
1. Download Unity Hub from [Unity's website](https://unity.com/download)
2. Install Unity Hub
3. Open Unity Hub and go to the "Installs" tab
4. Click "Install Editor"
5. Select Unity 2022.3.18f1 LTS (or newer 2022.3 LTS version)
6. Make sure to include these modules:
   - Android Build Support (for Android)
   - iOS Build Support (for iOS, Mac only)
   - Documentation (optional but recommended)

### 2. Clone the Repository
```bash
git clone https://github.com/ryantomegah/wandur-2.0.git
cd wandur-2.0
```

### 3. Open the Project
1. Launch Unity Hub
2. Click "Add" in the Projects tab
3. Browse to the `shopper-app` folder in the cloned repository
4. Select the folder and click "Add Project"
5. Click on the project to open it with Unity

## Building for Android

### Via Unity Editor
1. Open the project in Unity
2. Go to File → Build Settings
3. Select Android and click "Switch Platform" (this may take a few minutes)
4. Click "Player Settings" and configure:
   - Company Name: Wandur
   - Product Name: Wandur Shopper
   - Bundle Identifier: com.wandur.shopperapp
   - Minimum API Level: Android 7.0 (API level 24) or higher
5. Return to Build Settings and click "Build"
6. Choose a location to save the APK (e.g., Desktop)
7. Wait for the build to complete

### Via Command Line (Automated)
We've created a script to automate the build process:

1. Make sure Unity is installed and the project is set up
2. Open a terminal and navigate to the project root
3. Run the build script:
```bash
chmod +x build_android.sh  # Make the script executable (if needed)
./build_android.sh
```
4. The script will:
   - Locate your Unity installation
   - Build the app
   - Save the APK to your Desktop

## Installing on Android

### Method 1: Direct Installation
1. Connect your Android device to your computer via USB
2. Enable USB debugging on your device if prompted
3. Run the following command in terminal:
```bash
adb install ~/Desktop/WandurApp.apk
```

### Method 2: Manual Installation
1. Transfer the APK file to your Android device via USB, email, or cloud storage
2. On your Android device, navigate to the APK file
3. Tap the file to install (you may need to enable "Install from Unknown Sources" in your device settings)
4. Follow the on-screen prompts to complete the installation

## Building for iOS (Mac only)

1. Open the project in Unity
2. Go to File → Build Settings
3. Select iOS and click "Switch Platform"
4. Configure Player Settings:
   - Company Name: Wandur
   - Product Name: Wandur Shopper
   - Bundle Identifier: com.wandur.shopperapp
5. Click "Build"
6. Choose a location to save the Xcode project
7. Open the generated Xcode project
8. In Xcode, select your development team under Signing & Capabilities
9. Connect your iOS device
10. Select your device as the build target
11. Click the Run button to build and install on your device

## Testing the App

Once installed, you can test the Wandur app following these steps:

1. Launch the app on your device
2. The app will start in debug mode with simulated movement
3. Select any store from the list to start navigation
4. Observe the Divine Line guiding you to the destination
5. Walk around to test the responsiveness of the navigation
6. Notice how the Divine Line adjusts as you approach the destination

## Troubleshooting

### Android Build Issues
- If you see "ADB not found" errors, make sure Android SDK is properly installed
- If installation fails with "INSTALL_FAILED_VERSION_DOWNGRADE", uninstall any existing version first
- For "INSTALL_FAILED_USER_RESTRICTED" errors, check your device's security settings

### iOS Build Issues
- If Xcode shows signing errors, make sure your Apple Developer account is properly set up
- If the app doesn't install, verify that your device is registered in your developer account
- For "Untrusted Developer" warnings, go to Settings → General → Profiles & Device Management and trust your developer certificate

### Unity Errors
- If scripts are missing references, try reopening the project or rebuilding the script assemblies
- For shader errors, make sure your Graphics API settings match your target device capabilities

## Support

If you encounter any issues, please contact the development team or open an issue on the GitHub repository. 