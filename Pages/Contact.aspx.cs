using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Configuration;
using System.Net.Mail;
using System.Text.RegularExpressions;
using Oracle.ManagedDataAccess.Client;

namespace OnlinePastryShop.Pages
{
    public partial class Contact : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // Initialize page
            if (!IsPostBack)
            {
                // Set page title
                this.Title = "Contact Us - Pastry Palace";
            }
        }

        protected void btnSendMessage_Click(object sender, EventArgs e)
        {
            // Validate form inputs
            if (!ValidateForm())
            {
                // Display error message
                ScriptManager.RegisterStartupScript(this, GetType(), "ShowErrorMessage",
                    "showToast('Please fill in all required fields correctly.');", true);
                return;
            }

            try
            {
                // Get form values
                string name = txtName.Text.Trim();
                string email = txtEmail.Text.Trim();
                string phone = txtPhone.Text.Trim();
                string subject = txtSubject.Text.Trim();
                string message = txtMessage.Text.Trim();

                // Store message in database
                string connectionString = GetConnectionString();
                using (OracleConnection connection = new OracleConnection(connectionString))
                {
                    OracleCommand command = new OracleCommand();
                    command.Connection = connection;
                    command.CommandText = @"INSERT INTO CONTACTMESSAGES 
                                          (NAME, EMAIL, PHONE, SUBJECT, MESSAGE, DATESENT)
                                          VALUES (:Name, :Email, :Phone, :Subject, :Message, :DateSent)";

                    command.Parameters.Add(new OracleParameter("Name", name));
                    command.Parameters.Add(new OracleParameter("Email", email));
                    command.Parameters.Add(new OracleParameter("Phone", phone));
                    command.Parameters.Add(new OracleParameter("Subject", subject));
                    command.Parameters.Add(new OracleParameter("Message", message));
                    command.Parameters.Add(new OracleParameter("DateSent", DateTime.Now));

                    connection.Open();
                    command.ExecuteNonQuery();
                }

                // Clear form fields
                txtName.Text = string.Empty;
                txtEmail.Text = string.Empty;
                txtPhone.Text = string.Empty;
                txtSubject.Text = string.Empty;
                txtMessage.Text = string.Empty;

                // Display success message
                ScriptManager.RegisterStartupScript(this, GetType(), "ShowSuccessMessage",
                    "showToast('Your message has been sent successfully. We will get back to you soon!');", true);
            }
            catch (Exception ex)
            {
                // Log error
                // LogError(ex);

                // Display error message
                ScriptManager.RegisterStartupScript(this, GetType(), "ShowErrorMessage",
                    "showToast('An error occurred while sending your message. Please try again later.');", true);
            }
        }

        private bool ValidateForm()
        {
            // Check if required fields are filled
            if (string.IsNullOrEmpty(txtName.Text.Trim()) ||
                string.IsNullOrEmpty(txtEmail.Text.Trim()) ||
                string.IsNullOrEmpty(txtSubject.Text.Trim()) ||
                string.IsNullOrEmpty(txtMessage.Text.Trim()))
            {
                return false;
            }

            // Validate email format
            if (!IsValidEmail(txtEmail.Text.Trim()))
            {
                return false;
            }

            return true;
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                // Simple regex for email validation
                string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
                return Regex.IsMatch(email, pattern);
            }
            catch
            {
                return false;
            }
        }

        private string GetConnectionString()
        {
            try
            {
                var connString = ConfigurationManager.ConnectionStrings["OracleConnection"];
                if (connString != null)
                {
                    return connString.ConnectionString;
                }
                else
                {
                    // Log the error
                    System.Diagnostics.Debug.WriteLine("ERROR: OracleConnection string not found in Web.config");

                    // Return a hardcoded backup connection string
                    return "User Id=Aaron_IPT;Password=qwen123;Data Source=localhost:1521/xe;";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR getting connection string: {ex.Message}");

                // Fallback to hardcoded connection string
                return "User Id=Aaron_IPT;Password=qwen123;Data Source=localhost:1521/xe;";
            }
        }
    }
}