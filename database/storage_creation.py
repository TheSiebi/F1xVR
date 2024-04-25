# This file sets up a MySQL database, downloads country data, and stores it in the database.

# We need the location data and for that we need to know the meeting key and the session key and the driver number. 
# We therefore need to create three databases: location, meeting, and session. 

# From the year and the location, we can get the meeting key. With the meeting key we can look for the relevant session in the session database. Then we can get relevant location data.
# Should probably query and store. 

from urllib.request import urlopen
import json
import pandas as pd
from sqlalchemy import create_engine
import mysql.connector
import os

countries = ['Bahrain','Saudi Arabia','Australia','Azerbaijan','United States','Monaco','Spain','Canada','Austria','Great Britain','Hungary','Belgium','Netherlands','Italy','Singapore','Japan','Qatar','Mexico','Brazil','United Arab Emirates']

if not os.path.exists('sessions.csv'):

    df_session = pd.DataFrame()
    for country in countries:
        if ' ' in country:
            country = country.replace(' ','%20')
        response = urlopen(f'https://api.openf1.org/v1/sessions?country_name={country}&year=2023')
        data = json.loads(response.read().decode('utf-8'))

        df_session = pd.concat([df_session, pd.DataFrame(data)], ignore_index=True)

    # Save as CSV file
    df_session.to_csv('sessions.csv', index=True)

else:
    df_session = pd.read_csv('sessions.csv')


# Get location data based on interested race.

interest = 'Singapore'

# join dataframes on 
# result = pd.merge(df_meeting, df_session, on='country_code', how='inner')
filtered_data = df_session[(df_session['session_name'] == 'Race') & (df_session['circuit_short_name'] == interest)]

session_key = filtered_data['session_key'].values[0]

print(session_key)
