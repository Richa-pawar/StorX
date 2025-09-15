using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace StorX
{
    public partial class Reports : Form
    {
        string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=""D:\Programs\Inventory Management System\Inventory Management System\StorX\StorX_DB.mdf"";Integrated Security=True";
        //string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=""D:\Inventory Management System\Inventory Management System\StorX\StorX_DB.mdf"";Integrated Security=True";
        int lowStockThreshold = 10;
        public Reports()
        {
            InitializeComponent();
        }

        private void Reports_Load(object sender, EventArgs e)
        {
            LoadCategories();
            LoadReport("current"); // Default view
        }

        private void LoadCategories()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = "SELECT DISTINCT Category FROM Products";
                SqlDataAdapter da = new SqlDataAdapter(query, con);
                DataTable dt = new DataTable();
                da.Fill(dt);
                cbCategory.DataSource = dt;
                cbCategory.DisplayMember = "Category";
                cbCategory.ValueMember = "Category";
                cbCategory.SelectedIndex = -1;
            }
        }

        private void cbCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbCategory.SelectedIndex >= 0)
                LoadProductsByCategory(cbCategory.Text);
        }

        private void LoadProductsByCategory(string category)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = "SELECT ProductID, Name FROM Products WHERE Category=@Category";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@Category", category);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                cbProducts.DataSource = dt;
                cbProducts.DisplayMember = "Name";
                cbProducts.ValueMember = "ProductID";
                cbProducts.SelectedIndex = -1;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            LoadReport("current");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            LoadReport("low");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            LoadReport("history");
        }

        private void LoadReport(string type)
        {
            int? productId = cbProducts.SelectedValue != null ? (int?)Convert.ToInt32(cbProducts.SelectedValue) : null;
            string category = cbCategory.SelectedIndex >= 0 ? cbCategory.Text : null;

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = "";

                if (type == "current")
                {
                    query = @"SELECT ProductID, Name AS ProductName, Category, Price, Quantity,
                              (Price * Quantity) AS TotalValue
                              FROM Products
                              WHERE (@Category IS NULL OR Category = @Category)
                                AND (@ProductID IS NULL OR ProductID = @ProductID)
                              ORDER BY Category, Name";
                }
                else if (type == "low")
                {
                    query = @"SELECT ProductID, Name AS ProductName, Category, Price, Quantity,
                              (Price * Quantity) AS TotalValue
                              FROM Products
                              WHERE Quantity <= @LowStockThreshold
                                AND (@Category IS NULL OR Category = @Category)
                                AND (@ProductID IS NULL OR ProductID = @ProductID)
                              ORDER BY Category, Name";
                }
                else if (type == "history")
                {
                    query = @"SELECT t.TransactionID, p.Name AS Product, t.Category, t.TransactionType, t.Quantity, t.TransactionDate
                              FROM StockTransactions t
                              INNER JOIN Products p ON t.ProductID = p.ProductID
                              WHERE (@Category IS NULL OR t.Category = @Category)
                                AND (@ProductID IS NULL OR t.ProductID = @ProductID)
                              ORDER BY t.TransactionDate DESC";
                }

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@Category", (object)category ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ProductID", (object)productId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@LowStockThreshold", lowStockThreshold);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                dgvReport.DataSource = dt;

                if (type != "history")
                    LoadChart(dt, type == "low" ? "Low Stock" : "Current Stock", SeriesChartType.Column);
                else
                    LoadChart(dt, "Stock In vs Stock Out", SeriesChartType.Pie, true);
            }
        }

        private void LoadChart(DataTable dt, string title, SeriesChartType chartType, bool isTransaction = false)
        {
            chartReport.Series.Clear();
            chartReport.Titles.Clear();
            chartReport.Titles.Add(title);

            Series series = new Series(title);
            series.ChartType = chartType;

            if (isTransaction)
            {
                var grouped = dt.DefaultView.ToTable(true, "TransactionType");
                foreach (DataRow row in grouped.Rows)
                {
                    string type = row["TransactionType"].ToString();
                    int totalQty = Convert.ToInt32(dt.Compute("SUM(Quantity)", $"TransactionType = '{type}'"));
                    series.Points.AddXY(type, totalQty);
                }
            }
            else
            {
                foreach (DataRow row in dt.Rows)
                {
                    series.Points.AddXY(row["ProductName"], row["Quantity"]);
                }
            }

            chartReport.Series.Add(series);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }

}
