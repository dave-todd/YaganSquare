using UnityEngine;
using System.Xml;
using System;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.AzureSky;
using System.Globalization;

public class WeatherAPISync : MonoBehaviour
{
    [SerializeField] private AzureSkyController skyController;
    [SerializeField] private WindZone windZone;
    [SerializeField] private Clock clockArrow;

    [Tooltip("Rate at which the weather data is updated (in hours). URL is updated hourly.")]
    [SerializeField] private int updateRateInHours = 1;

    [SerializeField] private float windSpeed;
    [SerializeField] private float cloudCover;

    [SerializeField] private Text timeValue;
    [SerializeField] private Text temperatureValue;
    [SerializeField] private Text windSpeedValue;
    [SerializeField] private Text windDirectionValue;

    private Keyframe[] oldClouds;
    private Keyframe[] newClouds;

    private bool cloudsAreExpanding;
    private bool cloudsAreContracting;
    private float cloudChangeIncrement = 0.001f;

    float refreshWeatherTimer;
    float refreshTimeTimer;

    private void Start()
    {
        StartCoroutine(RefreshWeatherData());
    }

    private void Update()
    {
        if (refreshTimeTimer <= 1)
        {
            refreshTimeTimer += Time.deltaTime;
        }
        else
        {
            UpdateTime();
            refreshTimeTimer = 0;
        }


        if (cloudsAreExpanding)
        {
            ExpandClouds();
        }

        if (cloudsAreContracting)
        {
            ContractClouds();
        }

        if (refreshWeatherTimer <= updateRateInHours * 3600)
        {
            refreshWeatherTimer += Time.deltaTime;
        }
        else
        {
            StartCoroutine(RefreshWeatherData());
            refreshWeatherTimer = 0;
        }
    }

    private void UpdateTime()
    {
        var date = DateTime.Now;
        timeValue.text = DateTime.Now.ToString("h:mmtt", CultureInfo.InvariantCulture);
        clockArrow.UpdateTime(date.Hour, date.Minute);
    }

    IEnumerator RefreshWeatherData()
    {
        //string url = "http://api.openweathermap.org/data/2.5/find?q=Perth,AU&type=accurate&mode=xml&lang=nl&units=metric&appid=4507f625dc92072478bb520490452c4f";
        string url = "https://yagan.core.iion.cloud/bridge/?key=4507f625dc92072478bb520490452c4f&uri=" + WWW.EscapeURL("http://api.openweathermap.org/data/2.5/find?q=Perth,AU&type=accurate&mode=xml&lang=nl&units=metric&appid=4507f625dc92072478bb520490452c4f");
        WWW www = new WWW(url);
        yield return www;
        if (www.error == null)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(www.text);
            windSpeed = float.Parse(xmlDoc.SelectSingleNode("cities/list/item/wind/speed/@value").InnerText);
            cloudCover = float.Parse(xmlDoc.SelectSingleNode("cities/list/item/clouds/@value").InnerText);
            Debug.Log(String.Format("Updated weather. Windspeed = {0}. Cloud cover value = {1}", windSpeed.ToString(), cloudCover.ToString()));
            UpdatePanel(xmlDoc);
            ChangeWind();
            GetClouds();
        }
        else
        {
            Debug.Log("ERROR: " + www.error);

        }
    }

    private void UpdatePanel(XmlDocument xmlDoc)
    {
        float tempFloat = float.Parse(xmlDoc.SelectSingleNode("cities/list/item/temperature/@value").InnerText);
        int intTemp = int.Parse(Mathf.Round(tempFloat).ToString().Split('.')[0]);
        temperatureValue.text = intTemp.ToString() + "°C";
        float windSpeedFloat = float.Parse(xmlDoc.SelectSingleNode("cities/list/item/wind/speed/@value").InnerText) / 1000 * 60 * 60;
        int intWindSpeed = int.Parse(Mathf.Round(windSpeedFloat).ToString().Split('.')[0]);
        windSpeedValue.text = intWindSpeed.ToString() + "KM/H";
        windDirectionValue.text = xmlDoc.SelectSingleNode("cities/list/item/wind/direction/@code").InnerText;
    }

    private void ChangeWind()
    {
        //Change windspeed
        if (windSpeed >= 12)
        {
            windZone.windMain = 0.15f;
        }
        else if (windSpeed >= 8)
        {
            windZone.windMain = 0.11f;
        }
        else if (windSpeed >= 4)
        {
            windZone.windMain = 0.08f;
        }
        else
        {
            windZone.windMain = 0.05f;
        }
    }

    private void GetClouds()
    {
        oldClouds = skyController.currentDayProfile.clouds.dynamicCloudLayer1DensityCurve[0].keys;
        newClouds = skyController.currentDayProfile.clouds.dynamicCloudLayer1DensityCurve[0].keys;
        newClouds[0].value = cloudCover / 100;

        if (oldClouds[0].value < newClouds[0].value)
        {
            cloudsAreExpanding = true;
        }
        else if (oldClouds[0].value > newClouds[0].value)
        {
            cloudsAreContracting = true;
        }

    }

    private void ExpandClouds()
    {
        oldClouds[0].value += cloudChangeIncrement;

        skyController.currentDayProfile.clouds.dynamicCloudLayer1DensityCurve[0].keys = oldClouds;

        if (oldClouds[0].value >= newClouds[0].value)
        {
            cloudsAreExpanding = false;
        }
    }

    private void ContractClouds()
    {
        oldClouds[0].value -= cloudChangeIncrement;

        skyController.currentDayProfile.clouds.dynamicCloudLayer1DensityCurve[0].keys = oldClouds;

        if (oldClouds[0].value <= newClouds[0].value)
        {
            cloudsAreContracting = false;
        }
    }
}