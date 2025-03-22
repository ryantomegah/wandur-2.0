import os
import dash
from dash import dcc, html
from dash.dependencies import Input, Output, State
import dash_bootstrap_components as dbc
import plotly.express as px
import plotly.graph_objects as go
import pandas as pd
from dotenv import load_dotenv

# Import modules
from airtable_client import AirtableClient
from analytics import AnalyticsProcessor
from recommendation_engine import RecommendationEngine

# Load environment variables
load_dotenv()

# Initialize Airtable client
airtable_api_key = os.getenv('AIRTABLE_API_KEY')
airtable_base_id = os.getenv('AIRTABLE_BASE_ID')
airtable = AirtableClient(airtable_api_key, airtable_base_id)

# Initialize analytics processor
analytics = AnalyticsProcessor(airtable)

# Initialize recommendation engine
recommendation_engine = RecommendationEngine(airtable)

# Initialize Dash app
app = dash.Dash(__name__, 
                external_stylesheets=[dbc.themes.BOOTSTRAP],
                meta_tags=[{'name': 'viewport', 'content': 'width=device-width, initial-scale=1.0'}]
               )
server = app.server
app.title = "Wandur Dashboard"

# App layout
app.layout = dbc.Container([
    dbc.Row([
        dbc.Col([
            html.H1("Wandur Dashboard", className="text-center my-4"),
            html.P("Boutique and Mall Analytics Platform", className="text-center lead mb-4")
        ], width=12)
    ]),
    
    dbc.Row([
        dbc.Col([
            dbc.Card([
                dbc.CardHeader(html.H4("Store Selection", className="card-title")),
                dbc.CardBody([
                    dcc.Dropdown(
                        id="store-selector",
                        options=[],  # Will be populated from Airtable
                        placeholder="Select a store"
                    )
                ])
            ])
        ], width=12, md=4),
        
        dbc.Col([
            dbc.Card([
                dbc.CardHeader(html.H4("Date Range", className="card-title")),
                dbc.CardBody([
                    dcc.DatePickerRange(
                        id="date-range",
                        start_date_placeholder_text="Start Date",
                        end_date_placeholder_text="End Date",
                        calendar_orientation="horizontal"
                    )
                ])
            ])
        ], width=12, md=4),
        
        dbc.Col([
            dbc.Card([
                dbc.CardHeader(html.H4("Key Metrics", className="card-title")),
                dbc.CardBody([
                    html.Div(id="key-metrics-display")
                ])
            ])
        ], width=12, md=4)
    ], className="mb-4"),
    
    dbc.Row([
        dbc.Col([
            dbc.Card([
                dbc.CardHeader(html.H4("Foot Traffic Over Time", className="card-title")),
                dbc.CardBody([
                    dcc.Graph(id="traffic-graph")
                ])
            ])
        ], width=12, lg=8),
        
        dbc.Col([
            dbc.Card([
                dbc.CardHeader(html.H4("Customer Distribution", className="card-title")),
                dbc.CardBody([
                    dcc.Graph(id="customer-distribution")
                ])
            ])
        ], width=12, lg=4)
    ], className="mb-4"),
    
    dbc.Row([
        dbc.Col([
            dbc.Card([
                dbc.CardHeader(html.H4("AI Recommendations", className="card-title")),
                dbc.CardBody([
                    html.Div(id="recommendations-display",
                             children=[
                                 html.P("Select a store to view AI-powered recommendations")
                             ])
                ])
            ])
        ], width=12)
    ], className="mb-4"),
    
    dbc.Row([
        dbc.Col([
            dbc.Card([
                dbc.CardHeader(html.H4("Heatmap: Customer Density", className="card-title")),
                dbc.CardBody([
                    dcc.Graph(id="heatmap-graph")
                ])
            ])
        ], width=12, md=6),
        
        dbc.Col([
            dbc.Card([
                dbc.CardHeader(html.H4("Conversion Analysis", className="card-title")),
                dbc.CardBody([
                    dcc.Graph(id="conversion-graph")
                ])
            ])
        ], width=12, md=6)
    ], className="mb-4"),
    
    html.Footer([
        html.P("Wandur © 2023 - AR-Powered Shopping Navigation and Analytics", 
               className="text-center text-muted")
    ])
    
], fluid=True)

# Callback to populate store selector
@app.callback(
    Output("store-selector", "options"),
    Input("store-selector", "search_value")
)
def update_store_options(search_value):
    stores = airtable.get_stores()
    return [{"label": store["fields"]["Name"], "value": store["id"]} for store in stores]

# Callback to update key metrics
@app.callback(
    Output("key-metrics-display", "children"),
    Input("store-selector", "value"),
    Input("date-range", "start_date"),
    Input("date-range", "end_date")
)
def update_key_metrics(store_id, start_date, end_date):
    if not store_id:
        return [html.P("Select a store to view metrics")]
    
    # Get metrics from analytics processor
    metrics = analytics.get_key_metrics(store_id, start_date, end_date)
    
    return [
        dbc.Row([
            dbc.Col([
                html.H5(f"{metrics['foot_traffic']:,}", className="text-center"),
                html.P("Foot Traffic", className="text-center text-muted")
            ], width=4),
            dbc.Col([
                html.H5(f"{metrics['conversion_rate']:.1f}%", className="text-center"),
                html.P("Conversion Rate", className="text-center text-muted")
            ], width=4),
            dbc.Col([
                html.H5(f"${metrics['avg_purchase']:.2f}", className="text-center"),
                html.P("Avg. Purchase", className="text-center text-muted")
            ], width=4)
        ])
    ]

# Callback to update traffic graph
@app.callback(
    Output("traffic-graph", "figure"),
    Input("store-selector", "value"),
    Input("date-range", "start_date"),
    Input("date-range", "end_date")
)
def update_traffic_graph(store_id, start_date, end_date):
    if not store_id or not start_date or not end_date:
        # Return empty figure
        return go.Figure().update_layout(
            title="Select store and date range to view traffic data"
        )
    
    # Get traffic data from analytics processor
    traffic_data = analytics.get_traffic_data(store_id, start_date, end_date)
    
    # Create figure
    fig = px.line(traffic_data, x='date', y='visitors', 
                  title="Daily Foot Traffic",
                  labels={'date': 'Date', 'visitors': 'Visitors'})
    
    fig.update_layout(
        xaxis_title="Date",
        yaxis_title="Number of Visitors",
        plot_bgcolor='rgba(0,0,0,0)',
        paper_bgcolor='rgba(0,0,0,0)',
    )
    
    return fig

# Callback to update customer distribution pie chart
@app.callback(
    Output("customer-distribution", "figure"),
    Input("store-selector", "value"),
    Input("date-range", "start_date"),
    Input("date-range", "end_date")
)
def update_customer_distribution(store_id, start_date, end_date):
    if not store_id or not start_date or not end_date:
        # Return empty figure
        return go.Figure().update_layout(
            title="Select store and date range to view customer data"
        )
    
    # Get customer segment data from analytics processor
    segment_data = analytics.get_customer_segments(store_id, start_date, end_date)
    
    # Create figure
    fig = px.pie(segment_data, values='count', names='segment', 
                 title="Customer Segments",
                 hole=0.4,
                 color_discrete_sequence=px.colors.qualitative.Pastel)
    
    fig.update_layout(
        legend_title="Segments",
        plot_bgcolor='rgba(0,0,0,0)',
        paper_bgcolor='rgba(0,0,0,0)',
    )
    
    return fig

# Callback to update AI recommendations
@app.callback(
    Output("recommendations-display", "children"),
    Input("store-selector", "value")
)
def update_recommendations(store_id):
    if not store_id:
        return [html.P("Select a store to view AI-powered recommendations")]
    
    # Get recommendations from recommendation engine
    recommendations = recommendation_engine.get_recommendations(store_id)
    
    recommendation_cards = []
    for rec in recommendations:
        card = dbc.Card([
            dbc.CardBody([
                html.H5(rec["title"], className="card-title"),
                html.P(rec["description"]),
                html.P(f"Potential Impact: {rec['impact_score']}/10", className="text-muted")
            ])
        ], className="mb-3")
        recommendation_cards.append(card)
    
    return recommendation_cards

# Callback to update heatmap
@app.callback(
    Output("heatmap-graph", "figure"),
    Input("store-selector", "value"),
    Input("date-range", "start_date"),
    Input("date-range", "end_date")
)
def update_heatmap(store_id, start_date, end_date):
    if not store_id or not start_date or not end_date:
        # Return empty figure
        return go.Figure().update_layout(
            title="Select store and date range to view heatmap"
        )
    
    # Get heatmap data from analytics processor
    heatmap_data = analytics.get_heatmap_data(store_id, start_date, end_date)
    
    # Create figure (simple placeholder for now)
    # In a real implementation, this would be an actual floor plan with heatmap overlay
    fig = go.Figure(data=go.Heatmap(
        z=heatmap_data["density"],
        colorscale='Viridis',
        showscale=True
    ))
    
    fig.update_layout(
        title="Customer Density Heatmap",
        xaxis_title="X Position (feet)",
        yaxis_title="Y Position (feet)",
        plot_bgcolor='rgba(0,0,0,0)',
        paper_bgcolor='rgba(0,0,0,0)',
    )
    
    return fig

# Callback to update conversion graph
@app.callback(
    Output("conversion-graph", "figure"),
    Input("store-selector", "value"),
    Input("date-range", "start_date"),
    Input("date-range", "end_date")
)
def update_conversion_graph(store_id, start_date, end_date):
    if not store_id or not start_date or not end_date:
        # Return empty figure
        return go.Figure().update_layout(
            title="Select store and date range to view conversion data"
        )
    
    # Get conversion data from analytics processor
    conversion_data = analytics.get_conversion_data(store_id, start_date, end_date)
    
    # Create figure
    fig = px.bar(conversion_data, x='stage', y='count', 
                 title="Customer Journey Conversion",
                 color='stage',
                 labels={'count': 'Number of Customers', 'stage': 'Journey Stage'})
    
    fig.update_layout(
        xaxis_title="Customer Journey Stage",
        yaxis_title="Number of Customers",
        plot_bgcolor='rgba(0,0,0,0)',
        paper_bgcolor='rgba(0,0,0,0)',
        showlegend=False
    )
    
    return fig

if __name__ == "__main__":
    app.run_server(debug=True) 