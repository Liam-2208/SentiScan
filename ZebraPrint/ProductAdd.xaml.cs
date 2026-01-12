using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SentiScan
{
    /// <summary>
    /// Interaction logic for ProductAdd.xaml
    /// </summary>
    public partial class ProductAdd : Window
    {
        public ProductAdd()
        {
            InitializeComponent();
        }


        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }


        //Product Add Function
        private void AddProducts()
        {
            using (SqlConnection dbConnection = new SqlConnection(string.Format("Server={0}; database={1}; User Id={2}; Password={3};", Properties.Settings.Default.ServerName, Properties.Settings.Default.DatabaseName, Properties.Settings.Default.DatabaseUser, Properties.Settings.Default.DatabasePass)))
            {

                try
                {
                    dbConnection.Open();
                    String query = "SELECT COUNT(*) FROM tbl_product_types WHERE Prefix = @Prefix";
                    using (SqlCommand command = new SqlCommand(query, dbConnection))
                    {
                        command.Parameters.AddWithValue("@Prefix", Prefixtxt.Text);

                        int count = (int)command.ExecuteScalar();

                        if (count > 0)
                        {
                            MessageBox.Show("A product with this prefix already exists. Please choose a different prefix.");
                            dbConnection.Close();
                            return;
                        }
                    }
                }
                catch (SqlException ex)
                {
                    MessageBox.Show("UserCheck: SQL Exception occurred while trying retrieve tbl_product_types." + ex.SqlState);
                }


                try
                {
                    String query = "INSERT INTO dbo.tbl_product_types (Prefix,Name,Description,PartId) VALUES (@Prefix,@Name,@Description,@PartID)";
                    using (SqlCommand command = new SqlCommand(query, dbConnection))
                    {
                        command.Parameters.AddWithValue("@Prefix", Prefixtxt.Text);
                        command.Parameters.AddWithValue("@Name", ProductNametxt.Text);
                        command.Parameters.AddWithValue("@Description", Descriptiontxt.Text);
                        command.Parameters.AddWithValue("@PartID", "1");

                       int result = command.ExecuteNonQuery();


                        dbConnection.Close();
                        dbConnection.Dispose();

                        // Check Error 
                        if (result < 0)
                            Console.WriteLine("Error inserting data into Database!");
                        if (result > 0)
                        {
                            MessageBox.Show("Product Added Successfully!");
                            ProductWindow productWindow = new ProductWindow();
                            productWindow.Show();
                            this.Close();
                        }
                    }
                }
                catch (SqlException ex)
                {
                    MessageBox.Show("UserCheck: SQL Exception occurred while trying retrieve tbl_product_types." + ex.SqlState);
                }
            }


        }


        // Goes back to previous window
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            ProductWindow productWindow = new ProductWindow();
            productWindow.Show();
            this.Close();
        }
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            AddProducts();
        }
    }
}

