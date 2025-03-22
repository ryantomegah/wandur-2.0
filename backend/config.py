import os
from dotenv import load_dotenv

# Load environment variables
load_dotenv()

# Firebase Configuration
FIREBASE_CONFIG = {
    "apiKey": os.getenv("FIREBASE_API_KEY"),
    "authDomain": os.getenv("FIREBASE_AUTH_DOMAIN"),
    "projectId": os.getenv("FIREBASE_PROJECT_ID"),
    "storageBucket": os.getenv("FIREBASE_STORAGE_BUCKET"),
    "messagingSenderId": os.getenv("FIREBASE_MESSAGING_SENDER_ID"),
    "appId": os.getenv("FIREBASE_APP_ID")
}

# API Configuration
API_VERSION = "v1"
API_PREFIX = f"/api/{API_VERSION}"

# Geofencing Configuration
DEFAULT_GEOFENCE_RADIUS = 50.0  # meters
MALL_GEOFENCE_RADIUS = 100.0    # meters

# Analytics Configuration
ANALYTICS_COLLECTION = "analytics"
STORES_COLLECTION = "stores"

# Security
CORS_ORIGINS = [
    "http://localhost:3000",  # React frontend
    "http://localhost:8000",  # FastAPI backend
    "http://localhost:8080",  # Unity WebGL build
]

# Logging
LOG_LEVEL = os.getenv("LOG_LEVEL", "INFO")
LOG_FORMAT = "%(asctime)s - %(name)s - %(levelname)s - %(message)s" 