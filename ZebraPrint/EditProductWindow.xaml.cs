
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace SentiScan
{
    public partial class EditProductWindow : Window
    {
        private int _productId;
        DataTable dtParts = new DataTable();
        public ObservableCollection<PartSelectionModel> PartsList { get; set; }

        public EditProductWindow(int productId)
        {
            InitializeComponent();
            _productId = productId;

            LoadProduct();
            LoadParts();

            this.DataContext = this;
        }

        private void LoadProduct()
        {
            using (SqlConnection conn = new SqlConnection(
                $"Server={Properties.Settings.Default.ServerName}; database={Properties.Settings.Default.DatabaseName}; User Id={Properties.Settings.Default.DatabaseUser}; Password={Properties.Settings.Default.DatabasePass};"))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT * FROM tbl_product_types WHERE ID=@ID", conn);
                cmd.Parameters.AddWithValue("@ID", _productId);

                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    PrefixBox.Text = reader["Prefix"].ToString();
                    NameBox.Text = reader["Name"].ToString();
                    DescriptionBox.Text = reader["Description"].ToString();
                    ImageBox.Text = reader["Image"].ToString();

                    string partIDs = reader["PartID"].ToString();
                    SelectedPartIDs = partIDs.Split(',').Where(x => x != "").Select(int.Parse).ToList();
                }
            }
        }

        List<int> SelectedPartIDs = new List<int>();

        private void LoadParts()
        {
            using (SqlConnection conn = new SqlConnection(
                $"Server={Properties.Settings.Default.ServerName}; database={Properties.Settings.Default.DatabaseName}; User Id={Properties.Settings.Default.DatabaseUser}; Password={Properties.Settings.Default.DatabasePass};"))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT * FROM tbl_part_types", conn);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                dtParts.Clear();
                da.Fill(dtParts);
            }



            PartsList = new ObservableCollection<PartSelectionModel>(
                    dtParts.AsEnumerable().Select(row => new PartSelectionModel
                    {
                        ID = row.Field<int>("ID"),
                        Name = row.Field<string>("Name"),
                        IsSelected = SelectedPartIDs.Contains(row.Field<int>("ID"))
                    })
                );

            PartsGrid.ItemsSource = PartsList;
        }



        private void Save_Click(object sender, RoutedEventArgs e)
        {
            // Ask for confirmation before saving
            if (MessageBox.Show("Save changes?", "Confirm", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                return;

            // Clear existing selection
            SelectedPartIDs.Clear();

            foreach (PartSelectionModel item in PartsList)
            {

                if (item.IsSelected)
                {
                    SelectedPartIDs.Add(item.ID);
                }
                    
                
            }

            // STEP 2 — Convert the list to CSV format "1,2,3"
            string updatedParts = string.Join(",", SelectedPartIDs);

            // STEP 3 — Save to SQL
            using (SqlConnection conn = new SqlConnection(
                $"Server={Properties.Settings.Default.ServerName}; " +
                $"database={Properties.Settings.Default.DatabaseName}; " +
                $"User Id={Properties.Settings.Default.DatabaseUser}; " +
                $"Password={Properties.Settings.Default.DatabasePass};"))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand(
                    "UPDATE tbl_product_types " +
                    "SET Prefix=@Prefix, Name=@Name, Description=@Description, Image=@Image, PartID=@PartID " +
                    "WHERE ID=@ID",
                    conn);

                // Set parameters
                cmd.Parameters.AddWithValue("@Prefix", PrefixBox.Text);
                cmd.Parameters.AddWithValue("@Name", NameBox.Text);
                cmd.Parameters.AddWithValue("@Description", DescriptionBox.Text);
                cmd.Parameters.AddWithValue("@Image", ImageBox.Text);
                cmd.Parameters.AddWithValue("@PartID", updatedParts);
                cmd.Parameters.AddWithValue("@ID", _productId);

                cmd.ExecuteNonQuery();
            }

            // STEP 4 — Notify user
            MessageBox.Show("Product updated successfully!");

            // Close window
            this.DialogResult = true;
            this.Close();
        }


        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void BrowseImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            if (dlg.ShowDialog() == true)
                ImageBox.Text = dlg.FileName;
        }
    }
}

