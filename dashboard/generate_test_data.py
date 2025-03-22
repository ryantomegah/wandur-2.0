#!/usr/bin/env python3
"""
Script to generate test data in Airtable for the Wandur dashboard.
"""

import os
import sys
import argparse
from dotenv import load_dotenv
from airtable_client import AirtableClient

def create_test_store(airtable_client, store_name, store_type="Boutique"):
    """
    Create a test store in Airtable
    
    Args:
        airtable_client: AirtableClient instance
        store_name (str): Name of the store
        store_type (str): Type of store (Boutique, Restaurant, etc.)
        
    Returns:
        dict: Created store record
    """
    store_fields = {
        'Name': store_name,
        'Type': store_type,
        'Location': 'Test Mall',
        'FloorLevel': '1',
        'Size': 'Medium',
        'OpeningHours': '9:00 AM - 9:00 PM',
        'Description': f'Test store for {store_name}'
    }
    
    return airtable_client.create_record('Stores', store_fields)

def main():
    # Parse arguments
    parser = argparse.ArgumentParser(description='Generate test data for Wandur dashboard')
    parser.add_argument('--store-name', type=str, default='Test Store', help='Name of the test store')
    parser.add_argument('--store-type', type=str, default='Boutique', help='Type of the test store')
    parser.add_argument('--days', type=int, default=30, help='Number of days to generate data for')
    parser.add_argument('--env-file', type=str, default='.env', help='Path to .env file')
    
    args = parser.parse_args()
    
    # Load environment variables
    load_dotenv(args.env_file)
    
    # Get Airtable credentials
    airtable_api_key = os.getenv('AIRTABLE_API_KEY')
    airtable_base_id = os.getenv('AIRTABLE_BASE_ID')
    
    if not airtable_api_key or not airtable_base_id:
        print("Error: Airtable credentials not found in .env file")
        sys.exit(1)
    
    # Initialize Airtable client
    airtable = AirtableClient(airtable_api_key, airtable_base_id)
    
    print(f"Generating test data for {args.store_name} over {args.days} days...")
    
    # Create test store
    store = create_test_store(airtable, args.store_name, args.store_type)
    store_id = store['id']
    
    print(f"Created test store with ID: {store_id}")
    
    # Generate mock data
    print("Generating mock visitor and purchase data...")
    success = airtable.generate_mock_data(store_id, days=args.days)
    
    if success:
        print("Test data generation completed successfully!")
        print("\nYou can now run the dashboard app to view the data.")
        print("Use the following store ID in the dashboard: " + store_id)
    else:
        print("Error generating test data")
        sys.exit(1)

if __name__ == "__main__":
    main() 