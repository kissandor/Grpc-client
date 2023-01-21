using Grpc.Net.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using StockClient;
using Grpc.Core;
using System.IO;

namespace StockClient
{
    public partial class Form1 : Form
    {

        GrpcChannel channel = GrpcChannel.ForAddress("https://localhost:5001");
        Stock.StockClient client;
        static string uid = null;

        public Form1()
        {
            InitializeComponent();
            client = new Stock.StockClient(channel);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //client = new Stock.StockClient(channel);
        }

        private void logoutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Session_Id tempuid = new Session_Id();
                tempuid.Id = uid;
                Result res = client.Logout(tempuid);
                uidLabel.Text = res.ToString();
            }
            catch
            {
                uidLabel.Text = "Server is offline!";
            }

        }

        private void btnAddProduct_Click(object sender, EventArgs e)
        {
            try
            {
                NewItem newItem = new NewItem();

                if (txtBoxProductName.Text == "")
                    throw new Exception("Missing product name");
                newItem.Name = txtBoxProductName.Text;

                if (txtBoxProductNumber.Text == "")
                    throw new Exception("Missing product number");
                newItem.Code = txtBoxProductNumber.Text;

                if (int.Parse(txtBoxProductQty.Text) < 0)
                    throw new Exception("Incorrect quantity");
                newItem.Price = int.Parse(txtBoxProductQty.Text);

                newItem.Uid = uid;
                Result res = client.ItemAdd(newItem);
                uidLabel.Text = res.ToString();

                lbtnList_Click_1(sender, e);
            }
            catch (Exception exc)
            {
                uidLabel.Text = exc.Message;
            }

        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (uid != null && uid != "")
            {
                logoutToolStripMenuItem_Click(sender, e);
            }
            this.Close();
        }

        private void loginBtn_Click(object sender, EventArgs e)
        {
            try
            {
                User user = new User();
                user.Name = userNameTxtBox.Text;
                user.Passwd = passwordTxtBox.Text;
                Session_Id tempuid = client.Login(user);
                label4.Text = tempuid.ToString();

                if (tempuid.Id.ToString() != "Login Faild")
                {
                    uid = tempuid.ToString().Substring(9, 36);
                }
                else
                {
                    uid = "";
                }

                uidLabel.Text = uid;
            }
            catch
            {
                uidLabel.Text = "Server is offline";
            }
        }

        private async void lbtnList_Click_1(object sender, EventArgs e)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Item number");
            dt.Columns.Add("Item name");
            dt.Columns.Add("Qty in stock");
            dt.Columns.Add("U");
            dt.Columns.Add("D");

            DataRow dr;

            try
            {
                using (var call = client.List(new Empty { }))
                {
                    while (await call.ResponseStream.MoveNext())
                    {
                        StockItem stockItem = call.ResponseStream.Current;
                        dr = dt.NewRow();
                        dr["Item number"] = stockItem.Code.ToString();
                        dr["Item name"] = stockItem.Name.ToString();
                        dr["Qty in stock"] = stockItem.CurPrice.ToString();
                        dr["U"] = "U";
                        dr["D"] = "D";

                        dt.Rows.Add(dr);

                    }
                }
            }
            catch { uidLabel.Text = "Server is offline!"; }


            dataGridView1.DataSource = dt;
            dataGridView1.RowHeadersVisible = false;
            dataGridView1.Columns[0].ReadOnly = true;
            dataGridView1.Columns[1].ReadOnly = true;
            dataGridView1.Columns[3].ReadOnly = true;
            dataGridView1.Columns[4].ReadOnly = true;
            dataGridView1.Columns[0].Width = 100;
            dataGridView1.Columns[1].Width = 270;
            dataGridView1.Columns[2].Width = 100;
            dataGridView1.Columns[3].Width = 20;
            dataGridView1.Columns[4].Width = 20;
            
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridView1.CurrentCell.ColumnIndex.Equals(4) && e.RowIndex != -1)
            {
                if (dataGridView1.CurrentCell != null && dataGridView1.CurrentCell.Value != null) 
                {
                    try
                    {
                        //MessageBox.Show(dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString());
                        DeleteStockItem toDeleteitem = new DeleteStockItem();
                        toDeleteitem.Code = dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString();
                        toDeleteitem.Uid = uid;
                        Result res = client.ItemDelete(toDeleteitem);
                        uidLabel.Text = res.ToString();
                        lbtnList_Click_1(sender, e);
                    }
                    catch(Exception exc)
                    {
                        uidLabel.Text = exc.Message;
                    }

                }
            }

            if (dataGridView1.CurrentCell.ColumnIndex.Equals(3) && e.RowIndex != -1)
            {
                if (dataGridView1.CurrentCell != null && dataGridView1.CurrentCell.Value != null)
                {
                    try
                    {
                        //MessageBox.Show(dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString());
                        UpdatedStockItem itemToUpdate = new UpdatedStockItem();
                        itemToUpdate.Code = dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString();
                        itemToUpdate.Price = int.Parse(dataGridView1.Rows[e.RowIndex].Cells[2].Value.ToString());
                        itemToUpdate.Uid = uid;
                        Result res = client.StockItemUpdate(itemToUpdate);
                        uidLabel.Text = res.ToString();
                        lbtnList_Click_1(sender, e);
                    }
                    catch (Exception exc)
                    {
                        uidLabel.Text = exc.Message;
                    }

                }
            }
        }

    }
}