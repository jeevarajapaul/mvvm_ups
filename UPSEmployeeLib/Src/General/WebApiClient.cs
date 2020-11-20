using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UPSEmployeeLib.Model;

namespace UPSEmployeeLib.General
{
    /// <summary>
    /// This class contains all the operations to be performed on the web server
    /// GET, POST and PATCH operations are supported now
    /// </summary>
    internal class WebApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly IConnectionHandler _connectionAgent;
        private readonly string _contentType = "application/json";

        private readonly log4net.ILog _webApiClientLogger = log4net.LogManager.GetLogger
            (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// This gives the total number of pages exists
        /// Each page contains certain number of records
        /// </summary>
        public int TotalNoOfPages { get; private set; }

        public WebApiClient(IConnectionHandler connectionAgent)
        {
            _connectionAgent = connectionAgent;
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue
                ("Bearer", _connectionAgent.AuthenticationInfo);
            _contentType = ConfigurationManager.AppSettings.Get("content_type");
        }

        /// <summary>
        /// This function does the POST to add a new employee record
        /// </summary>
        /// <param name="employeeToBeAdded"></param>
        /// <returns>True is return if the http status code is 200 and the 'code' key in the response payload contains 201
        /// that indicates the successfull creation of the record in the web server</returns>
        public Employee AddEmployee(Employee employeeToBeAdded)
        {
            try
            {
                //serilize the object to Json
                string paysLoadForPost = JsonConvert.SerializeObject(employeeToBeAdded);
                HttpContent httpContent = new StringContent(paysLoadForPost, Encoding.UTF8, _contentType);
                Task<HttpResponseMessage> httpResponse = Task.Run(() => _httpClient.PostAsync(_connectionAgent.ConnectionInfo, httpContent));
                httpResponse.Wait();
                HttpResponseMessage httpResponseMsg = httpResponse.Result;
                //httpResponseMsg.EnsureSuccessStatusCode();
                if (httpResponseMsg.IsSuccessStatusCode)
                {
                    string retrievedPayload = httpResponseMsg.Content.ReadAsStringAsync().Result;
                    JObject responseObject = JObject.Parse(retrievedPayload);
                    if (httpResponseMsg.StatusCode == HttpStatusCode.OK &&
                        responseObject["code"].ToString().Equals("201"))
                    {
                        JToken dataContentToken = responseObject["data"];
                        if (dataContentToken.GetType().Name.Equals("JObject"))
                        {
                            employeeToBeAdded = responseObject["data"].ToObject<Employee>();
                            _webApiClientLogger.Info($"New emplyee is created (Emp Id: {employeeToBeAdded.Id}).");
                        }
                    }
                }
                else
                {
                    throw new System.Exception($"Failed to add the employee details. Error: {httpResponseMsg.ReasonPhrase}.");
                }
            }
            catch (HttpRequestException ec)
            {
                _webApiClientLogger.Error("Failed to add the employee details.", ec);
                throw ec;
            }
            return employeeToBeAdded;
        }

        /// <summary>
        /// This function does the GET operation depends on the parameter provided
        /// </summary>
        /// <param name="filterToApply">Filter can be employee id or name or page</param>
        /// <returns></returns>
        public async Task<List<Employee>> GetEmployeesAsync(string filterToApply = "")
        {
            List<Employee> _retrievedEmployees = new List<Employee>();
            try
            {
                string requestUri = string.Concat(_connectionAgent.ConnectionInfo, filterToApply);
                HttpResponseMessage httpResponse = await _httpClient.GetAsync(requestUri);

                if (httpResponse.IsSuccessStatusCode && httpResponse.StatusCode == HttpStatusCode.OK)
                {
                    string retrievedPayLoad = await httpResponse.Content.ReadAsStringAsync();
                    JObject responseObject = JObject.Parse(retrievedPayLoad);
                    if (responseObject["code"].ToString().Equals("200"))
                    {
                        JToken dataContentToken = responseObject["data"];
                        if (dataContentToken.GetType().Name.Equals("JObject"))
                        {
                            Employee employeeRetrieved = responseObject["data"].ToObject<Employee>();
                            _retrievedEmployees.Add(employeeRetrieved);
                            _webApiClientLogger.Info($"Emplyee info (Emp Id: {employeeRetrieved.Id}) has been retrived.");
                        }
                        else
                        {
                            _retrievedEmployees = responseObject["data"].ToObject<List<Employee>>();
                            _webApiClientLogger.Info($"{_retrievedEmployees.Count} Employee(s) is/are retrieved for the given employee name.");
                        }
                        if (responseObject["meta"].HasValues && responseObject["meta"]["pagination"].HasValues
                            && responseObject["meta"]["pagination"]["pages"] != null)
                        {
                            TotalNoOfPages = (int)responseObject["meta"]["pagination"]["pages"];
                        }
                    }
                }
                else
                {
                    throw new System.Exception($"Failed to retrieve the employee details. Error: {httpResponse.ReasonPhrase}.");
                }
            }
            catch (HttpRequestException ec)
            {
                _webApiClientLogger.Error("Failed to retrieve the employee details.", ec);
                throw ec;
            }
            return _retrievedEmployees;
        }

        public async Task<List<Employee>> GetEmployeesByPageAsync(int pageNo)
        {
            List<Employee> retrievedEmployees = new List<Employee>();
            Task<List<Employee>> employeesRetrievedTask = Task.Run(() => GetEmployeesAsync($"?page={pageNo}"));
            await employeesRetrievedTask;
            if (employeesRetrievedTask.Result.Count > 0)
            {
                retrievedEmployees = employeesRetrievedTask.Result;
            }
            return retrievedEmployees;
        }

        public Employee GetEmployeeById(int emplyoeeId)
        {
            Employee retrievedEmployee = null;
            Task<List<Employee>> employeesRetrievedTask = Task.Run(() => GetEmployeesAsync($"/{emplyoeeId}"));
            employeesRetrievedTask.Wait();
            if (employeesRetrievedTask.Result.Count > 0)
            {
                retrievedEmployee = employeesRetrievedTask.Result[0];
            }
            return retrievedEmployee;
        }

        public async Task<List<Employee>> GetEmployeesByNameAsync(string employeeName)
        {
            List<Employee> retrievedEmployees = new List<Employee>();
            Task<List<Employee>> employeesRetrievedTask = Task.Run(() => GetEmployeesAsync($"?name={employeeName}"));
            await employeesRetrievedTask;
            if (employeesRetrievedTask.Result.Count > 0)
            {
                retrievedEmployees = employeesRetrievedTask.Result;
            }
            return retrievedEmployees;
        }

        /// <summary>
        /// This function does the PATCH operation to modify the employee details
        /// </summary>
        /// <param name="employeeToBeModified">Employee record to be modified</param>
        /// <returns></returns>
        public bool EditEmployee(Employee employeeToBeModified)
        {
            bool isEditSuccessfull = false;
            try
            {
                //serilize the object to Json
                string paysLoadForPost = JsonConvert.SerializeObject(employeeToBeModified);
                HttpContent httpContent = new StringContent(paysLoadForPost, Encoding.UTF8, _contentType);
                string requestUri = string.Concat(_connectionAgent.ConnectionInfo, $"/{employeeToBeModified.Id}");

                HttpRequestMessage httpRequestMessage = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri)
                { Content = httpContent };

                Task<HttpResponseMessage> httpResponseTask = Task.Run(() => _httpClient.SendAsync(httpRequestMessage));
                httpResponseTask.Wait();
                HttpResponseMessage httpResponseMsg = httpResponseTask.Result;
                httpResponseMsg.EnsureSuccessStatusCode();
                if (httpResponseMsg.IsSuccessStatusCode)
                {
                    string retrievedPayLoad = httpResponseMsg.Content.ReadAsStringAsync().Result;
                    JObject responseObject = JObject.Parse(retrievedPayLoad);
                    if (responseObject["code"].ToString().Equals("200") && httpResponseMsg.StatusCode == HttpStatusCode.OK)
                    {
                        isEditSuccessfull = true;
                    }
                    else
                    {
                        JToken dataContentToken = responseObject["data"];
                        if (dataContentToken.GetType().Name.Equals("JObject"))
                        {
                            if (responseObject["data"]["message"] != null)
                            {
                                string errMsg = (string)responseObject["data"]["message"];
                                _webApiClientLogger.Error($"Failed to modify the employee details. Error: {errMsg}");
                            }
                        }
                    }
                }
            }
            catch (HttpRequestException ec)
            {
                _webApiClientLogger.Error("Failed to modify the employee details.", ec);
                throw ec;
            }
            //return based on the status code
            return isEditSuccessfull;
        }

        /// <summary>
        /// This funtion does the DELETE operation
        /// </summary>
        /// <param name="employeeId">Employee record to be deleted</param>
        /// <returns></returns>
        public bool RemoveEmployee(int employeeId)
        {
            bool isRemovalSuccessful = false;
            try
            {
                Task<HttpResponseMessage> httpResponseTask = Task.Run(() => _httpClient.DeleteAsync($"/{employeeId}"));
                httpResponseTask.Wait();
                HttpResponseMessage httpResponseMsg = httpResponseTask.Result;
                isRemovalSuccessful = httpResponseMsg.StatusCode == HttpStatusCode.Accepted;
            }
            catch (System.Exception ec)
            {
                _webApiClientLogger.Error("Failed to remove the employee details.", ec);
                throw ec;
            }
            return isRemovalSuccessful;
        }
    }
}