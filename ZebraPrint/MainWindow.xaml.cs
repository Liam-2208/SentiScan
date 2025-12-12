using System.Configuration;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace ZebraPrint
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private TextBlock txtPasswordCount;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void textUsername_MouseDown(object sender, MouseButtonEventArgs e)
        {
            txtUsername.Focus();
        }

        private void txtUsername_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(txtUsername.Text) && txtUsername.Text.Length > 0)
            {
                textUsername.Visibility = Visibility.Collapsed;
            }
            else
            {
                textUsername.Visibility = Visibility.Visible;
            }
        }

        private void textPassword_MouseDown(object sender, MouseButtonEventArgs e)
        {
            txtPassword.Focus();
        }

        private void txtPassword_TextChanged(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(txtPassword.Password) && txtPassword.Password.Length > 0)
            {
                textPassword.Visibility = Visibility.Collapsed;
            }
            else
            {
                textPassword.Visibility = Visibility.Visible;
            }

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtUsername.Text) && !string.IsNullOrEmpty(txtPassword.Password))
            {
                MessageBox.Show("Please enter your username.");
            }
            else if (!string.IsNullOrEmpty(txtUsername.Text) && string.IsNullOrEmpty(txtPassword.Password))
            {
                MessageBox.Show("Please enter your password.");
            }
            else if (string.IsNullOrEmpty(txtUsername.Text) && string.IsNullOrEmpty(txtPassword.Password))
            {
                MessageBox.Show("Please enter your username and password.");
            }
            else
            {
                // Proceed with login

                // Test database connection
                if (TestSQLConnection("Sentinor-dc1", "SP2025", "SPAdmin", "Bounty+Mars1"))
                {
                    MessageBox.Show("Database connected successfully.");

                    // Check if user exists in database
                    if (UserCheck(txtUsername.Text) > 0)
                    {
                        MessageBox.Show("User found in database.");
                    }
                    else
                    {
                        MessageBox.Show("User not found in database.");
                    }
                    if (PassCheck(txtPassword.Password) > 0)
                    {
                        MessageBox.Show("Password Correct");
                    }
                    else
                    {
                        MessageBox.Show("Password not correct");
                    }


                }
                else
                {
                    MessageBox.Show("Database connection failed.");
                }  

            }
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        private static int UserCheck(string username)
        {
            using (SqlConnection dbConnection = new SqlConnection(string.Format("Server={0}; database={1}; User Id={2}; Password={3};", "Sentinor-dc1", "SP2025", "SPAdmin", "Bounty+Mars1")))
            {
                try
                {
                    dbConnection.Open();
                    string query = "SELECT COUNT(1) FROM tbl_Users WHERE Username = @Username";
                    using (SqlCommand command = new SqlCommand(query, dbConnection))
                    {
                        command.Parameters.AddWithValue("@Username", username);
                        int count = Convert.ToInt32(command.ExecuteScalar());
                        return count;
                    }
                }
                catch (SqlException ex)
                {
                    MessageBox.Show("UserCheck: SQL Exception occurred while trying to check the user." + ex.SqlState);
                    return 0;
                }
            }
        }

        private static int PassCheck(string password)
        {
            using (SqlConnection dbConnection = new SqlConnection(string.Format("Server={0}; database={1}; User Id={2}; Password={3};", "Sentinor-dc1", "SP2025", "SPAdmin", "Bounty+Mars1")))
            {
                try
                {
                    dbConnection.Open();
                    string query = "SELECT COUNT(1) FROM tbl_Users WHERE Password = @Password";
                    using (SqlCommand command = new SqlCommand(query, dbConnection))
                    {
                        command.Parameters.AddWithValue("@Password", password);
                        int count = Convert.ToInt32(command.ExecuteScalar());
                        return count;
                    }
                }
                catch (SqlException ex)
                {
                    MessageBox.Show("UserCheck: SQL Exception occurred while trying to check the user." + ex.SqlState);
                    return 0;
                }
            }
        }
        private static bool TestSQLConnection(string server, string database, string username, string password)
        {
            using (SqlConnection dbConnection = new SqlConnection(string.Format("Server={0}; database={1}; User Id={2}; Password={3};", server, database, username, password)))
            {
                try
                {
                    if (dbConnection != null)
                    {
                        dbConnection.Open();
                        dbConnection.Close();
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                catch (SqlException ex)
                {
                    MessageBox.Show("TestSQLConnection: SQL Exception occurred while trying to connect to the database." + ex.SqlState);
                    return false;
                }
            }
        }

    }
}