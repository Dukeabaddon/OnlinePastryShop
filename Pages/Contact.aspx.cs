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
                bool subscribeNewsletter = chkNewsletter.Checked;

                // Store message in database
                string connectionString = GetConnectionString();
                using (OracleConnection connection = new OracleConnection(connectionString))
                {
                    OracleCommand command = new OracleCommand();
                    command.Connection = connection;
                    command.CommandText = @"INSERT INTO CONTACTMESSAGES 
                                          (NAME, EMAIL, PHONE, SUBJECT, MESSAGE, SUBSCRIBENEWSLETTER, DATESENT)
                                          VALUES (:Name, :Email, :Phone, :Subject, :Message, :SubscribeNewsletter, :DateSent)";

                    command.Parameters.Add(new OracleParameter("Name", name));
                    command.Parameters.Add(new OracleParameter("Email", email));
                    command.Parameters.Add(new OracleParameter("Phone", phone));
                    command.Parameters.Add(new OracleParameter("Subject", subject));
                    command.Parameters.Add(new OracleParameter("Message", message));
                    command.Parameters.Add(new OracleParameter("SubscribeNewsletter", subscribeNewsletter ? 1 : 0)); // 1 for true, 0 for false
                    command.Parameters.Add(new OracleParameter("DateSent", DateTime.Now));

                    connection.Open();
                    command.ExecuteNonQuery();
                }

                // Subscribe to newsletter if checked
                if (subscribeNewsletter)
                {
                    SubscribeToNewsletter(email, name);
                }

                // Send confirmation email to user
                // SendConfirmationEmail(name, email);

                // Clear form fields
                txtName.Text = string.Empty;
                txtEmail.Text = string.Empty;
                txtPhone.Text = string.Empty;
                txtSubject.Text = string.Empty;
                txtMessage.Text = string.Empty;
                chkNewsletter.Checked = false;

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

        protected void btnSubscribe_Click(object sender, EventArgs e)
        {
            string email = txtNewsletterEmail.Text.Trim();

            if (string.IsNullOrEmpty(email) || !IsValidEmail(email))
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "ShowErrorMessage",
                    "showToast('Please enter a valid email address.');", true);
                return;
            }

            try
            {
                // Subscribe to newsletter
                SubscribeToNewsletter(email, "");
                
                // Clear the email field
                txtNewsletterEmail.Text = string.Empty;
                
                // Display success message
                ScriptManager.RegisterStartupScript(this, GetType(), "ShowSuccessMessage",
                    "showToast('Thank you for subscribing to our newsletter!');", true);
            }
            catch (Exception ex)
            {
                // Log error
                // LogError(ex);
                
                // Display error message
                ScriptManager.RegisterStartupScript(this, GetType(), "ShowErrorMessage",
                    "showToast('An error occurred while subscribing. Please try again later.');", true);
            }
        }

        private void SubscribeToNewsletter(string email, string name)
        {
            // Check if already subscribed
            bool isAlreadySubscribed = false;
            string connectionString = GetConnectionString();
            
            using (OracleConnection connection = new OracleConnection(connectionString))
            {
                connection.Open();
                
                // Check if email already exists
                OracleCommand checkCommand = new OracleCommand();
                checkCommand.Connection = connection;
                checkCommand.CommandText = "SELECT COUNT(*) FROM NEWSLETTERSUBSCRIBERS WHERE EMAIL = :Email";
                checkCommand.Parameters.Add(new OracleParameter("Email", email));
                
                isAlreadySubscribed = Convert.ToInt32(checkCommand.ExecuteScalar()) > 0;
                
                // If not already subscribed, add to database
                if (!isAlreadySubscribed)
                {
                    OracleCommand insertCommand = new OracleCommand();
                    insertCommand.Connection = connection;
                    insertCommand.CommandText = @"INSERT INTO NEWSLETTERSUBSCRIBERS 
                                              (EMAIL, NAME, SUBSCRIPTIONDATE, ISACTIVE)
                                              VALUES (:Email, :Name, :SubscriptionDate, :IsActive)";
                    
                    insertCommand.Parameters.Add(new OracleParameter("Email", email));
                    insertCommand.Parameters.Add(new OracleParameter("Name", name));
                    insertCommand.Parameters.Add(new OracleParameter("SubscriptionDate", DateTime.Now));
                    insertCommand.Parameters.Add(new OracleParameter("IsActive", 1)); // Using 1 for true in Oracle
                    
                    insertCommand.ExecuteNonQuery();
                }
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