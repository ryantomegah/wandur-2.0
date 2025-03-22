from airtable import Airtable
import os
import pandas as pd
from datetime import datetime, timedelta

class AirtableClient:
    """
    Client for interacting with Airtable tables for the Wandur dashboard.
    Handles data retrieval and manipulation from Airtable bases.
    """
    
    def __init__(self, api_key, base_id):
        """
        Initialize the Airtable client
        
        Args:
            api_key (str): Airtable API key
            base_id (str): Airtable base ID
        """
        self.api_key = api_key
        self.base_id = base_id
        
        # Initialize table connections
        self.stores_table = Airtable(self.base_id, 'Stores', api_key=self.api_key)
        self.visitors_table = Airtable(self.base_id, 'Visitors', api_key=self.api_key)
        self.purchases_table = Airtable(self.base_id, 'Purchases', api_key=self.api_key)
        self.customer_segments_table = Airtable(self.base_id, 'CustomerSegments', api_key=self.api_key)
        self.heatmap_data_table = Airtable(self.base_id, 'HeatmapData', api_key=self.api_key)
        
    def get_stores(self, formula=None):
        """
        Get all stores from Airtable
        
        Args:
            formula (str, optional): Airtable formula for filtering
            
        Returns:
            list: List of store records
        """
        return self.stores_table.get_all(formula=formula)
    
    def get_store_by_id(self, store_id):
        """
        Get a store by its ID
        
        Args:
            store_id (str): The Airtable record ID for the store
            
        Returns:
            dict: Store record
        """
        return self.stores_table.get(store_id)
    
    def get_visitors_data(self, store_id=None, start_date=None, end_date=None):
        """
        Get visitor data for a store within a date range
        
        Args:
            store_id (str, optional): The Airtable record ID for the store
            start_date (str, optional): Start date in YYYY-MM-DD format
            end_date (str, optional): End date in YYYY-MM-DD format
            
        Returns:
            pandas.DataFrame: Visitor data
        """
        formula_parts = []
        
        if store_id:
            formula_parts.append(f"{{StoreID}} = '{store_id}'")
        
        if start_date:
            formula_parts.append(f"{{Date}} >= '{start_date}'")
            
        if end_date:
            formula_parts.append(f"{{Date}} <= '{end_date}'")
            
        formula = None
        if formula_parts:
            formula = "AND(" + ",".join(formula_parts) + ")"
        
        visitors = self.visitors_table.get_all(formula=formula)
        
        # Convert to pandas DataFrame for easier manipulation
        records = []
        for visitor in visitors:
            records.append({
                'id': visitor['id'],
                'store_id': visitor['fields'].get('StoreID', ''),
                'date': visitor['fields'].get('Date', ''),
                'time': visitor['fields'].get('Time', ''),
                'visitor_id': visitor['fields'].get('VisitorID', ''),
                'duration': visitor['fields'].get('Duration', 0),
                'converted': visitor['fields'].get('Converted', False)
            })
        
        if not records:
            return pd.DataFrame(columns=['id', 'store_id', 'date', 'time', 'visitor_id', 'duration', 'converted'])
        
        return pd.DataFrame(records)
    
    def get_purchases_data(self, store_id=None, start_date=None, end_date=None):
        """
        Get purchase data for a store within a date range
        
        Args:
            store_id (str, optional): The Airtable record ID for the store
            start_date (str, optional): Start date in YYYY-MM-DD format
            end_date (str, optional): End date in YYYY-MM-DD format
            
        Returns:
            pandas.DataFrame: Purchase data
        """
        formula_parts = []
        
        if store_id:
            formula_parts.append(f"{{StoreID}} = '{store_id}'")
        
        if start_date:
            formula_parts.append(f"{{Date}} >= '{start_date}'")
            
        if end_date:
            formula_parts.append(f"{{Date}} <= '{end_date}'")
            
        formula = None
        if formula_parts:
            formula = "AND(" + ",".join(formula_parts) + ")"
        
        purchases = self.purchases_table.get_all(formula=formula)
        
        # Convert to pandas DataFrame for easier manipulation
        records = []
        for purchase in purchases:
            records.append({
                'id': purchase['id'],
                'store_id': purchase['fields'].get('StoreID', ''),
                'date': purchase['fields'].get('Date', ''),
                'time': purchase['fields'].get('Time', ''),
                'visitor_id': purchase['fields'].get('VisitorID', ''),
                'amount': purchase['fields'].get('Amount', 0),
                'items': purchase['fields'].get('Items', 0)
            })
        
        if not records:
            return pd.DataFrame(columns=['id', 'store_id', 'date', 'time', 'visitor_id', 'amount', 'items'])
        
        return pd.DataFrame(records)
    
    def get_customer_segments(self, store_id=None):
        """
        Get customer segment data for a store
        
        Args:
            store_id (str, optional): The Airtable record ID for the store
            
        Returns:
            pandas.DataFrame: Customer segment data
        """
        formula = None
        if store_id:
            formula = f"{{StoreID}} = '{store_id}'"
        
        segments = self.customer_segments_table.get_all(formula=formula)
        
        # Convert to pandas DataFrame
        records = []
        for segment in segments:
            records.append({
                'id': segment['id'],
                'store_id': segment['fields'].get('StoreID', ''),
                'segment': segment['fields'].get('Segment', ''),
                'count': segment['fields'].get('Count', 0),
                'avg_spend': segment['fields'].get('AvgSpend', 0)
            })
        
        if not records:
            # Return dummy data if no real data is available
            return pd.DataFrame([
                {'segment': 'First-time', 'count': 35},
                {'segment': 'Occasional', 'count': 45},
                {'segment': 'Regular', 'count': 20},
                {'segment': 'Loyal', 'count': 10}
            ])
        
        return pd.DataFrame(records)
    
    def get_heatmap_data(self, store_id, date=None):
        """
        Get heatmap data for a store
        
        Args:
            store_id (str): The Airtable record ID for the store
            date (str, optional): Date in YYYY-MM-DD format
            
        Returns:
            dict: Heatmap data with density matrix
        """
        formula_parts = [f"{{StoreID}} = '{store_id}'"]
        
        if date:
            formula_parts.append(f"{{Date}} = '{date}'")
            
        formula = "AND(" + ",".join(formula_parts) + ")"
        
        heatmap_records = self.heatmap_data_table.get_all(formula=formula)
        
        if not heatmap_records:
            # Return dummy data if no real data is available
            import numpy as np
            # Create a 10x10 grid with random values for demonstration
            density = np.random.rand(10, 10)
            # Add some hotspots
            density[2:4, 2:4] *= 3
            density[7:9, 7:9] *= 2
            return {"density": density}
        
        # In a real implementation, you would process the actual data from Airtable
        # For now, we'll return a placeholder
        
        # Placeholder heatmap data (10x10 grid)
        density = [[0 for _ in range(10)] for _ in range(10)]
        
        # Process heatmap records to populate the density matrix
        for record in heatmap_records:
            x = record['fields'].get('X', 0)
            y = record['fields'].get('Y', 0)
            value = record['fields'].get('Density', 0)
            
            # Ensure x and y are within bounds
            x = min(max(int(x), 0), 9)
            y = min(max(int(y), 0), 9)
            
            density[y][x] = value
        
        return {"density": density}
    
    def create_record(self, table_name, fields):
        """
        Create a new record in an Airtable table
        
        Args:
            table_name (str): Name of the table
            fields (dict): Fields to insert
            
        Returns:
            dict: Created record
        """
        table = Airtable(self.base_id, table_name, api_key=self.api_key)
        return table.insert(fields)
    
    def update_record(self, table_name, record_id, fields):
        """
        Update an existing record in an Airtable table
        
        Args:
            table_name (str): Name of the table
            record_id (str): ID of the record to update
            fields (dict): Fields to update
            
        Returns:
            dict: Updated record
        """
        table = Airtable(self.base_id, table_name, api_key=self.api_key)
        return table.update(record_id, fields)
    
    def delete_record(self, table_name, record_id):
        """
        Delete a record from an Airtable table
        
        Args:
            table_name (str): Name of the table
            record_id (str): ID of the record to delete
            
        Returns:
            dict: Deleted record
        """
        table = Airtable(self.base_id, table_name, api_key=self.api_key)
        return table.delete(record_id)
    
    def generate_mock_data(self, store_id, days=30):
        """
        Generate mock data for testing purposes
        
        Args:
            store_id (str): The Airtable record ID for the store
            days (int): Number of days to generate data for
            
        Returns:
            bool: Success flag
        """
        import random
        from datetime import datetime, timedelta
        
        # Get store details
        store = self.get_store_by_id(store_id)
        if not store:
            return False
        
        store_name = store['fields'].get('Name', 'Unknown Store')
        
        # Generate visitor data
        today = datetime.now().date()
        
        for day in range(days):
            date_str = (today - timedelta(days=day)).strftime("%Y-%m-%d")
            
            # Random number of visitors for the day (20-100)
            num_visitors = random.randint(20, 100)
            
            for i in range(num_visitors):
                # Random time during the day
                hour = random.randint(9, 20)  # 9 AM to 8 PM
                minute = random.randint(0, 59)
                time_str = f"{hour:02d}:{minute:02d}"
                
                # Random duration in minutes (1-60)
                duration = random.randint(1, 60)
                
                # Random conversion (20% chance)
                converted = random.random() < 0.2
                
                # Create visitor record
                visitor_fields = {
                    'StoreID': store_id,
                    'Date': date_str,
                    'Time': time_str,
                    'VisitorID': f"V{random.randint(10000, 99999)}",
                    'Duration': duration,
                    'Converted': converted
                }
                
                visitor = self.create_record('Visitors', visitor_fields)
                
                # If converted, create a purchase record
                if converted:
                    amount = random.uniform(10, 200)
                    items = random.randint(1, 5)
                    
                    purchase_fields = {
                        'StoreID': store_id,
                        'Date': date_str,
                        'Time': time_str,
                        'VisitorID': visitor_fields['VisitorID'],
                        'Amount': round(amount, 2),
                        'Items': items
                    }
                    
                    self.create_record('Purchases', purchase_fields)
        
        # Generate customer segments
        segment_fields = [
            {
                'StoreID': store_id,
                'Segment': 'First-time',
                'Count': random.randint(30, 50),
                'AvgSpend': round(random.uniform(20, 40), 2)
            },
            {
                'StoreID': store_id,
                'Segment': 'Occasional',
                'Count': random.randint(40, 60),
                'AvgSpend': round(random.uniform(30, 60), 2)
            },
            {
                'StoreID': store_id,
                'Segment': 'Regular',
                'Count': random.randint(20, 30),
                'AvgSpend': round(random.uniform(50, 80), 2)
            },
            {
                'StoreID': store_id,
                'Segment': 'Loyal',
                'Count': random.randint(5, 15),
                'AvgSpend': round(random.uniform(70, 120), 2)
            }
        ]
        
        for fields in segment_fields:
            self.create_record('CustomerSegments', fields)
        
        # Generate heatmap data
        import numpy as np
        
        # Create a 10x10 grid with random values
        density = np.random.rand(10, 10)
        # Add some hotspots
        density[2:4, 2:4] *= 3
        density[7:9, 7:9] *= 2
        
        for y in range(10):
            for x in range(10):
                heatmap_fields = {
                    'StoreID': store_id,
                    'Date': today.strftime("%Y-%m-%d"),
                    'X': x,
                    'Y': y,
                    'Density': round(density[y][x], 2)
                }
                
                self.create_record('HeatmapData', heatmap_fields)
        
        return True 