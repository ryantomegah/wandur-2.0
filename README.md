# Wandur 2.0

An AR-powered shopping navigation and analytics platform.

## Project Structure

```
wandur-2.0/
├── shopper-app/           # Unity-based AR navigation app
├── dashboard/            # Boutique/Mall analytics dashboard
├── backend/             # Python backend for data processing
└── docs/               # Project documentation
```

## Tech Stack

### Shopper App
- Unity + AR Foundation
- Oriient SDK (geomagnetic positioning)
- ARway.ai SDK
- Google Maps API + ARCore
- Firebase

### Dashboard
- Airtable
- Python
- Figma (UI/UX)

## Getting Started

### Prerequisites
- Unity 2022.3 LTS or later
- Python 3.8+
- Node.js 16+
- Firebase CLI

### Installation
1. Clone the repository:
```bash
git clone git@github.com:ryantomegah/wandur-2.0.git
cd wandur-2.0
```

2. Set up Unity project:
- Open Unity Hub
- Add the `shopper-app` directory as a project
- Install required packages through Unity Package Manager

3. Set up Python environment:
```bash
cd backend
python -m venv venv
source venv/bin/activate  # On Windows: venv\Scripts\activate
pip install -r requirements.txt
```

4. Set up Firebase:
```bash
firebase login
firebase init
```

## Development Workflow

1. AR Navigation Development
   - Unity project setup
   - Oriient SDK integration
   - Divine Lines implementation
   - Geofencing setup

2. Dashboard Development
   - Airtable setup
   - Mock data generation
   - Analytics visualization

3. Backend Development
   - Data processing scripts
   - AI recommendation engine
   - API endpoints

## Contributing

1. Create a feature branch
2. Make your changes
3. Submit a pull request

## License

This project is proprietary and confidential.
