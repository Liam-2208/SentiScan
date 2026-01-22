
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;

namespace SentiScan
{
    public partial class ViewProductWindow : Window
    {
        private readonly int _productId;
        private List<int> _selectedPartIDs = new();
        private const string ThemeKey = "AppTheme"; // optional: persist in Properties.Settings

        public ViewProductWindow(int productId)
        {
            InitializeComponent();
            _productId = productId;

            // Apply last theme or default to Light
            var current = LoadThemePreference();
            ApplyTheme(current);

            LoadProduct();
            LoadParts();
        }

        #region Data Loading

        private void LoadProduct()
        {
            using var conn = new SqlConnection(
                $"Server={Properties.Settings.Default.ServerName}; " +
                $"Database={Properties.Settings.Default.DatabaseName}; " +
                $"User Id={Properties.Settings.Default.DatabaseUser}; " +
                $"Password={Properties.Settings.Default.DatabasePass};");

            conn.Open();

            using var cmd = new SqlCommand("SELECT * FROM tbl_product_types WHERE ID=@ID", conn);
            cmd.Parameters.AddWithValue("@ID", _productId);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                PrefixBox.Text = reader["Prefix"]?.ToString();
                NameBox.Text = reader["Name"]?.ToString();
                DescriptionBox.Text = reader["Description"]?.ToString();
                ImageBox.Text = reader["Image"]?.ToString();

                var partIDs = reader["PartID"]?.ToString() ?? string.Empty;
                _selectedPartIDs = partIDs
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => int.TryParse(s, out var v) ? v : (int?)null)
                    .Where(v => v.HasValue)
                    .Select(v => v!.Value)
                    .ToList();

                LoadPreviewImage(ImageBox.Text);
            }
        }

        private void LoadParts()
        {
            using var conn = new SqlConnection(
                $"Server={Properties.Settings.Default.ServerName}; " +
                $"Database={Properties.Settings.Default.DatabaseName}; " +
                $"User Id={Properties.Settings.Default.DatabaseUser}; " +
                $"Password={Properties.Settings.Default.DatabasePass};");

            conn.Open();

            using var cmd = new SqlCommand("SELECT ID, Name FROM tbl_part_types", conn);
            using var da = new SqlDataAdapter(cmd);
            var dt = new DataTable();
            da.Fill(dt);

            var names = dt.AsEnumerable()
                          .Where(r => _selectedPartIDs.Contains(r.Field<int>("ID")))
                          .Select(r => r.Field<string>("Name"))
                          .ToList();

            PartsChips.ItemsSource = names;
        }

        private void LoadPreviewImage(string? path)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
                {
                    var bmp = new BitmapImage();
                    bmp.BeginInit();
                    bmp.CacheOption = BitmapCacheOption.OnLoad;
                    bmp.UriSource = new Uri(path);
                    bmp.EndInit();
                    bmp.Freeze();
                    ProductImage.Source = bmp;
                    ImageFallback.Visibility = Visibility.Collapsed;
                }
                else
                {
                    ProductImage.Source = null;
                    ImageFallback.Visibility = Visibility.Visible;
                }
            }
            catch
            {
                ProductImage.Source = null;
                ImageFallback.Visibility = Visibility.Visible;
            }
        }

        #endregion

        #region Theme

        private string LoadThemePreference()
        {
            try
            {
                var saved = Properties.Settings.Default[ThemeKey]?.ToString();
                return string.IsNullOrWhiteSpace(saved) ? "Light" : saved!;
            }
            catch { return "Light"; }
        }

        private void SaveThemePreference(string value)
        {
            try
            {
                Properties.Settings.Default[ThemeKey] = value;
                Properties.Settings.Default.Save();
            }
            catch { /* ignore persistence errors */ }
        }

        private void ApplyTheme(string theme) // "Light" or "Dark"
        {
            // Remove any previous theme dictionaries
            var toRemove = Application.Current.Resources.MergedDictionaries
                .Where(d => d.Source != null &&
                            (d.Source.OriginalString.Contains("Themes/Colors.Light.xaml") ||
                             d.Source.OriginalString.Contains("Themes/Colors.Dark.xaml")))
                .ToList();

            foreach (var dict in toRemove)
                Application.Current.Resources.MergedDictionaries.Remove(dict);

            // Add selected one
            var uri = new Uri($"Themes/Colors.{theme}.xaml", UriKind.Relative);
            Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = uri });

            // Sync toggle
            ThemeToggleButton.IsChecked = string.Equals(theme, "Dark", StringComparison.OrdinalIgnoreCase);

            // Persist
            SaveThemePreference(theme);
        }

        private void ThemeToggleButton_Checked(object sender, RoutedEventArgs e)
            => ApplyTheme("Dark");

        private void ThemeToggleButton_Unchecked(object sender, RoutedEventArgs e)
            => ApplyTheme("Light");

        #endregion

        #region Actions

        private void Close_Click(object sender, RoutedEventArgs e) => Close();

        private void CopyImagePath_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(ImageBox.Text))
                {
                    Clipboard.SetText(ImageBox.Text);
                    // Optional: show a subtle toast/snackbar; keeping it silent here.
                }
            }
            catch { /* ignore */ }
        }

        private void OpenImage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(ImageBox.Text) && File.Exists(ImageBox.Text))
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = ImageBox.Text,
                        UseShellExecute = true
                    });
            }
            catch { /* ignore */ }
        }

        #endregion
    }
}
