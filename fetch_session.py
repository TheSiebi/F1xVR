import os
import wget
import requests
import csv

# Usage: Input session_key, run
# Output: ./session_key/driver_num.csv, starting from position (0,0)
#         ./session_key/session.csv
#         ./session_key/drivers.csv
# Could be improved to remove until race start_time, but this part is anyway done in process_traj.py

# Define the store_path and session_key
session_key_list = ["9157", "9129", "9133", "9098", "9102", "9169", "9173", "7783", "7787", "9153"]

def create_folder_if_not_exists(folder_path):
    if not os.path.exists(folder_path):
        os.makedirs(folder_path)
        print(f"Folder created: {folder_path}")

def download_session_meta(store_path, session_key):
    # Construct the URL
    url_session_data = f"https://api.openf1.org/v1/sessions?session_key={session_key}&csv=true"

    # Download the file to the default download folder
    downloaded_file = wget.download(url_session_data)

    # Define the new file path and name
    new_file_path = os.path.join(store_path, "session.csv")

    # Check if the file already exists in the destination directory
    if os.path.exists(new_file_path):
        # Remove the existing file
        os.remove(new_file_path)

    # Move the downloaded file to the specified store_path and rename it to "session.csv"
    os.rename(downloaded_file, new_file_path)

    print(f"File downloaded and moved to {new_file_path}.")


def download_car_location(driver_number, store_path, session_key):
    # Construct the URL for car data
    url_car_data = f"https://api.openf1.org/v1/location?session_key={session_key}&driver_number={driver_number}&csv=true"

    # Download the file to the default download folder
    downloaded_file = wget.download(url_car_data)

    # Define the new file path and name
    new_file_path = os.path.join(store_path, f"{driver_number}.csv")

    # Check if the file already exists in the destination directory
    if os.path.exists(new_file_path):
        # Remove the existing file
        os.remove(new_file_path)

    # Move the downloaded file to the specified store_path and rename it to "{driver_number}.csv"
    os.rename(downloaded_file, new_file_path)

    print(f"File downloaded and moved to {new_file_path}.")

def get_all_drivers(session_key):
    url_drivers = f"https://api.openf1.org/v1/drivers?session_key={session_key}"
    
    # Make a request to get driver data
    response = requests.get(url_drivers)
    
    # Check if the request was successful (status code 200)
    if response.status_code == 200:
        # Parse the JSON response
        driver_data = response.json()
        
        # Extract all driver numbers from the response
        driver_numbers = [driver['driver_number'] for driver in driver_data]
        
        return driver_numbers
    else:
        print("Failed to fetch driver data")
        return []

def download_driver_names(store_path, session_key):
    driver_numbers = get_all_drivers(session_key)
    driver_names = []
    for driver in driver_numbers:
        url_driver = f"https://api.openf1.org/v1/drivers?driver_number={driver}"
        response = requests.get(url_driver)
    
        # Check if the request was successful (status code 200)
        if response.status_code == 200:
            # Parse the JSON response
            driver_data = response.json()
            
            # Extract all driver numbers from the response
            driver_name = driver_data[0]['broadcast_name']
            driver_team = driver_data[0]['team_name']
            driver_names.append((driver, driver_name, driver_team))
        else:
            print("Failed to fetch driver data")

    new_file_path = os.path.join(store_path, "drivers.csv")

    # Check if the file already exists in the destination directory
    if os.path.exists(new_file_path):
        # Remove the existing file
        os.remove(new_file_path)

    with open(new_file_path, mode='w', newline='') as file:
        writer = csv.writer(file)
        writer.writerow(['driver_number', 'driver_name', 'team_name'])
        writer.writerows(driver_names)


if __name__ == "__main__":
    for session_key in session_key_list:
        store_path = f'./{session_key}/'
        create_folder_if_not_exists(store_path)
        download_session_meta(store_path, session_key)
        for driver in get_all_drivers(session_key):
            download_car_location(driver, store_path, session_key)
        driver_number_names = download_driver_names(store_path, session_key)