using System;
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Windows;

namespace TaskForSgmk
{
    public partial class AddWindow : Window
    {
        private readonly Regex _isDigit = new Regex("[^1-9.-]+");

        public AddWindow()
        {
            InitializeComponent();
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            DataTable table = new DataTable();
            HamsterKindTable(table);
            FillHamsterBox(table);
        }

        private void HamsterKindTable(DataTable table)
        {
            SqlConnection connection = new SqlConnection(SingletonData.Instance.GetConnectionString());
            SqlDataAdapter adapter = new SqlDataAdapter("SELECT hamster_kind_id, hamster_name FROM Kind", connection);
            adapter.Fill(table);
        }

        private void FillHamsterBox(DataTable table)
        {
            kindBox.ItemsSource = table.DefaultView;
            kindBox.DisplayMemberPath = "hamster_name";
            kindBox.SelectedValuePath = "hamster_kind_id";
        }

        private void AcceptButtonClick(object sender, RoutedEventArgs e)
        {
            if (kindBox.SelectedIndex < 0)
            {
                MessageBox.Show("Вы не выбрали вид хомяка");
            }
            else if (ParseToInt(weightBox.Text))
            {
                MessageBox.Show("Введите вес");
            }
            else if (ParseToInt(ageBox.Text))
            {
                MessageBox.Show("Введите возраст");
            }
            else if (string.IsNullOrWhiteSpace(rationBox.Text))
            {
                MessageBox.Show("Введите рацион");
            }
            else
            {
                DialogResult = true;
            }
        }

        private bool ParseToInt(string str)
        {
            return _isDigit.IsMatch(str);
        }
    }
}
