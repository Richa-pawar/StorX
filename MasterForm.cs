using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StorX
{
    public partial class MasterForm : Form
    {
        public MasterForm()
        {
            InitializeComponent();
           
        }

        private Form activeForm = null;
        //private void openChildForm(Form childForm)
        //{
            //if (activeForm != null)
            //    activeForm.Close();
            //activeForm = childForm;
            //childForm.TopLevel = false;
            //childForm.FormBorderStyle = FormBorderStyle.None;
            //childForm.Dock = DockStyle.Fill;
            //childForm.BringToFront();
            //childForm.Show();
        //}
        private void button5_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Products pro = new Products();
            pro.MdiParent = this;
            pro.Show();
            //openChildForm(new Products());
            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Suppliers supply = new Suppliers();
            supply.MdiParent = this;
            supply.Show();
            
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Stock stock = new Stock();
            stock.MdiParent = this;
            stock.Show();
            

        }

        private void button4_Click(object sender, EventArgs e)
        {
            Reports repo = new Reports();
            repo.MdiParent = this;
            repo.Show();
            
        }
    }
}
