using Microsoft.VisualBasic;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
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

namespace SentiScan
{
    /// <summary>
    /// Interaction logic for ProductAdd.xaml
    /// </summary>
    
    public partial class ProductAdd : Window
    {
        string sourcePath;
        DataTable dtPartTypes = new DataTable();
        
        public ProductAdd()
        {
            InitializeComponent();
            GetProductsFromDatabase();
        }
       
    public List<int> SelectedPartIDs { get; set; } = new List<int>();

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        // Check Boxes functions
        private void PartCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox chk = sender as CheckBox;
            DataRowView row = chk.DataContext as DataRowView;
            if (row == null) return;

            int id = Convert.ToInt32(row["ID"]);

            if (!SelectedPartIDs.Contains(id))
                SelectedPartIDs.Add(id);
        }

        private void PartCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckBox chk = sender as CheckBox;
            DataRowView row = chk.DataContext as DataRowView;
            if (row == null) return;

            int id = Convert.ToInt32(row["ID"]);

            if (SelectedPartIDs.Contains(id))
                SelectedPartIDs.Remove(id);
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

                // Choose destination folder
                string destinationFolder = @"W:\SentiScan\Products";

                // Make sure folder exists
                Directory.CreateDirectory(destinationFolder);

                // Build the full destination path
                string destinationPath = Path.Combine(destinationFolder, Path.GetFileName(sourcePath));

                try
                {
                    File.Move(sourcePath, destinationPath);
                    MessageBox.Show("File moved successfully!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }

                try
                {
                    String query = "INSERT INTO dbo.tbl_product_types (Prefix,Name,Description,Image,PartId) VALUES (@Prefix,@Name,@Description,@Image,@PartID)";
                    using (SqlCommand command = new SqlCommand(query, dbConnection))
                    {
                        command.Parameters.AddWithValue("@Prefix", Prefixtxt.Text);
                        command.Parameters.AddWithValue("@Name", ProductNametxt.Text);
                        command.Parameters.AddWithValue("@Description", Descriptiontxt.Text);
                        command.Parameters.AddWithValue("@Image", destinationPath);

                        string partList = string.Join(",", SelectedPartIDs);
                        command.Parameters.AddWithValue("@PartID", partList);


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
                    MessageBox.Show("SQL ERROR: " + ex.Message + "\n\n" + ex.ToString());

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

        public void GetProductsFromDatabase()
        {
            using (SqlConnection dbConnection = new SqlConnection(string.Format("Server={0}; database={1}; User Id={2}; Password={3};", Properties.Settings.Default.ServerName, Properties.Settings.Default.DatabaseName, Properties.Settings.Default.DatabaseUser, Properties.Settings.Default.DatabasePass)))
            {
                try
                {
                    dbConnection.Open();
                    string query = "SELECT * FROM tbl_part_types";
                    using (SqlCommand command = new SqlCommand(query, dbConnection))
                    {


                        // create data adapter
                        SqlDataAdapter dataAdapter = new SqlDataAdapter(command);
                        // this will query your database and return the result to your datatable
                        dataAdapter.Fill(dtPartTypes);
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
            if (dtPartTypes.Rows.Count > 0)
            {
                // Bind the DataTable to the DataGrid
                MyParts.ItemsSource = dtPartTypes.DefaultView;

                // Fill DataGrid Columns to Width of datagrid
                MyParts.ColumnWidth = new DataGridLength(1, DataGridLengthUnitType.Star);


            }
        }




        





        private void UploadButton_Click(object sender, RoutedEventArgs e)
        {
            // being able to upload an image for the product
            Microsoft.Win32.OpenFileDialog openBrowser = new OpenFileDialog();

            bool? result = openBrowser.ShowDialog();


            if (result == true)
            {
                sourcePath = openBrowser.FileName;
                
            }
        }

    }
}

    


