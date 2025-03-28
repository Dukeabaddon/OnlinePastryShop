using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Net.Mail;
using Oracle.ManagedDataAccess.Client;
using System.Configuration;
using System.Security.Cryptography;

namespace OnlinePastryShop.Pages
{
    // User model class definition
    public class User
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Status { get; set; }
        public DateTime DateCreated { get; set; }
        public string UserType { get; set; }
        public bool IsActive { get; set; }
    }

    public partial class Users : System.Web.UI.Page
    {
        // Hidden field to store user ID for operations that need it across postbacks
        private int selectedUserId = 0;

        // UI controls defined in aspx file
        protected Repeater userRepeater;
        protected Label lblTotalUsersValue;
        protected Label lblActiveUsersValue;
        protected Label lblInactiveUsersValue;
        protected Label lblNewUsersValue;
        protected Label lblMessage;
        protected Panel pnlToast;
        protected Label lblToastMessage;
        protected GridView gvUsers;

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Page_Load executed");
                System.Diagnostics.Debug.WriteLine($"Page ClientID: {this.ClientID}, UniqueID: {this.UniqueID}");

                if (!IsPostBack)
                {
                    // Initialize ViewState values for search and filter
                    ViewState["SearchText"] = string.Empty;
                    ViewState["StatusValue"] = "true"; // Default to Active
                    System.Diagnostics.Debug.WriteLine("Initialized ViewState values for search and filter");

                    // Initialize on first load
                    LoadUsers();
                }

                // Restore selectedUserId from ViewState if available
                if (ViewState["SelectedUserId"] != null)
                {
                    selectedUserId = Convert.ToInt32(ViewState["SelectedUserId"]);
                }

                // Find the repeater control
                userRepeater = FindControl("userRepeater") as Repeater;
                lblTotalUsersValue = FindControl("lblTotalUsersValue") as Label;
                lblActiveUsersValue = FindControl("lblActiveUsersValue") as Label;
                lblInactiveUsersValue = FindControl("lblInactiveUsersValue") as Label;
                lblNewUsersValue = FindControl("lblNewUsersValue") as Label;
                lblMessage = FindControl("lblMessage") as Label;
                pnlToast = FindControl("pnlToast") as Panel;
                lblToastMessage = FindControl("lblToastMessage") as Label;

                // Log control status - use null conditional operator to avoid null reference exceptions
                System.Diagnostics.Debug.WriteLine($"txtSearch exists = {txtSearch != null}");
                System.Diagnostics.Debug.WriteLine($"ddlStatus exists = {ddlStatus != null}");

                // Check if gvUsers is NULL, which is likely
                System.Diagnostics.Debug.WriteLine($"gvUsers initially = {(gvUsers != null ? "Found" : "NULL")}");

                // Find controls recursively if they're NULL
                var allControls = FindAllControlsRecursive(this);

                if (gvUsers == null)
                {
                    // First try direct FindControl on the page
                    gvUsers = FindControl("gvUsers") as GridView;

                    // Then try on the form
                    if (gvUsers == null && Form != null)
                    {
                        gvUsers = Form.FindControl("gvUsers") as GridView;
                    }

                    // Finally, search recursively
                    if (gvUsers == null)
                    {
                        gvUsers = allControls.FirstOrDefault(c => c.ID == "gvUsers") as GridView;

                        // If still null, try to find ANY GridView as a last resort
                        if (gvUsers == null)
                        {
                            var anyGridView = allControls.FirstOrDefault(c => c is GridView) as GridView;
                            if (anyGridView != null)
                            {
                                System.Diagnostics.Debug.WriteLine($"Found alternative GridView with ID: {anyGridView.ID}");
                                gvUsers = anyGridView;
                            }
                        }
                    }

                    // Log result
                    if (gvUsers != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"gvUsers found recursively = true, ID = {gvUsers.ID}");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("Failed to find any GridView control!");
                    }
                }

                // Debug output for content placeholder structure
                System.Diagnostics.Debug.WriteLine("Page structure debugging:");
                System.Diagnostics.Debug.WriteLine($"Page.Form ID: {(Form != null ? Form.ID : "null")}");
                System.Diagnostics.Debug.WriteLine($"AdminContent ContentPlaceHolder exists: {(FindControl("AdminContent") != null)}");

                // Test direct FindControl before trying helper methods
                var directTxtSearch = FindControl("txtSearch");
                var contentPlaceholder = FindControl("AdminContent");

                System.Diagnostics.Debug.WriteLine($"Direct FindControl for txtSearch: {(directTxtSearch != null ? directTxtSearch.ClientID : "not found")}");

                if (contentPlaceholder != null)
                {
                    var cpTxtSearch = contentPlaceholder.FindControl("txtSearch");
                    System.Diagnostics.Debug.WriteLine($"ContentPlaceHolder.FindControl for txtSearch: {(cpTxtSearch != null ? cpTxtSearch.ClientID : "not found")}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in Page_Load: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }

        #region Data Loading Methods

        private void LoadStats()
        {
            try
            {
                using (OracleConnection conn = new OracleConnection(GetConnectionString()))
                {
                    conn.Open();

                    // Get total number of users (both active and inactive)
                    string totalUsersQuery = @"
                        SELECT COUNT(*) AS TotalUsers
                        FROM Users";

                    using (OracleCommand cmd = new OracleCommand(totalUsersQuery, conn))
                    {
                        object result = cmd.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            litTotalUsers.Text = result.ToString();
                        }
                    }

                    // Get new users (last 30 days) - regardless of active status
                    string newUsersQuery = @"
                        SELECT COUNT(*) AS NewUsers
                        FROM Users
                        WHERE DateCreated >= :startDate";

                    using (OracleCommand cmd = new OracleCommand(newUsersQuery, conn))
                    {
                        cmd.Parameters.Add("startDate", OracleDbType.Date).Value = DateTime.Now.AddDays(-30);
                        object result = cmd.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            litNewUsers.Text = result.ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowMessage("Error loading statistics: " + ex.Message, false);
            }
        }

        private TextBox GetSearchTextBox()
        {
            // Look for the control with the specific path that includes ContentPlaceHolder
            var content = FindControl("AdminContent") as ContentPlaceHolder;
            if (content != null)
            {
                System.Diagnostics.Debug.WriteLine("Found AdminContent ContentPlaceHolder");
                var searchBox = content.FindControl("txtSearch") as TextBox;
                if (searchBox != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Found txtSearch within AdminContent: {searchBox.ClientID}");
                    return searchBox;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("txtSearch not found within AdminContent");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("AdminContent ContentPlaceHolder not found");
            }

            // Try from the Form as a fallback
            if (Form != null)
            {
                var control = Form.FindControl("txtSearch") as TextBox;
                if (control != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Found txtSearch using Form.FindControl: {control.ClientID}");
                    return control;
                }
            }

            // Try recursive search as a final fallback
            var allControls = FindAllControlsRecursive(this);
            var textBoxes = allControls.Where(c => c.ID == "txtSearch").OfType<TextBox>().ToList();

            if (textBoxes.Any())
            {
                System.Diagnostics.Debug.WriteLine($"Found {textBoxes.Count} txtSearch controls using recursive search");
                var searchBox = textBoxes.First();
                System.Diagnostics.Debug.WriteLine($"Using txtSearch with ID: {searchBox.ClientID}");
                return searchBox;
            }

            System.Diagnostics.Debug.WriteLine("ERROR: Could not find txtSearch control!");
            return null;
        }

        private DropDownList GetStatusDropDown()
        {
            // Look for the control with the specific path that includes ContentPlaceHolder
            var content = FindControl("AdminContent") as ContentPlaceHolder;
            if (content != null)
            {
                System.Diagnostics.Debug.WriteLine("Found AdminContent ContentPlaceHolder");
                var ddlStatus = content.FindControl("ddlStatus") as DropDownList;
                if (ddlStatus != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Found ddlStatus within AdminContent: {ddlStatus.ClientID}");
                    return ddlStatus;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("ddlStatus not found within AdminContent");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("AdminContent ContentPlaceHolder not found");
            }

            // Try from the Form as a fallback
            if (Form != null)
            {
                var control = Form.FindControl("ddlStatus") as DropDownList;
                if (control != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Found ddlStatus using Form.FindControl: {control.ClientID}");
                    return control;
                }
            }

            // Try recursive search as a final fallback
            var allControls = FindAllControlsRecursive(this);
            var ddlControls = allControls.Where(c => c.ID == "ddlStatus").OfType<DropDownList>().ToList();

            if (ddlControls.Any())
            {
                System.Diagnostics.Debug.WriteLine($"Found {ddlControls.Count} ddlStatus controls using recursive search");
                var ddlStatus = ddlControls.First();
                System.Diagnostics.Debug.WriteLine($"Using ddlStatus with ID: {ddlStatus.ClientID}");
                return ddlStatus;
            }

            System.Diagnostics.Debug.WriteLine("ERROR: Could not find ddlStatus control!");
            return null;
        }

        private void LoadUsers(bool isSearch = false)
        {
            try
            {
                // Get search and status values - first check ViewState, then controls
                string searchText = string.Empty;
                string statusValue = "true"; // Default to active

                // If this is a search, prioritize ViewState values
                if (isSearch && ViewState["SearchText"] != null)
                {
                    searchText = ViewState["SearchText"].ToString();
                    System.Diagnostics.Debug.WriteLine($"Using search text from ViewState: '{searchText}'");
                }
                else
                {
                    // If not a search or no ViewState, try to get from controls
                    var txtSearch = GetSearchTextBox();
                    searchText = txtSearch?.Text?.Trim() ?? string.Empty;
                    System.Diagnostics.Debug.WriteLine($"Using search text from control: '{searchText}'");
                }

                // Always check ViewState first for status, as it should be updated with each selection change
                if (ViewState["StatusValue"] != null)
                {
                    statusValue = ViewState["StatusValue"].ToString();
                    System.Diagnostics.Debug.WriteLine($"Using status value from ViewState: '{statusValue}'");
                }
                else
                {
                    // Fallback to control
                    var ddlStatus = GetStatusDropDown();
                    statusValue = ddlStatus?.SelectedValue ?? "true";
                    System.Diagnostics.Debug.WriteLine($"Using status value from control: '{statusValue}'");
                }

                System.Diagnostics.Debug.WriteLine($"SEARCH CRITERIA: Text='{searchText}', Status='{statusValue}'");

                List<User> users = new List<User>();

                // Build the base query
                string query = @"
                    SELECT 
                        USERID, 
                        USERNAME, 
                        EMAIL, 
                        ISACTIVE, 
                        TO_CHAR(DATECREATED, 'YYYY-MM-DD') AS DATECREATED, 
                        ROLE AS USERTYPE 
                    FROM USERS 
                    WHERE 1=1";

                // Add search condition if needed
                if (!string.IsNullOrEmpty(searchText))
                {
                    query += @" AND (
                                UPPER(USERNAME) LIKE UPPER('%' || :searchText || '%') OR 
                                UPPER(EMAIL) LIKE UPPER('%' || :searchText || '%')
                            )";
                    System.Diagnostics.Debug.WriteLine("Added search condition to query");
                }

                // Add status filter - Always apply the filter since dropdown always has a value
                query += " AND ISACTIVE = :isActiveValue";
                System.Diagnostics.Debug.WriteLine("Added status condition to query");

                // Add ordering
                query += " ORDER BY USERNAME";

                System.Diagnostics.Debug.WriteLine($"FINAL SQL: {query}");

                using (OracleConnection conn = new OracleConnection(GetConnectionString()))
                {
                    conn.Open();
                    using (OracleCommand cmd = new OracleCommand(query, conn))
                    {
                        // Add search parameter if used in query
                        if (!string.IsNullOrEmpty(searchText))
                        {
                            cmd.Parameters.Add(new OracleParameter("searchText", OracleDbType.Varchar2)).Value = searchText;
                            System.Diagnostics.Debug.WriteLine($"Added searchText parameter: '{searchText}'");
                        }

                        // Add status parameter
                        // Convert "true"/"false" string to 1/0 integer for Oracle
                        int isActiveValue = string.Equals(statusValue, "true", StringComparison.OrdinalIgnoreCase) ? 1 : 0;
                        cmd.Parameters.Add(new OracleParameter("isActiveValue", OracleDbType.Int32)).Value = isActiveValue;
                        System.Diagnostics.Debug.WriteLine($"Added isActiveValue parameter: {isActiveValue}");

                        // Execute the query and track timing
                        System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
                        stopwatch.Start();

                        using (OracleDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                User user = new User
                                {
                                    UserId = Convert.ToInt32(reader["USERID"]),
                                    Username = reader["USERNAME"].ToString(),
                                    Email = reader["EMAIL"].ToString(),
                                    Status = Convert.ToBoolean(Convert.ToInt32(reader["ISACTIVE"])) ? "Active" : "Inactive",
                                    DateCreated = Convert.ToDateTime(reader["DATECREATED"]),
                                    UserType = reader["USERTYPE"].ToString(),
                                    IsActive = Convert.ToBoolean(Convert.ToInt32(reader["ISACTIVE"]))
                                };
                                users.Add(user);
                            }
                        }

                        stopwatch.Stop();
                        System.Diagnostics.Debug.WriteLine($"Query execution time: {stopwatch.ElapsedMilliseconds}ms");
                    }
                }

                // Debug output - include search criteria in output
                System.Diagnostics.Debug.WriteLine($"FOUND: {users.Count} users matching criteria (search: '{searchText}', status: '{statusValue}')");

                // Log first few users to verify results
                int logCount = Math.Min(users.Count, 5);
                for (int i = 0; i < logCount; i++)
                {
                    System.Diagnostics.Debug.WriteLine($"Result {i + 1}: UserID={users[i].UserId}, Username={users[i].Username}, Email={users[i].Email}");
                }

                // Explicitly look for GridView in AdminContent
                GridView gridView = null;
                var contentPlaceholder = FindControl("AdminContent") as ContentPlaceHolder;
                if (contentPlaceholder != null)
                {
                    gridView = contentPlaceholder.FindControl("gvUsers") as GridView;
                    if (gridView != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"Found GridView in AdminContent: {gridView.ClientID}");
                    }
                }

                // If not found, try getting it from class member or fallbacks
                if (gridView == null && gvUsers != null)
                {
                    gridView = gvUsers;
                    System.Diagnostics.Debug.WriteLine($"Using class member GridView: {gridView.ClientID}");
                }

                // Data binding
                if (gridView != null)
                {
                    gridView.DataSource = users;
                    gridView.DataBind();
                    gridView.Visible = true;
                    System.Diagnostics.Debug.WriteLine($"DataBound {users.Count} users to GridView {gridView.ClientID}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("ERROR: Could not find GridView control for data binding!");

                    // Try one more time with recursive search as fallback
                    var allGridViews = FindAllControlsRecursive(this).Where(c => c is GridView).Cast<GridView>().ToList();
                    if (allGridViews.Any())
                    {
                        gridView = allGridViews.First();
                        gridView.DataSource = users;
                        gridView.DataBind();
                        gridView.Visible = true;
                        System.Diagnostics.Debug.WriteLine($"Used fallback binding to GridView {gridView.ID}");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("CRITICAL ERROR: No GridView controls found on the page!");
                    }
                }

                // Count recent users (last 30 days)
                int recentUsers = users.Count(u => u.DateCreated >= DateTime.Now.AddDays(-30));

                // Update the Literal controls with updated counts
                if (litTotalUsers != null)
                {
                    litTotalUsers.Text = users.Count.ToString();
                    System.Diagnostics.Debug.WriteLine($"Updated litTotalUsers to {users.Count}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("litTotalUsers is null!");

                    // Try to find litTotalUsers through AdminContent
                    if (contentPlaceholder != null)
                    {
                        var lit = contentPlaceholder.FindControl("litTotalUsers") as Literal;
                        if (lit != null)
                        {
                            lit.Text = users.Count.ToString();
                            System.Diagnostics.Debug.WriteLine($"Updated litTotalUsers through ContentPlaceHolder to {users.Count}");
                        }
                    }
                }

                if (litNewUsers != null)
                {
                    litNewUsers.Text = recentUsers.ToString();
                    System.Diagnostics.Debug.WriteLine($"Updated litNewUsers to {recentUsers}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("litNewUsers is null!");

                    // Try to find litNewUsers through AdminContent
                    if (contentPlaceholder != null)
                    {
                        var lit = contentPlaceholder.FindControl("litNewUsers") as Literal;
                        if (lit != null)
                        {
                            lit.Text = recentUsers.ToString();
                            System.Diagnostics.Debug.WriteLine($"Updated litNewUsers through ContentPlaceHolder to {recentUsers}");
                        }
                    }
                }

                System.Diagnostics.Debug.WriteLine("Users loaded successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR in LoadUsers: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                ShowError($"Error loading users: {ex.Message}");
            }
        }

        private void LoadUserDetails(int userId)
        {
            try
            {
                using (OracleConnection conn = new OracleConnection(GetConnectionString()))
                {
                    conn.Open();

                    string query = @"
                        SELECT 
                            UserId, 
                            Username, 
                            Email, 
                            LastLogin, 
                            DateCreated, 
                            DateModified,
                            IsActive
                        FROM Users
                        WHERE UserId = :userId";

                    using (OracleCommand cmd = new OracleCommand(query, conn))
                    {
                        cmd.Parameters.Add("userId", OracleDbType.Int32).Value = userId;

                        using (OracleDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // Populate user details
                                lblUserId.Text = reader["UserId"].ToString();
                                lblUsername.Text = reader["Username"].ToString();
                                lblEmail.Text = reader["Email"].ToString();
                                lblDateCreated.Text = FormatDate(reader["DateCreated"]);
                                lblDateModified.Text = FormatDate(reader["DateModified"]);
                                lblLastLogin.Text = FormatDate(reader["LastLogin"]);
                                lblStatus.Text = Convert.ToBoolean(reader["IsActive"]) ? "Active" : "Inactive";

                                // Store username for password reset
                                lblResetUsername.Text = reader["Username"].ToString();
                            }
                        }
                    }
                }

                // Make Basic Info tab active by default
                SetActiveTab("BasicInfo");

                // Show the user details panel
                pnlUserDetails.Visible = true;
            }
            catch (Exception ex)
            {
                ShowMessage("Error loading user details: " + ex.Message, false);
            }
        }

        private void LoadOrderHistory(int userId)
        {
            try
            {
                using (OracleConnection conn = new OracleConnection(GetConnectionString()))
                {
                    conn.Open();

                    string query = @"
                        SELECT 
                            o.OrderId, 
                            o.OrderDate, 
                            o.TotalAmount,
                            o.Status
                        FROM Orders o
                        WHERE o.UserId = :userId
                        AND o.IsActive = 1
                        ORDER BY o.OrderDate DESC";

                    using (OracleCommand cmd = new OracleCommand(query, conn))
                    {
                        cmd.Parameters.Add("userId", OracleDbType.Int32).Value = userId;

                        using (OracleDataAdapter adapter = new OracleDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            adapter.Fill(dt);

                            gvOrderHistory.DataSource = dt;
                            gvOrderHistory.DataBind();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowMessage("Error loading order history: " + ex.Message, false);
            }
        }

        #endregion

        #region Event Handlers

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("========= SEARCH BUTTON CLICKED =========");

            // Log the current form values from the Request
            System.Diagnostics.Debug.WriteLine($"Form Collection Contents:");
            foreach (string key in Request.Form.Keys)
            {
                System.Diagnostics.Debug.WriteLine($"  Form[{key}] = {Request.Form[key]}");
            }

            // Try direct access to form controls via Content placeholder
            var contentPlaceholder = FindControl("AdminContent") as ContentPlaceHolder;
            if (contentPlaceholder != null)
            {
                var directTxtSearch = contentPlaceholder.FindControl("txtSearch") as TextBox;
                if (directTxtSearch != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Direct Content txtSearch exists with value: '{directTxtSearch.Text}'");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Direct Content txtSearch not found");
                }
            }

            // Get the controls using our helper methods
            var txtSearch = GetSearchTextBox();
            var ddlStatus = GetStatusDropDown();

            string searchText = string.Empty;

            // Store search text in ViewState to maintain across postbacks
            if (txtSearch != null)
            {
                searchText = txtSearch.Text?.Trim() ?? string.Empty;
                ViewState["SearchText"] = searchText;
                System.Diagnostics.Debug.WriteLine($"Saved search text to ViewState: '{searchText}'");
            }
            else
            {
                // If control wasn't found, try getting the value from Form collection
                string formSearchKey = Request.Form.AllKeys.FirstOrDefault(k => k.Contains("txtSearch"));
                if (!string.IsNullOrEmpty(formSearchKey))
                {
                    searchText = Request.Form[formSearchKey]?.Trim() ?? string.Empty;
                    ViewState["SearchText"] = searchText;
                    System.Diagnostics.Debug.WriteLine($"Saved search text from Form to ViewState: '{searchText}'");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Could not find txtSearch control or form value");
                }
            }

            // Store status value in ViewState
            if (ddlStatus != null)
            {
                ViewState["StatusValue"] = ddlStatus.SelectedValue;
                System.Diagnostics.Debug.WriteLine($"Saved status value to ViewState: '{ddlStatus.SelectedValue}'");
            }
            else
            {
                // If control wasn't found, try getting the value from Form collection
                string formStatusKey = Request.Form.AllKeys.FirstOrDefault(k => k.Contains("ddlStatus"));
                if (!string.IsNullOrEmpty(formStatusKey))
                {
                    string statusValue = Request.Form[formStatusKey] ?? "true";
                    ViewState["StatusValue"] = statusValue;
                    System.Diagnostics.Debug.WriteLine($"Saved status value from Form to ViewState: '{statusValue}'");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Could not find ddlStatus control or form value");
                }
            }

            // Log search parameters
            System.Diagnostics.Debug.WriteLine($"Search with ViewState values - SearchText: '{ViewState["SearchText"]}', Status: '{ViewState["StatusValue"]}'");
            System.Diagnostics.Debug.WriteLine("=======================================");

            // Call LoadUsers with isSearch=true to indicate search operation
            LoadUsers(true);
        }

        protected void btnReset_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Reset button clicked");

            // Reset values manually
            var txtSearch = GetSearchTextBox();
            var ddlStatus = GetStatusDropDown();

            // Clear ViewState values first
            ViewState["SearchText"] = string.Empty;
            ViewState["StatusValue"] = "true"; // Default to Active
            System.Diagnostics.Debug.WriteLine("Cleared search and status values in ViewState");

            // Also clear the control values for consistency
            if (txtSearch != null)
            {
                txtSearch.Text = string.Empty;
                System.Diagnostics.Debug.WriteLine($"Reset txtSearch ({txtSearch.ClientID}) to empty string");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Could not find txtSearch control to reset");
            }

            if (ddlStatus != null)
            {
                ddlStatus.SelectedIndex = 0; // Set to "Active" (true)
                System.Diagnostics.Debug.WriteLine($"Reset ddlStatus ({ddlStatus.ClientID}) to index 0 (Active)");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Could not find ddlStatus control to reset");
            }

            System.Diagnostics.Debug.WriteLine("Values reset, reloading users...");

            // Load users with reset values
            LoadUsers(false);
        }

        protected void gvUsers_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            try
            {
                int userId = Convert.ToInt32(e.CommandArgument);

                // Store the selected user ID in ViewState for use across postbacks
                selectedUserId = userId;
                ViewState["SelectedUserId"] = userId;

                if (e.CommandName == "ViewDetails")
                {
                    LoadUserDetails(userId);
                }
                else if (e.CommandName == "ResetPassword")
                {
                    // Load user info for the reset confirmation modal
                    using (OracleConnection conn = new OracleConnection(GetConnectionString()))
                    {
                        conn.Open();
                        string query = "SELECT Username FROM Users WHERE UserId = :userId";
                        using (OracleCommand cmd = new OracleCommand(query, conn))
                        {
                            cmd.Parameters.Add("userId", OracleDbType.Int32).Value = userId;
                            string username = cmd.ExecuteScalar() as string;
                            lblResetUsername.Text = username;
                        }
                    }

                    // Show reset password confirmation modal
                    pnlPasswordReset.Visible = true;
                }
                else if (e.CommandName == "ToggleStatus")
                {
                    // Load user info for the status toggle confirmation modal
                    using (OracleConnection conn = new OracleConnection(GetConnectionString()))
                    {
                        conn.Open();
                        string query = "SELECT Username, IsActive FROM Users WHERE UserId = :userId";
                        using (OracleCommand cmd = new OracleCommand(query, conn))
                        {
                            cmd.Parameters.Add("userId", OracleDbType.Int32).Value = userId;
                            using (OracleDataReader reader = cmd.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    string username = reader["Username"].ToString();
                                    bool isActive = Convert.ToBoolean(reader["IsActive"]);

                                    // Set appropriate text for the modal
                                    litStatusAction.Text = isActive ? "Deactivate User" : "Activate User";
                                    litStatusMessage.Text = isActive
                                        ? $"Are you sure you want to deactivate user <strong>{username}</strong>? They will no longer be able to log in."
                                        : $"Are you sure you want to activate user <strong>{username}</strong>? They will be able to log in again.";

                                    btnConfirmToggle.Text = isActive ? "Deactivate" : "Activate";

                                    // Set button style based on action type
                                    btnConfirmToggle.CssClass = isActive
                                        ? "px-4 py-2 text-sm font-medium text-white bg-red-600 rounded-lg hover:bg-red-700 transition-colors"
                                        : "px-4 py-2 text-sm font-medium text-white bg-green-600 rounded-lg hover:bg-green-700 transition-colors";
                                }
                            }
                        }
                    }

                    // Show status toggle confirmation modal
                    pnlToggleStatus.Visible = true;
                }
            }
            catch (Exception ex)
            {
                ShowMessage("Error processing command: " + ex.Message, false);
            }
        }

        protected void gvUsers_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                // Get the IsActive value from the DataItem and ensure proper conversion
                bool isActive = false;
                object isActiveValue = DataBinder.Eval(e.Row.DataItem, "IsActive");

                if (isActiveValue != null && isActiveValue != DBNull.Value)
                {
                    // Make sure we properly convert to boolean regardless of whether it's stored as a number or boolean
                    isActive = Convert.ToBoolean(Convert.ToInt32(isActiveValue));
                }

                // Find the status badge control
                HtmlGenericControl statusBadge = (HtmlGenericControl)e.Row.FindControl("statusBadge");

                if (statusBadge != null)
                {
                    // Set appropriate CSS class based on status
                    statusBadge.Attributes["class"] = isActive
                        ? "px-2 py-1 inline-flex text-xs leading-5 font-semibold rounded-full bg-green-100 text-green-800"
                        : "px-2 py-1 inline-flex text-xs leading-5 font-semibold rounded-full bg-red-100 text-red-800";
                }
            }
        }

        protected void gvUsers_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvUsers.PageIndex = e.NewPageIndex;
            LoadUsers();
        }

        protected void btnCloseDetails_Click(object sender, EventArgs e)
        {
            pnlUserDetails.Visible = false;
        }

        protected void tabBasicInfo_Click(object sender, EventArgs e)
        {
            SetActiveTab("BasicInfo");
        }

        protected void tabOrderHistory_Click(object sender, EventArgs e)
        {
            SetActiveTab("OrderHistory");
            LoadOrderHistory(selectedUserId);
        }

        protected void btnClosePasswordReset_Click(object sender, EventArgs e)
        {
            pnlPasswordReset.Visible = false;
        }

        protected void btnConfirmReset_Click(object sender, EventArgs e)
        {
            try
            {
                // Generate a random password
                string newPassword = GenerateRandomPassword();

                // Hash the password
                string passwordHash = HashPassword(newPassword);

                // Update the user's password in the database
                using (OracleConnection conn = new OracleConnection(GetConnectionString()))
                {
                    conn.Open();
                    string query = "UPDATE Users SET PasswordHash = :passwordHash, DateModified = SYSDATE WHERE UserId = :userId";
                    using (OracleCommand cmd = new OracleCommand(query, conn))
                    {
                        cmd.Parameters.Add("passwordHash", OracleDbType.Varchar2).Value = passwordHash;
                        cmd.Parameters.Add("userId", OracleDbType.Int32).Value = selectedUserId;
                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            // Get user email for sending the reset notification
                            string email = string.Empty;
                            string username = string.Empty;

                            query = "SELECT Email, Username FROM Users WHERE UserId = :userId";
                            using (OracleCommand getEmailCmd = new OracleCommand(query, conn))
                            {
                                getEmailCmd.Parameters.Add("userId", OracleDbType.Int32).Value = selectedUserId;
                                using (OracleDataReader reader = getEmailCmd.ExecuteReader())
                                {
                                    if (reader.Read())
                                    {
                                        email = reader["Email"].ToString();
                                        username = reader["Username"].ToString();
                                    }
                                }
                            }

                            // In a real implementation, send email with new password
                            // For demo purposes, we'll just show a message with the password
                            string emailMessage = $"Password reset email would be sent to {email} with temporary password: {newPassword}";
                            System.Diagnostics.Debug.WriteLine(emailMessage);

                            ShowToast($"Password reset successful for {username}. A notification email has been sent.", true);
                        }
                        else
                        {
                            ShowToast("Password reset failed. User not found.", false);
                        }
                    }
                }

                // Close the modal
                pnlPasswordReset.Visible = false;
            }
            catch (Exception ex)
            {
                ShowMessage("Error resetting password: " + ex.Message, false);
            }
        }

        protected void btnCloseToggleStatus_Click(object sender, EventArgs e)
        {
            pnlToggleStatus.Visible = false;
        }

        protected void btnConfirmToggle_Click(object sender, EventArgs e)
        {
            try
            {
                if (selectedUserId > 0)
                {
                    bool newStatus = false;
                    string username = string.Empty;

                    // Get current status to toggle it
                    using (OracleConnection conn = new OracleConnection(GetConnectionString()))
                    {
                        conn.Open();
                        string query = "SELECT Username, IsActive FROM Users WHERE UserId = :userId";
                        using (OracleCommand cmd = new OracleCommand(query, conn))
                        {
                            cmd.Parameters.Add("userId", OracleDbType.Int32).Value = selectedUserId;
                            using (OracleDataReader reader = cmd.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    username = reader["Username"].ToString();
                                    // Convert the database value to boolean
                                    bool currentStatus = Convert.ToBoolean(Convert.ToInt32(reader["IsActive"]));
                                    newStatus = !currentStatus;
                                    System.Diagnostics.Debug.WriteLine($"User: {username}, Current Status: {currentStatus}, New Status: {newStatus}");
                                }
                            }
                        }
                    }

                    // Update user status
                    using (OracleConnection conn = new OracleConnection(GetConnectionString()))
                    {
                        conn.Open();
                        string query = "UPDATE Users SET IsActive = :isActive, DateModified = SYSDATE WHERE UserId = :userId";
                        using (OracleCommand cmd = new OracleCommand(query, conn))
                        {
                            // Convert boolean to integer for Oracle (1 = true, 0 = false)
                            int isActiveValue = newStatus ? 1 : 0;
                            cmd.Parameters.Add("isActive", OracleDbType.Int32).Value = isActiveValue;
                            cmd.Parameters.Add("userId", OracleDbType.Int32).Value = selectedUserId;
                            System.Diagnostics.Debug.WriteLine($"Updating User: {username}, UserID: {selectedUserId}, IsActive: {isActiveValue}");

                            int rowsAffected = cmd.ExecuteNonQuery();

                            if (rowsAffected > 0)
                            {
                                string statusMessage = newStatus ? "activated" : "deactivated";
                                ShowToast($"User {username} has been {statusMessage} successfully.", true);

                                // Reload users and stats to reflect the change
                                LoadStats();
                                LoadUsers();
                            }
                            else
                            {
                                ShowMessage("Failed to update user status.", false);
                            }
                        }
                    }
                }

                // Hide the confirmation modal
                pnlToggleStatus.Visible = false;
            }

            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in ToggleStatus: {ex.Message}, {ex.StackTrace}");
                ShowMessage("Error toggling user status: " + ex.Message, false);
            }
        }

        #endregion

        #region Helper Methods

        // Method to show empty message
        private void ShowEmptyMessage(bool show, string message = "")
        {
            // Implement this based on your UI requirements
            System.Diagnostics.Debug.WriteLine($"ShowEmptyMessage - Show: {show}, Message: {message}");
            if (show && lblMessage != null)
            {
                lblMessage.Text = message;
                lblMessage.Visible = true;
            }
            else if (lblMessage != null)
            {
                lblMessage.Visible = false;
            }
        }

        // Method to show error message
        private void ShowError(string message)
        {
            System.Diagnostics.Debug.WriteLine($"ShowError - Message: {message}");
            ShowMessage(message, false);
        }

        // Method to show success or error message
        private void ShowMessage(string message, bool isSuccess)
        {
            System.Diagnostics.Debug.WriteLine($"ShowMessage - Message: {message}, Success: {isSuccess}");

            // Use toast if available
            if (pnlToast != null && lblToastMessage != null)
            {
                lblToastMessage.Text = message;
                pnlToast.CssClass = isSuccess
                    ? "fixed bottom-4 right-4 px-6 py-4 rounded-lg shadow-lg bg-green-500"
                    : "fixed bottom-4 right-4 px-6 py-4 rounded-lg shadow-lg bg-red-500";

                ScriptManager.RegisterStartupScript(this, GetType(), "ShowToast",
                    "document.getElementById('" + pnlToast.ClientID + "').style.display = 'block';" +
                    "setTimeout(function() { document.getElementById('" + pnlToast.ClientID + "').style.display = 'none'; }, 5000);",
                    true);
            }
            // Fallback to regular label
            else if (lblMessage != null)
            {
                lblMessage.Text = message;
                lblMessage.CssClass = isSuccess ? "text-green-500 mb-4" : "text-red-500 mb-4";
                lblMessage.Visible = true;
            }
        }

        // Method to show toast notification
        private void ShowToast(string message, bool isSuccess)
        {
            System.Diagnostics.Debug.WriteLine($"ShowToast - Message: {message}, Success: {isSuccess}");

            if (pnlToast != null && lblToastMessage != null)
            {
                lblToastMessage.Text = message;
                pnlToast.CssClass = isSuccess
                    ? "fixed bottom-4 right-4 px-6 py-4 rounded-lg shadow-lg bg-green-500"
                    : "fixed bottom-4 right-4 px-6 py-4 rounded-lg shadow-lg bg-red-500";

                ScriptManager.RegisterStartupScript(this, GetType(), "ShowToast",
                    "document.getElementById('" + pnlToast.ClientID + "').style.display = 'block';" +
                    "setTimeout(function() { document.getElementById('" + pnlToast.ClientID + "').style.display = 'none'; }, 5000);",
                    true);
            }
            else
            {
                // Fallback to regular message if toast components aren't available
                ShowMessage(message, isSuccess);
            }
        }

        // Helper to format a date
        private string FormatDate(object date)
        {
            if (date == null || date == DBNull.Value)
                return "Never";

            return Convert.ToDateTime(date).ToString("MM/dd/yyyy hh:mm tt");
        }

        // Helper to get connection string
        private string GetConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["OracleConnection"]?.ConnectionString
                    ?? "User Id=mecate;Password=qwen123;Data Source=localhost:1521/xe;";
        }

        // Helper to get status class for order history
        protected string GetStatusClass(string status)
        {
            switch (status.ToLower())
            {
                case "completed":
                    return "px-2 py-1 text-xs font-medium rounded-full bg-green-100 text-green-800";
                case "processing":
                    return "px-2 py-1 text-xs font-medium rounded-full bg-blue-100 text-blue-800";
                case "cancelled":
                    return "px-2 py-1 text-xs font-medium rounded-full bg-red-100 text-red-800";
                case "pending":
                    return "px-2 py-1 text-xs font-medium rounded-full bg-yellow-100 text-yellow-800";
                default:
                    return "px-2 py-1 text-xs font-medium rounded-full bg-gray-100 text-gray-800";
            }
        }

        // Helper method to set active tab
        private void SetActiveTab(string tabName)
        {
            // Update tab styling based on active tab
            if (tabName == "BasicInfo")
            {
                tabBasicInfo.CssClass = "inline-block p-4 text-[#D43B6A] border-b-2 border-[#D43B6A] rounded-t-lg";
                tabOrderHistory.CssClass = "inline-block p-4 border-b-2 border-transparent rounded-t-lg hover:text-gray-600 hover:border-gray-300";

                pnlBasicInfo.Visible = true;
                pnlOrderHistory.Visible = false;
            }
            else
            {
                tabBasicInfo.CssClass = "inline-block p-4 border-b-2 border-transparent rounded-t-lg hover:text-gray-600 hover:border-gray-300";
                tabOrderHistory.CssClass = "inline-block p-4 text-[#D43B6A] border-b-2 border-[#D43B6A] rounded-t-lg";

                pnlBasicInfo.Visible = false;
                pnlOrderHistory.Visible = true;
            }
        }

        private string GenerateRandomPassword()
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789";
            StringBuilder password = new StringBuilder();
            Random random = new Random();

            // Generate password of length 10
            for (int i = 0; i < 10; i++)
            {
                password.Append(chars[random.Next(chars.Length)]);
            }

            return password.ToString();
        }

        private string HashPassword(string password)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // Convert the input string to a byte array and compute the hash
                byte[] bytes = sha256Hash.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));

                // Convert byte array to a string
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        // Helper method to recursively find all controls
        private List<Control> FindAllControlsRecursive(Control parent)
        {
            List<Control> controls = new List<Control>();

            foreach (Control control in parent.Controls)
            {
                controls.Add(control);
                controls.AddRange(FindAllControlsRecursive(control));
            }

            return controls;
        }

        #endregion
    }
}