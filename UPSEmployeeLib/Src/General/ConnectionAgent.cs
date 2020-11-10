// ===================================================================================================================
// Author       : Jeeva Raja Paul, UPS
// Description  : This .cs file contains the class which implements the connection to the web server
// Created on   : 07.11.2020
// ===================================================================================================================

using System;
using System.Net;
using UPSEmployeeLib.Model;

namespace UPSEmployeeLib.General
{
    /// <summary>
    /// This class is a singleton and has the connection operations to the web server
    /// </summary>
    public sealed class ConnectionAgentWebServer : IConnectionHandler
    {
        public string ConnectionInfo { get; set; }
        public string AuthenticationInfo { get; set; }

        private static ConnectionAgentWebServer _connectionAgentInstance = null;
        private static readonly object _mutexObject = new object();

        private readonly log4net.ILog _connectionHandlerLogger = log4net.LogManager.GetLogger
            (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Allow the user to create only one instance which contains the
        /// connection object to be used by the entire application
        /// </summary>
        private ConnectionAgentWebServer()
        {
        }

        private ConnectionAgentWebServer(string loginUrl)
        {
            ConnectionInfo = loginUrl;
        }

        /// <summary>
        /// This method is used to create the Singleton object
        /// </summary>
        /// <param name="serverUrl"></param>
        /// <returns></returns>
        public static ConnectionAgentWebServer GetInstance(string loginUrl, string authApiKey)
        {
            //Making sure that only one call is made to get the instance at any point of time
            lock (_mutexObject)
            {
                if (_connectionAgentInstance == null || new Uri(_connectionAgentInstance.ConnectionInfo) != new Uri(loginUrl))
                {
                    _connectionAgentInstance = new ConnectionAgentWebServer(loginUrl)
                    {
                        AuthenticationInfo = authApiKey
                    };
                }
            }
            return _connectionAgentInstance;
        }

        /// <summary>
        /// This function connects to the web server with the given URL and
        /// returns true/false based on the connection result
        /// </summary>
        /// <param name="urlToConnect">URL to connect the web server</param>
        /// <param name="timeOutInMs">Timeout in milliseconds</param>
        /// <returns></returns>
        public bool Connect(int timeOutInMs = 5000)
        {
            Uri currentUrl = new Uri(ConnectionInfo);
            bool isUriAccessible = false;
            try
            {
                var webRequest = (HttpWebRequest)WebRequest.Create(currentUrl.GetLeftPart(UriPartial.Authority));
                webRequest.Method = "HEAD";
                webRequest.Timeout = timeOutInMs;
                webRequest.AllowAutoRedirect = false;
                HttpWebResponse httpWebResponse = (HttpWebResponse)webRequest.GetResponse();

                isUriAccessible = httpWebResponse.StatusCode == HttpStatusCode.OK;

                httpWebResponse.Close();
            }
            catch (Exception ec)
            {
                _connectionHandlerLogger.Error($"Failed to connect to '{currentUrl.Host}'", ec);
                isUriAccessible = false;
            }
            return isUriAccessible;
        }
    }
}