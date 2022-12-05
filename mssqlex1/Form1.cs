using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace mssqlex1
{
    public partial class Form1 : Form
    {
        private DataTable data_table = null;
        private int[] numers = { 1, 2, 3, 4, 5 };
        string mode = "";

        public Form1()
        {
            InitializeComponent();

            LoadDbTableUsingAdapter();
        }

        public void ClientMethod(IAbstractFactory abstractFactory)
        {
            var productA = abstractFactory.CreateProductA();
            var productB = abstractFactory.CreateProductB();

            MessageBox.Show(productB.UsefulFunctionB());
            MessageBox.Show(productB.AnotherUsefulFunctionB(productA));

        }

        public void LoadDbTableUsingAdapter()
        {
            try
            {
                lock (DBHelper.DBConn)
                {
                    if (!DBHelper.IsDBConnected)
                    {
                        MessageBox.Show("DB 연결을 확인하세요");
                        return;
                    }
                    else
                    {
                        SqlDataAdapter adapter = new SqlDataAdapter("Select * FROM Table1", DBHelper.DBConn);
                        data_table = new DataTable();
                        try
                        {
                            adapter.Fill(data_table);

                            chart1.DataSource = data_table;
                            chart1.Series[0].XValueMember = "Name";
                            chart1.Series[0].YValueMembers = "Age";
                            chart1.DataBind();

                            dataGridView1.DataSource = data_table;
                            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show(e.Message, "Datagridveiw_Load_Error");
                        }
                        DBHelper.Close();
                    }
                }
            }
            catch (ArgumentNullException ane)
            {
                MessageBox.Show(ane.Message, "Datagridveiw_Load_Error");
            }
        }
        private void btn_Delete_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridView1.SelectedRows.Count != 0)
                {
                    foreach (DataGridViewRow item in dataGridView1.SelectedRows)
                    {

                        SqlCommand sqlCommand = new SqlCommand();
                        sqlCommand.Connection = DBHelper.DBConn;
                        sqlCommand.CommandText = string.Format("Delete Table1 Where ID = {0}", item.Cells[2].Value);
                        sqlCommand.ExecuteNonQuery();

                        LoadDbTableUsingAdapter();
                    }
                    
                }
                else
                {
                    MessageBox.Show("선택해주세요.");
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btn_Create_Click(object sender, EventArgs e)
        {

        }

        private void btn_Save_Click(object sender, EventArgs e)
        {
            try
            {
                SqlCommand sqlCommand = new SqlCommand();
                sqlCommand.Connection = DBHelper.DBConn;
                sqlCommand.CommandText = string.Format("insert into Table1(Name,Age,ID) values('{0}',{1},'{2}')", txt_name.Text, txt_age.Text, txt_id.Text);
                sqlCommand.ExecuteNonQuery();

                LoadDbTableUsingAdapter();

            }
            catch (SqlException ex)
            {
                MessageBox.Show(ex.Message);
            }

            //sqlCommand.ex
        }

        private void btn_Refresh_Click(object sender, EventArgs e)
        {
            LoadDbTableUsingAdapter();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            MessageBox.Show("바꾸지마라~");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //ClientMethod(new ConcreteFactory1());
           // ClientMethod(new ConcreteFactory2());
        }
    }

    public class DBHelper
    {
        private static SqlConnection conn = null;
        public static string DBConnString { get; private set; }

        public static bool bDBConnCheck = false;

        private static int errorBoxCount = 0;

        public DBHelper() { }

        public static SqlConnection DBConn
        {
            get
            {
                if(!ConnectToDB())
                {
                    return null;
                }
                return conn;
            }
        }

        public static bool IsDBConnected
        {
            get
            {
                if(conn.State != ConnectionState.Open)
                {
                    return false;
                }
                return true;
            }
        }

        private static bool ConnectToDB()
        {
            if (conn == null)
            {
                DBConnString = string.Format("Data Source = 127.0.0.1; Initial Catalog = Test; User ID = User1; Password = 1234");

                conn = new SqlConnection(DBConnString);
            }

            try
            {
                if(!IsDBConnected)
                {
                    conn.Open();
                    if(conn.State == ConnectionState.Open)
                    {
                        bDBConnCheck = true;
                    }
                    else
                    {
                        bDBConnCheck = false;
                    }
                }
            }
            catch (SqlException e)
            {
                errorBoxCount++;
                if(errorBoxCount == 1)
                {
                    MessageBox.Show(e.Message, "DBHelper - ConnectToDB()");
                }
                return false;
            }

            return true;
        }

        public static void Close()
        {
            if(IsDBConnected)
            {
                DBConn.Close();
            }
        }
    }

    public interface IAbstractFactory
    {
        IAbstractProductA CreateProductA();

        IAbstractProductB CreateProductB();
    }

    // Concrete Factories produce a family of products that belong to a single
    // variant. The factory guarantees that resulting products are compatible.
    // Note that signatures of the Concrete Factory's methods return an abstract
    // product, while inside the method a concrete product is instantiated.
    class ConcreteFactory1 : IAbstractFactory
    {
        public IAbstractProductA CreateProductA()
        {
            return new ConcreteProductA1();
        }

        public IAbstractProductB CreateProductB()
        {
            return new ConcreteProductB1();
        }
    }

    // Each Concrete Factory has a corresponding product variant.
    class ConcreteFactory2 : IAbstractFactory
    {
        public IAbstractProductA CreateProductA()
        {
            return new ConcreteProductA2();
        }

        public IAbstractProductB CreateProductB()
        {
            return new ConcreteProductB2();
        }
    }

    // Each distinct product of a product family should have a base interface.
    // All variants of the product must implement this interface.
    public interface IAbstractProductA
    {
        string UsefulFunctionA();
    }

    // Concrete Products are created by corresponding Concrete Factories.
    class ConcreteProductA1 : IAbstractProductA
    {
        public string UsefulFunctionA()
        {
            return "The result of the product A1.";
        }
    }

    class ConcreteProductA2 : IAbstractProductA
    {
        public string UsefulFunctionA()
        {
            return "The result of the product A2.";
        }
    }

    // Here's the the base interface of another product. All products can
    // interact with each other, but proper interaction is possible only between
    // products of the same concrete variant.
    public interface IAbstractProductB
    {
        // Product B is able to do its own thing...
        string UsefulFunctionB();

        // ...but it also can collaborate with the ProductA.
        //
        // The Abstract Factory makes sure that all products it creates are of
        // the same variant and thus, compatible.
        string AnotherUsefulFunctionB(IAbstractProductA collaborator);
    }

    // Concrete Products are created by corresponding Concrete Factories.
    class ConcreteProductB1 : IAbstractProductB
    {
        public string UsefulFunctionB()
        {
            return "The result of the product B1.";
        }

        // The variant, Product B1, is only able to work correctly with the
        // variant, Product A1. Nevertheless, it accepts any instance of
        // AbstractProductA as an argument.
        public string AnotherUsefulFunctionB(IAbstractProductA collaborator)
        {
            var result = collaborator.UsefulFunctionA();

            return $"The result of the B1 collaborating with the ({result})";
        }
    }

    class ConcreteProductB2 : IAbstractProductB
    {
        public string UsefulFunctionB()
        {
            return "The result of the product B2.";
        }

        // The variant, Product B2, is only able to work correctly with the
        // variant, Product A2. Nevertheless, it accepts any instance of
        // AbstractProductA as an argument.
        public string AnotherUsefulFunctionB(IAbstractProductA collaborator)
        {
            var result = collaborator.UsefulFunctionA();

            return $"The result of the B2 collaborating with the ({result})";
        }
    }
}
