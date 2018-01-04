﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using Interfaces.Cookies;
using Interfaces.Serialization;
using Interfaces.SerializedStorage;
using Interfaces.Destination.Filename;
using Interfaces.Status;

using Models.Separators;

namespace Controllers.Cookies
{
    public class CookieController : ICookieController
    {
        private ISerializedStorageController serializedStorageController;
        private IStrongTypeSerializationController<(string, string), string> cookieSerializationController;
        private IDictionary<string, string> storedCookies;
        private IGetFilenameDelegate getFilenameDelegate;
        private IStatusController statusController;

        public CookieController(
            IStrongTypeSerializationController<(string, string), string> cookieSerializationController,
            ISerializedStorageController serializedStorageController,
            IGetFilenameDelegate getFilenameDelegate,
            IStatusController statusController)
        {
            this.cookieSerializationController = cookieSerializationController;
            this.serializedStorageController = serializedStorageController;
            this.getFilenameDelegate = getFilenameDelegate;
            this.statusController = statusController;

            this.storedCookies = new Dictionary<string, string>();
        }

        public async Task<string> GetCookiesStringAsync(IStatus status)
        {
            if (!DataAvailable) await LoadAsync(status);

            var cookies = new List<string>();
            foreach (var cookieName in storedCookies.Keys)
            {
                var serializedCookie = cookieSerializationController.Serialize((cookieName, storedCookies[cookieName]));
                cookies.Add(serializedCookie);
            }
            return string.Join(Separators.Common.SemiColon, cookies);
        }

        public bool DataAvailable
        {
            get;
            private set;
        }

        public async Task LoadAsync(IStatus status)
        {
            var loadStatus = statusController.Create(status, "Load cookies");

            storedCookies = await serializedStorageController.DeserializePullAsync<Dictionary<string, string>>(
                getFilenameDelegate.GetFilename(),
                loadStatus);

            if (storedCookies == null)
                storedCookies = new Dictionary<string, string>();

            DataAvailable = true;

            statusController.Complete(loadStatus);
        }

        public async Task SaveAsync(IStatus status)
        {
            if (!DataAvailable) throw new InvalidOperationException("Cannot save data before it's available");

            var saveStatus = statusController.Create(status, "Save cookies");

            await serializedStorageController.SerializePushAsync(
                getFilenameDelegate.GetFilename(),
                storedCookies,
                saveStatus);

            statusController.Complete(saveStatus);
        }

        public async Task SetCookiesAsync(IEnumerable<string> cookies, IStatus status)
        {
            if (!DataAvailable) await LoadAsync(status);

            foreach (var cookie in cookies)
            {
                var deserializedCookie = cookieSerializationController.Deserialize(cookie);

                if (storedCookies.ContainsKey(deserializedCookie.Item1))
                    storedCookies[deserializedCookie.Item1] = deserializedCookie.Item2;
                else storedCookies.Add(deserializedCookie.Item1, deserializedCookie.Item2);
            }

            await SaveAsync(status);
        }
    }
}