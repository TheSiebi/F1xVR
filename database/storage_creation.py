# We need the location data and for that we need to know the meeting key and the session key and the driver number. 
# We therefore need to create three databases: location, meeting, and session. 

# From the year and the location, we can get the meeting key. With the meeting key we can look for the relevant session in the session database. Then we can get relevant location data.
# Should probably query and store. 

from urllib.request import urlopen
import json
import pandas as pd
import os


class Event:

    location: str
    session: str
    year: str
    df_session: pd.DataFrame
    driver_list: list
    session_key: str

    def __init__(self):
        self.location = None
        self.session = None
        self.session_key = None
        self.driver_list = None
        self.year = '2023'

        # Set up session csv
        if not os.path.exists('database/sessions.csv'):
            response = urlopen(f'https://api.openf1.org/v1/sessions?&year={self.year}')
            data = json.loads(response.read().decode('utf-8'))
            self.df_session = pd.DataFrame(data)
            self.df_session.to_csv('database/sessions.csv', index=True) 

        else:
            self.df_session = pd.read_csv('database/sessions.csv')

    def promptRaceAndSession(self):
        self.location = input('Enter the location: ')
        self.session = input('Enter the type of event: ') # Options are: 'Race', 'Qualifying', 'Practice 1', 'Practice 2', 'Practice 3', 'Sprint', 'Sprint Shootout'
        filtered_data = self.df_session[(self.df_session['session_name'] == self.session) & (self.df_session['circuit_short_name'] == self.location)]
        self.session_key = filtered_data['session_key'].values[0]

        # Get the drivers that are participating in a particular session
        response = urlopen(f'https://api.openf1.org/v1/drivers?&session_key={self.session_key}')
        data = json.loads(response.read().decode('utf-8'))

        df_driver = pd.DataFrame(data)
        self.driver_list = [str(value) for value in df_driver['driver_number'].values.tolist()]

    
    def getDriverNumbers(self):
        if self.driver_list is None:
            self.promptRaceAndSession()
        return self.driver_list
    
    def getSessionKey(self):
        if self.session_key is None:
            self.promptRaceAndSession()
        return self.session_key


event = Event()

event.promptRaceAndSession()

print(event.getDriverNumbers())
print(event.getSessionKey())
