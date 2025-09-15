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

namespace StorX
{
    public partial class Stock : Form
    {
        string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=""D:\Programs\Inventory Management System\Inventory Management System\StorX\StorX_DB.mdf"";Integrated Security=True";
        //string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=""D:\Inventory Management System\Inventory Management System\StorX\StorX_DB.mdf"";Integrated Security=True";
        public Stock()
        {
            InitializeComponent();
        }

        private void Stock_Load(object sender, EventArgs e)
        {
            LoadCategories();
            LoadTransactions();
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
            }
        }

        private void cbCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
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
            }
        }

        private void btnStockIn_Click(object sender, EventArgs e)
        {
            //int productId = Convert.ToInt32(cbProducts.SelectedValue);
            //int qty = Convert.ToInt32(txtQty.Text);

            //using (SqlConnection con = new SqlConnection(connectionString))
            //{
            //    con.Open();

            //    SqlCommand updateCmd = new SqlCommand("UPDATE Products SET Quantity = Quantity + @Qty WHERE ProductID=@ProductID", con);
            //    updateCmd.Parameters.AddWithValue("@Qty", qty);
            //    updateCmd.Parameters.AddWithValue("@ProductID", productId);
            //    updateCmd.ExecuteNonQuery();

            //    SqlCommand insertCmd = new SqlCommand("INSERT INTO StockTransactions (ProductID, TransactionType, Quantity) VALUES (@ProductID, 'IN', @Qty)", con);
            //    insertCmd.Parameters.AddWithValue("@ProductID", productId);
            //    insertCmd.Parameters.AddWithValue("@Qty", qty);
            //    insertCmd.ExecuteNonQuery();

            //    con.Close();
            //}

            //MessageBox.Show("✅ Stock Added Successfully!");
            //LoadCurrentStock(productId);
            //LoadTransactions();
            ProcessTransaction("IN");
        }



        private void btnStockOut_Click(object sender, EventArgs e)
        {
            //int productId = Convert.ToInt32(cbProducts.SelectedValue);
            //int qty = Convert.ToInt32(txtQty.Text);

            //using (SqlConnection con = new SqlConnection(connectionString))
            //{
            //    // Check available stock first
            //    SqlCommand checkCmd = new SqlCommand("SELECT Quantity FROM Products WHERE ProductID=@ProductID", con);
            //    checkCmd.Parameters.AddWithValue("@ProductID", productId);
            //    con.Open();
            //    int currentStock = Convert.ToInt32(checkCmd.ExecuteScalar());

            //    if (currentStock < qty)
            //    {
            //        MessageBox.Show("❌ Not enough stock available!");
            //        con.Close();
            //        return;
            //    }

            //    // Deduct stock
            //    SqlCommand updateCmd = new SqlCommand("UPDATE Products SET Quantity = Quantity - @Qty WHERE ProductID=@ProductID", con);
            //    updateCmd.Parameters.AddWithValue("@Qty", qty);
            //    updateCmd.Parameters.AddWithValue("@ProductID", productId);
            //    updateCmd.ExecuteNonQuery();

            //    // Log transaction
            //    SqlCommand insertCmd = new SqlCommand("INSERT INTO StockTransactions (ProductID, TransactionType, Quantity) VALUES (@ProductID, 'OUT', @Qty)", con);
            //    insertCmd.Parameters.AddWithValue("@ProductID", productId);
            //    insertCmd.Parameters.AddWithValue("@Qty", qty);
            //    insertCmd.ExecuteNonQuery();

            //    con.Close();

            //    MessageBox.Show("✅ Stock Removed Successfully!");
            //    LoadCurrentStock(productId);
            //    LoadTransactions();
            //}

            ProcessTransaction("OUT");
        }

        private void ProcessTransaction(string type)
        {
            if (cbProducts.SelectedValue == null)
            {
                MessageBox.Show("Please select a product.");
                return;
            }

            int productId = Convert.ToInt32(cbProducts.SelectedValue);
            int qty = Convert.ToInt32(txtQty.Text);

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                SqlTransaction tran = con.BeginTransaction();

                try
                {
                    // Insert into StockTransactions
                    string insertQuery = @"INSERT INTO StockTransactions (ProductID, Category, TransactionType, Quantity)
                                           VALUES (@ProductID, @Category, @Type, @Qty)";
                    SqlCommand cmdInsert = new SqlCommand(insertQuery, con, tran);
                    cmdInsert.Parameters.AddWithValue("@ProductID", productId);
                    cmdInsert.Parameters.AddWithValue("@Category", cbCategory.Text);
                    cmdInsert.Parameters.AddWithValue("@Type", type);
                    cmdInsert.Parameters.AddWithValue("@Qty", qty);
                    cmdInsert.ExecuteNonQuery();

                    // Update Products Quantity
                    string updateQuery = type == "IN"
                        ? "UPDATE Products SET Quantity = Quantity + @Qty WHERE ProductID = @ProductID"
                        : "UPDATE Products SET Quantity = Quantity - @Qty WHERE ProductID = @ProductID";

                    SqlCommand cmdUpdate = new SqlCommand(updateQuery, con, tran);
                    cmdUpdate.Parameters.AddWithValue("@Qty", qty);
                    cmdUpdate.Parameters.AddWithValue("@ProductID", productId);
                    cmdUpdate.ExecuteNonQuery();

                    tran.Commit();
                    MessageBox.Show("✅ Stock transaction recorded successfully!");
                    LoadTransactions();
                    cbProducts_SelectedIndexChanged(null, null); // refresh stock label
                }
                catch
                {
                    tran.Rollback();
                    MessageBox.Show("❌ Error processing transaction!");
                }
            }
        }
        private void LoadTransactions()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = @"SELECT t.TransactionID, p.Name AS Product, t.Category, t.TransactionType, 
                                 t.Quantity, t.TransactionDate
                                 FROM StockTransactions t
                                 INNER JOIN Products p ON t.ProductID = p.ProductID
                                 ORDER BY t.TransactionDate DESC";
                SqlDataAdapter da = new SqlDataAdapter(query, con);
                DataTable dt = new DataTable();
                da.Fill(dt);
                dgvTransactions.DataSource = dt;
            }
        }

        private void cbProducts_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbProducts.SelectedValue != null)
            {
                int productId = Convert.ToInt32(cbProducts.SelectedValue);
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    string query = "SELECT Quantity FROM Products WHERE ProductID=@ProductID";
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@ProductID", productId);

                    con.Open();
                    object result = cmd.ExecuteScalar();
                    if (result != null)
                        lblStock.Text = result.ToString();
                }
            }
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
