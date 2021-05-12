using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WeatherApp.Helper;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using WeatherApp.Models;
using Xamarin.Essentials;

namespace WeatherApp.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CurrentWeatherPage : ContentPage
    {
        public CurrentWeatherPage()
        {
            InitializeComponent();
            GetCoordiantes();
        }

        private string Location { get; set; }
        public double Latitude { get; set; }   //широта
        public double Longitude { get; set; }   //долгота

        private async void GetCoordiantes()
        {
            try
            {
                var request = new GeolocationRequest(GeolocationAccuracy.Best);
                var location = await Geolocation.GetLocationAsync(request);

                if (location != null)
                {
                    Latitude = location.Latitude;
                    Longitude = location.Longitude;
                    Location = await GetCity(location);

                    GetWeatherInfo();
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("GetGoordinates", ex.Message, "OK");
            }
        }

        private async Task<string> GetCity(Location location)
        {
            var places = await Geocoding.GetPlacemarksAsync(location);
            var currentPlace = places?.FirstOrDefault(); //? - if not null

            if (currentPlace != null)
            {
                return $"{currentPlace.Locality},{currentPlace.CountryName}";
            }

            return null;
        }

        private async void GetGackground()
        {
            var url = $"https://api.pexels.com/v1/search?query={Location}&per_page=15&page=1";

            var result = await ApiCaller.Get(url, "563492ad6f917000010000016cbc65bf4f724840ab22e86ed18b2c15");

            if (result.Successfull)
            {
                try
                {
                    var bgInfo = JsonConvert.DeserializeObject<BackgroundInfo>(result.Response);

                    if (bgInfo != null && bgInfo.photos.Length > 0)
                    {
                        bgImg.Source = ImageSource.FromUri(new Uri(bgInfo.photos[new Random().Next(0,bgInfo.photos.Length - 1)].src.medium));
                    }
                }
                catch(Exception ex)
                {
                    await DisplayAlert("GetBackground", ex.Message, "OK");
                }
            }
        }

        private async void GetWeatherInfo()
        {
            var url = $"http://api.openweathermap.org/data/2.5/weather?q={Location}&appid=5b86fb217c404d4545d8ee4841309a30&units=metric";

            var result = await ApiCaller.Get(url);
            
            if (result.Successfull)
            { 
                try
                {
                    var weatherInfo = JsonConvert.DeserializeObject<WeatherInfo>(result.Response);
                    descriptionTxt.Text = weatherInfo.weather[0].description.ToUpper();
                    iconImg.Source = $"w{weatherInfo.weather[0].icon}";
                    cityTxt.Text = weatherInfo.name.ToUpper();
                    temperatureTxt.Text = weatherInfo.main.temp.ToString("0");
                    humidityTxt.Text = $"{weatherInfo.main.humidity}%";
                    pressureTxt.Text = $"{weatherInfo.main.pressure} гПа";
                    windTxt.Text = $"{weatherInfo.wind.speed} м/с";
                    cloudinessTxt.Text = $"{weatherInfo.clouds.all}%";

                    var dt = new DateTime().ToUniversalTime().AddSeconds(weatherInfo.dt);
                    dateTxt.Text = dt.ToString("dddd, MMM dd").ToUpper();

                    GetForecastInfo();
                    GetGackground();
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Weather Info", ex.Message, "OK");
                }
            }
            else
            {
                await DisplayAlert("Weather Info", "No weather information found", "OK");
            }
        }

        private async void GetForecastInfo()
        {
            var url = $"http://api.openweathermap.org/data/2.5/forecast?q={Location}&appid=5b86fb217c404d4545d8ee4841309a30&units=metric";
            var result = await ApiCaller.Get(url);

            if (result.Successfull)
            {
                try
                {
                    var forecastInfo = JsonConvert.DeserializeObject<ForecastInfo>(result.Response);

                    List<List> allList = new List<List>();

                    foreach (var list in forecastInfo.list)
                    {
                        var date = DateTime.Parse(list.dt_txt);

                        if (date > DateTime.Now && date.Hour == 0 && date.Minute == 0 && date.Second == 0)
                        {
                            allList.Add(list);
                        }
                    }

                    dayOneTxt.Text = DateTime.Parse(allList[0].dt_txt).ToString("dddd");
                    dateOneTxt.Text = DateTime.Parse(allList[0].dt_txt).ToString("dd MMM");
                    iconOneImg.Source = $"w{allList[0].weather[0].icon}";
                    tempOneTxt.Text = allList[0].main.temp.ToString("0");

                    dayTwoTxt.Text = DateTime.Parse(allList[1].dt_txt).ToString("dddd");
                    dateTwoTxt.Text = DateTime.Parse(allList[1].dt_txt).ToString("dd MMM");
                    iconTwoImg.Source = $"w{allList[1].weather[0].icon}";
                    tempTwoTxt.Text = allList[1].main.temp.ToString("0");

                    dayThreeTxt.Text = DateTime.Parse(allList[2].dt_txt).ToString("dddd");
                    dateThreeTxt.Text = DateTime.Parse(allList[2].dt_txt).ToString("dd MMM");
                    iconThreeImg.Source = $"w{allList[2].weather[0].icon}";
                    tempThreeTxt.Text = allList[0].main.temp.ToString("0");

                    dayFourTxt.Text = DateTime.Parse(allList[3].dt_txt).ToString("dddd");
                    dateFourTxt.Text = DateTime.Parse(allList[3].dt_txt).ToString("dd MMM");
                    iconFourImg.Source = $"w{allList[3].weather[0].icon}";
                    tempFourTxt.Text = allList[3].main.temp.ToString("0");
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Forecast Info", ex.Message, "OK");
                }
            }
        }
    }
}