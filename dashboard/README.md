# Wandur Dashboard

A web-based analytics dashboard for boutiques and malls using the Wandur AR shopping platform. The dashboard integrates with Airtable to store and analyze customer data, visualize foot traffic patterns, and provide AI-powered recommendations for improving store performance.

## Features

- **Real-time Analytics**: Track foot traffic, engagement, and conversion rates
- **Interactive Visualizations**: View data trends with interactive charts and graphs
- **Heatmap Analysis**: Visualize customer density within stores
- **Conversion Funnel**: Understand the customer journey from entrance to purchase
- **AI Recommendations**: Receive data-driven suggestions for improving store performance
- **Airtable Integration**: Seamlessly store and retrieve data using Airtable as a backend

## Installation

### Prerequisites

- Python 3.8 or higher
- Airtable account with API access
- Pip package manager

### Setup

1. Clone the repository (if not already done):
   ```
   git clone https://github.com/yourusername/wandur-2.0.git
   cd wandur-2.0/dashboard
   ```

2. Create a virtual environment:
   ```
   python -m venv venv
   ```

3. Activate the virtual environment:
   - On Windows:
     ```
     .\venv\Scripts\activate
     ```
   - On macOS/Linux:
     ```
     source venv/bin/activate
     ```

4. Install dependencies:
   ```
   pip install -r requirements.txt
   ```

5. Set up your Airtable:
   - Create a new Airtable base with the following tables:
     - Stores (Name, Type, Location, FloorLevel, Size, OpeningHours, Description)
     - Visitors (StoreID, Date, Time, VisitorID, Duration, Converted)
     - Purchases (StoreID, Date, Time, VisitorID, Amount, Items)
     - CustomerSegments (StoreID, Segment, Count, AvgSpend)
     - HeatmapData (StoreID, Date, X, Y, Density)

6. Set up environment variables:
   - Copy the .env.example file to .env:
     ```
     cp .env.example .env
     ```
   - Edit the .env file and fill in your Airtable API key and Base ID

## Usage

### Running the Dashboard

1. Make sure your virtual environment is activated
2. Start the dashboard:
   ```
   python app.py
   ```
3. Open your browser and go to http://localhost:8050

### Generating Test Data

To populate your Airtable with test data for demonstration purposes:

```
python generate_test_data.py --store-name "My Test Store" --days 30
```

This will create a test store in your Airtable and generate 30 days of simulated visitor and purchase data.

## Dashboard Sections

- **Store Selection**: Choose which store to analyze
- **Date Range**: Select the time period for data analysis
- **Key Metrics**: View at-a-glance performance metrics
- **Foot Traffic**: Track visitor counts over time
- **Customer Distribution**: Analyze customer segments
- **AI Recommendations**: Get smart suggestions for store improvements
- **Heatmap**: Visualize customer density within the store
- **Conversion Analysis**: Track the customer journey funnel

## Airtable Integration

The dashboard uses Airtable as its database. The integration allows for:

- Storing visitor tracking data
- Recording purchase information
- Organizing customer segments
- Mapping customer movement patterns
- Generating and storing AI recommendations

## Development

### Project Structure

- `app.py` - Main Dash application
- `airtable_client.py` - Client for Airtable API interaction
- `analytics.py` - Data processing and analytics
- `recommendation_engine.py` - AI recommendation generation
- `generate_test_data.py` - Utility for creating test data
- `requirements.txt` - Python dependencies

### Adding New Features

To add new visualizations or analytics:

1. Create any required data processing functions in `analytics.py`
2. Add new UI components to the layout in `app.py`
3. Create callback functions to update the UI based on user interactions

## License

This project is proprietary and confidential. Unauthorized copying, distribution, or use is strictly prohibited. 