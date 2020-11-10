// ====================================================================================================================
// Author       : Jeeva Raja Paul, UPS
// Description  : This .cs file contains the classes which implements CRUD operations to be performed on the web server
// Created on   : 07.11.2020
// ====================================================================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using UPSEmployeeLib.Model;

namespace UPSEmployeeLib.General
{
    /// <summary>
    /// This class implements the Read of CRUD operations on the web server
    /// </summary>
    public class EmployeeQueryHandlerWebServer : IEmployeeQueryHandler
    {
        private readonly WebApiClient _webApiClient;
        public int ToTalNoOfPages { get; set; }

        public EmployeeQueryHandlerWebServer(IConnectionHandler connectionAgent)
        {
            _webApiClient = new WebApiClient(connectionAgent);
        }

        public async Task<List<Employee>> GetEmployeesAsync()
        {
            Task<List<Employee>> retrievalTask = Task.Run(() => _webApiClient.GetEmployeesAsync());
            await retrievalTask;
            return retrievalTask.Result;
        }

        public async Task<List<Employee>> GetEmployeesByPageAsync(int pageNo)
        {
            Task<List<Employee>> employeesRetrievedTask = Task.Run(() => _webApiClient.GetEmployeesByPageAsync(pageNo));
            await employeesRetrievedTask;
            ToTalNoOfPages = _webApiClient.TotalNoOfPages;
            return employeesRetrievedTask.Result;
        }

        public async Task<List<Employee>> GetEmployeesByNameAsync(string emplyoeeName)
        {
            Task<List<Employee>> employeesRetrievedTask = Task.Run(() => _webApiClient.GetEmployeesByNameAsync(emplyoeeName));
            await employeesRetrievedTask;
            return employeesRetrievedTask.Result;
        }

        public Employee GetEmployeeById(int employeeId)
        {
            return _webApiClient.GetEmployeeById(employeeId);
        }
    }

    /// <summary>
    /// This class implements the Create, Update and Delete of the CRUD on the web server
    /// </summary>
    public class EmployeeUpdateHandlerWebServer : IEmployeeUpdateHandler
    {
        private readonly WebApiClient _webApiClient;

        public EmployeeUpdateHandlerWebServer(IConnectionHandler connectionAgent)
        {
            _webApiClient = new WebApiClient(connectionAgent);
        }

        public Employee AddEmployee(Employee employee)
        {
            Task<Employee> addTask = Task.Run(() => _webApiClient.AddEmployee(employee));
            addTask.Wait();
            return addTask.Result;
        }

        public bool RemoveEmployee(int employeeId)
        {
            return _webApiClient.RemoveEmployee(employeeId);
        }

        public bool EditEmployee(Employee employeeToBeModified)
        {
            return _webApiClient.EditEmployee(employeeToBeModified); ;
        }
    }
}