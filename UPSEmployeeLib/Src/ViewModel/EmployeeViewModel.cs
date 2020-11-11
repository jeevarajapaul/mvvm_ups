using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using UPSEmployeeLib.General;
using UPSEmployeeLib.Model;

namespace UPSEmployeeLib.ViewModel
{
    /// <summary>
    /// This class retrieves the employess based on the filter provided in the user interface
    /// and give back the retrieved list of employees and messages to the user interface
    /// </summary>
    public class EmplyeeViewModelQuery
    {
        #region Public Members

        public enum SearchFilter
        {
            NoFilter = 0,
            ByEmployeeId = 1,
            ByEmployeeName = 2,
            ByPageNumber = 3
        }

        public List<Employee> RetrievedEmployees { get; set; }
        public int TotalNoOfPages { get; set; }

        public delegate void ShowMessageToUserHandler(object sender, MessageToUserEventArgs args);

        public event ShowMessageToUserHandler ShowMessageToUserEvent;

        #endregion Public Members

        private readonly SearchFilter _searchFilter = SearchFilter.NoFilter;
        private readonly IEmployeeQueryHandler _employeeQueryHandler;
        private readonly MessageToUserEventArgs _messageToUserEventArgs = new MessageToUserEventArgs();

        public EmplyeeViewModelQuery(IEmployeeQueryHandler employeeQueryHandler, SearchFilter searchFilter)
        {
            _searchFilter = searchFilter;
            //Initiate the Handlers
            _employeeQueryHandler = employeeQueryHandler;
        }

        /// <summary>
        /// This function is asynchronous, since the retrieval takes some time.
        /// This function retrievs
        /// </summary>
        /// <param name="filterValue">Filter can be Employee Id or Name.
        /// In case th</param>
        /// <returns>List of Employee records</returns>
        public async Task GetEmployeesAsync(string filterValue = "")
        {
            Task<List<Employee>> retrievalTask;
            List<Employee> _retrievedEmployees = new List<Employee>();
            try
            {
                switch (_searchFilter)
                {
                    case SearchFilter.NoFilter:
                        //implement only page wise retrieval of employess and show the first page
                        retrievalTask = Task.Run(() => _employeeQueryHandler.GetEmployeesAsync());
                        await retrievalTask;
                        _retrievedEmployees = retrievalTask.Result;
                        break;

                    case SearchFilter.ByEmployeeId:
                        int.TryParse(filterValue, out int employeeId);
                        Employee rerievedEmployee = _employeeQueryHandler.GetEmployeeById(employeeId);
                        if (rerievedEmployee != null)
                        {
                            _retrievedEmployees.Add(rerievedEmployee);
                        }
                        break;

                    case SearchFilter.ByEmployeeName:
                        retrievalTask = Task.Run(() => _employeeQueryHandler.GetEmployeesByNameAsync(filterValue));
                        await retrievalTask;
                        _retrievedEmployees = retrievalTask.Result;
                        break;

                    case SearchFilter.ByPageNumber:
                        int.TryParse(filterValue, out int pageToRetrieve);
                        retrievalTask = Task.Run(() => _employeeQueryHandler.GetEmployeesByPageAsync(pageToRetrieve));
                        await retrievalTask;
                        _retrievedEmployees = retrievalTask.Result;
                        TotalNoOfPages = _employeeQueryHandler.ToTalNoOfPages;
                        break;

                    default:
                        break;
                }
            }
            catch (Exception ec)
            {
                _messageToUserEventArgs.IsOperationSucceeded = false;
                _messageToUserEventArgs.MessageToShow = ec.Message;
                ShowMessageToUserEvent?.Invoke(this, _messageToUserEventArgs);
            }
            RetrievedEmployees = _retrievedEmployees;
            return;
        }
    } //class EmplyeeViewModelQuery

    /// <summary>
    /// This class validates the data given in the user interface and the performs the Create Update and Delete operations.
    /// The result of the operation is raised via an event to the user interface
    /// </summary>
    public class EmployeeViewModelUpdate : INotifyPropertyChanged, IDataErrorInfo
    {
        public enum UpdateType
        {
            AddEmployee = 0,
            EditEmployee = 1,
            RemoveEmployee = 2
        }

        private readonly IEmployeeUpdateHandler _employeeUpdateHandler;
        private readonly UpdateType _updateType;
        private MessageToUserEventArgs _messageToUserEventArgs = new MessageToUserEventArgs();
        private Employee _employeeToProcess;

        public event PropertyChangedEventHandler PropertyChanged;

        public delegate void ShowMessageToUserHandler(object sender, MessageToUserEventArgs args);

        public event ShowMessageToUserHandler ShowMessageToUserEvent;

        //private string _EmployeeId = string.Empty;
        private ICommand _updateEmployeeCommand;

        //public EmployeeViewModel(ConnectionAgent connectionAgent, Employee employeeToBeUpdated)
        public EmployeeViewModelUpdate(IEmployeeUpdateHandler employeeUpdateHandler, UpdateType updateType)
        {
            _employeeUpdateHandler = employeeUpdateHandler;
            _updateType = updateType;
            //Initiate the Handlers
            _employeeToProcess = new Employee();
        }

        /// <summary>
        /// This property is kept as string, since due to TwoWay property,
        /// by default the employee id text box was showing 0 which is the default value of the int
        /// </summary>
        public int EmployeeId
        {
            get
            {
                return _employeeToProcess.Id;
                //return _EmployeeId;
            }
            set
            {
                /*
                int.TryParse(value.ToString(), out int empId);
                if (value != _EmployeeId)
                */
                if (value != _employeeToProcess.Id)
                {
                    _employeeToProcess.Id = value;
                    //_EmployeeId = value;
                    OnPropertyChanged("EmployeeId");
                }
            }
        }

        public string EmployeeName
        {
            get
            {
                return _employeeToProcess.Name;
            }
            set
            {
                if (value != _employeeToProcess.Name)
                {
                    _employeeToProcess.Name = value.Trim();
                    OnPropertyChanged("EmployeeName");
                }
            }
        }

        public string EmployeeEmail
        {
            get
            {
                return _employeeToProcess.Email;
            }
            set
            {
                if (value != _employeeToProcess.Email)
                {
                    _employeeToProcess.Email = value.Trim();
                    OnPropertyChanged("EmployeeEmail");
                }
            }
        }

        public string EmployeeGender
        {
            get
            {
                return _employeeToProcess.Gender;
            }
            set
            {
                if (value != _employeeToProcess.Gender)
                {
                    _employeeToProcess.Gender = value.Trim();
                    OnPropertyChanged("EmployeeGender");
                }
            }
        }

        /*
        public string EmployeeStatus
        {
            get
            {
                return _employeeToProcess.Status;
            }
            set
            {
                if (value != _employeeToProcess.Status)
                {
                    _employeeToProcess.Status = value.Trim();
                    OnPropertyChanged("EmployeeStatus");
                }
            }
        }
        */

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ICommand SaveEmployeeCommand
        {
            get
            {
                if (_updateEmployeeCommand == null)
                {
                    _updateEmployeeCommand = new CommandHandler(
                        param => UpdateEmployee(),
                        param => IsEmployeeDetailsValid()
                    );
                }
                return _updateEmployeeCommand;
            }
        }

        public string Error
        {
            get
            {
                return null;
            }
        }

        public string this[string propertyName]
        {
            get
            {
                string result = string.Empty;
                switch (propertyName)
                {
                    /*
                    case "EmployeeId":
                        //bool isNumeric = int.TryParse(EmployeeId.ToString(), out int empId);
                        bool isNumeric = int.TryParse(EmployeeId, out int empId);
                        if (EmployeeId.Length > 0 && empId == 0)
                        {
                            result = "Emplyee Id should be a valid number.";
                        }
                        else
                        {
                            _employeeToProcess.Id = empId;
                        }
                        break;
                    */
                    case "EmployeeEmail":
                        if (!string.IsNullOrEmpty(EmployeeEmail))
                        {
                            if (!IsEmailFormatCorrect())
                            {
                                result = "Given Email is not valid.";
                            }
                        }
                        break;
                }

                return result;
            }
        }

        private bool IsEmailFormatCorrect()
        {
            return new EmailAddressAttribute().IsValid(EmployeeEmail);
        }

        private bool IsEmployeeDetailsValid()
        {
            //int.TryParse(EmployeeId, out int empId);
            bool isValidEmployee = false;
            if (!string.IsNullOrEmpty(EmployeeName) && !string.IsNullOrEmpty(EmployeeGender)
                && !string.IsNullOrEmpty(EmployeeEmail) && IsEmailFormatCorrect()
                )
            {
                isValidEmployee = true;
            }
            return isValidEmployee;
        }

        /// <summary>
        /// This function takes care of adding, modifying and making inactive the employ record
        /// </summary>
        /// <returns></returns>
        public bool UpdateEmployee()
        {
            Mouse.OverrideCursor = Cursors.Wait;
            bool isEmployeeUpdated = false;
            string msgToShow = string.Empty;
            try
            {
                string updatedTime = DateTimeOffset.Now.ToString("yyyy-MM-dd'T'HH:mm:ssK");
                _employeeToProcess.UpdatedAt = updatedTime;
                switch (_updateType)
                {
                    case UpdateType.AddEmployee:
                        _employeeToProcess.Status = "Active";
                        _employeeToProcess.CreatedAt = updatedTime;
                        _employeeToProcess = _employeeUpdateHandler.AddEmployee(_employeeToProcess);
                        isEmployeeUpdated = (_employeeToProcess.Id > 0);
                        break;

                    case UpdateType.EditEmployee:
                        isEmployeeUpdated = _employeeUpdateHandler.EditEmployee(_employeeToProcess);
                        break;

                    case UpdateType.RemoveEmployee:
                        _employeeToProcess.Status = "Inactive";
                        isEmployeeUpdated = _employeeUpdateHandler.RemoveEmployee(_employeeToProcess.Id);
                        break;
                }
                _messageToUserEventArgs.IsOperationSucceeded = isEmployeeUpdated;
                ShowMessageToUserEvent?.Invoke(this, _messageToUserEventArgs);

                if (isEmployeeUpdated)
                {
                    //create a new employee object
                    _employeeToProcess = new Employee();
                    ClearView();
                }
            }
            catch (Exception ec)
            {
                _messageToUserEventArgs.IsOperationSucceeded = false;
                _messageToUserEventArgs.MessageToShow = ec.Message;
                ShowMessageToUserEvent?.Invoke(this, _messageToUserEventArgs);
            }
            Mouse.OverrideCursor = null;
            return isEmployeeUpdated;
        }

        private void ClearView()
        {
            EmployeeName = string.Empty;
            EmployeeEmail = string.Empty;
            EmployeeGender = string.Empty;
        }
    } //class EmployeeViewModelUpdate

    public class BooleanToStringValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((string)parameter == (string)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? parameter : null;
        }
    }

    public class CommandHandler : ICommand
    {
        #region Declarations

        private readonly Predicate<object> _canExecute;
        private readonly Action<object> _execute;

        #endregion Declarations

        #region Constructors

        public CommandHandler(Action<object> execute)
            : this(execute, null)
        {
        }

        public CommandHandler(Action<object> execute, Predicate<object> canExecute)
        {
            _execute = execute ?? throw new ArgumentNullException("execute");
            _canExecute = canExecute;
        }

        #endregion Constructors

        #region ICommand Members

        public event EventHandler CanExecuteChanged
        {
            add
            {
                if (_canExecute != null)
                {
                    CommandManager.RequerySuggested += value;
                }
            }
            remove
            {
                if (_canExecute != null)
                {
                    CommandManager.RequerySuggested -= value;
                }
            }
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute != null && _canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            _execute(parameter);
        }

        #endregion ICommand Members
    }
}