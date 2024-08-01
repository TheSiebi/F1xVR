import pandas as pd
import os

# Usage: Input session_key, run
# Input: ./session_key/driver_num.csv
# Output: ./session_key/driver_num.csv removed invalid data until race begins
#         ./session_key/car_drivernum_interpolated.csv aligned all timestamps

# Define the directory containing CSV files
session_key_list = ["9157", "9129", "9133", "9098", "9102", "9169", "9173", "7783", "7787", "9153"]
ignore_files = ["session.csv", "drivers.csv"]

def remove_until_start(store_path, session_key):
    # Read the CSV file with the start date
    start_df = pd.read_csv(f"{store_path}session.csv")
    date_start = start_df['date_start'].iloc[0]
    if '.' not in str(date_start):
        date_start = date_start.replace('+', ".000000+")

    # Iterate over each file in the directory
    for filename in os.listdir(store_path):
        if filename in ignore_files:
            continue
        if filename.endswith(".csv"):
            file_path = os.path.join(store_path, filename)
            # Read the CSV file
            df = pd.read_csv(file_path)
            
            # Create a list to store indices of rows to be removed
            rows_to_remove = []
            
            # Iterate over rows to identify rows with dates before start date
            for idx, date in enumerate(df['date']):
                if '.' not in str(date):
                    df.at[idx, 'date'] = date.replace('+', ".000000+")  # Insert .000000 microseconds
                # Convert date strings to datetime objects for comparison
            rows_to_remove = pd.to_datetime(df['date']) < pd.to_datetime(date_start)

            # Drop rows outside the loop
            df = df[~rows_to_remove]
            df.to_csv(file_path, index=False)


def interpolate_missing(store_path, session_key):
    # Initialize a list to store DataFrames for each car trajectory
    dfs = []

    # Read all CSV files into separate DataFrames
    for filename in os.listdir(store_path):
        if filename in ignore_files:
            continue
        if filename.endswith(".csv") and filename[:-4].isdigit():
            file_path = os.path.join(store_path, filename)
            df = pd.read_csv(file_path)
            dfs.append(df)

    # Concatenate all DataFrames into a single DataFrame
    all_data = pd.concat(dfs, ignore_index=True)

    # Find the complete set of timestamps
    all_timestamps = pd.to_datetime(all_data['date']).unique()

    # Interpolate missing points for each car trajectory
    for car_number, car_data in all_data.groupby('driver_number'):
        # Convert timestamps to datetime objects
        car_data['date'] = pd.to_datetime(car_data['date'])
        # Reindex the DataFrame using all timestamps
        car_data = car_data.set_index('date').reindex(all_timestamps).reset_index()
        # Interpolate missing values
        car_data['x'] = car_data['x'].interpolate(method='linear')
        car_data['y'] = car_data['y'].interpolate(method='linear')
        car_data['z'] = car_data['z'].interpolate(method='linear')
        # Save only 'date', 'x', 'y', and 'z' columns to the CSV file
        car_filename = f"car_{car_number}_interpolated.csv"
        car_data[['date', 'x', 'y', 'z']].to_csv(os.path.join(store_path, car_filename), index=False)

    print("Interpolation and saving completed.")


if __name__ == "__main__":
    for session_key in session_key_list:
        print(f"Processing {session_key}")
        store_path = f'./{session_key}/'
        remove_until_start(store_path, session_key)
        interpolate_missing(store_path, session_key)