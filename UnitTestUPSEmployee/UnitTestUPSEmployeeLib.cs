using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UPSEmployeeLib.General;
using UPSEmployeeLib.Model;
using UPSEmployeeLib.ViewModel;

namespace UnitTestUPSEmployee
{
    [TestClass]
    public class UnitTestUPSEmployeeLib
    {
        private ConnectionAgentWebServer _connectionAgent;
        private readonly string _reqUri = "https://gorest.co.in/public-api/users";
        private readonly string _authApiKey = "fa114107311259f5f33e70a5d85de34a2499b4401da069af0b1d835cd5ec0d56";
        private List<Employee> employees = new List<Employee>();
        private string updatedTime = DateTimeOffset.Now.ToString("yyyy-MM-dd'T'HH:mm:ssK");

        [TestInitialize()]
        public void InitilizeVars()
        {
            _connectionAgent = ConnectionAgentWebServer.GetInstance(_reqUri, _authApiKey);
            employees.Add(new Employee { Id = 54, Name = "givna", Email = "givna@de.co", Gender = "Male", Status = "Active", CreatedAt = updatedTime, UpdatedAt = updatedTime });
            employees.Add(new Employee { Id = 534, Name = "givna", Email = "givna@de.co", Gender = "Male", Status = "Active", CreatedAt = updatedTime, UpdatedAt = updatedTime });
        }

        [TestMethod]
        public void GetEmployeesFromJsonContent()
        {
            string reqPayLoad = System.IO.File.ReadAllText(@"D:\CodeChallenge\RetrievedEmps.json");
            dynamic reqPayload = JsonConvert.DeserializeObject(reqPayLoad);

            JObject responseObject = JObject.Parse(reqPayLoad);
            List<Employee> emps = responseObject["data"].ToObject<List<Employee>>(); ;
        }

        [DataTestMethod]
        [DataRow(34)]
        [DataRow(13)]
        [DataRow(33332)]
        public void QueryEmployeeById(int empId)
        {
            Employee employeeReturned = GetEmployeeById(empId);
            Assert.IsTrue(employeeReturned != null);
        }

        private Employee GetEmployeeById(int empId)
        {
            Employee retrievedEmployee = null;
            EmployeeQueryHandlerWebServer employeeQueryHandlerWebServer = new EmployeeQueryHandlerWebServer(_connectionAgent);
            EmplyeeViewModelQuery emplyeeViewModelQuery = new EmplyeeViewModelQuery(employeeQueryHandlerWebServer,
                EmplyeeViewModelQuery.SearchFilter.ByEmployeeId);
            Task workItemsLoadingTask = Task.Run(() => emplyeeViewModelQuery.GetEmployeesAsync(empId.ToString()));
            workItemsLoadingTask.Wait();
            if (emplyeeViewModelQuery.RetrievedEmployees.Count == 1)
            {
                retrievedEmployee = emplyeeViewModelQuery.RetrievedEmployees[0];
            }
            return retrievedEmployee;
        }

        /*
        [TestMethod]
        public void QueryEmployeesByPage()
        {
            var connectionHandlerToMock = new Mock<IConnectionHandler>();
            connectionHandlerToMock.Setup(x => x.IsConnected).Returns(true);
            ConnectionAgentWebServer connectionAgentWebServer = (ConnectionAgentWebServer)connectionHandlerToMock.Object;
            connectionAgentWebServer.ConnectionInfo = _reqUri;
            connectionAgentWebServer.AuthenticationInfo = _authApiKey;
            EmployeeQueryHandlerWebServer employeeQueryHandlerWebServer = new EmployeeQueryHandlerWebServer(connectionAgentWebServer);
            EmplyeeViewModelQuery emplyeeViewModelQuery = new EmplyeeViewModelQuery(employeeQueryHandlerWebServer,
                EmplyeeViewModelQuery.SearchFilter.ByPageNumber);
            Task workItemsLoadingTask = Task.Run(() => emplyeeViewModelQuery.GetEmployeesAsync());
            workItemsLoadingTask.Wait();
            Assert.IsTrue(emplyeeViewModelQuery.RetrievedEmployees.Count > 0);
        }
        */

        [DataTestMethod]
        [DataRow("rita")]
        [DataRow("34d")]
        [DataRow("testved")]
        public void QueryEmployeeByName(string empName)
        {
            EmployeeQueryHandlerWebServer employeeQueryHandlerWebServer = new EmployeeQueryHandlerWebServer(_connectionAgent);
            EmplyeeViewModelQuery emplyeeViewModelQuery = new EmplyeeViewModelQuery(employeeQueryHandlerWebServer,
                EmplyeeViewModelQuery.SearchFilter.ByEmployeeName);
            Task workItemsLoadingTask = Task.Run(() => emplyeeViewModelQuery.GetEmployeesAsync(empName));
            workItemsLoadingTask.Wait();
            Assert.IsTrue(emplyeeViewModelQuery.RetrievedEmployees.Count > 0);
        }

        private static IEnumerable<object[]> EmployeeData =>
            new List<object[]> {
                new object[] { "54", "givna","givna@de.co", "Male"},
                new object[] { "534", "Kajori Jal", "kajori.jal@de.co","Female"}
            };

        [DataTestMethod]
        [DynamicData(nameof(EmployeeData))]
        //public void EditEmployee(List<Employee> employeesList)
        public void EditEmployee(string empId, string empName, string empEmail, string empGender)
        {
            int.TryParse(empId, out int employeeId);
            Employee employeeRetrieved = GetEmployeeById(employeeId);
            if (employeeRetrieved != null)
            {
                EmployeeUpdateHandlerWebServer employeeUpdateHandlerWebServer = new EmployeeUpdateHandlerWebServer(_connectionAgent);
                EmployeeViewModelUpdate employeeViewModelUpdate = new EmployeeViewModelUpdate(employeeUpdateHandlerWebServer, EmployeeViewModelUpdate.UpdateType.EditEmployee)
                {
                    EmployeeId = employeeId,
                    EmployeeName = empName,
                    EmployeeEmail = empEmail,
                    EmployeeGender = empGender
                };
                bool isEmployeeSaved = employeeViewModelUpdate.UpdateEmployee();
                Assert.IsTrue(isEmployeeSaved);
            }
            else
            {
                Assert.Fail();
            }
        }
    }
}