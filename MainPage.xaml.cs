using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace TemperatureConverter
{
    public sealed partial class MainPage : Page
    {
        private const string ApiKey = "3593e7c64f5424b605b43376ba0a096c"; 

        private ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        private List<string> searchHistory = new List<string>();

        public MainPage()
        {
            this.InitializeComponent();
            LoadSearchHistory();
            ListCityView.SelectionChanged += ListCityView_SelectionChanged;
            InputCity.TextChanged += InputCity_TextChanged;
        }

        // incarca lista oraselor local
        private void LoadSearchHistory()
        {
            if (localSettings.Values["SearchHistory"] != null)
            {
                searchHistory = localSettings.Values["SearchHistory"].ToString().Split(';').ToList();
                UpdateHistoryUI();
            }
        }

        // salveaza lista oraselor local
        private void SaveSearchHistory()
        {
            localSettings.Values["SearchHistory"] = string.Join(";", searchHistory);
        }

        // afiseaza vremea pentru orasul introdus in TextBox
        private async void OnShowWeatherClicked(object sender, RoutedEventArgs e)
        {
            string city = InputCity.Text.Trim();
            var selectedCity = ListCityView.SelectedItem as string;

            if (!string.IsNullOrWhiteSpace(city))
            {
                await ShowWeather(city);
            }
            else if (!string.IsNullOrEmpty(selectedCity))
            {
                await ShowWeather(selectedCity);
            }
            else
            {
                ShowMessage("Please enter or select a city.");
            }
        }

        // adauga orasul introdus in TextBox in lista oraselor adaugate
        private async void OnAddCityClicked(object sender, RoutedEventArgs e)
        {
            string city = InputCity.Text.Trim();
            if (string.IsNullOrWhiteSpace(city))
            {
                WeatherTextBlock.Text = "Please enter a city name.";
                return;
            }
            // verifica daca orasul este deja in lista
            if (!searchHistory.Contains(city))
            {
                searchHistory.Add(city);
                SaveSearchHistory();
                UpdateHistoryUI();

                ListCityView.SelectedItem = null;
            }

            await ShowWeather(city);
        }

        // sterge orasul selectat din lista oraselor adaugate
        private void OnDeleteCityClicked(object sender, RoutedEventArgs e)
        {
            string selectedCity = ListCityView.SelectedItem as string;
            if (selectedCity != null)
            {
                searchHistory.Remove(selectedCity);
                SaveSearchHistory();
                UpdateHistoryUI();
            }
            else
            {
                ShowMessage("Select a city to delete.");
            }
        }

        // afiseaza vremea pentru orasul selectat din istoric
        private void ListCityView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListCityView.SelectedItems.Count > 1)
            {
                ShowMessage("Select one city.");
            }
            else
            {
                WeatherTextBlock.Text = string.Empty; 
            }
        }

        // sterge textul din TextBox cand se selecteaza un oras din istoric
        private void InputCity_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ListCityView.SelectedItem != null)
            {
                ListCityView.SelectedItem = null;
            }
        }
        // afiseaza vremea pentru oras
        private async System.Threading.Tasks.Task ShowWeather(string city)
        {
            try
            {
                HttpClient client = new HttpClient();
                string url = $"http://api.openweathermap.org/data/2.5/weather?q={city}&appid={ApiKey}&units=metric";
                HttpResponseMessage response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    // preia datele JSON
                    string responseBody = await response.Content.ReadAsStringAsync();
                    // deserializare JSON
                    dynamic weatherData = Newtonsoft.Json.JsonConvert.DeserializeObject(responseBody);

                    // preia datele meteo
                    string description = weatherData["weather"][0]["description"];
                    double temperature = weatherData["main"]["temp"];
                    string cityName = weatherData["name"];

                    // afiseaza vremea
                    WeatherTextBlock.Text = $"Weather in {cityName}: {description}, Temperature: {temperature}°C";
                }
                else
                {
                    WeatherTextBlock.Text = $"Error: {response.StatusCode} - {response.ReasonPhrase}";
                }
            }
            catch (Exception ex)
            {
                WeatherTextBlock.Text = $"Error: {ex.Message}";
            }
        }

        // actualizeaza istoricul oraselor  in UI
        private void UpdateHistoryUI()
        {
            ListCityView.Items.Clear();
            foreach (string city in searchHistory)
            {
                ListCityView.Items.Add(city);
            }
        }
        // afiseaza mesajul in TextBlock

        private void ShowMessage(string message)
        {
            WeatherTextBlock.Text = message;
        }
    }
}
