// ===================================================================================================================
// Author       : Jeeva Raja Paul, UPS
// Description  : This .cs file contains the domain models as well as the interfaces
// Created on   : 07.11.2020
// ===================================================================================================================

using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// This namespace contains the domain models as well as the interfaces
/// </summary>
namespace UPSEmployeeLib.Model
{
    /// <summary>
    /// This class has the details of employee which are serialized and send via web request in case of web server
    /// This same object can also be sent via Entity Framework to send to any DB Server
    /// </summary>
    public class Employee
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("gender")]
        public string Gender { get; set; }

        [JsonProperty("status", NullValueHandling = NullValueHandling.Ignore)]
        public string Status { get; set; }

        [JsonProperty("created_at", NullValueHandling = NullValueHandling.Ignore)]
        public string CreatedAt { get; set; }

        [JsonProperty("updated_at")]
        public string UpdatedAt { get; set; }
    }

    /// <summary>
    /// This interface can be implemented by any class which takes care the connection responsibility to web server or DB server
    /// </summary>
    public interface IConnectionHandler
    {
        /// <summary>
        /// This connection string can contain the connection parameters
        /// </summary>
        string ConnectionInfo { get; set; }

        string AuthenticationInfo { get; set; }

        bool Connect(int timeOutInSeconds = 5000);
    }

    /// <summary>
    /// This interface can be implemented by a class which takes care of querying the web server or DB server
    /// </summary>
    public interface IEmployeeQueryHandler
    {
        /// <summary>
        /// In case of reading from a DB server, then the below pages can be calculated
        /// by the total cound devided by the number of records to be retrieved at a time
        /// </summary>
        int ToTalNoOfPages { get; set; }

        Task<List<Employee>> GetEmployeesAsync();

        Task<List<Employee>> GetEmployeesByNameAsync(string employeeName);

        Employee GetEmployeeById(int employeeId);

        /// <summary>
        /// This function can be implemented to retrieve the emplyoee records page by page in case of web server
        /// or to retrieve a certain numer of records at a time in case of DB server
        /// </summary>
        /// <param name="pageNumber">In case of reading from DB sever, then
        /// this parameter can be replaced with the number of records to be retrieved</param>
        /// <returns></returns>
        Task<List<Employee>> GetEmployeesByPageAsync(int pageNumber);
    }

    /// <summary>
    /// This interface can be implemented by a class which takes care updating the employee details in the web server or DB server
    /// </summary>
    public interface IEmployeeUpdateHandler
    {
        Employee AddEmployee(Employee employee);

        //only the status is made as 'Inactive'
        bool RemoveEmployee(int employeeId);

        bool EditEmployee(Employee employee);
    }
}