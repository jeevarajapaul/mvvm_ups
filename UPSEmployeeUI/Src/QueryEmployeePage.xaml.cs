using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using UPSEmployeeLib.General;
using UPSEmployeeLib.Model;
using UPSEmployeeLib.ViewModel;
using static UPSEmployeeLib.ViewModel.EmplyeeViewModelQuery;

namespace UPSAssessment.UPSEmployeeUI
{
    /// <summary>
    /// Interaction logic for QueryEmployeeInfo.xaml
    /// </summary>
    public partial class QueryEmployeePage : Page
    {
        private string _outputFileName = string.Empty;
        private int _pageNoToRetrieve = 1;
        private readonly EmplyeeViewModelQuery _employeeViewModelQuery;
        private readonly SearchFilter _searchFilter = SearchFilter.NoFilter;

        public QueryEmployeePage(IConnectionHandler connectionHandler, SearchFilter searchFilter)
        {
            InitializeComponent();
            _searchFilter = searchFilter;
            EmployeeQueryHandlerWebServer employeeQueryHandlerWebServer = new EmployeeQueryHandlerWebServer(connectionHandler);
            _employeeViewModelQuery = new EmplyeeViewModelQuery(employeeQueryHandlerWebServer, _searchFilter);
            _employeeViewModelQuery.ShowMessageToUserEvent += _employeeViewModelQuery_ShowMessageToUserEvent;
            DataContext = _employeeViewModelQuery;
            InitializeUIElementsBasedOnFilter();
        }

        private void _employeeViewModelQuery_ShowMessageToUserEvent(object sender, MessageToUserEventArgs args)
        {
            if (!args.IsOperationSucceeded)
            {
                //dispatcher is required, since this function is called from an async (another thread) function
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    MessageBox.Show(args.MessageToShow, Title, MessageBoxButton.OK, MessageBoxImage.Information);
                }));
            }
        }

        private void InitializeUIElementsBasedOnFilter()
        {
            switch (_searchFilter)
            {
                case SearchFilter.NoFilter:
                    LabelSearchItem.Visibility = Visibility.Hidden;
                    TextBoxSearchEmployee.Visibility = Visibility.Hidden;
                    ButtonSearch.HorizontalAlignment = HorizontalAlignment.Left;
                    ButtonSearch.Content = "View";
                    ButtonSearch.Margin = new Thickness(5, 5, 0, 0);
                    ButtonNextPage.Visibility = Visibility.Hidden;
                    ButtonPreviousPage.Visibility = Visibility.Hidden;
                    break;

                case SearchFilter.ByEmployeeId:
                    LabelSearchItem.Content = "Employee Id: ";
                    LabelSearchItem.Width = 150;
                    TextBoxSearchEmployee.Margin = new Thickness(110, 5, 0, 0);
                    TextBoxSearchEmployee.Width = 100;
                    ButtonNextPage.Visibility = Visibility.Hidden;
                    ButtonPreviousPage.Visibility = Visibility.Hidden;
                    break;

                case SearchFilter.ByEmployeeName:
                    LabelSearchItem.Content = "Employee Name: ";
                    LabelSearchItem.Width = 150;
                    TextBoxSearchEmployee.Margin = new Thickness(120, 5, 0, 0);
                    TextBoxSearchEmployee.Width = 250;
                    ButtonNextPage.Visibility = Visibility.Hidden;
                    ButtonPreviousPage.Visibility = Visibility.Hidden;
                    break;

                case SearchFilter.ByPageNumber:
                    LabelSearchItem.Content = "Page Number: ";
                    LabelSearchItem.Width = 100;
                    TextBoxSearchEmployee.Margin = new Thickness(110, 5, 0, 0);
                    TextBoxSearchEmployee.Width = 75;
                    ButtonSearch.Content = "View";
                    ButtonNextPage.Visibility = Visibility.Visible;
                    ButtonPreviousPage.Visibility = Visibility.Visible;
                    break;

                default:
                    break;
            }
            TextBoxSearchEmployee.Focus();
        }

        private void RetrieveAndShowEmployees(string filterToApply = "")
        {
            bool showWorkitems = true;
            string txtToDisplayInLabel = "Retrieving the selected employees' information ...";

            if (DataGridEmployees.HasItems && _searchFilter == SearchFilter.NoFilter)
            {
                MessageBoxResult messageBoxResult = MessageBox.Show
                        ("Loading employees takes a while." + Environment.NewLine +
                        "Do you still want to clear the currently loaded data and preceed with the search?"
                        , Title, MessageBoxButton.YesNo);
                showWorkitems = (messageBoxResult == MessageBoxResult.Yes);
            }

            if (showWorkitems)
            {
                LabelPageInfo.Content = txtToDisplayInLabel;
                Task employeesLoadingTask;

                if (_searchFilter == SearchFilter.ByPageNumber)
                {
                    txtToDisplayInLabel = $"Retrieving the employees' information in page {_pageNoToRetrieve} of {_employeeViewModelQuery.TotalNoOfPages} ...";
                    employeesLoadingTask = Task.Run(() => _employeeViewModelQuery.GetEmployeesAsync(_pageNoToRetrieve.ToString()));
                }
                else
                {
                    employeesLoadingTask = Task.Run(() => _employeeViewModelQuery.GetEmployeesAsync(filterToApply));
                }
                ButtonGoBack.IsEnabled = false;
                Mouse.OverrideCursor = Cursors.Wait;
                //wait till the retrieval process completes
                employeesLoadingTask.Wait();

                ButtonGoBack.IsEnabled = true;
                Mouse.OverrideCursor = null;
                DataGridEmployees.ItemsSource = _employeeViewModelQuery.RetrievedEmployees;
                if (DataGridEmployees.HasItems)
                {
                    DataGridEmployees.Columns.LastOrDefault().Width = new DataGridLength(1, DataGridLengthUnitType.Star);
                    if (_searchFilter == SearchFilter.ByPageNumber)
                    {
                        ButtonNextPage.IsEnabled = _employeeViewModelQuery.TotalNoOfPages > _pageNoToRetrieve;
                        ButtonPreviousPage.IsEnabled = _pageNoToRetrieve > 1;
                        LabelPageInfo.Content = $"Employees in page {_pageNoToRetrieve} of {_employeeViewModelQuery.TotalNoOfPages}";
                    }
                    else
                    {
                        LabelPageInfo.Content = $"{ _employeeViewModelQuery.RetrievedEmployees.Count} employee(s) is/are found.";
                    }
                }
                else
                {
                    LabelPageInfo.Content = "No employee(s) is/are found for the given search critera.";
                }
                //Save the workitems (or serialize) into the XML
                //_outputFileName = Path.Combine(GeneralUtils.OutPath, $"QueryEmployeeInfo.xml");
                ExportToExcel();
            }
        }

        private void ExportToExcel()
        {
            try
            {
            }
            catch (Exception ec)
            {
                throw ec;
            }
        }

        private void ButtonGoBack_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        private void ButtonNextPage_Click(object sender, RoutedEventArgs e)
        {
            _pageNoToRetrieve++;
            RetrieveAndShowEmployees(string.Empty);
        }

        private void ButtonPreviousPage_Click(object sender, RoutedEventArgs e)
        {
            _pageNoToRetrieve--;
            RetrieveAndShowEmployees(string.Empty);
        }

        private void TextBoxSearchEmployee_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBoxSearchEmployee.SelectAll();
        }

        private void TextBoxSearchItem_GotMouseCapture(object sender, MouseEventArgs e)
        {
            TextBoxSearchEmployee.SelectAll();
        }

        private void ButtonSearch_Click(object sender, RoutedEventArgs e)
        {
            string searchEntry = TextBoxSearchEmployee.Text.Trim();
            if (_searchFilter == SearchFilter.NoFilter || _searchFilter == SearchFilter.ByPageNumber)
            {
                if (_searchFilter == SearchFilter.ByPageNumber && !string.IsNullOrEmpty(searchEntry))
                {
                    int.TryParse(searchEntry, out _pageNoToRetrieve);
                }
                RetrieveAndShowEmployees(string.Empty);
            }
            else
            {
                if (string.IsNullOrEmpty(searchEntry))
                {
                    string searchField = _searchFilter == SearchFilter.ByEmployeeId ? "Employee Id" : "Employee Name";
                    MessageBox.Show($"Please enter the '{searchField}' to be searched", Title, MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    RetrieveAndShowEmployees(searchEntry);
                }
            }
        }

        private void PageQueryEmployee_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;
        }

        private void PageQueryEmployee_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            e.Handled = true;
        }
    } // Class
} // Namespace