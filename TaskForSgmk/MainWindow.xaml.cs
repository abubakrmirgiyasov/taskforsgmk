using System;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace TaskForSgmk
{
    public class SingletonData
    {
        private static readonly Lazy<SingletonData> _instance = 
            new Lazy<SingletonData>(() => new SingletonData());
        private const string CONNECTION_STRING = "DefaultConnection";

        public static SingletonData Instance => _instance.Value;

        public SingletonData()
        {
            
        }
        
        public string GetConnectionString()
        {
            return ConfigurationManager.ConnectionStrings[CONNECTION_STRING].ConnectionString;
        }
    }

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            SqlConnection connection = new SqlConnection(SingletonData.Instance.GetConnectionString());
            connection.Open();

            SqlCommand command = new SqlCommand("SELECT hamster_id, Kind.hamster_name, hamster_weight, hamster_age, hamster_ration FROM Hamster " +
                "INNER JOIN Kind ON Kind.hamster_kind_id=Hamster.hamster_kind", connection);
            {
                command.CommandType = CommandType.Text;
            }
            DataTable table = new DataTable();

            SqlDataReader reader = command.ExecuteReader();
            
            table.Load(reader);
            
            dataGrid.ItemsSource = table.DefaultView;
            connection.Close();
        }

        private void AddButtonClick(object sender, RoutedEventArgs e)
        {
            AddWindow add = new AddWindow();

            if (add.ShowDialog() == null)
            {
                add.ShowDialog();
            }
            else
            {
                add.Activate();
            }

            if (add.DialogResult == true)
            {
                try
                {
                    SqlConnection connection = new SqlConnection(SingletonData.Instance.GetConnectionString());
                    SqlCommand command = new SqlCommand("INSERT INTO Hamster VALUES(@hamster_kind, @hamster_weight, @hamster_age, @hamster_ration)", connection);
                    
                    connection.Open();

                    command.CommandType = CommandType.Text;

                    command.Parameters.AddWithValue("@hamster_kind", add.kindBox.SelectedValue);
                    command.Parameters.AddWithValue("@hamster_weight", add.weightBox.Text);
                    command.Parameters.AddWithValue("@hamster_age", add.ageBox.Text);
                    command.Parameters.AddWithValue("@hamster_ration", add.rationBox.Text);

                    command.ExecuteNonQuery();

                    WindowLoaded(sender, e);

                    connection.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void ChangeButtonClick(object sender, RoutedEventArgs e)
        {
            AddWindow add = new AddWindow();

            if (dataGrid.SelectedItem != null)
            {
                if (add.ShowDialog() == true)
                {
                    var cellInfo = dataGrid.SelectedCells[0];
                    var content = (cellInfo.Column.GetCellContent(cellInfo.Item) as TextBlock).Text;

                    try
                    {
                        SqlConnection connection = new SqlConnection(SingletonData.Instance.GetConnectionString());
                        connection.Open();

                        SqlCommand command = new SqlCommand($"UPDATE Hamster SET " +
                            $"hamster_kind = '{add.kindBox.SelectedValue}', " +
                            $"hamster_weight = '{add.weightBox.Text}', " +
                            $"hamster_age = '{add.ageBox.Text}', " +
                            $"hamster_ration = '{add.rationBox.Text}' " +
                            $"WHERE hamster_id = {content}", connection);

                        command.ExecuteNonQuery();

                        WindowLoaded(sender, e);

                        connection.Close();
                    }
                    catch (Exception exception)
                    {
                        MessageBox.Show(exception.Message);
                    }
                }
            }
            else
            {
                MessageBox.Show("Choose item!");
            }
        }

        private void DeleteButtonClick(object sender, RoutedEventArgs e)
        {
            if (dataGrid.SelectedItem != null)
            {
                var message = MessageBox.Show("Are you sure?", "Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                var cellInfo = dataGrid.SelectedCells[0];
                var content = (cellInfo.Column.GetCellContent(cellInfo.Item) as TextBlock).Text;

                if (message == MessageBoxResult.Yes)
                {
                    try
                    {
                        SqlConnection connection = new SqlConnection(SingletonData.Instance.GetConnectionString());
                        connection.Open();

                        SqlCommand command = new SqlCommand($"DELETE FROM Hamster WHERE hamster_id = {content}", connection);

                        command.ExecuteNonQuery();

                        connection.Close();
                    }
                    catch (Exception exception)
                    {
                        MessageBox.Show(exception.Message);
                    }

                    WindowLoaded(sender, e);
                }
            }
            else
            {
                MessageBox.Show("Choose item!");
            }
        }

        private void SortByAscending(object sender, MouseButtonEventArgs e)
        {
            SortDataGrid(dataGrid, 0, ListSortDirection.Ascending);
        }

        private void SortByDescending(object sender, MouseButtonEventArgs e)
        {
            SortDataGrid(dataGrid, 0, ListSortDirection.Descending);
        }

        public static void SortDataGrid(DataGrid dataGrid, int columnIndex, ListSortDirection sortDirection)
        {
            var column = dataGrid.Columns[columnIndex];
            dataGrid.Items.SortDescriptions.Clear();
            dataGrid.Items.SortDescriptions.Add(new SortDescription(column.SortMemberPath, sortDirection));

            foreach (var col in dataGrid.Columns)
            {
                col.SortDirection = null;
            }

            column.SortDirection = sortDirection;
            dataGrid.Items.Refresh();
        }
    }
}
