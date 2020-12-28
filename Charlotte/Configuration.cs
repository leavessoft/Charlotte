/*
 * This file is subject to the terms and conditions defined in
 * file 'LICENSE.txt', which is part of this source code package.
 */
using AmapAPITool.AmapAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Charlotte
{
    public class Configuration : INotifyPropertyChanged
    {
        private bool enabled;
        public bool Enabled
        {
            get
            {
                return enabled;
            }
            set
            {
                if (enabled == value)
                {
                    return;
                }
                enabled = value;
                this.NotifyPropertyChanged("Enabled");
            }
        }

        private string displayName;
        public string DisplayName
        {
            get
            {
                return displayName;
            }
            set
            {
                if (displayName == value)
                {
                    return;
                }
                displayName = value;
                this.NotifyPropertyChanged("DisplayName");
            }
        }

        private string key;
        public string Key
        {
            get
            {
                return key;
            }
            set
            {
                if (key == value)
                {
                    return;
                }
                key = value;

                if (APIService != null)
                {
                    APIService.SetAppKey(value);
                }
                else
                {
                    APIService = new AmapAPIService(value);
                }

                this.NotifyPropertyChanged("Key");
            }
        }

        [JsonIgnore]
        public AmapAPIService APIService { get; set; }

        //[JsonIgnore]
        //private int requestCountToday;
        [JsonIgnore]
        public int RequestCountToday
        {
            get
            {
                if (!RequestCountOfDateDict.ContainsKey(DateTime.Now.Date))
                {
                    RequestCountOfDateDict.Add(DateTime.Now.Date, 0);
                }
                return RequestCountOfDateDict[DateTime.Now.Date];
            }
            set
            {
                if (RequestCountOfDateDict.ContainsKey(DateTime.Now.Date))
                {
                    RequestCountOfDateDict[DateTime.Now.Date] = value;
                }
                else
                {
                    RequestCountOfDateDict.Add(DateTime.Now.Date, value);
                }

                this.NotifyPropertyChanged("RequestCountToday");
            }
        }

        public Dictionary<DateTime, int> RequestCountOfDateDict { get; set; }

        private int maximumRequestPerDay;
        public int MaximumRequestPerDay
        {
            get
            {
                return maximumRequestPerDay;
            }
            set
            {
                if (maximumRequestPerDay == value)
                {
                    return;
                }
                maximumRequestPerDay = value;
                this.NotifyPropertyChanged("MaximumRequestPerDay");
            }
        }

        private int maximumRequestPerSecond;
        public int MaximumRequestPerSecond
        {
            get
            {
                return maximumRequestPerSecond;
            }
            set
            {
                if (maximumRequestPerSecond == value)
                {
                    return;
                }
                maximumRequestPerSecond = value;
                this.NotifyPropertyChanged("MaximumRequestPerSecond");
            }
        }

        public long LastUseTimestamp = 0;

        public Configuration(string displayName, string appKey, int maximumRequestPerDay, int maximumRequestPerSecond)
        {
            this.DisplayName = displayName;
            this.Key = appKey;
            this.APIService = new AmapAPIService(appKey);

            RequestCountOfDateDict = new Dictionary<DateTime, int>();
            
            if (RequestCountOfDateDict.ContainsKey(DateTime.Now.Date))
            {
                RequestCountToday = RequestCountOfDateDict[DateTime.Now.Date];
            }
            else
            {
                RequestCountOfDateDict.Add(DateTime.Now.Date, 0);
            }

            this.MaximumRequestPerDay = maximumRequestPerDay;
            this.MaximumRequestPerSecond = maximumRequestPerSecond;
        }

        public string RequestCountOfDateDictToString()
        {
            return string.Join(";", RequestCountOfDateDict.Select(x => x.Key + "=" + x.Value).ToArray());
        }

        public bool IsAvailable()
        {
            return (Enabled
                && RequestCountToday < MaximumRequestPerDay && LastUseTimestamp + (1000 / MaximumRequestPerSecond) <= GetCurrentTimestamp()
                && !String.IsNullOrEmpty(Key));
        }

        public void UpdateCount()
        {
            RequestCountToday++;
            UpdateTimestamp();
        }

        private void UpdateTimestamp()
        {
            LastUseTimestamp = GetCurrentTimestamp();
        }

        private long GetCurrentTimestamp()
        {
            return (long)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1).ToUniversalTime())).TotalMilliseconds;
        }


        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }

    }
}
