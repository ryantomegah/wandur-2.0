from fastapi import FastAPI, HTTPException
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel
from typing import List, Optional
import firebase_admin
from firebase_admin import credentials, firestore

app = FastAPI(title="Wandur Backend API")

# Configure CORS
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],  # In production, replace with specific origins
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# Initialize Firebase Admin
cred = credentials.Certificate("serviceAccountKey.json")
firebase_admin.initialize_app(cred)
db = firestore.client()

class Store(BaseModel):
    id: str
    name: str
    location: dict
    type: str  # "mall" or "standalone"
    geofence_radius: float

class Analytics(BaseModel):
    store_id: str
    timestamp: str
    visitor_count: int
    dwell_time: float
    zone: str

@app.get("/")
async def root():
    return {"message": "Welcome to Wandur Backend API"}

@app.get("/stores")
async def get_stores():
    try:
        stores_ref = db.collection("stores")
        stores = stores_ref.stream()
        return [{"id": store.id, **store.to_dict()} for store in stores]
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))

@app.get("/analytics/{store_id}")
async def get_store_analytics(store_id: str):
    try:
        analytics_ref = db.collection("analytics").where("store_id", "==", store_id)
        analytics = analytics_ref.stream()
        return [{"id": doc.id, **doc.to_dict()} for doc in analytics]
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))

@app.post("/analytics")
async def create_analytics(analytics: Analytics):
    try:
        db.collection("analytics").add(analytics.dict())
        return {"message": "Analytics data created successfully"}
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))

if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="0.0.0.0", port=8000) 