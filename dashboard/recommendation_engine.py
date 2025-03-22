import pandas as pd
import numpy as np
from datetime import datetime, timedelta
import random

class RecommendationEngine:
    """
    Generates AI-powered recommendations for stores based on analytics data.
    """
    
    def __init__(self, airtable_client):
        """
        Initialize the recommendation engine
        
        Args:
            airtable_client: AirtableClient instance
        """
        self.airtable = airtable_client
    
    def get_recommendations(self, store_id):
        """
        Generate recommendations for a store
        
        Args:
            store_id (str): The Airtable record ID for the store
            
        Returns:
            list: List of recommendation objects
        """
        # In a real implementation, this would analyze data and generate
        # personalized recommendations based on trends and patterns.
        # For now, we'll return placeholder recommendations
        
        # Get store details
        store = self.airtable.get_store_by_id(store_id)
        store_name = store['fields'].get('Name', 'your store') if store else 'your store'
        
        # Get recent analytics data
        end_date = datetime.now().strftime("%Y-%m-%d")
        start_date = (datetime.now() - timedelta(days=30)).strftime("%Y-%m-%d")
        
        visitors_df = self.airtable.get_visitors_data(store_id, start_date, end_date)
        purchases_df = self.airtable.get_purchases_data(store_id, start_date, end_date)
        
        # List of potential recommendations
        potential_recommendations = self._get_potential_recommendations(store_name)
        
        # Select 3-5 recommendations
        num_recommendations = random.randint(3, 5)
        selected_recommendations = []
        
        # If we have real data, try to make data-driven recommendations
        if len(visitors_df) > 0 and len(purchases_df) > 0:
            # Calculate average dwell time
            avg_dwell_time = visitors_df['duration'].mean()
            
            # Calculate conversion rate
            conversion_rate = len(visitors_df[visitors_df['converted'] == True]) / len(visitors_df) * 100
            
            # Calculate average purchase amount
            avg_purchase = purchases_df['amount'].mean()
            
            # Prioritize recommendations based on metrics
            if avg_dwell_time < 10:
                # Low dwell time - suggest engagement improvements
                selected_recommendations.append(self._get_recommendation_by_category(potential_recommendations, 'engagement'))
            
            if conversion_rate < 20:
                # Low conversion - suggest conversion improvements
                selected_recommendations.append(self._get_recommendation_by_category(potential_recommendations, 'conversion'))
            
            if avg_purchase < 50:
                # Low purchase value - suggest upselling
                selected_recommendations.append(self._get_recommendation_by_category(potential_recommendations, 'upsell'))
            
            # Fill remaining slots with random recommendations
            remaining_slots = num_recommendations - len(selected_recommendations)
            for _ in range(remaining_slots):
                # Exclude categories we've already used
                used_categories = [rec['category'] for rec in selected_recommendations]
                available_recommendations = [rec for rec in potential_recommendations if rec['category'] not in used_categories]
                
                if available_recommendations:
                    selected_recommendations.append(random.choice(available_recommendations))
                else:
                    # If we've used all categories, just pick random ones
                    selected_recommendations.append(random.choice(potential_recommendations))
        else:
            # No real data, just pick random recommendations
            selected_recommendations = random.sample(potential_recommendations, num_recommendations)
        
        # Set impact scores
        for rec in selected_recommendations:
            rec['impact_score'] = random.randint(6, 10)
        
        return selected_recommendations
    
    def _get_recommendation_by_category(self, recommendations, category):
        """
        Get a recommendation by category
        
        Args:
            recommendations (list): List of recommendation objects
            category (str): Category to filter by
            
        Returns:
            dict: Recommendation object
        """
        category_recommendations = [rec for rec in recommendations if rec['category'] == category]
        
        if category_recommendations:
            return random.choice(category_recommendations)
        else:
            # Fallback to any recommendation
            return random.choice(recommendations)
    
    def _get_potential_recommendations(self, store_name):
        """
        Get a list of potential recommendations
        
        Args:
            store_name (str): Name of the store
            
        Returns:
            list: List of recommendation objects
        """
        recommendations = [
            # Engagement recommendations
            {
                'title': 'Optimize Store Layout',
                'description': f'Analysis shows customers spend less time in the north section of {store_name}. Consider rearranging displays or adding attention-grabbing elements.',
                'category': 'engagement'
            },
            {
                'title': 'Interactive Product Displays',
                'description': f'Adding interactive elements to key product displays could increase engagement time by up to 40% based on industry benchmarks.',
                'category': 'engagement'
            },
            {
                'title': 'Staff Engagement Training',
                'description': f'Customer interaction data suggests that personalized greetings and product information sharing could significantly boost engagement at {store_name}.',
                'category': 'engagement'
            },
            
            # Conversion recommendations
            {
                'title': 'Optimize Checkout Process',
                'description': f'Data indicates some customers abandon purchases at checkout. Streamlining the process could improve conversion rates at {store_name}.',
                'category': 'conversion'
            },
            {
                'title': 'Strategic Product Placement',
                'description': f'Moving high-interest items to areas with the highest foot traffic could increase conversions by 15-25% at {store_name}.',
                'category': 'conversion'
            },
            {
                'title': 'Limited-Time Offers',
                'description': f'Implementing time-sensitive promotions could create urgency and boost conversion rates, especially during peak hours (12-2pm and 5-7pm).',
                'category': 'conversion'
            },
            
            # Upselling recommendations
            {
                'title': 'Bundle Popular Items',
                'description': f'Creating product bundles with your top-selling items could increase average transaction value by 20-30%.',
                'category': 'upsell'
            },
            {
                'title': 'Premium Product Spotlight',
                'description': f'Highlighting premium products near popular items could increase their visibility and sales potential at {store_name}.',
                'category': 'upsell'
            },
            {
                'title': 'Staff Upselling Training',
                'description': f'Training staff to suggest complementary products could significantly increase average purchase value at {store_name}.',
                'category': 'upsell'
            },
            
            # Loyalty recommendations
            {
                'title': 'Launch Loyalty Program',
                'description': f'Based on repeat visitor data, implementing a loyalty program could increase customer retention by 25-40% at {store_name}.',
                'category': 'loyalty'
            },
            {
                'title': 'Personalized Offers',
                'description': f'Sending personalized offers to repeat customers based on their purchase history could significantly increase return visits.',
                'category': 'loyalty'
            },
            
            # Marketing recommendations
            {
                'title': 'Social Media Showcase',
                'description': f'Creating Instagram-worthy product displays or areas could increase social sharing and attract new customers to {store_name}.',
                'category': 'marketing'
            },
            {
                'title': 'In-Mall Advertising',
                'description': f'Data shows many first-time visitors discover {store_name} through in-mall advertising. Increasing this presence could boost new customer acquisition.',
                'category': 'marketing'
            },
            {
                'title': 'Targeted Geofenced Ads',
                'description': f'Using the Wandur app\'s geofenced advertising feature to target potential customers near {store_name} could increase walk-ins by up to 30%.',
                'category': 'marketing'
            },
            
            # Operational recommendations
            {
                'title': 'Adjust Staffing Levels',
                'description': f'Foot traffic analysis suggests {store_name} needs additional staff during peak hours (12-2pm and 5-7pm) to maintain service quality.',
                'category': 'operations'
            },
            {
                'title': 'Optimize Opening Hours',
                'description': f'Visitor data indicates potential for increased revenue by extending hours on weekends at {store_name}.',
                'category': 'operations'
            }
        ]
        
        return recommendations
    
    def get_recommendation_impact(self, recommendation_id):
        """
        Calculate the potential impact of a recommendation
        
        Args:
            recommendation_id (str): ID of the recommendation
            
        Returns:
            dict: Impact metrics
        """
        # In a real implementation, this would provide detailed impact analysis
        # For now, we'll return placeholder metrics
        return {
            'revenue_increase': f"{random.randint(5, 25)}%",
            'customer_satisfaction': f"+{random.randint(5, 15)} points",
            'implementation_difficulty': random.choice(['Low', 'Medium', 'High']),
            'expected_timeframe': f"{random.randint(1, 6)} months"
        } 