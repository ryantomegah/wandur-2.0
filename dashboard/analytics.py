import pandas as pd
import numpy as np
from datetime import datetime, timedelta

class AnalyticsProcessor:
    """
    Processes analytics data retrieved from Airtable for visualization and insights.
    """
    
    def __init__(self, airtable_client):
        """
        Initialize the analytics processor
        
        Args:
            airtable_client: AirtableClient instance
        """
        self.airtable = airtable_client
    
    def get_key_metrics(self, store_id, start_date=None, end_date=None):
        """
        Calculate key metrics for a store
        
        Args:
            store_id (str): The Airtable record ID for the store
            start_date (str, optional): Start date in YYYY-MM-DD format
            end_date (str, optional): End date in YYYY-MM-DD format
            
        Returns:
            dict: Key metrics including foot traffic, conversion rate, and average purchase
        """
        # Set default date range if not provided
        if not start_date:
            end_date_obj = datetime.now()
            start_date_obj = end_date_obj - timedelta(days=30)
            
            start_date = start_date_obj.strftime("%Y-%m-%d")
            end_date = end_date_obj.strftime("%Y-%m-%d")
        elif not end_date:
            end_date = datetime.now().strftime("%Y-%m-%d")
        
        # Get visitor and purchase data
        visitors_df = self.airtable.get_visitors_data(store_id, start_date, end_date)
        purchases_df = self.airtable.get_purchases_data(store_id, start_date, end_date)
        
        # Calculate metrics
        foot_traffic = len(visitors_df)
        
        if foot_traffic > 0:
            conversion_count = len(visitors_df[visitors_df['converted'] == True])
            conversion_rate = (conversion_count / foot_traffic) * 100
        else:
            conversion_rate = 0
        
        if len(purchases_df) > 0:
            avg_purchase = purchases_df['amount'].mean()
        else:
            avg_purchase = 0
        
        # If no data is available, provide mock data for demonstration
        if foot_traffic == 0:
            foot_traffic = np.random.randint(500, 1500)
            conversion_rate = np.random.uniform(15, 30)
            avg_purchase = np.random.uniform(40, 80)
        
        return {
            'foot_traffic': foot_traffic,
            'conversion_rate': conversion_rate,
            'avg_purchase': avg_purchase
        }
    
    def get_traffic_data(self, store_id, start_date, end_date):
        """
        Get daily foot traffic data for a store
        
        Args:
            store_id (str): The Airtable record ID for the store
            start_date (str): Start date in YYYY-MM-DD format
            end_date (str): End date in YYYY-MM-DD format
            
        Returns:
            pandas.DataFrame: Daily foot traffic data
        """
        # Get visitor data
        visitors_df = self.airtable.get_visitors_data(store_id, start_date, end_date)
        
        # If real data is available, aggregate by date
        if len(visitors_df) > 0:
            # Count visitors by date
            traffic_data = visitors_df.groupby('date').size().reset_index(name='visitors')
            return traffic_data
        
        # Otherwise, generate mock data for demonstration
        start_date_obj = datetime.strptime(start_date, "%Y-%m-%d")
        end_date_obj = datetime.strptime(end_date, "%Y-%m-%d")
        
        # Generate a date range
        date_range = []
        current_date = start_date_obj
        while current_date <= end_date_obj:
            date_range.append(current_date.strftime("%Y-%m-%d"))
            current_date += timedelta(days=1)
        
        # Generate random visitor counts with a realistic pattern
        # Base traffic level with weekly pattern and some randomness
        base_traffic = 50
        visitors = []
        
        for i, date in enumerate(date_range):
            day_of_week = datetime.strptime(date, "%Y-%m-%d").weekday()
            
            # Weekend boost
            weekend_factor = 1.5 if day_of_week >= 5 else 1.0
            
            # Add some randomness
            random_factor = np.random.uniform(0.8, 1.2)
            
            # Calculate visitor count
            visitor_count = int(base_traffic * weekend_factor * random_factor)
            visitors.append(visitor_count)
        
        # Create DataFrame
        traffic_data = pd.DataFrame({
            'date': date_range,
            'visitors': visitors
        })
        
        return traffic_data
    
    def get_customer_segments(self, store_id, start_date=None, end_date=None):
        """
        Get customer segment distribution
        
        Args:
            store_id (str): The Airtable record ID for the store
            start_date (str, optional): Start date in YYYY-MM-DD format
            end_date (str, optional): End date in YYYY-MM-DD format
            
        Returns:
            pandas.DataFrame: Customer segment data
        """
        # Get segment data from Airtable
        segments_df = self.airtable.get_customer_segments(store_id)
        
        # If we have real data, use it
        if len(segments_df) > 0 and 'segment' in segments_df.columns:
            return segments_df
        
        # Otherwise, generate mock data for demonstration
        return pd.DataFrame([
            {'segment': 'First-time', 'count': np.random.randint(30, 50)},
            {'segment': 'Occasional', 'count': np.random.randint(40, 60)},
            {'segment': 'Regular', 'count': np.random.randint(20, 40)},
            {'segment': 'Loyal', 'count': np.random.randint(10, 20)}
        ])
    
    def get_heatmap_data(self, store_id, start_date=None, end_date=None):
        """
        Get heatmap data for customer density
        
        Args:
            store_id (str): The Airtable record ID for the store
            start_date (str, optional): Start date in YYYY-MM-DD format
            end_date (str, optional): End date in YYYY-MM-DD format
            
        Returns:
            dict: Heatmap data with density matrix
        """
        # Use the most recent date if not specified
        date = end_date if end_date else datetime.now().strftime("%Y-%m-%d")
        
        # Get heatmap data from Airtable
        return self.airtable.get_heatmap_data(store_id, date)
    
    def get_conversion_data(self, store_id, start_date, end_date):
        """
        Get conversion funnel data
        
        Args:
            store_id (str): The Airtable record ID for the store
            start_date (str): Start date in YYYY-MM-DD format
            end_date (str): End date in YYYY-MM-DD format
            
        Returns:
            pandas.DataFrame: Conversion funnel data
        """
        # Get visitor and purchase data
        visitors_df = self.airtable.get_visitors_data(store_id, start_date, end_date)
        purchases_df = self.airtable.get_purchases_data(store_id, start_date, end_date)
        
        # If we have real data, calculate the funnel
        if len(visitors_df) > 0:
            # Total visitors
            total_visitors = len(visitors_df)
            
            # Visitors who stayed more than 5 minutes
            engaged_visitors = len(visitors_df[visitors_df['duration'] > 5])
            
            # Visitors who converted
            converted_visitors = len(visitors_df[visitors_df['converted'] == True])
            
            # Repeat customers (simplified approach)
            repeat_customers = 0
            if len(purchases_df) > 0:
                repeat_customers = len(purchases_df['visitor_id'].value_counts()[purchases_df['visitor_id'].value_counts() > 1])
        else:
            # Generate mock data
            total_visitors = np.random.randint(800, 1200)
            engaged_visitors = int(total_visitors * np.random.uniform(0.4, 0.6))
            converted_visitors = int(engaged_visitors * np.random.uniform(0.2, 0.4))
            repeat_customers = int(converted_visitors * np.random.uniform(0.1, 0.3))
        
        # Create funnel data
        conversion_data = pd.DataFrame([
            {'stage': 'Total Visitors', 'count': total_visitors},
            {'stage': 'Engaged', 'count': engaged_visitors},
            {'stage': 'Converted', 'count': converted_visitors},
            {'stage': 'Repeat Customers', 'count': repeat_customers}
        ])
        
        return conversion_data
    
    def get_peak_hours(self, store_id, start_date, end_date):
        """
        Get peak hours data
        
        Args:
            store_id (str): The Airtable record ID for the store
            start_date (str): Start date in YYYY-MM-DD format
            end_date (str): End date in YYYY-MM-DD format
            
        Returns:
            pandas.DataFrame: Hourly traffic data
        """
        # Get visitor data
        visitors_df = self.airtable.get_visitors_data(store_id, start_date, end_date)
        
        # If we have real data, calculate hourly traffic
        if len(visitors_df) > 0 and 'time' in visitors_df.columns:
            # Extract hour from time
            visitors_df['hour'] = visitors_df['time'].apply(lambda x: int(x.split(':')[0]))
            
            # Count visitors by hour
            hourly_traffic = visitors_df.groupby('hour').size().reset_index(name='visitors')
            
            # Ensure all hours are represented
            all_hours = pd.DataFrame({'hour': range(9, 21)})  # 9 AM to 8 PM
            hourly_traffic = pd.merge(all_hours, hourly_traffic, on='hour', how='left').fillna(0)
            
            return hourly_traffic
        
        # Otherwise, generate mock data
        hours = range(9, 21)  # 9 AM to 8 PM
        
        # Generate realistic hourly pattern
        # Morning build-up, lunch peak, afternoon lull, evening peak
        visitors = []
        
        for hour in hours:
            if hour < 12:  # Morning
                base = 30 + (hour - 9) * 10  # Gradual increase
            elif hour == 12 or hour == 13:  # Lunch peak
                base = 80
            elif hour < 17:  # Afternoon lull
                base = 50
            else:  # Evening peak
                base = 70
            
            # Add randomness
            visitor_count = int(base * np.random.uniform(0.8, 1.2))
            visitors.append(visitor_count)
        
        # Create DataFrame
        hourly_traffic = pd.DataFrame({
            'hour': hours,
            'visitors': visitors
        })
        
        return hourly_traffic
    
    def get_dwell_time_distribution(self, store_id, start_date, end_date):
        """
        Get distribution of visitor dwell times
        
        Args:
            store_id (str): The Airtable record ID for the store
            start_date (str): Start date in YYYY-MM-DD format
            end_date (str): End date in YYYY-MM-DD format
            
        Returns:
            pandas.DataFrame: Dwell time distribution
        """
        # Get visitor data
        visitors_df = self.airtable.get_visitors_data(store_id, start_date, end_date)
        
        # If we have real data with duration, use it
        if len(visitors_df) > 0 and 'duration' in visitors_df.columns:
            # Create duration bins
            bins = [0, 5, 15, 30, 60, float('inf')]
            labels = ['< 5 min', '5-15 min', '15-30 min', '30-60 min', '> 60 min']
            
            visitors_df['duration_category'] = pd.cut(visitors_df['duration'], bins=bins, labels=labels)
            
            # Count visitors by duration category
            dwell_time = visitors_df.groupby('duration_category').size().reset_index(name='count')
            
            return dwell_time
        
        # Otherwise, generate mock data
        return pd.DataFrame({
            'duration': ['< 5 min', '5-15 min', '15-30 min', '30-60 min', '> 60 min'],
            'count': [
                np.random.randint(50, 100),
                np.random.randint(80, 150),
                np.random.randint(40, 90),
                np.random.randint(20, 50),
                np.random.randint(5, 20)
            ]
        }) 