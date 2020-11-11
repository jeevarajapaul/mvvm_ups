using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using UPSEmployeeLib.General;
using UPSEmployeeLib.Model;
using UPSEmployeeLib.ViewModel;

namespace UPSAssessment.UPSEmployeeUI
{
    /// <summary>
    /// Interaction logic for UpdateEmployeePage.xaml
    /// </summary>
    public partial class UpdateEmployeePage : Page
    {
        private readonly EmployeeViewModelUpdate _employeeViewModelUpdate;

        //below object is set only in case of edit or remove employee functionality
        private readonly EmplyeeViewModelQuery _employeeViewModelQuery;

        public UpdateEmployeePage(IConnectionHandler connectionHandler, EmployeeViewModelUpdate.UpdateType updateType)
        {
            InitializeComponent();
            EmployeeUpdateHandlerWebServer employeeUpdateHandlerWebServer = new EmployeeUpdateHandlerWebServer(connectionHandler);
            _employeeViewModelUpdate = new EmployeeViewModelUpdate(employeeUpdateHandlerWebServer, updateType);
            _employeeViewModelUpdate.ShowMessageToUserEvent += _employeeViewModel_ShowMessageToUserEvent;
            DataContext = _employeeViewModelUpdate;
            TextBoxEmployeeId.Clear();
            InitializeUIBasedOnUpdateType(updateType);
            if (updateType == EmployeeViewModelUpdate.UpdateType.EditEmployee || updateType == EmployeeViewModelUpdate.UpdateType.RemoveEmployee)
            {
                EmployeeQueryHandlerWebServer employeeQueryHandlerWebServer = new EmployeeQueryHandlerWebServer(connectionHandler);
                _employeeViewModelQuery = new EmplyeeViewModelQuery(employeeQueryHandlerWebServer, EmplyeeViewModelQuery.SearchFilter.ByEmployeeId);
            }
        }

        private void InitializeUIBasedOnUpdateType(EmployeeViewModelUpdate.UpdateType updateType)
        {
            Visibility makeVisibleEmpIdFields = updateType != EmployeeViewModelUpdate.UpdateType.AddEmployee ? Visibility.Visible : Visibility.Hidden;
            TextBoxEmployeeId.Visibility = makeVisibleEmpIdFields;
            LabelEmployeeId.Visibility = makeVisibleEmpIdFields;
            ButtonFind.Visibility = makeVisibleEmpIdFields;
            switch (updateType)
            {
                case EmployeeViewModelUpdate.UpdateType.AddEmployee:
                    GridUpdateEmployee.Margin = new Thickness(0, -10, 0, 0);
                    TextBoxEmployeeName.Focus();
                    ButtonAdd.Content = "Add";
                    Title = "Add Employee Details";
                    break;

                case EmployeeViewModelUpdate.UpdateType.EditEmployee:
                    TextBoxEmployeeId.Focus();
                    ButtonAdd.Content = "Modify";
                    Title = "Modify Employee Details";
                    break;

                case EmployeeViewModelUpdate.UpdateType.RemoveEmployee:
                    TextBoxEmployeeId.Focus();
                    ButtonAdd.Content = "Remove";
                    Title = "Remove Employee Details";
                    break;

                default:
                    break;
            }
        }

        private void _employeeViewModel_ShowMessageToUserEvent(object sender, MessageToUserEventArgs args)
        {
            if (args.IsOperationSucceeded)
            {
                MessageBox.Show($"Employee (ID: {_employeeViewModelUpdate.EmployeeId}) details are updated sucessfully.", Title, MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show($"Failed to update the Employee (ID: {_employeeViewModelUpdate.EmployeeId}) details.", Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ButtonBack_Click(object sender, RoutedEventArgs e)
        {
            Debug.Print(_employeeViewModelUpdate.EmployeeName);
            NavigationService.GoBack();
        }

        private void ButtonFind_Click(object sender, RoutedEventArgs e)
        {
            string empId = TextBoxEmployeeId.Text.Trim();
            try
            {
                int.TryParse(empId, out int employeeId);
                if (employeeId == 0)
                {
                    MessageBox.Show("Please enter valid Employee Id.", Title, MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    Task employeesLoadingTask = Task.Run(() => _employeeViewModelQuery.GetEmployeesAsync(empId));
                    Mouse.OverrideCursor = Cursors.Wait;
                    //wait till the retrieval process completes
                    employeesLoadingTask.Wait();
                    Mouse.OverrideCursor = null;
                    if (_employeeViewModelQuery.RetrievedEmployees.Count > 1)
                    {
                        MessageBox.Show("More than one employee exist for the given filter.", Title, MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else if (_employeeViewModelQuery.RetrievedEmployees.Count == 0)
                    {
                        MessageBox.Show("No employee is found for the given Id.", Title, MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        _employeeViewModelUpdate.EmployeeId = _employeeViewModelQuery.RetrievedEmployees[0].Id;
                        _employeeViewModelUpdate.EmployeeName = _employeeViewModelQuery.RetrievedEmployees[0].Name;
                        _employeeViewModelUpdate.EmployeeEmail = _employeeViewModelQuery.RetrievedEmployees[0].Email;
                        _employeeViewModelUpdate.EmployeeGender = _employeeViewModelQuery.RetrievedEmployees[0].Gender;
                    }
                }
            }
            catch (Exception ec)
            {
                MessageBox.Show("Error while updating the employee details." + Environment.NewLine +
                    $"Error: {ec.Message}", Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}