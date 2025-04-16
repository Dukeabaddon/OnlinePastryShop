using System;
using System.Web.UI;
using Oracle.ManagedDataAccess.Client;
using System.Configuration;
using System.Security.Cryptography;
using System.Text;

namespace OnlinePastryShop.Pages
{
    public partial class Database : System.Web.UI.Page
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["OracleConnection"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                TestDatabaseConnection();
            }
        }

        private void TestDatabaseConnection()
        {
            try
            {
                using (OracleConnection conn = new OracleConnection(connectionString))
                {
                    conn.Open();
                    lblConnectionStatus.Text = "Connection successful! Oracle database is accessible.";
                    lblConnectionStatus.CssClass = "success";
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                lblConnectionStatus.Text = "Connection failed: " + ex.Message;
                lblConnectionStatus.CssClass = "error";
            }
        }

        protected void btnCreateTable_Click(object sender, EventArgs e)
        {
            try
            {
                using (OracleConnection conn = new OracleConnection(connectionString))
                {
                    conn.Open();
                    StringBuilder result = new StringBuilder();

                    // Create USERS table if it doesn't exist
                    string checkUserTableSQL = "SELECT COUNT(*) FROM USER_TABLES WHERE TABLE_NAME = 'USERS'";
                    bool usersTableExists = false;

                    using (OracleCommand cmd = new OracleCommand(checkUserTableSQL, conn))
                    {
                        usersTableExists = Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                    }

                    if (!usersTableExists)
                    {
                        string createUsersTableSQL = @"
                            CREATE TABLE USERS (
                                UserID NUMBER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
                                Username VARCHAR2(50) UNIQUE NOT NULL,
                                Password VARCHAR2(100) NOT NULL,
                                Email VARCHAR2(100) UNIQUE NOT NULL,
                                FirstName VARCHAR2(50),
                                LastName VARCHAR2(50),
                                UserType VARCHAR2(20) NOT NULL,
                                CreatedDate DATE DEFAULT SYSDATE
                            )";

                        using (OracleCommand cmd = new OracleCommand(createUsersTableSQL, conn))
                        {
                            cmd.ExecuteNonQuery();
                            result.AppendLine("USERS table created successfully.<br/>");
                        }
                    }
                    else
                    {
                        result.AppendLine("USERS table already exists.<br/>");
                    }
                    
                    // Create LOGIN_ATTEMPTS table if it doesn't exist
                    string checkLoginAttemptsTableSQL = "SELECT COUNT(*) FROM USER_TABLES WHERE TABLE_NAME = 'LOGIN_ATTEMPTS'";
                    bool loginAttemptsTableExists = false;

                    using (OracleCommand cmd = new OracleCommand(checkLoginAttemptsTableSQL, conn))
                    {
                        loginAttemptsTableExists = Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                    }

                    if (!loginAttemptsTableExists)
                    {
                        string createLoginAttemptsTableSQL = @"
                            CREATE TABLE LOGIN_ATTEMPTS (
                                AttemptID NUMBER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
                                Username VARCHAR2(50) NOT NULL,
                                IP_ADDRESS VARCHAR2(50) NOT NULL,
                                ATTEMPT_TIME TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                                SUCCESS NUMBER(1) DEFAULT 0
                            )";

                        using (OracleCommand cmd = new OracleCommand(createLoginAttemptsTableSQL, conn))
                        {
                            cmd.ExecuteNonQuery();
                            result.AppendLine("LOGIN_ATTEMPTS table created successfully.<br/>");
                        }
                    }
                    else
                    {
                        result.AppendLine("LOGIN_ATTEMPTS table already exists.<br/>");
                    }

                    // Create PRODUCTS table if it doesn't exist
                    string checkProductsTableSQL = "SELECT COUNT(*) FROM USER_TABLES WHERE TABLE_NAME = 'PRODUCTS'";
                    bool productsTableExists = false;

                    using (OracleCommand cmd = new OracleCommand(checkProductsTableSQL, conn))
                    {
                        productsTableExists = Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                    }

                    if (!productsTableExists)
                    {
                        string createProductsTableSQL = @"
                            CREATE TABLE PRODUCTS (
                                ProductID NUMBER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
                                Name VARCHAR2(100) NOT NULL,
                                Description VARCHAR2(500),
                                Price NUMBER(10,2) NOT NULL,
                                Category VARCHAR2(50) NOT NULL,
                                ImageUrl VARCHAR2(255),
                                CreatedDate DATE DEFAULT SYSDATE
                            )";

                        using (OracleCommand cmd = new OracleCommand(createProductsTableSQL, conn))
                        {
                            cmd.ExecuteNonQuery();
                            result.AppendLine("PRODUCTS table created successfully.<br/>");
                        }
                    }
                    else
                    {
                        result.AppendLine("PRODUCTS table already exists.<br/>");
                    }

                    // Create ORDERS table if it doesn't exist
                    string checkOrdersTableSQL = "SELECT COUNT(*) FROM USER_TABLES WHERE TABLE_NAME = 'ORDERS'";
                    bool ordersTableExists = false;

                    using (OracleCommand cmd = new OracleCommand(checkOrdersTableSQL, conn))
                    {
                        ordersTableExists = Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                    }

                    if (!ordersTableExists)
                    {
                        string createOrdersTableSQL = @"
                            CREATE TABLE ORDERS (
                                OrderID NUMBER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
                                UserID NUMBER NOT NULL,
                                OrderDate DATE DEFAULT SYSDATE,
                                TotalAmount NUMBER(10,2) NOT NULL,
                                Status VARCHAR2(50) DEFAULT 'Pending',
                                ShippingAddress VARCHAR2(255),
                                CONSTRAINT FK_ORDERS_USERS FOREIGN KEY (UserID) REFERENCES USERS(UserID)
                            )";

                        using (OracleCommand cmd = new OracleCommand(createOrdersTableSQL, conn))
                        {
                            cmd.ExecuteNonQuery();
                            result.AppendLine("ORDERS table created successfully.<br/>");
                        }
                    }
                    else
                    {
                        result.AppendLine("ORDERS table already exists.<br/>");
                    }

                    // Create ORDER_ITEMS table if it doesn't exist
                    string checkOrderItemsTableSQL = "SELECT COUNT(*) FROM USER_TABLES WHERE TABLE_NAME = 'ORDER_ITEMS'";
                    bool orderItemsTableExists = false;

                    using (OracleCommand cmd = new OracleCommand(checkOrderItemsTableSQL, conn))
                    {
                        orderItemsTableExists = Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                    }

                    if (!orderItemsTableExists)
                    {
                        string createOrderItemsTableSQL = @"
                            CREATE TABLE ORDER_ITEMS (
                                OrderItemID NUMBER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
                                OrderID NUMBER NOT NULL,
                                ProductID NUMBER NOT NULL,
                                Quantity NUMBER NOT NULL,
                                UnitPrice NUMBER(10,2) NOT NULL,
                                CONSTRAINT FK_ORDERITEMS_ORDERS FOREIGN KEY (OrderID) REFERENCES ORDERS(OrderID),
                                CONSTRAINT FK_ORDERITEMS_PRODUCTS FOREIGN KEY (ProductID) REFERENCES PRODUCTS(ProductID)
                            )";

                        using (OracleCommand cmd = new OracleCommand(createOrderItemsTableSQL, conn))
                        {
                            cmd.ExecuteNonQuery();
                            result.AppendLine("ORDER_ITEMS table created successfully.<br/>");
                        }
                    }
                    else
                    {
                        result.AppendLine("ORDER_ITEMS table already exists.<br/>");
                    }

                    lblTableResult.Text = result.ToString();
                    lblTableResult.CssClass = "success";
                }
            }
            catch (Exception ex)
            {
                lblTableResult.Text = "Error creating tables: " + ex.Message;
                lblTableResult.CssClass = "error";
            }
        }

        protected void btnCreateUsers_Click(object sender, EventArgs e)
        {
            try
            {
                using (OracleConnection conn = new OracleConnection(connectionString))
                {
                    conn.Open();
                    StringBuilder result = new StringBuilder();

                    // Check if admin user exists
                    string checkAdminSQL = "SELECT COUNT(*) FROM USERS WHERE Username = 'admin'";
                    bool adminExists = false;

                    using (OracleCommand cmd = new OracleCommand(checkAdminSQL, conn))
                    {
                        adminExists = Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                    }

                    if (!adminExists)
                    {
                        // Create admin user
                        string createAdminSQL = @"
                            INSERT INTO USERS (Username, Password, Email, FirstName, LastName, UserType) 
                            VALUES ('admin', :password, 'admin@pastryhub.com', 'Admin', 'User', 'Admin')";

                        using (OracleCommand cmd = new OracleCommand(createAdminSQL, conn))
                        {
                            cmd.Parameters.Add(new OracleParameter("password", HashPassword("password123")));
                            cmd.ExecuteNonQuery();
                            result.AppendLine("Admin user created successfully. (Username: admin, Password: password123)<br/>");
                        }
                    }
                    else
                    {
                        result.AppendLine("Admin user already exists.<br/>");
                    }

                    // Check if customer user exists
                    string checkCustomerSQL = "SELECT COUNT(*) FROM USERS WHERE Username = 'customer'";
                    bool customerExists = false;

                    using (OracleCommand cmd = new OracleCommand(checkCustomerSQL, conn))
                    {
                        customerExists = Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                    }

                    if (!customerExists)
                    {
                        // Create customer user
                        string createCustomerSQL = @"
                            INSERT INTO USERS (Username, Password, Email, FirstName, LastName, UserType) 
                            VALUES ('customer', :password, 'customer@example.com', 'Sample', 'Customer', 'Customer')";

                        using (OracleCommand cmd = new OracleCommand(createCustomerSQL, conn))
                        {
                            cmd.Parameters.Add(new OracleParameter("password", HashPassword("password123")));
                            cmd.ExecuteNonQuery();
                            result.AppendLine("Customer user created successfully. (Username: customer, Password: password123)<br/>");
                        }
                    }
                    else
                    {
                        result.AppendLine("Customer user already exists.<br/>");
                    }

                    lblUsersResult.Text = result.ToString();
                    lblUsersResult.CssClass = "success";
                }
            }
            catch (Exception ex)
            {
                lblUsersResult.Text = "Error creating users: " + ex.Message;
                lblUsersResult.CssClass = "error";
            }
        }

        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    builder.Append(hashBytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        protected void btnBack_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Pages/Login.aspx");
        }
    }
} 