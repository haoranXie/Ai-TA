using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System.Collections.Generic;

// https://www.youtube.com/watch?v=lHzZRWtlzNM
// https://github.com/GameDevEducation/UnityTutorial_QueryLocalWeather/blob/master/Assets/Scripts/GetCurrentWeatherInfo.cs
public class WeatherInfo : AIModule
{
    public enum EPhase
    {
        NotStarted,
        GetPublicIP,
        GetGeographicData,
        GetWeatherData,

        Failed,
        Succeeded
    }

    class geoPluginResponse
    {
        [JsonProperty("geoplugin_request")] public string Request { get; set; }
        [JsonProperty("geoplugin_status")] public int Status { get; set; }
        [JsonProperty("geoplugin_delay")] public string Delay { get; set; }
        [JsonProperty("geoplugin_credit")] public string Credit { get; set; }
        [JsonProperty("geoplugin_city")] public string City { get; set; }
        [JsonProperty("geoplugin_region")] public string Region { get; set; }
        [JsonProperty("geoplugin_regionCode")] public string RegionCode { get; set; }
        [JsonProperty("geoplugin_regionName")] public string RegionName { get; set; }
        [JsonProperty("geoplugin_areaCode")] public string AreaCode { get; set; }
        [JsonProperty("geoplugin_dmaCode")] public string DMACode { get; set; }
        [JsonProperty("geoplugin_countryCode")] public string CountryCode { get; set; }
        [JsonProperty("geoplugin_countryName")] public string CountryName { get; set; }
        [JsonProperty("geoplugin_inEU")] public int InEU { get; set; }
        [JsonProperty("geoplugin_euVATrate")] public bool EUVATRate { get; set; }
        [JsonProperty("geoplugin_continentCode")] public string ContinentCode { get; set; }
        [JsonProperty("geoplugin_continentName")] public string ContinentName { get; set; }
        [JsonProperty("geoplugin_latitude")] public string Latitude { get; set; }
        [JsonProperty("geoplugin_longitude")] public string Longitude { get; set; }
        [JsonProperty("geoplugin_locationAccuracyRadius")] public string LocationAccuracyRadius { get; set; }
        [JsonProperty("geoplugin_timezone")] public string TimeZone { get; set; }
        [JsonProperty("geoplugin_currencyCode")] public string CurrencyCode { get; set; }
        [JsonProperty("geoplugin_currencySymbol")] public string CurrencySymbol { get; set; }
        [JsonProperty("geoplugin_currencySymbol_UTF8")] public string CurrencySymbolUTF8 { get; set; }
        [JsonProperty("geoplugin_currencyConverter")] public double CurrencyConverter { get; set; }
    }


    public class OpenWeather_Coordinates
    {
        [JsonProperty("lon")] public double Longitude { get; set; }
        [JsonProperty("lat")] public double Latitude { get; set; }
    }
    public class OpenWeather_Condition
    {
        [JsonProperty("id")] public int ConditionID { get; set; }
        [JsonProperty("main")] public string Group { get; set; }
        [JsonProperty("description")] public string Description { get; set; }
        [JsonProperty("icon")] public string Icon { get; set; }
    }

    public class OpenWeather_KeyInfo
    {
        [JsonProperty("temp")] public double Temperature { get; set; }
        [JsonProperty("feels_like")] public double Temperature_FeelsLike { get; set; }
        [JsonProperty("temp_min")] public double Temperature_Minimum { get; set; }
        [JsonProperty("temp_max")] public double Temperature_Maximum { get; set; }
        [JsonProperty("pressure")] public int Pressure { get; set; }
        [JsonProperty("sea_level")] public int PressureAtSeaLevel { get; set; }
        [JsonProperty("grnd_level")] public int PressureAtGroundLevel { get; set; }
        [JsonProperty("humidity")] public int Humidity { get; set; }
    }

    public class OpenWeather_Wind
    {
        [JsonProperty("speed")] public double Speed { get; set; }
        [JsonProperty("deg")] public int Direction { get; set; }
        [JsonProperty("gust")] public double Gust { get; set; }
    }

    public class OpenWeather_Clouds
    {
        [JsonProperty("all")] public int Cloudiness { get; set; }
    }

    public class OpenWeather_Rain
    {
        [JsonProperty("1h")] public int VolumeInLastHour { get; set; }
        [JsonProperty("3h")] public int VolumeInLast3Hours { get; set; }
    }

    public class OpenWeather_Snow
    {
        [JsonProperty("1h")] public int VolumeInLastHour { get; set; }
        [JsonProperty("3h")] public int VolumeInLast3Hours { get; set; }
    }

    public class OpenWeather_Internal
    {
        [JsonProperty("type")] public int Internal_Type { get; set; }
        [JsonProperty("id")] public int Internal_ID { get; set; }
        [JsonProperty("message")] public double Internal_Message { get; set; }
        [JsonProperty("country")] public string CountryCode { get; set; }
        [JsonProperty("sunrise")] public int SunriseTime { get; set; }
        [JsonProperty("sunset")] public int SunsetTime { get; set; }
    }

    public class OpenWeatherResponse
    {
        [JsonProperty("coord")] public OpenWeather_Coordinates Location { get; set; }
        [JsonProperty("weather")] public List<OpenWeather_Condition> WeatherConditions { get; set; }
        [JsonProperty("base")] public string Internal_Base { get; set; }
        [JsonProperty("main")] public OpenWeather_KeyInfo KeyInfo { get; set; }
        [JsonProperty("visibility")] public int Visibility { get; set; }
        [JsonProperty("wind")] public OpenWeather_Wind Wind { get; set; }
        [JsonProperty("clouds")] public OpenWeather_Clouds Clouds { get; set; }
        [JsonProperty("rain")] public OpenWeather_Rain Rain { get; set; }
        [JsonProperty("snow")] public OpenWeather_Snow Snow { get; set; }
        [JsonProperty("dt")] public int TimeOfCalculation { get; set; }
        [JsonProperty("sys")] public OpenWeather_Internal Internal_Sys { get; set; }
        [JsonProperty("timezone")] public int Timezone { get; set; }
        [JsonProperty("id")] public int CityID { get; set; }
        [JsonProperty("name")] public string CityName { get; set; }
        [JsonProperty("cod")] public int Internal_COD { get; set; }
    }
    const string URL_Get_PublicIP = "https://api.ipify.org";
    const string URL_GetGeographicalData = "http://www.geoplugin.net/json.gp?ip=";
    const string URL_GetWeatherData = "http://api.openweathermap.org/data/2.5/weather";
    
    public EPhase Phase {  get; private set;} = EPhase.NotStarted;

    private string publicIP;

    private geoPluginResponse geographicData;
    public OpenWeatherResponse weatherData;
    public bool isWeatherDataAvailable = false;


    public override void ModuleUpdate() { }

    protected override void Awake()
    {
        base.Awake();
    }
    protected override void Start()
    {
        InvokeRepeating("FetchWeatherData", 0, 3600); // updates every hour
    }

    void FetchWeatherData()
    {
        StartCoroutine(GetWeather_Phase1_PublicIP());
    }

    IEnumerator GetWeather_Phase1_PublicIP()
    {
        Phase = EPhase.GetPublicIP;
        using (UnityWebRequest request = UnityWebRequest.Get(URL_Get_PublicIP))
        {
            request.timeout = 1;
            yield return request.SendWebRequest();

            if(request.result == UnityWebRequest.Result.Success)
            {
                publicIP = request.downloadHandler.text.Trim();
                // Debug.Log(publicIP);
                StartCoroutine(GetWeather_Phase2_GeographicInformation());
            } 
            else
            {
                Debug.LogError($"Failed to get public IP: {request.downloadHandler.text}");
                Phase = EPhase.Failed;
            }
        }
        yield return null;
    }

    IEnumerator GetWeather_Phase2_GeographicInformation()
    {
        Phase = EPhase.GetGeographicData;
        using (UnityWebRequest request = UnityWebRequest.Get(URL_GetGeographicalData + publicIP))
        {
            request.timeout = 1;
            yield return request.SendWebRequest();

            if(request.result == UnityWebRequest.Result.Success)
            {
                geographicData = JsonConvert.DeserializeObject<geoPluginResponse>(request.downloadHandler.text);
                // Debug.Log(geographicData.City.ToString());
                StartCoroutine(GetWeather_Phase3_WeatherInformation());
            } 
            else
            {
                Debug.LogError($"Failed to get geographical data: {request.downloadHandler.text}");
                Phase = EPhase.Failed;
            }
        }

        yield return null;
    }

    IEnumerator GetWeather_Phase3_WeatherInformation()
    {
        Phase = EPhase.GetWeatherData;

        string weatherURL = URL_GetWeatherData;
        weatherURL += $"?lat={geographicData.Latitude}";
        weatherURL += $"&lon={geographicData.Longitude}";
        weatherURL += $"&APPID={_brain.OpenWeatherAPIKey}";

        using (UnityWebRequest request = UnityWebRequest.Get(weatherURL))
        {
            request.timeout = 1;
            yield return request.SendWebRequest();

            if(request.result == UnityWebRequest.Result.Success)
            {
                weatherData = JsonConvert.DeserializeObject<OpenWeatherResponse>(request.downloadHandler.text);
                isWeatherDataAvailable = true;
                Phase = EPhase.Succeeded;
            } 
            else
            {
                Debug.LogError($"Failed to get geographical data: {request.downloadHandler.text}");
                Phase = EPhase.Failed;
            }
        }

        yield return null;
    }
}