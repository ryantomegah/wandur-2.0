# Wandur Shopper App

## Unity AR Project Setup Guide

### Prerequisites
- Unity 2022.3 LTS or newer
- AR Foundation 5.0.7 or newer
- XR Plugin Management 4.2.1 or newer
- Android Build Support (for Android deployment)
- iOS Build Support (for iOS deployment)
- Oriient SDK (credentials required)

### Setting Up the Project

#### 1. Create a New Unity Project
1. Open Unity Hub
2. Click "New Project"
3. Select "3D (URP)" template
4. Name the project "Wandur Shopper"
5. Set the location to this directory
6. Click "Create Project"

#### 2. Configure AR Foundation
1. In Unity, go to Window > Package Manager
2. Click the "+" button and select "Add package from git URL..."
3. Enter the following URLs one by one:
   - `com.unity.xr.arfoundation@5.0.7`
   - `com.unity.xr.arkit@5.0.7` (iOS)
   - `com.unity.xr.arcore@5.0.7` (Android)
   - `com.unity.xr.management@4.2.1`

4. Go to Edit > Project Settings > XR Plug-in Management
5. Install XR Plugin Management if prompted
6. Enable "ARCore" for Android and "ARKit" for iOS

#### 3. Install Oriient SDK
1. Contact Oriient to obtain the SDK package and credentials
2. Import the Oriient SDK package:
   - Go to Assets > Import Package > Custom Package...
   - Navigate to the Oriient SDK package file
   - Import all assets

3. Configure the Oriient SDK:
   - Add your Oriient API key to the Oriient configuration
   - Set up the required permissions in your app

#### 4. Project Structure
The project includes the following directories:
- `Assets/Scripts`: Contains all C# scripts
- `Assets/Prefabs`: Contains reusable game objects
- `Assets/Scenes`: Contains all Unity scenes
- `Assets/Materials`: Contains materials used in the project
- `Assets/ThirdParty/Oriient`: Contains Oriient SDK files

#### 5. Setting Up Google Maps API (if applicable)
1. Obtain a Google Maps API key from the Google Cloud Console
2. Enable the necessary Google Maps APIs:
   - Maps SDK for Android
   - Maps SDK for iOS
   - Places API
   - Directions API

#### 6. Initial Scene Setup
1. Create a new scene named "MainScene"
2. Add an AR Session component to the scene
3. Add an AR Session Origin component
4. Configure the camera settings for AR

### Development Notes
- The "Divine Lines" AR Navigation feature will utilize AR Foundation's raycast functionality combined with Oriient's indoor positioning
- Geofenced Ads will leverage GPS data and Oriient's indoor positioning system
- Social & Loyalty features will be integrated via the Firebase backend

### Build Configuration
- Set the minimum API level to Android 7.0 (API level 24) for Android
- Set the minimum iOS version to iOS 11.0 for iOS
- Enable "ARCore Required" for Android builds
- Enable "ARKit Required" for iOS builds 