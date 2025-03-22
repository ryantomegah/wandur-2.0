#!/bin/bash

# Find Unity installation
UNITY_PATH=$(find /Applications -name "Unity.app" -type d | head -n 1)

if [ -z "$UNITY_PATH" ]; then
    echo "Unity not found. Please install Unity first via Unity Hub."
    echo "Opening Unity Hub..."
    open "/Applications/Unity Hub.app"
    exit 1
fi

echo "Unity found at: $UNITY_PATH"
UNITY_EXECUTABLE="$UNITY_PATH/Contents/MacOS/Unity"

# Build the Android app
echo "Starting Android build..."
"$UNITY_EXECUTABLE" -batchmode -projectPath "$(pwd)/shopper-app" -executeMethod BuildScript.PerformAndroidBuild -logFile build.log -quit

if [ $? -eq 0 ]; then
    echo "Build process initiated. Check build.log for details."
    echo "APK will be saved to your Desktop when complete."
    
    # Install on connected Android device
    echo "Would you like to install the app on a connected Android device? (y/n)"
    read -r answer
    if [ "$answer" = "y" ]; then
        echo "Trying to install APK..."
        adb install ~/Desktop/WandurApp.apk
    fi
else
    echo "Build process failed. Check build.log for details."
fi 