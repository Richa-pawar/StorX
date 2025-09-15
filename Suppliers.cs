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
    public partial class Suppliers : Form
    {
        string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=""D:\Programs\Inventory Management System\Inventory Management System\StorX\StorX_DB.mdf"";Integrated Security=True";
        //string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=""D:\Inventory Management System\Inventory Management System\StorX\StorX_DB.mdf"";Integrated Security=True";
        public Suppliers()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = "INSERT INTO Suppliers (Name, Contact, Email) VALUES (@Name, @Contact, @Email)";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@Name", txtName.Text);
                cmd.Parameters.AddWithValue("@Contact", txtContact.Text);
                cmd.Parameters.AddWithValue("@Email", txtEmail.Text);

                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();

                MessageBox.Show("✅ Supplier Added Successfully!");
                LoadSuppliers();
                
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (dgvSuppliers.SelectedRows.Count > 0)
            {
                int supplierId = Convert.ToInt32(dgvSuppliers.SelectedRows[0].Cells["SupplierID"].Value);

                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    string query = "UPDATE Suppliers SET Name=@Name, Contact=@Contact, Email=@Email WHERE SupplierID=@SupplierID";
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@Name", txtName.Text);
                    cmd.Parameters.AddWithValue("@Contact", txtContact.Text);
                    cmd.Parameters.AddWithValue("@Email", txtEmail.Text);
                    cmd.Parameters.AddWithValue("@SupplierID", supplierId);

                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();

                    MessageBox.Show("✅ Supplier Updated Successfully!");
                    LoadSuppliers();
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (dgvSuppliers.SelectedRows.Count > 0)
            {
                int supplierId = Convert.ToInt32(dgvSuppliers.SelectedRows[0].Cells["SupplierID"].Value);

                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    string query = "DELETE FROM Suppliers WHERE SupplierID=@SupplierID";
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@SupplierID", supplierId);

                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();

                    MessageBox.Show("✅ Supplier Deleted Successfully!");
                    LoadSuppliers();
                }
            }
        }

        private void LoadSuppliers()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = "SELECT * FROM Suppliers";
                SqlDataAdapter da = new SqlDataAdapter(query, con);
                DataTable dt = new DataTable();
                da.Fill(dt);
                dgvSuppliers.DataSource = dt;
            }
        }

        private void Suppliers_Load(object sender, EventArgs e)
        {
            LoadSuppliers();
        }

        private void label7_Click(object sender, EventArgs e)
        {
            txtName.Clear();
            txtContact.Clear();
            txtEmail.Clear();
            txtName.Focus();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = "SELECT * FROM Suppliers WHERE Name LIKE @Name";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@Name", "%" + txtName.Text + "%");

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                if (dt.Rows.Count > 0)
                {
                    dgvSuppliers.DataSource = dt;

                    // Optional: fill the textboxes with the first result
                    txtName.Text = dt.Rows[0]["Name"].ToString();
                    txtContact.Text = dt.Rows[0]["Contact"].ToString();
                    txtEmail.Text = dt.Rows[0]["Email"].ToString();
                }
                else
                {
                    MessageBox.Show("Supplier not found!");
                    dgvSuppliers.DataSource = null;
                }
            }
        }

        private void dgvSuppliers_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                txtName.Text = dgvSuppliers.Rows[e.RowIndex].Cells["Name"].Value.ToString();
                txtContact.Text = dgvSuppliers.Rows[e.RowIndex].Cells["Contact"].Value.ToString();
                txtEmail.Text = dgvSuppliers.Rows[e.RowIndex].Cells["Email"].Value.ToString();
            }
        }
    }
}
