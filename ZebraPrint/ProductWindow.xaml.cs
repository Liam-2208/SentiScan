using System;
using System.Collections.Generic;
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
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace SentiScan
{
    /// <summary>
    /// Interaction logic for ProductWindow.xaml
    /// </summary>
    public partial class ProductWindow : Window
    {
        DataTable dtProductTypes = new DataTable();

        public ProductWindow()
        {
            InitializeComponent();

            // Retrieve product types from the database
            GetProductsFromDatabase();
        }
        private void AddProductButton_Click(object sender, RoutedEventArgs e)
        {
            ProductAdd productAdd = new ProductAdd();
            productAdd.Show();
            this.Close();


        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void GetProductsFromDatabase()
        {
            using (SqlConnection dbConnection = new SqlConnection(string.Format("Server={0}; database={1}; User Id={2}; Password={3};", Properties.Settings.Default.ServerName, "SP2025", "SPAdmin", "Bounty+Mars1")))
            {
                try
                {
                    dbConnection.Open();
                    string query = "SELECT * FROM tbl_product_types";
                    using (SqlCommand command = new SqlCommand(query, dbConnection))
                    {


                        // create data adapter
                        SqlDataAdapter dataAdapter = new SqlDataAdapter(command);
                        // this will query your database and return the result to your datatable
                        dataAdapter.Fill(dtProductTypes);
                        dbConnection.Close();
                        dataAdapter.Dispose();
                    }
                }
                catch (SqlException ex)
                {
                    MessageBox.Show("UserCheck: SQL Exception occurred while trying retrieve tbl_product_types." + ex.SqlState);
                }
            }

            // Check if any rows exist
            if (dtProductTypes.Rows.Count > 0)
            {
                // Bind the DataTable to the DataGrid
                MyProducts.DataContext = dtProductTypes.DefaultView;
            }
        }
    }


    public class Product
    {
        public int Id { get; set; }
        public string Prefix { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
     

   

        private void Close()
        {
            throw new NotImplementedException();
        }
    } }
    
    
