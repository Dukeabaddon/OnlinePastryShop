﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using Oracle.ManagedDataAccess.Client;

namespace OnlinePastryShop.Pages
{
    public partial class Users : System.Web.UI.Page
    {
        // Constants
        private const int PAGE_SIZE = 10;
        private int selectedUserId = 0;
        private int currentPage = 1;



        protected void Page_Load(object sender, EventArgs e)
        {
            Debug.WriteLine("----- Page_Load called -----");
            
            try
            {
                if (!IsPostBack)
                {
                    Debug.WriteLine("Initial page load - loading default data");
                    
                    // Set defaults for first load
                    DropDownList ddlStatus = FindControl("ddlStatus") as DropDownList;
                    TextBox txtSearch = FindControl("txtSearch") as TextBox;
                    
                    if (ddlStatus != null) ddlStatus.SelectedValue = "true";
                    if (txtSearch != null) txtSearch.Text = string.Empty;
                    
                    // Load user stats
                    LoadStats();
                    
                    // Load users with default parameters (active users, page 1)
                    LoadUsers(1, "true", string.Empty);
                        }
                        else
                        {
                    // Check if we're handling a page change from JavaScript
                    if (Request.Form["__EVENTTARGET"] == "btnChangePage")
                    {
                        string pageArg = Request.Form["__EVENTARGUMENT"];
                        if (!string.IsNullOrEmpty(pageArg) && int.TryParse(pageArg, out int page))
                        {
                            // Get current filter values from hidden fields
                            HiddenField hdnStatus = FindControl("hdnStatus") as HiddenField;
                            HiddenField hdnSearch = FindControl("hdnSearch") as HiddenField;
                            
                            string activeStatus = hdnStatus?.Value ?? "true";
                            string searchText = hdnSearch?.Value ?? string.Empty;
                            
                            Debug.WriteLine($"JavaScript page change to page {page} detected");
                            LoadUsers(page, activeStatus, searchText);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in Page_Load: {ex.Message}");
                Debug.WriteLine(ex.StackTrace);
                ShowError("An error occurred while loading the page. Please try again.");
            }
        }

        #region Data Loading Methods

        private void LoadUsers(int page = 1, string activeStatus = "all", string searchText = "")
        {
            try
            {
                Debug.WriteLine($"LoadUsers called with page={page}, activeStatus={activeStatus}, searchText={searchText}");
                
                // Store current page for pagination
                currentPage = page;
                
                // Validate and set parameters
                if (string.IsNullOrEmpty(activeStatus)) activeStatus = "all";
                if (searchText == null) searchText = "";

                string connectionString = GetConnectionString();

                // Create a list to hold all user data
                List<UserModel> allUsers = new List<UserModel>();
                
                using (OracleConnection conn = new OracleConnection(connectionString))
                {
                    conn.Open();

                    // Use a very simple query to get all users first
                    string sql = @"SELECT USERID, USERNAME, EMAIL, ROLE, DATECREATED, DATEMODIFIED, LASTLOGIN, ISACTIVE 
                                 FROM AARON_IPT.USERS";
                    
                    using (OracleCommand cmd = new OracleCommand(sql, conn))
                    {
                        using (OracleDataReader reader = cmd.ExecuteReader())
                        {
                            // Read all users into memory
                            while (reader.Read())
                            {
                                // Convert data directly from reader to UserModel
                                UserModel user = new UserModel
                                {
                                    UserId = Convert.ToInt32(reader["USERID"]),
                                    Username = reader["USERNAME"].ToString(),
                                    Email = reader["EMAIL"].ToString(),
                                    Role = reader["ROLE"] != DBNull.Value ? reader["ROLE"].ToString() : string.Empty,
                                    DateCreated = reader["DATECREATED"] != DBNull.Value ? Convert.ToDateTime(reader["DATECREATED"]) : DateTime.MinValue,
                                    DateModified = reader["DATEMODIFIED"] != DBNull.Value ? Convert.ToDateTime(reader["DATEMODIFIED"]) : DateTime.MinValue,
                                    LastLogin = reader["LASTLOGIN"] != DBNull.Value ? Convert.ToDateTime(reader["LASTLOGIN"]) : DateTime.MinValue,
                                    IsActive = reader["ISACTIVE"] != DBNull.Value && Convert.ToInt32(reader["ISACTIVE"]) == 1
                                };
                                
                                allUsers.Add(user);
                            }
                        }
                    }
                    
                    Debug.WriteLine($"Retrieved {allUsers.Count} total users from database");
                }
                
                // Filter users in memory - much more reliable than SQL in this case
                var filteredUsers = allUsers.AsQueryable();
                
                // Apply active/inactive filter if needed
                if (activeStatus.ToLower() == "true")
                {
                    filteredUsers = filteredUsers.Where(u => u.IsActive);
                    Debug.WriteLine("Filtering for active users only");
                }
                else if (activeStatus.ToLower() == "false")
                {
                    filteredUsers = filteredUsers.Where(u => !u.IsActive);
                    Debug.WriteLine("Filtering for inactive users only");
                }
                
                // Apply search filter if provided
                if (!string.IsNullOrEmpty(searchText))
                {
                    string search = searchText.ToLower();
                    filteredUsers = filteredUsers.Where(u => 
                        u.Username.ToLower().Contains(search) || 
                        u.Email.ToLower().Contains(search) || 
                        (u.Role != null && u.Role.ToLower().Contains(search))
                    );
                    Debug.WriteLine($"Applied search filter: '{searchText}'");
                }
                
                // Order by UserId
                filteredUsers = filteredUsers.OrderBy(u => u.UserId);
                
                // Get total count after filtering
                int totalUsers = filteredUsers.Count();
                Debug.WriteLine($"After filtering: {totalUsers} users match criteria");
                
                // Calculate total pages
                int pageSize = PAGE_SIZE;
                int totalPages = (int)Math.Ceiling((double)totalUsers / pageSize);
                
                // Make sure the requested page is valid
                if (page > totalPages && totalPages > 0)
                {
                    page = totalPages;
                    currentPage = page;
                }
                else if (page < 1)
                {
                    page = 1;
                    currentPage = page;
                }
                
                // Apply pagination in memory
                var pagedUsers = filteredUsers
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();
                
                Debug.WriteLine($"After paging: Showing {pagedUsers.Count} users for page {page}");
                
                // List all users in the current page for debugging
                if (pagedUsers.Any())
                {
                    Debug.WriteLine("Users in current page:");
                    foreach (var user in pagedUsers)
                    {
                        Debug.WriteLine($"  User {user.UserId}: {user.Username}, IsActive={user.IsActive}");
                    }
                }
                
                // Store filter values in hidden fields
                HiddenField hdnStatusField = GetControlById("hdnStatus") as HiddenField;
                HiddenField hdnSearchField = GetControlById("hdnSearch") as HiddenField;
                if (hdnStatusField != null) hdnStatusField.Value = activeStatus;
                if (hdnSearchField != null) hdnSearchField.Value = searchText;
                
                // Save to ViewState too for reliability
                ViewState["CurrentActiveStatus"] = activeStatus;
                ViewState["CurrentSearchText"] = searchText;
                
                // Bind data to ListView
                lvUsers.DataSource = pagedUsers;
                lvUsers.DataBind();
                
                // Update pagination info
                lblCurrentPage.Text = page.ToString();
                lblTotalPages.Text = totalPages.ToString();
                hdnCurrentPage.Value = page.ToString();
                hdnTotalPages.Value = totalPages.ToString();
                
                // Handle the case when no users are found
                if (pagedUsers.Count == 0)
                {
                    Debug.WriteLine("No users found with the current filter");
                    Label lblNoUsers = GetControlById("lblNoUsers") as Label;
                    if (lblNoUsers != null)
                    {
                        if (!string.IsNullOrEmpty(searchText))
                        {
                            lblNoUsers.Text = $"No users found matching '{searchText}'.";
                        }
                        else if (activeStatus.ToLower() != "all")
                        {
                            lblNoUsers.Text = $"No {(activeStatus.ToLower() == "true" ? "active" : "inactive")} users found.";
                            }
                            else
                            {
                            lblNoUsers.Text = "No users found.";
                        }
                        lblNoUsers.Visible = true;
                    }
                    
                    ListView lvUsersList = GetControlById("lvUsers") as ListView;
                    if (lvUsersList != null) lvUsersList.Visible = false;
                    
                    // Update the total user count in the UI
                    Literal litTotalUsers = GetControlById("litTotalUsers") as Literal;
                    if (litTotalUsers != null) litTotalUsers.Text = totalUsers.ToString();
                    }
                    else
                    {
                    // Show the ListView
                    Label lblNoUsers = GetControlById("lblNoUsers") as Label;
                    if (lblNoUsers != null) lblNoUsers.Visible = false;
                    
                    ListView lvUsersList = GetControlById("lvUsers") as ListView;
                    if (lvUsersList != null) lvUsersList.Visible = true;
                    
                    // Generate pagination buttons
                    GeneratePaginationButtons();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in LoadUsers: {ex.Message}");
                Debug.WriteLine(ex.StackTrace);
                ShowError("Error loading users. Please try again.");
            }
        }
        
        private void LoadStats()
        {
            try
            {
                // Get the connection string
                string connectionString = GetConnectionString();

                using (OracleConnection conn = new OracleConnection(connectionString))
                {
                    conn.Open();
                    Debug.WriteLine("LoadStats: Connection opened successfully");

                    // DEBUGGING: Check ISACTIVE values directly from the database
                    try
                    {
                        Debug.WriteLine("==== CHECKING ISACTIVE VALUES START ====");
                        using (OracleCommand cmd = new OracleCommand("SELECT USERID, USERNAME, ISACTIVE FROM AARON_IPT.USERS", conn))
                        {
                            using (OracleDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    int userId = reader.GetInt32(0);
                                    string username = reader.GetString(1);
                                    object isActiveObj = reader[2];
                                    
                                    Debug.WriteLine($"User ID: {userId}, Username: {username}");
                                    Debug.WriteLine($"ISACTIVE type: {isActiveObj?.GetType().FullName ?? "null"}, Value: '{isActiveObj?.ToString() ?? "null"}'");
                                    
                                    // Try different conversions to understand the data type
                                    try 
                                    {
                                        if (isActiveObj != null && isActiveObj != DBNull.Value)
                                        {
                                            bool? boolValue = null;
                                            int? intValue = null;
                                            string strValue = isActiveObj.ToString().ToLower();
                                            
                                            // Try bool conversion
                                            try { boolValue = Convert.ToBoolean(isActiveObj); } catch { }
                                            
                                            // Try int conversion
                                            try { intValue = Convert.ToInt32(isActiveObj); } catch { }
                                            
                                            Debug.WriteLine($"  - As bool: {boolValue}");
                                            Debug.WriteLine($"  - As int: {intValue}");
                                            Debug.WriteLine($"  - As string: '{strValue}'");
                                            Debug.WriteLine($"  - String equals 'true': {strValue == "true"}");
                                            Debug.WriteLine($"  - String equals '1': {strValue == "1"}");
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Debug.WriteLine($"Error in conversion tests: {ex.Message}");
                                    }
                                }
                            }
                        }
                        Debug.WriteLine("==== CHECKING ISACTIVE VALUES END ====");
            }
            catch (Exception ex)
            {
                        Debug.WriteLine($"Error checking ISACTIVE values: {ex.Message}");
                    }

                    // Continue with original LoadStats functionality 
                    // Test table existence first
                    try
                    {
                        using (OracleCommand testCmd = new OracleCommand(
                            "SELECT COUNT(*) FROM ALL_TABLES WHERE OWNER = 'AARON_IPT' AND TABLE_NAME = 'USERS'", conn))
                        {
                            int tableExists = Convert.ToInt32(testCmd.ExecuteScalar());
                            Debug.WriteLine($"USERS table exists check for stats: {tableExists > 0}");
                            
                            if (tableExists == 0)
                            {
                                Debug.WriteLine("USERS table does not exist in the AARON_IPT schema");
                                // Set defaults for controls
                                Literal litTotalUsers = GetControlById("litTotalUsers") as Literal;
                                Literal litNewUsers = GetControlById("litNewUsers") as Literal;
                                
                                if (litTotalUsers != null) litTotalUsers.Text = "0";
                                if (litNewUsers != null) litNewUsers.Text = "0";
                                
                                return;
                            }
                        }
                    }
                    catch (Exception tableEx)
                    {
                        Debug.WriteLine($"Error checking table existence for stats: {tableEx.Message}");
                    }

                    // STEP 1: Get total users count
                int totalUsers = 0;
                    try
                    {
                        using (OracleCommand cmd = new OracleCommand("SELECT COUNT(*) FROM AARON_IPT.USERS", conn))
                        {
                            totalUsers = Convert.ToInt32(cmd.ExecuteScalar());
                            Debug.WriteLine($"Total users count: {totalUsers}");
                            
                            // Find control
                            Literal litTotalUsers = GetControlById("litTotalUsers") as Literal;
                            if (litTotalUsers != null)
                            {
                                litTotalUsers.Text = totalUsers.ToString();
                            }
                        }
                    }
                    catch (Exception countEx)
                    {
                        Debug.WriteLine($"Error getting total users count: {countEx.Message}");
                        // Set default value
                        Literal litTotalUsers = GetControlById("litTotalUsers") as Literal;
                        if (litTotalUsers != null) litTotalUsers.Text = "0";
                    }
                    
                    // STEP 2: Get new users count (last 30 days)
                    int newUsers = 0;
                    try
                    {
                        DateTime startDate = DateTime.Now.AddDays(-30);
                        string newUsersQuery = "SELECT COUNT(*) FROM AARON_IPT.USERS WHERE DATECREATED >= :startDate";
                        
                        using (OracleCommand cmd = new OracleCommand(newUsersQuery, conn))
                        {
                            cmd.Parameters.Add(new OracleParameter("startDate", OracleDbType.Date)).Value = startDate;
                            
                            newUsers = Convert.ToInt32(cmd.ExecuteScalar());
                            Debug.WriteLine($"New users count (last 30 days): {newUsers}");
                            
                            // Find control
                            Literal litNewUsers = GetControlById("litNewUsers") as Literal;
                            if (litNewUsers != null)
                            {
                                litNewUsers.Text = newUsers.ToString();
                            }
                        }
                    }
                    catch (Exception newUsersEx)
                    {
                        Debug.WriteLine($"Error getting new users count: {newUsersEx.Message}");
                        // Set default value
                        Literal litNewUsers = GetControlById("litNewUsers") as Literal;
                        if (litNewUsers != null) litNewUsers.Text = "0";
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in LoadStats: {ex.Message}");
                ShowError("An error occurred while loading statistics.");
            }
        }

        private void LoadOrderHistory(int userId)
        {
             try
            {
                Debug.WriteLine($"LoadOrderHistory called for userId: {userId}");
                
                // Reset debug label
                lblOrderHistoryDebug.Visible = false;
                
                string connectionString = GetConnectionString();
                using (OracleConnection conn = new OracleConnection(connectionString))
                {
                    conn.Open();
                    
                    try
                    {
                        // Simple query that works with the actual table structure
                        // Include a count of ORDER_DETAIL items as ITEMCOUNT to match what GridView expects
                        string simpleQuery = @"
                            SELECT 
                                o.ORDERID, 
                                o.ORDERDATE, 
                                o.TOTALAMOUNT, 
                                o.STATUS,
                                0 AS ITEMCOUNT
                            FROM 
                                ORDERS o
                            WHERE 
                                o.USERID = :userId
                            ORDER BY 
                                o.ORDERDATE DESC";
                        
                        Debug.WriteLine($"Executing simplified order history query for user {userId}");
                        
                        using (OracleCommand cmd = new OracleCommand(simpleQuery, conn))
                        {
                            cmd.Parameters.Add(new OracleParameter("userId", OracleDbType.Int32) { Value = userId });
                            
                            OracleDataAdapter adapter = new OracleDataAdapter(cmd);
                            DataTable orderTable = new DataTable();
                            adapter.Fill(orderTable);
                            
                            Debug.WriteLine($"Found {orderTable.Rows.Count} orders for user ID {userId}");
                            
                            if (orderTable.Rows.Count > 0)
                            {
                                gvOrderHistory.DataSource = orderTable;
                                gvOrderHistory.DataBind();
                            }
                            else
                            {
                                // No orders found, set empty data text
                                gvOrderHistory.DataSource = null;
                                gvOrderHistory.DataBind();
                                
                                Debug.WriteLine("No orders found for this user");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error executing order query: {ex.Message}");
                        throw; // Re-throw to be caught by outer try/catch
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in LoadOrderHistory: {ex.Message}");
                lblOrderHistoryDebug.Text = $"Error loading order history: {ex.Message}";
                lblOrderHistoryDebug.Visible = true;
            }
        }

        private void DiagnoseDatabase()
        {
            try
            {
                Debug.WriteLine("===== RUNNING DATABASE DIAGNOSIS =====");
                
                // Check connection and examine ISACTIVE values
                string connectionString = GetConnectionString();
                using (OracleConnection conn = new OracleConnection(connectionString))
                {
                    conn.Open();
                    Debug.WriteLine("Successfully connected to the database");

                    // Check if USERS table exists
                    using (OracleCommand cmd = new OracleCommand("SELECT COUNT(*) FROM ALL_TABLES WHERE OWNER = 'AARON_IPT' AND TABLE_NAME = 'USERS'", conn))
                    {
                        int tableCount = Convert.ToInt32(cmd.ExecuteScalar());
                        Debug.WriteLine($"USERS table exists: {tableCount > 0}");
                    }

                    // Get column information for USERS table
                    using (OracleCommand cmd = new OracleCommand(@"
                        SELECT COLUMN_NAME, DATA_TYPE, DATA_LENGTH, NULLABLE 
                        FROM ALL_TAB_COLUMNS 
                        WHERE OWNER = 'AARON_IPT' AND TABLE_NAME = 'USERS'
                        ORDER BY COLUMN_ID", conn))
                    {
                        Debug.WriteLine("USERS table columns:");
                        using (OracleDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Debug.WriteLine($"  {reader["COLUMN_NAME"]} ({reader["DATA_TYPE"]}, {reader["DATA_LENGTH"]}) {(reader["NULLABLE"].ToString() == "Y" ? "NULL" : "NOT NULL")}");
                            }
                        }
                    }

                    // Check for distinct ISACTIVE values
                    using (OracleCommand cmd = new OracleCommand("SELECT ISACTIVE, COUNT(*) FROM AARON_IPT.USERS GROUP BY ISACTIVE", conn))
                    {
                        Debug.WriteLine("ISACTIVE value distribution:");
                        using (OracleDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string isActiveValue = reader[0] == DBNull.Value ? "NULL" : reader[0].ToString();
                                Debug.WriteLine($"  ISACTIVE = {isActiveValue}: {reader[1]} records (Type: {(reader[0] == DBNull.Value ? "NULL" : reader[0].GetType().Name)})");
                            }
                        }
                    }

                    // Try direct query with limited columns and no WHERE clause
                    using (OracleCommand cmd = new OracleCommand("SELECT USERID, USERNAME, EMAIL, ISACTIVE FROM AARON_IPT.USERS", conn))
                    {
                        Debug.WriteLine("Direct query results (first 5 users):");
                        int count = 0;
                        using (OracleDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read() && count < 5)
                            {
                                count++;
                                Debug.WriteLine($"  User {reader["USERID"]}: {reader["USERNAME"]}, {reader["EMAIL"]}, ISACTIVE={reader["ISACTIVE"]} (Type: {reader["ISACTIVE"].GetType().Name})");
                            }
                            // Check if there are more records
                            if (reader.Read())
                            {
                                Debug.WriteLine("  ... more records exist");
                            }
                        }
                        
                        if (count == 0)
                        {
                            Debug.WriteLine("  NO USERS FOUND in direct query!");
                        }
                    }

                    // Try a simple version of our pagination query
                    string testQuery = @"
                        SELECT * FROM (
                            SELECT a.*, ROWNUM AS rn FROM (
                                SELECT USERID, USERNAME, EMAIL, ISACTIVE
                                FROM AARON_IPT.USERS
                                ORDER BY USERID
                            ) a
                            WHERE ROWNUM <= 10
                        )
                        WHERE rn >= 1";

                    using (OracleCommand cmd = new OracleCommand(testQuery, conn))
                    {
                        Debug.WriteLine("Testing simplified pagination query:");
                        try
                        {
                            using (OracleDataReader reader = cmd.ExecuteReader())
                            {
                                int count = 0;
                                while (reader.Read() && count < 5)
                                {
                                    count++;
                                    Debug.WriteLine($"  User {reader["USERID"]}: {reader["USERNAME"]}, ISACTIVE={reader["ISACTIVE"]}");
                                }
                                
                                if (count == 0)
                                {
                                    Debug.WriteLine("  NO USERS FOUND in pagination test query!");
                                }
                            }
            }
            catch (Exception ex)
            {
                            Debug.WriteLine($"  ERROR in test query: {ex.Message}");
                        }
                    }
                }
                
                Debug.WriteLine("===== DATABASE DIAGNOSIS COMPLETE =====");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Database diagnosis error: {ex.Message}");
                Debug.WriteLine(ex.StackTrace);
            }
        }

        #endregion

        #region Button Event Handlers

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            Debug.WriteLine("Search button clicked");
            
            // Get filter values directly from controls
            string searchText = txtSearch.Text.Trim();
            string status = ddlStatus.SelectedValue;
            
            Debug.WriteLine($"Search clicked with text: '{searchText}', status: {status}");
            
            // Update hidden fields - get them by ID first
            HiddenField hdnStatusField = FindControl("hdnStatus") as HiddenField;
            HiddenField hdnSearchField = FindControl("hdnSearch") as HiddenField;
            
            if (hdnStatusField != null) hdnStatusField.Value = status;
            if (hdnSearchField != null) hdnSearchField.Value = searchText;
            
            // Reset to page 1 for new search and pass selected status value directly to LoadUsers
            LoadUsers(1, status, searchText);
        }
        
        protected void btnReset_Click(object sender, EventArgs e)
        {
            Debug.WriteLine("Reset search clicked");
            
            // Reset controls to default values
                txtSearch.Text = string.Empty;
                    ddlStatus.SelectedValue = "true";
            
            // Reset hidden fields
            HiddenField hdnStatusField = FindControl("hdnStatus") as HiddenField;
            HiddenField hdnSearchField = FindControl("hdnSearch") as HiddenField;
            
            if (hdnStatusField != null) hdnStatusField.Value = "true";
            if (hdnSearchField != null) hdnSearchField.Value = string.Empty;
            
            // Reset to page 1 and load with default filters
            LoadUsers(1, "true", string.Empty);
        }

        protected void Page_Click(object sender, EventArgs e)
        {
            try
            {
                Debug.WriteLine("Page_Click called");
                
                // Get current page and total pages
                int currentPageNum = 1;
                int totalPages = 1;
                
                if (int.TryParse(hdnCurrentPage.Value, out currentPageNum) && 
                    int.TryParse(hdnTotalPages.Value, out totalPages))
                {
                    Debug.WriteLine($"Current page: {currentPageNum}, Total pages: {totalPages}");
                }
                
                // Determine which page to navigate to based on the sender
                int targetPage = currentPageNum;
                
                if (sender is LinkButton btn)
                {
                    string arg = btn.CommandArgument.ToLower();
                    Debug.WriteLine($"Command argument: {arg}");
                    
                    switch (arg)
                    {
                        case "first":
                            targetPage = 1;
                            break;
                        case "prev":
                            targetPage = Math.Max(1, currentPageNum - 1);
                            break;
                        case "next":
                            targetPage = Math.Min(totalPages, currentPageNum + 1);
                            break;
                        case "last":
                            targetPage = totalPages;
                            break;
                        default:
                            // Try to parse as a page number
                            if (int.TryParse(arg, out int pageNum))
                            {
                                targetPage = pageNum;
                            }
                            break;
                    }
                }
                else if (sender is Button)
                {
                    // For btnChangePage, we use the value from hdnCurrentPage
                    // which is set by the JavaScript changePage function
                    targetPage = currentPageNum;
                    Debug.WriteLine($"Button click changing to page {targetPage}");
                }
                
                Debug.WriteLine($"Navigating to page {targetPage}");
                
                // Get filter values
                string activeStatus = ddlStatus.SelectedValue;
                string searchText = txtSearch.Text;
                
                // Load the users for the target page
                LoadUsers(targetPage, activeStatus, searchText);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in Page_Click: {ex.Message}");
                ShowError("Error navigating to page: " + ex.Message);
            }
        }

        protected void lvUsers_ItemCommand(object sender, ListViewCommandEventArgs e)
        {
            try
            {
                // Get the user ID
                    if (e.CommandArgument != null)
                    {
                        selectedUserId = Convert.ToInt32(e.CommandArgument);
                    }
                    
                // Execute the appropriate action
                switch (e.CommandName)
                    {
                    case "ViewDetails":
                        ShowUserDetails(selectedUserId);
                        break;
                    case "ResetPassword":
                        ShowResetPassword(selectedUserId);
                        break;
                    case "ToggleStatus":
                        ShowToggleStatus(selectedUserId);
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error processing command: {ex.Message}");
                ShowError($"Error processing command: {ex.Message}");
            }
        }

        protected void btnCloseDetails_Click(object sender, EventArgs e)
        {
            pnlUserDetails.Visible = false;
        }

        protected void tabBasicInfo_Click(object sender, EventArgs e)
        {
            try
            {
                pnlBasicInfo.Visible = true;
                pnlOrderHistory.Visible = false;
                
                // Update tab styling
                tabBasicInfo.CssClass = "border-[#D43B6A] text-[#D43B6A] whitespace-nowrap py-4 px-1 border-b-2 font-medium text-sm";
                tabOrderHistory.CssClass = "border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300 whitespace-nowrap py-4 px-1 border-b-2 font-medium text-sm";
                
                Debug.WriteLine("Basic Info tab activated");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in tabBasicInfo_Click: {ex.Message}");
            }
        }

        protected void tabOrderHistory_Click(object sender, EventArgs e)
        {
            try
            {
                // Get user ID from ViewState
                int userId = 0;
                if (ViewState["SelectedUserId"] != null)
                {
                    userId = Convert.ToInt32(ViewState["SelectedUserId"]);
                    Debug.WriteLine($"Loading order history for userId: {userId}");
                    
                    // Load order history data
                    LoadOrderHistory(userId);
                }
                else
                {
                    Debug.WriteLine("ERROR: SelectedUserId not found in ViewState for order history");
                    lblOrderHistoryDebug.Text = "User ID not found. Cannot load order history.";
                    lblOrderHistoryDebug.Visible = true;
                }
                
                // Show order history panel and hide basic info
                pnlBasicInfo.Visible = false;
                pnlOrderHistory.Visible = true;
                
                // Update tab styling
                tabBasicInfo.CssClass = "border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300 whitespace-nowrap py-4 px-1 border-b-2 font-medium text-sm";
                tabOrderHistory.CssClass = "border-[#D43B6A] text-[#D43B6A] whitespace-nowrap py-4 px-1 border-b-2 font-medium text-sm";
                
                Debug.WriteLine("Order History tab activated");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in tabOrderHistory_Click: {ex.Message}");
                lblOrderHistoryDebug.Text = $"Error loading order history: {ex.Message}";
                lblOrderHistoryDebug.Visible = true;
            }
        }

        protected void btnCancelReset_Click(object sender, EventArgs e)
        {
            pnlPasswordReset.Visible = false;
        }

        protected void btnConfirmReset_Click(object sender, EventArgs e)
        {
            int userId = 0;

            try
            {
                // Retrieve the user ID from ViewState
                if (ViewState["SelectedUserId"] != null)
                {
                    userId = Convert.ToInt32(ViewState["SelectedUserId"]);
                    Debug.WriteLine($"Resetting password for userId: {userId}");
                }
                else
                {
                    Debug.WriteLine("ERROR: SelectedUserId not found in ViewState");
                    ShowError("User ID not found. Please try again.");
                    pnlPasswordReset.Visible = false;
                    return;
                }
                
                // Generate a random password (8 characters, alphanumeric)
                string newPassword = GenerateRandomPassword(10);
                string hashedPassword = HashPassword(newPassword);

                // Update the user's password in the database
                string connectionString = GetConnectionString();
                
                using (OracleConnection conn = new OracleConnection(connectionString))
                {
                    conn.Open();
                    string sql = "UPDATE AARON_IPT.USERS SET PASSWORD = :password, DATEMODIFIED = SYSDATE WHERE USERID = :userId";
                    
                    using (OracleCommand cmd = new OracleCommand(sql, conn))
                    {
                        cmd.Parameters.Add(new OracleParameter("password", OracleDbType.Varchar2) { Value = hashedPassword });
                        cmd.Parameters.Add(new OracleParameter("userId", OracleDbType.Int32) { Value = userId });
                        
                        int rowsAffected = cmd.ExecuteNonQuery();
                        Debug.WriteLine($"Password reset affected {rowsAffected} rows");
                        
                        if (rowsAffected > 0)
                        {
                            // Show the new password
                            pnlResetConfirm.Visible = false;
                            pnlResetComplete.Visible = true;
                            txtNewPassword.Text = newPassword;
                            lblResetConfirmMessage.Text = "Password has been reset successfully!";
                            }
                            else
                            {
                            ShowError("Failed to reset password. Please try again.");
                            pnlPasswordReset.Visible = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in btnConfirmReset_Click: {ex.Message}");
                Debug.WriteLine(ex.StackTrace);
                ShowError($"An error occurred: {ex.Message}");
                pnlPasswordReset.Visible = false;
            }
        }

        protected void btnCloseReset_Click(object sender, EventArgs e)
        {
            // Clear the password field and hide the panel
            txtNewPassword.Text = string.Empty;
            pnlPasswordReset.Visible = false;
            pnlResetConfirm.Visible = true;
            pnlResetComplete.Visible = false;
            
            // Hide any success messages when closing the modal
            pnlSuccess.Visible = false;
        }

        protected void btnCancelStatus_Click(object sender, EventArgs e)
        {
            pnlToggleStatus.Visible = false;
        }

        protected void btnConfirmStatus_Click(object sender, EventArgs e)
        {
            int userId = 0;

            try
            {
                // Retrieve the user ID from ViewState
                if (ViewState["SelectedUserId"] != null)
                {
                    userId = Convert.ToInt32(ViewState["SelectedUserId"]);
                    Debug.WriteLine($"btnConfirmStatus_Click for userId: {userId}");
                }
                else
                {
                    Debug.WriteLine("ERROR: SelectedUserId not found in ViewState");
                    ShowError("User ID not found. Please try again.");
                    pnlToggleStatus.Visible = false;
                    return;
                }
                
                // Get the connection string
                string connectionString = GetConnectionString();
                
                using (OracleConnection conn = new OracleConnection(connectionString))
                {
                    conn.Open();

                    // First get the current status
                    int currentStatus = 0;
                    string getCurrentStatusSql = "SELECT ISACTIVE FROM AARON_IPT.USERS WHERE USERID = :userId";
                    
                    using (OracleCommand getCmd = new OracleCommand(getCurrentStatusSql, conn))
                    {
                        getCmd.Parameters.Add(new OracleParameter("userId", OracleDbType.Int32) { Value = userId });
                        
                        var result = getCmd.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            currentStatus = Convert.ToInt32(result);
                            Debug.WriteLine($"Current status for userId {userId}: {currentStatus}");
                }
                else
                {
                            Debug.WriteLine($"User not found with ID: {userId}");
                    ShowError("User not found.");
                            pnlToggleStatus.Visible = false;
                            return;
                        }
                    }

                    // Toggle the status (0 to 1 or 1 to 0)
                    int newStatus = (currentStatus == 1) ? 0 : 1;
                    Debug.WriteLine($"Changing status from {currentStatus} to {newStatus}");

                    // Update the user status
                    string updateSql = "UPDATE AARON_IPT.USERS SET ISACTIVE = :isActive, DATEMODIFIED = SYSDATE WHERE USERID = :userId";
                    
                    using (OracleCommand updateCmd = new OracleCommand(updateSql, conn))
                    {
                        updateCmd.Parameters.Add(new OracleParameter("isActive", OracleDbType.Int32) { Value = newStatus });
                        updateCmd.Parameters.Add(new OracleParameter("userId", OracleDbType.Int32) { Value = userId });
                    
                    int rowsAffected = updateCmd.ExecuteNonQuery();
                        Debug.WriteLine($"Status update affected {rowsAffected} rows");
                    
                    if (rowsAffected > 0)
                    {
                            string statusText = newStatus == 1 ? "activated" : "deactivated";
                            ShowSuccess($"User has been successfully {statusText}.");
                }
                else
                {
                            ShowError("Failed to update user status. Please try again.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in btnConfirmStatus_Click: {ex.Message}");
                Debug.WriteLine(ex.StackTrace);
                ShowError($"An error occurred: {ex.Message}");
            }
            finally
            {
                // Close the modal
                pnlToggleStatus.Visible = false;
                
                // Refresh the user list
                LoadUsers(Convert.ToInt32(hdnCurrentPage.Value), ddlStatus.SelectedValue, txtSearch.Text);
            }
        }

        protected void ddlStatus_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                Debug.WriteLine("Status dropdown selection changed");
                
                // Get the selected status
                string status = ddlStatus.SelectedValue;
                
                // Get current search text
                string searchText = txtSearch.Text.Trim();
                
                Debug.WriteLine($"Status changed to: {status}, with search text: '{searchText}'");
                
                // Update hidden fields with proper casting and null checks
                var hdnStatusField = Page.FindControl("hdnStatus") as HiddenField;
                var hdnSearchField = Page.FindControl("hdnSearch") as HiddenField;
                
                if (hdnStatusField != null) hdnStatusField.Value = status;
                if (hdnSearchField != null) hdnSearchField.Value = searchText;
                
                // Reset to page 1 and reload users with new status filter
                LoadUsers(1, status, searchText);
                
                // Show feedback with popup notification
                string statusText = status.ToLower() == "true" ? "active" : "inactive";
                ShowSuccess($"Showing {statusText} users");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in ddlStatus_SelectedIndexChanged: {ex.Message}");
                ShowError("An error occurred while updating the status filter.");
            }
        }

        #endregion

        #region Helper Methods

        private void ShowUserDetails(int userId)
        {
            try
            {
                Debug.WriteLine($"ShowUserDetails called for userId: {userId}");
                
                // Update the currently selected user ID
                selectedUserId = userId;
                
                // Add to ViewState for redundancy
                ViewState["SelectedUserId"] = userId;
                
                // Store userId in hidden field via client-side script 
                ClientScript.RegisterHiddenField("hdnSelectedUserId", userId.ToString());
                
                // Get the connection string
                string connectionString = GetConnectionString();
                
                using (OracleConnection conn = new OracleConnection(connectionString))
                {
                    conn.Open();
                    
                    string sql = @"
                        SELECT 
                            USERID, USERNAME, EMAIL, ROLE, 
                            DATECREATED, DATEMODIFIED, LASTLOGIN, ISACTIVE
                        FROM AARON_IPT.USERS 
                        WHERE USERID = :userId";
                    
                    using (OracleCommand cmd = new OracleCommand(sql, conn))
                    {
                        cmd.Parameters.Add(new OracleParameter("userId", OracleDbType.Int32) { Value = userId });
                        
                        using (OracleDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // Get the ISACTIVE value and convert it properly
                                int isActiveValue = 0;
                                bool isActive = false;
                                
                                if (reader["ISACTIVE"] != DBNull.Value)
                                {
                                    isActiveValue = Convert.ToInt32(reader["ISACTIVE"]);
                                    isActive = (isActiveValue == 1);
                                }
                                
                                Debug.WriteLine($"User details: USERID={reader["USERID"]}, USERNAME={reader["USERNAME"]}, ISACTIVE={isActiveValue} (IsActive={isActive})");
                                
                                // Populate the details form with user information
                                lblUserId.Text = reader["USERID"].ToString();
                                lblUsername.Text = reader["USERNAME"].ToString();
                                lblEmail.Text = reader["EMAIL"].ToString();
                                lblRole.Text = reader["ROLE"] != DBNull.Value ? reader["ROLE"].ToString() : "N/A";
                                
                                // Set the status with appropriate styling
                                lblStatus.Text = isActive ? "Active" : "Inactive";
                                lblStatus.CssClass = isActive 
                                    ? "mt-1 block w-full text-sm text-green-600 font-medium"
                                    : "mt-1 block w-full text-sm text-red-600 font-medium";
                                
                                // Format dates if present
                                lblDateCreated.Text = reader["DATECREATED"] != DBNull.Value ? Convert.ToDateTime(reader["DATECREATED"]).ToString("MM/dd/yyyy hh:mm tt") : "N/A";
                                lblDateModified.Text = reader["DATEMODIFIED"] != DBNull.Value ? Convert.ToDateTime(reader["DATEMODIFIED"]).ToString("MM/dd/yyyy hh:mm tt") : "N/A";
                                lblLastLogin.Text = reader["LASTLOGIN"] != DBNull.Value ? Convert.ToDateTime(reader["LASTLOGIN"]).ToString("MM/dd/yyyy hh:mm tt") : "N/A";
                                
                                // Show the user details panel and activate the basic info tab by default
                                pnlUserDetails.Visible = true;
                                
                                // Set tab styling
                                tabBasicInfo.CssClass = "border-[#D43B6A] text-[#D43B6A] whitespace-nowrap py-4 px-1 border-b-2 font-medium text-sm";
                                tabOrderHistory.CssClass = "border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300 whitespace-nowrap py-4 px-1 border-b-2 font-medium text-sm";
                                
                                // Show basic info panel, hide order history
                                pnlBasicInfo.Visible = true;
                                pnlOrderHistory.Visible = false;
                                
                                // Register a script to ensure user modal stays visible
                                string script = @"
                                    document.addEventListener('DOMContentLoaded', function() {
                                        // Show the modal
                                        var modal = document.getElementById('pnlUserDetails');
                                        if (modal) {
                                            modal.style.display = 'block';
                                        }
                                        
                                        // Set up tab functionality
                                        showUserTab('basicInfo');
                                    });";
                                
                                ScriptManager.RegisterStartupScript(this, this.GetType(), "showUserDetailsScript", script, true);
                        }
                        else
                        {
                                ShowError("User not found.");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in ShowUserDetails: {ex.Message}");
                Debug.WriteLine(ex.StackTrace);
                ShowError($"Error retrieving user details: {ex.Message}");
            }
        }

        private void ShowResetPassword(int userId)
        {
            try
            {
                Debug.WriteLine($"Showing password reset for user {userId}");
                
                // Store userId in multiple places for reliability
                selectedUserId = userId;
                ViewState["SelectedUserId"] = userId;
                
                // Create a hidden field if it doesn't exist
                HiddenField hdnSelectedUserId = GetControlById("hdnSelectedUserId") as HiddenField;
                if (hdnSelectedUserId != null)
                {
                    hdnSelectedUserId.Value = userId.ToString();
                }
                
                // Get username for the confirmation message
                string username = "";
                string connectionString = GetConnectionString();
                
                using (OracleConnection conn = new OracleConnection(connectionString))
                {
                    conn.Open();
                    
                    string query = "SELECT USERNAME FROM AARON_IPT.USERS WHERE USERID = :UserId";
                    
                    using (OracleCommand cmd = new OracleCommand(query, conn))
                    {
                        cmd.Parameters.Add(new OracleParameter("UserId", OracleDbType.Int32)).Value = userId;
                        
                        object result = cmd.ExecuteScalar();
                        if (result != null)
                        {
                            username = result.ToString();
                        }
                    }
                }
                
                if (!string.IsNullOrEmpty(username))
                {
                    // Find labels and panels
                    Label lblResetConfirmMessage = GetControlById("lblResetConfirmMessage") as Label;
                    Panel pnlPasswordReset = GetControlById("pnlPasswordReset") as Panel;
                    Panel pnlResetConfirm = GetControlById("pnlResetConfirm") as Panel;
                    Panel pnlResetComplete = GetControlById("pnlResetComplete") as Panel;
                    Button btnConfirmReset = GetControlById("btnConfirmReset") as Button;
                    
                    // Set the confirmation message
                    if (lblResetConfirmMessage != null)
                        lblResetConfirmMessage.Text = $"Are you sure you want to reset the password for user '{username}'?";
                    
                    // Store the user ID in the command argument for direct retrieval
                    if (btnConfirmReset != null)
                        btnConfirmReset.CommandArgument = userId.ToString();
                    
                    // Show reset panel
                    if (pnlPasswordReset != null) pnlPasswordReset.Visible = true;
                    if (pnlResetConfirm != null) pnlResetConfirm.Visible = true;
                    if (pnlResetComplete != null) pnlResetComplete.Visible = false;
                    
                    // Register script to ensure modal is displayed
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "showPasswordResetModal", "$('#passwordResetModal').modal('show');", true);
                }
                else
                {
                    ShowError("User not found.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error showing password reset: {ex.Message}");
                ShowError($"Error showing password reset: {ex.Message}");
            }
        }

        private void ShowToggleStatus(int userId)
        {
            try
            {
                Debug.WriteLine($"Showing toggle status for user {userId}");
                
                // Get user info for the confirmation message
                string username = "";
                bool isActive = false;
                
                string connectionString = GetConnectionString();
                using (OracleConnection conn = new OracleConnection(connectionString))
                {
                    conn.Open();
                    
                    string query = "SELECT USERNAME, ISACTIVE FROM \"AARON_IPT\".\"USERS\" WHERE USERID = :UserId";
                    
                    using (OracleCommand cmd = new OracleCommand(query, conn))
                    {
                        cmd.Parameters.Add(new OracleParameter("UserId", OracleDbType.Int32)).Value = userId;
                        
                        using (OracleDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                username = reader["USERNAME"].ToString();
                                isActive = Convert.ToBoolean(Convert.ToInt32(reader["ISACTIVE"]));
                            }
                        }
                    }
                }
                
                if (!string.IsNullOrEmpty(username))
                {
                    // Find the literal control
                    Literal litStatusMessage = GetControlById("litStatusMessage") as Literal;
                    Panel pnlToggleStatus = GetControlById("pnlToggleStatus") as Panel;
                    
                    // Set the confirmation message
                    string action = isActive ? "deactivate" : "activate";
                    if (litStatusMessage != null)
                        litStatusMessage.Text = $"Are you sure you want to {action} user '{username}'?";
                    
                    // Store values for use in confirmation
                    ViewState["IsCurrentlyActive"] = isActive;
                    selectedUserId = userId;
                    
                    // Show the confirmation panel
                            if (pnlToggleStatus != null)
                        pnlToggleStatus.Visible = true;
                        }
                        else
                        {
                    ShowError("User not found.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error showing toggle status: {ex.Message}");
                ShowError($"Error showing toggle status: {ex.Message}");
            }
        }

        private string GenerateRandomPassword(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*()";
            StringBuilder password = new StringBuilder();
            Random random = new Random();
            
            for (int i = 0; i < length; i++)
            {
                password.Append(chars[random.Next(chars.Length)]);
            }
            
            return password.ToString();
        }

        private string HashPassword(string password)
        {
            // In a real implementation, use a proper password hashing algorithm
            // For demonstration, we'll use SHA256
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(password);
                byte[] hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }

        private string GetConnectionString()
        {
            try
            {
                // Try multiple connection string names in order of preference
                string[] connectionStringNames = new string[] { 
                    "OracleConnection", 
                    "OraAspNetConString",
                    "ConnectionString", 
                    "DefaultConnection", 
                    "AARON_IPT"
                };
                
                Debug.WriteLine("Attempting to retrieve connection string from Web.config...");
                
                foreach (string name in connectionStringNames)
                {
                    Debug.WriteLine($"Trying connection string name: {name}");
                    var connectionString = ConfigurationManager.ConnectionStrings[name];
                    
                    if (connectionString != null)
                    {
                        Debug.WriteLine($"Found connection string with name: {name}");
                        string connStr = connectionString.ConnectionString;
                        Debug.WriteLine($"Connection string value (partial for security): {connStr.Substring(0, Math.Min(20, connStr.Length))}...");
                        return connStr;
                    }
                }
                
                // Fallback to the first available connection string if none of the expected names are found
                if (ConfigurationManager.ConnectionStrings.Count > 0)
                {
                    var firstConnectionString = ConfigurationManager.ConnectionStrings[0];
                    Debug.WriteLine($"Using first available connection string: {firstConnectionString.Name}");
                    return firstConnectionString.ConnectionString;
                }
                
                // Last resort fallback with detailed diagnostic output
                Debug.WriteLine("WARNING: No connection strings found in Web.config. Using hardcoded fallback connection string.");
                Debug.WriteLine("Please add the following to your Web.config inside the <configuration> section:");
                Debug.WriteLine(@"
<connectionStrings>
    <add name=""OracleConnection"" 
         connectionString=""Data Source=localhost:1521/XE;User Id=AARON_IPT;Password=qwen123;"" 
         providerName=""Oracle.ManagedDataAccess.Client""/>
</connectionStrings>");
                
                // Default to a likely server configuration based on schema script
                return "Data Source=localhost:1521/XE;User Id=AARON_IPT;Password=qwen123;";
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error retrieving connection string: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                // Last resort fallback - this should be configured in web.config
                Debug.WriteLine("Using hardcoded emergency fallback connection string due to error");
                return "Data Source=localhost:1521/XE;User Id=AARON_IPT;Password=qwen123;";
            }
        }

        private void ShowError(string message)
        {
            // Find panels and literals
            Panel pnlError = GetControlById("pnlError") as Panel;
            Literal litErrorMessage = GetControlById("litErrorMessage") as Literal;
            Panel pnlSuccess = GetControlById("pnlSuccess") as Panel;
            
            if (pnlError != null) pnlError.Visible = true;
            if (litErrorMessage != null) litErrorMessage.Text = message;
            if (pnlSuccess != null) pnlSuccess.Visible = false;
        }

        private void ShowSuccess(string message)
        {
            // Use the client-side success popup
            string script = $"showSuccessPopup('{message.Replace("'", "\\'")}');";
            ScriptManager.RegisterStartupScript(this, this.GetType(), "successPopup", script, true);
            
            // Hide the default success panel
            if (pnlSuccess != null) pnlSuccess.Visible = false;
        }

        protected string GetOrderStatusClass(string status)
        {
            if (string.IsNullOrEmpty(status))
                return "px-2 inline-flex text-xs leading-5 font-semibold rounded-full bg-gray-100 text-gray-800";
        
            switch (status.ToUpper())
            {
                case "COMPLETED":
                    return "px-2 inline-flex text-xs leading-5 font-semibold rounded-full bg-green-100 text-green-800";
                case "PROCESSING":
                    return "px-2 inline-flex text-xs leading-5 font-semibold rounded-full bg-blue-100 text-blue-800";
                case "PENDING":
                    return "px-2 inline-flex text-xs leading-5 font-semibold rounded-full bg-yellow-100 text-yellow-800";
                case "CANCELLED":
                    return "px-2 inline-flex text-xs leading-5 font-semibold rounded-full bg-red-100 text-red-800";
                default:
                    return "px-2 inline-flex text-xs leading-5 font-semibold rounded-full bg-gray-100 text-gray-800";
            }
        }

        /// <summary>
        /// Recursively finds a control in the current page or any of its containers
        /// </summary>
        private Control FindControlRecursively(Control root, string id)
        {
            if (root.ID == id)
                return root;

            foreach (Control c in root.Controls)
            {
                Control found = FindControlRecursively(c, id);
                if (found != null)
                    return found;
            }
            return null;
        }

        /// <summary>
        /// Safely finds a control by ID in the page, avoiding infinite recursion
        /// </summary>
        private Control GetControlById(string id)
        {
            // First try the standard FindControl method from the base class
            Control control = base.FindControl(id);
            if (control != null)
                return control;
            
            // If not found, try recursive search
            return FindControlRecursively(this, id);
        }

        private void ShowBasicInfoTab()
        {
            try
            {
                // Ensure proper tab display
                pnlBasicInfo.Visible = true;
                pnlOrderHistory.Visible = false;
                
                // Set the active tab styling via JavaScript
                string script = @"
                            document.addEventListener('DOMContentLoaded', function() {
                        showUserTab('basicInfo');
                    });";
                
                ScriptManager.RegisterStartupScript(this, this.GetType(), "showBasicInfoTab", script, true);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in ShowBasicInfoTab: {ex.Message}");
            }
        }

        private void GeneratePaginationButtons()
        {
            try
            {
                // Get current pagination state
                int currentPage = Convert.ToInt32(hdnCurrentPage.Value);
                int totalPages = Convert.ToInt32(hdnTotalPages.Value);
                
                Debug.WriteLine($"GeneratePaginationButtons: currentPage={currentPage}, totalPages={totalPages}");
                
                // Clear existing controls first
                phPageNumbers.Controls.Clear();
                
                if (totalPages <= 1)
                {
                    // No pagination needed for single page
                    return;
                }
                
                // Calculate page range (show max 5 pages centered around current)
                int startPage = Math.Max(1, currentPage - 2);
                int endPage = Math.Min(totalPages, startPage + 4);
                
                // Adjust start page if we're at the end of the range
                if (endPage == totalPages)
                {
                    startPage = Math.Max(1, endPage - 4);
                }
                
                Debug.WriteLine($"Generating page buttons from {startPage} to {endPage}");
                
                // Create a simple literal control with HTML links instead of server controls
                // This avoids ASP.NET event validation issues with dynamically created controls
                StringBuilder sb = new StringBuilder();
                
                for (int i = startPage; i <= endPage; i++)
                {
                    string cssClass = (i == currentPage) 
                        ? "px-3 py-1 rounded-md text-sm font-medium bg-[#D43B6A] text-white pointer-events-none border border-gray-300"
                        : "px-3 py-1 rounded-md text-sm font-medium bg-white text-gray-700 hover:bg-gray-50 border border-gray-300";
                    
                    // Use client-side JavaScript directly instead of server controls
                    sb.Append($"<a href=\"javascript:void(0);\" onclick=\"return changePage({i});\" class=\"{cssClass} mx-1\">{i}</a>");
                }
                
                // Create a literal control to hold our pagination HTML
                Literal litPageNumbers = new Literal();
                litPageNumbers.Text = sb.ToString();
                phPageNumbers.Controls.Add(litPageNumbers);
                
                // Register script to initialize pagination UI
                string script = "initializePagination();";
                ScriptManager.RegisterStartupScript(this, this.GetType(), "initPagination", script, true);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in GeneratePaginationButtons: {ex.Message}");
                // Don't show error to user for pagination issues
            }
        }

        #endregion
    }

    // User model class
    public class UserModel
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }
        public DateTime LastLogin { get; set; }
        public bool IsActive { get; set; }
    }
} 