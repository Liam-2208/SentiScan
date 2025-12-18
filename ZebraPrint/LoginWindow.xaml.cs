using System.Configuration;
using System.Data.SqlClient;
using System.Security.AccessControl;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using SentiScan.View;

namespace SentiScan
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        private TextBlock txtPasswordCount;

        public LoginWindow()
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
                   

                    // Check if user exists in database
                    if (UserCheck(txtUsername.Text) > 0)
                    {
                        
                    }
                    else
                    {
                        
                    }
                    if (PassCheck(txtPassword.Password) > 0)
                    {
                        

                        // Opens Home Window

                        HomeWindow homeWindow = new HomeWindow();
                        homeWindow.Show();
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("Incorrect Password");
                    }


                }
                else
                {
                   
                }  

            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
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