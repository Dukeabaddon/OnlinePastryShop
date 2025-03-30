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
using System.Web.Configuration;
using Oracle.ManagedDataAccess.Types;
using System.Diagnostics;
// Removed: using System.Web.UI.WebControls.Expressions;

namespace OnlinePastryShop.Pages
{
    #region User Model
    public class UserModel
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Status { get; set; }
        public DateTime DateCreated { get; set; }
        public string UserType { get; set; }
        public bool IsActive { get; set; }

        public static UserModel FromReader(OracleDataReader reader)
        {
            bool isActive = Convert.ToBoolean(Convert.ToInt32(reader["ISACTIVE"]));
            return new UserModel
            {
                UserId = Convert.ToInt32(reader["USERID"]),
                Username = reader["USERNAME"].ToString(),
                Email = reader["EMAIL"].ToString(),
                Status = isActive ? "Active" : "Inactive",
                DateCreated = Convert.ToDateTime(reader["DATECREATED"]),
                UserType = reader["USERTYPE"].ToString(),
                IsActive = isActive
            };
        }
    }
    #endregion

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

        // Add missing control declarations
        protected Label lblToggleConfirmMessage;
        protected Label lblResetConfirmMessage;
        protected Label lblNewPassword;
        protected Panel pnlResetPasswordSuccess;
        // lvUsers is now properly declared in the designer file


        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"Page_Load called - IsPostBack: {IsPostBack}, ViewState count: {ViewState.Count}");

                if (!IsPostBack)
                {
                    // Initialize dropdown on first load
                    if (ddlStatus != null)
                    {
                        ddlStatus.SelectedValue = "true"; // Default to active users
                        System.Diagnostics.Debug.WriteLine("Set ddlStatus default value to 'true'");
                    }

                    // Set initial ViewState values
                    ViewState["StatusFilter"] = "true";
                    ViewState["CurrentPageIndex"] = 0;
                    ViewState["SearchText"] = string.Empty;

                    // Load initial data
                    LoadStats();
                    LoadUsers(false);
                }
                else
                {
                    // For postbacks, check if it's a pagination event
                    // If it is, handle custom pagination events
                    bool isPaginationEvent = false;

                    // Look at the current event target
                    string eventTarget = Request.Form["__EVENTTARGET"] ?? string.Empty;
                    if (!string.IsNullOrEmpty(eventTarget) &&
                        (eventTarget.Contains("Pager")))
                    {
                        isPaginationEvent = true;
                        System.Diagnostics.Debug.WriteLine($"Pagination event detected: {eventTarget}");
                    }

                    // If it's a pagination event, handle custom pagination events
                    if (isPaginationEvent)
                    {
                        System.Diagnostics.Debug.WriteLine("Our custom pagination will handle the pagination event");
                        return;
                    }

                    // Otherwise, restore values from ViewState as needed
                    // Note: LoadUsers will be called by the specific event handlers
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR in Page_Load: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                ShowError("Error loading page: " + ex.Message);
            }
        }

        // Check if the current request is a pagination event
        private bool IsPaginationEvent()
        {
            string eventTarget = Request["__EVENTTARGET"] ?? string.Empty;
            string eventArgument = Request["__EVENTARGUMENT"] ?? string.Empty;

            bool isPagingEvent =
                eventTarget.Contains("Pager") ||
                eventArgument.Contains("Page$");

            if (isPagingEvent)
            {
                System.Diagnostics.Debug.WriteLine($"Pagination event detected: Target='{eventTarget}', Argument='{eventArgument}'");
            }

            return isPagingEvent;
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
                        FROM Users
                        WHERE UPPER(USERNAME) <> 'ADMIN'";

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
                        WHERE DateCreated >= :startDate
                        AND UPPER(USERNAME) <> 'ADMIN'";

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
                // Reset pagination if this is a new search
                if (isSearch)
                {
                    ViewState["CurrentPageIndex"] = 0;
                    Debug.WriteLine("New search - resetting to page 1");
                }

                // Get search and status values from ViewState or controls
                string searchText = string.Empty;
                string statusValue = "true"; // Default to active users

                // Try to get search text from ViewState or control
                if (ViewState["SearchText"] != null)
                {
                    searchText = ViewState["SearchText"].ToString();
                }
                else if (txtSearch != null)
                {
                    searchText = txtSearch.Text.Trim();
                    ViewState["SearchText"] = searchText;
                }

                // Try to get status filter from ViewState or control
                if (ViewState["StatusFilter"] != null)
                {
                    statusValue = ViewState["StatusFilter"].ToString();
                }
                else if (ddlStatus != null)
                {
                    statusValue = ddlStatus.SelectedValue;
                    ViewState["StatusFilter"] = statusValue;
                }

                Debug.WriteLine($"LOADING USERS: SearchText='{searchText}', StatusValue='{statusValue}'");

                // Store to track if we found any users
                List<UserModel> users = new List<UserModel>();

                using (OracleConnection conn = new OracleConnection(GetConnectionString()))
                {
                    conn.Open();
                    Debug.WriteLine("Database connection opened successfully");

                    // Build the SQL query with parameters
                    string query = @"
                            SELECT 
                                USERID, 
                                USERNAME, 
                                EMAIL, 
                                ISACTIVE, 
                                TO_CHAR(DATECREATED, 'YYYY-MM-DD') AS DATECREATED, 
                                ROLE AS USERTYPE 
                            FROM USERS 
                        WHERE UPPER(USERNAME) <> 'ADMIN'";

                    using (OracleCommand cmd = new OracleCommand())
                    {
                        cmd.Connection = conn;

                        // Apply status filter if specified
                        if (!string.IsNullOrEmpty(statusValue))
                        {
                            // Ensure we're converting the string value correctly
                            // "true"/"false" or "1"/"0" to integer 1/0 for database query
                            int isActiveValue = 0;

                            if (statusValue.Equals("true", StringComparison.OrdinalIgnoreCase) ||
                                statusValue.Equals("1", StringComparison.OrdinalIgnoreCase))
                            {
                                isActiveValue = 1;
                            }

                            query += " AND ISACTIVE = :status";
                            cmd.Parameters.Add("status", OracleDbType.Int32).Value = isActiveValue;
                            Debug.WriteLine($"Added status filter: ISACTIVE = {isActiveValue}");
                        }

                        // Apply search filter if specified
                        if (!string.IsNullOrEmpty(searchText))
                        {
                            query += @" AND (
                                UPPER(USERNAME) LIKE UPPER(:search) OR 
                                UPPER(EMAIL) LIKE UPPER(:search)
                            )";
                            cmd.Parameters.Add("search", OracleDbType.Varchar2).Value = "%" + searchText + "%";
                            Debug.WriteLine($"Added search filter: '{searchText}'");
                        }

                        // Add order by clause
                        query += " ORDER BY USERNAME";

                        cmd.CommandText = query;
                        Debug.WriteLine($"Executing query: {query}");

                        // Execute the query and build the user list
                        using (OracleDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                UserModel user = UserModel.FromReader(reader);
                                users.Add(user);
                            }
                        }

                        Debug.WriteLine($"Retrieved {users.Count} users from database");
                    }
                }

                // Set up pagination for our user list
                if (users.Count > 0)
                {
                    // Get the current page index from ViewState (0-based)
                    int currentPageIndex = 0;
                    if (ViewState["CurrentPageIndex"] != null)
                    {
                        currentPageIndex = Convert.ToInt32(ViewState["CurrentPageIndex"]);
                    }

                    Debug.WriteLine($"Current page index: {currentPageIndex}");

                    // Define page size
                    int pageSize = 10;

                    // Calculate total number of pages
                    int totalItems = users.Count;
                    int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

                    Debug.WriteLine($"Total items: {totalItems}, Total pages: {totalPages}");

                    // Make sure we're not trying to display a page that doesn't exist
                    if (currentPageIndex >= totalPages)
                    {
                        currentPageIndex = totalPages - 1;
                        ViewState["CurrentPageIndex"] = currentPageIndex;
                        Debug.WriteLine($"Adjusted page index to {currentPageIndex}");
                    }

                    // Store pagination values in ViewState
                    ViewState["TotalItems"] = totalItems;
                    ViewState["TotalPages"] = totalPages;

                    // Apply pagination by taking only the items for the current page
                    int startIndex = currentPageIndex * pageSize;
                    int itemsToTake = Math.Min(pageSize, totalItems - startIndex);

                    // Handle case where startIndex might be out of range
                    if (startIndex < totalItems)
                    {
                        // Get the users for just this page
                        List<UserModel> pagedUsers = users.Skip(startIndex).Take(itemsToTake).ToList();

                        Debug.WriteLine($"Displaying page {currentPageIndex + 1} of {totalPages}, " +
                            $"showing {pagedUsers.Count} items starting at index {startIndex}");

                        // Bind the ListView to the paged data
                        lvUsers.DataSource = pagedUsers;
                        lvUsers.DataBind();

                        // Update pagination controls
                        UpdatePaginationControls(currentPageIndex + 1, totalPages); // Convert to 1-based page numbers for display

                        // Hide any "no data" message
                        ShowEmptyMessage(false);
                    }
                    else
                    {
                        // This should not happen with our bounds checking, but just in case
                        Debug.WriteLine($"WARNING: Start index {startIndex} is out of range for {totalItems} items");
                        lvUsers.DataSource = null;
                        lvUsers.DataBind();
                        ShowEmptyMessage(true, "No users found on this page.");
                    }
                }
                else
                {
                    // No users found
                    lvUsers.DataSource = null;
                    lvUsers.DataBind();

                    // If we have no users, show message
                    ShowEmptyMessage(true, "No users found matching your criteria.");

                    // Reset pagination info in ViewState
                    ViewState["TotalItems"] = 0;
                    ViewState["TotalPages"] = 0;

                    // Update pagination controls with zeros
                    UpdatePaginationControls(1, 1); // Show "Page 1 of 1" when empty
                }

                // Update stats after loading users
                LoadStats();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ERROR in LoadUsers: {ex.Message}");
                Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                ShowError("Error loading users: " + ex.Message);
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
            try
            {
                Debug.WriteLine("btnSearch_Click triggered");

                // Get search text and store in ViewState
                string searchText = string.Empty;

                // First try to get the search text box directly
                if (txtSearch != null)
                {
                    searchText = txtSearch.Text.Trim();
                    Debug.WriteLine($"Search text from control: '{searchText}'");
                }
                else
                {
                    Debug.WriteLine("WARNING: txtSearch control not found!");

                    // Try to find the search text box
                    TextBox searchBox = GetSearchTextBox();
                    if (searchBox != null)
                    {
                        searchText = searchBox.Text.Trim();
                        Debug.WriteLine($"Search text from FindControl: '{searchText}'");
                    }
                    else
                    {
                        Debug.WriteLine("Could not find search text box");
                    }
                }

                // Update ViewState regardless of whether we found a control
                ViewState["SearchText"] = searchText;
                Debug.WriteLine($"Stored search text in ViewState: '{searchText}'");

                // Get status filter and store in ViewState
                string statusValue = "true"; // Default to active users

                // First try to get the status dropdown directly
                if (ddlStatus != null)
                {
                    statusValue = ddlStatus.SelectedValue;
                    Debug.WriteLine($"Status filter from control: '{statusValue}'");
                }
                else
                {
                    Debug.WriteLine("WARNING: ddlStatus control not found!");

                    // Try to find the status dropdown
                    DropDownList statusDropDown = GetStatusDropDown();
                    if (statusDropDown != null)
                    {
                        statusValue = statusDropDown.SelectedValue;
                        Debug.WriteLine($"Status filter from FindControl: '{statusValue}'");
                    }
                    else
                    {
                        Debug.WriteLine("Could not find status dropdown");
                    }
                }

                // Update ViewState regardless of whether we found a control
                ViewState["StatusFilter"] = statusValue;
                Debug.WriteLine($"Stored status filter in ViewState: '{statusValue}'");

                // Load users with the search flag set to true to reset pagination
                LoadUsers(true);

                // Show confirmation of the search
                string filterDesc = statusValue.Equals("true", StringComparison.OrdinalIgnoreCase)
                    ? "Active" : "Inactive";

                if (!string.IsNullOrEmpty(searchText))
                {
                    ShowToast($"Showing {filterDesc} users matching '{searchText}'");
                }
                else
                {
                    ShowToast($"Showing all {filterDesc} users");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ERROR in btnSearch_Click: {ex.Message}");
                Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                ShowError("Error performing search: " + ex.Message);
            }
        }

        protected void btnReset_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("btnReset_Click triggered");

                // Clear search text
                if (txtSearch != null)
                {
                    txtSearch.Text = string.Empty;
                }
                else
                {
                    // Try to get the control by ID
                    TextBox searchBox = GetControl<TextBox>("txtSearch");
                    if (searchBox != null)
                    {
                        searchBox.Text = string.Empty;
                    }
                }

                // Reset status filter to "Active"
                if (ddlStatus != null)
                {
                    ddlStatus.SelectedValue = "true";
                }
                else
                {
                    // Try to get the control by ID
                    DropDownList statusDropDown = GetControl<DropDownList>("ddlStatus");
                    if (statusDropDown != null)
                    {
                        statusDropDown.SelectedValue = "true";
                    }
                }

                // Clear ViewState search values
                ViewState["SearchText"] = string.Empty;
                ViewState["StatusFilter"] = "true";
                ViewState["CurrentPageIndex"] = 0;

                // Reload users with reset filters
                LoadUsers(true);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR in btnReset_Click: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                ShowError("Error resetting search: " + ex.Message);
            }
        }

        protected void lvUsers_ItemCommand(object sender, ListViewCommandEventArgs e)
        {
            try
            {
                Debug.WriteLine($"lvUsers_ItemCommand called with command: {e.CommandName}");

                // Get the user ID from the CommandArgument
                string userId = e.CommandArgument.ToString();
                Debug.WriteLine($"Command for User ID: {userId}");

                if (string.IsNullOrEmpty(userId))
                {
                    ShowError("Invalid user ID.");
                    return;
                }

                // Store the user ID for subsequent operations - both in instance variable and ViewState for persistence
                selectedUserId = Convert.ToInt32(userId);
                ViewState["SelectedUserId"] = userId;

                // Handle different commands
                switch (e.CommandName)
                {
                    case "ViewDetails":
                        Debug.WriteLine($"Viewing details for user ID: {userId}");
                        ShowUserDetails(userId);
                        break;

                    case "ResetPassword":
                        Debug.WriteLine($"Reset password confirmation for user ID: {userId}");
                        // Before showing the confirmation, check if the user exists and is active
                        if (IsUserActive(selectedUserId))
                        {
                            ShowResetPasswordConfirmation(userId);
                        }
                        else
                        {
                            ShowError("Cannot reset password for inactive users.");
                        }
                        break;

                    case "ToggleStatus":
                        Debug.WriteLine($"Toggle status for user ID: {userId}");
                        // Get the current status
                        bool isActive = IsUserActive(selectedUserId);

                        // Show confirmation dialog
                        string username = GetUsernameById(userId);
                        string action = isActive ? "deactivate" : "activate";

                        // Set message for confirmation dialog
                        if (lblToggleConfirmMessage != null)
                        {
                            lblToggleConfirmMessage.Text = $"Are you sure you want to {action} user '{username}'?";
                        }

                        // Show the toggle confirmation panel
                        if (pnlToggleStatus != null)
                        {
                            pnlToggleStatus.Visible = true;
                            Debug.WriteLine($"Toggle status confirmation panel made visible for {username}");
                        }
                        else
                        {
                            // Direct toggle without confirmation as fallback
                            ToggleUserStatus(userId);
                        }
                        break;

                    default:
                        Debug.WriteLine($"Unhandled command: {e.CommandName}");
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ERROR in lvUsers_ItemCommand: {ex.Message}");
                Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                ShowError("Error processing command: " + ex.Message);
            }
        }

        protected void lvUsers_ItemDataBound(object sender, ListViewItemEventArgs e)
        {
            try
            {
                if (e.Item.ItemType == ListViewItemType.DataItem)
                {
                    // Get the user model for this row
                    UserModel user = (UserModel)((ListViewDataItem)e.Item).DataItem;
                    if (user != null)
                    {
                        // Find the status badge span
                        HtmlGenericControl statusBadge = (HtmlGenericControl)e.Item.FindControl("statusBadge");
                        if (statusBadge != null)
                        {
                            // Set the status badge class based on the user's active status
                            if (user.IsActive)
                            {
                                statusBadge.Attributes["class"] = "px-2 inline-flex text-xs leading-5 font-semibold rounded-full bg-green-100 text-green-800";
                            }
                            else
                            {
                                statusBadge.Attributes["class"] = "px-2 inline-flex text-xs leading-5 font-semibold rounded-full bg-red-100 text-red-800";
                            }
                        }

                        // Find the toggle status button
                        LinkButton btnToggleStatus = (LinkButton)e.Item.FindControl("btnToggleStatus");
                        if (btnToggleStatus != null)
                        {
                            // Set button color and tooltip based on current status
                            if (user.IsActive)
                            {
                                btnToggleStatus.ToolTip = "Deactivate User";
                                btnToggleStatus.CssClass = "text-red-600 hover:text-red-900";
                            }
                            else
                            {
                                btnToggleStatus.ToolTip = "Activate User";
                                btnToggleStatus.CssClass = "text-green-600 hover:text-green-900";
                            }

                            System.Diagnostics.Debug.WriteLine($"Setup toggle button for user {user.Username} (Active: {user.IsActive})");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in lvUsers_ItemDataBound: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            }
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
                // Get the user ID from ViewState - this persists across postbacks
                string userId = ViewState["SelectedUserId"] as string;

                System.Diagnostics.Debug.WriteLine($"btnConfirmReset_Click called for user ID: {userId}");

                if (string.IsNullOrEmpty(userId))
                {
                    ShowError("Invalid user ID for password reset.");
                    return;
                }

                // Reset the password for this user
                ResetPassword(userId);

                // Hide the confirmation panel
                if (pnlPasswordReset != null)
                {
                    pnlPasswordReset.Visible = false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR in btnConfirmReset_Click: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                ShowError("Error resetting password: " + ex.Message);
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
                // Get the user ID from ViewState - this persists across postbacks
                string userId = ViewState["SelectedUserId"] as string;

                System.Diagnostics.Debug.WriteLine($"btnConfirmToggle_Click called for user ID: {userId}");

                if (string.IsNullOrEmpty(userId))
                {
                    ShowError("Invalid user ID for status toggle.");
                    return;
                }

                // Toggle the user status
                ToggleUserStatus(userId);

                // Hide the confirmation panel
                if (pnlToggleStatus != null)
                {
                    pnlToggleStatus.Visible = false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR in btnConfirmToggle_Click: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                ShowError("Error toggling user status: " + ex.Message);
            }
        }

        protected void ActivateUser(string userId)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"Activating user ID: {userId}");

                using (OracleConnection conn = new OracleConnection(GetConnectionString()))
                {
                    conn.Open();

                    string query = "UPDATE USERS SET ISACTIVE = 1 WHERE USERID = :userId";

                    using (OracleCommand cmd = new OracleCommand(query, conn))
                    {
                        cmd.Parameters.Add("userId", OracleDbType.Varchar2).Value = userId;
                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            System.Diagnostics.Debug.WriteLine("User activated successfully");
                            ShowToast("User activated successfully.", true);

                            // Reload the user list to reflect the change
                            LoadUsers(false);
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("User activation failed - no rows affected");
                            ShowError("Failed to activate user.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR in ActivateUser: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                ShowError("Error activating user: " + ex.Message);
            }
        }

        protected void DeactivateUser(string userId)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"Deactivating user ID: {userId}");

                using (OracleConnection conn = new OracleConnection(GetConnectionString()))
                {
                    conn.Open();

                    string query = "UPDATE USERS SET ISACTIVE = 0 WHERE USERID = :userId";

                    using (OracleCommand cmd = new OracleCommand(query, conn))
                    {
                        cmd.Parameters.Add("userId", OracleDbType.Varchar2).Value = userId;
                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            System.Diagnostics.Debug.WriteLine("User deactivated successfully");
                            ShowToast("User deactivated successfully.", true);

                            // Reload the user list to reflect the change
                            LoadUsers(false);
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("User deactivation failed - no rows affected");
                            ShowError("Failed to deactivate user.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR in DeactivateUser: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                ShowError("Error deactivating user: " + ex.Message);
            }
        }

        // Helper method to get user name by ID
        private string GetUserName(int userId)
        {
            try
            {
                string userName = "Unknown User";
                using (OracleConnection conn = new OracleConnection(GetConnectionString()))
                {
                    conn.Open();
                    using (OracleCommand cmd = new OracleCommand("SELECT USERNAME FROM USERS WHERE USER_ID = :userId", conn))
                    {
                        cmd.Parameters.Add(":userId", OracleDbType.Int32).Value = userId;
                        object result = cmd.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            userName = result.ToString();
                        }
                    }
                }
                return userName;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetUserName: {ex.Message}");
                return "Unknown User";
            }
        }

        #endregion

        #region Helper Methods

        // Method to show empty message
        private void ShowEmptyMessage(bool show, string message = "No users found.")
        {
            // Find the empty message panel
            Panel pnlEmpty = GetControl<Panel>("pnlEmpty");
            if (pnlEmpty != null)
            {
                pnlEmpty.Visible = show;

                // Set the message text if provided
                Label lblEmpty = GetControl<Label>("lblEmpty");
                if (lblEmpty != null && show)
                {
                    lblEmpty.Text = message;
                }
            }
        }

        // Method to show error message
        private void ShowError(string message)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"Showing error message: {message}");

                // Use the toast system to show the error
                ShowToast(message, false);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR in ShowError: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");

                // Last resort - use JavaScript alert
                string jsMessage = message.Replace("'", "\\'");
                ScriptManager.RegisterStartupScript(this, GetType(), "AlertError" + Guid.NewGuid().ToString("N"),
                    $"alert('Error: {jsMessage}');", true);
            }
        }

        // Method to show message on the page
        protected void ShowMessage(string message, bool isSuccess)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"ShowMessage - Message: {message}, Success: {isSuccess}");

                // Directly use the client-side JavaScript notification
                string jsMessage = message.Replace("'", "\\'").Replace("\r", "").Replace("\n", " ");
                ScriptManager.RegisterStartupScript(this, GetType(), "DirectNotification" + Guid.NewGuid().ToString("N"),
                    $"showUserNotification('{jsMessage}', {isSuccess.ToString().ToLower()});", true);

                System.Diagnostics.Debug.WriteLine("Direct JavaScript notification registered from ShowMessage");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR in ShowMessage: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");

                // Ultimate fallback - use JavaScript alert
                string jsMessage = message.Replace("'", "\\'");
                ScriptManager.RegisterStartupScript(this, GetType(), "AlertMessage" + Guid.NewGuid().ToString("N"),
                    $"alert('{(isSuccess ? "Success" : "Error")}: {jsMessage}');", true);
            }
        }

        // Method to show toast notification
        private void ShowToast(string message, bool isSuccess = true)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"Showing toast message: {message}, Success: {isSuccess}");

                // First try using the panel if it exists
                if (pnlToast != null && lblToastMessage != null)
                {
                    lblToastMessage.Text = message;
                    pnlToast.CssClass = isSuccess
                            ? "fixed bottom-4 right-4 px-6 py-4 rounded-lg shadow-lg bg-green-500 z-50"
                            : "fixed bottom-4 right-4 px-6 py-4 rounded-lg shadow-lg bg-red-500 z-50";

                    // Make the panel visible
                    pnlToast.Style["display"] = "block";

                    // Add a script to hide the toast after a few seconds
                    string toastScript = @"
                        setTimeout(function() {
                            var toast = document.getElementById('" + pnlToast.ClientID + @"');
                            if (toast) {
                                toast.style.opacity = '0';
                                toast.style.transition = 'opacity 0.5s';
                                setTimeout(function() {{ toast.style.display = 'none'; toast.style.opacity = '1'; }}, 500);
                            }
                        }, 3000);";

                    ScriptManager.RegisterStartupScript(this, GetType(), "HideToast" + Guid.NewGuid().ToString("N"), toastScript, true);

                    System.Diagnostics.Debug.WriteLine("Toast displayed using panel");
                }
                else
                {
                    // Fall back to JavaScript toast
                    string color = isSuccess ? "bg-green-500" : "bg-red-500";
                    string toastScript = $@"
                        var toast = document.createElement('div');
                        toast.className = 'fixed top-4 right-4 px-4 py-3 rounded-lg shadow-lg z-50 {color} text-white max-w-md';
                        toast.innerHTML = '{message.Replace("'", "\\'")}';
                        document.body.appendChild(toast);
                        setTimeout(function() {{
                            toast.classList.add('opacity-0', 'transition-opacity', 'duration-500');
                            setTimeout(function() {{ toast.remove(); }}, 500);
                        }}, 3000);";

                    ScriptManager.RegisterStartupScript(this, GetType(), "ShowToast" + Guid.NewGuid().ToString("N"), toastScript, true);

                    System.Diagnostics.Debug.WriteLine("Toast displayed using JavaScript fallback");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR in ShowToast: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");

                // Last resort - use JavaScript alert
                string jsMessage = message.Replace("'", "\\'");
                ScriptManager.RegisterStartupScript(this, GetType(), "AlertMessage" + Guid.NewGuid().ToString("N"),
                    $"alert('{(isSuccess ? "Success" : "Error")}: {jsMessage}');", true);
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
            try
            {
                // First try to get the connection string from the configuration
                var connString = System.Configuration.ConfigurationManager.ConnectionStrings["OracleConnection"];
                if (connString != null)
                {
                    System.Diagnostics.Debug.WriteLine("Found OracleConnection string in Web.config");
                    return connString.ConnectionString;
                }

                // If not found, try alternate names
                connString = System.Configuration.ConfigurationManager.ConnectionStrings["ConnectionString"];
                if (connString != null)
                {
                    System.Diagnostics.Debug.WriteLine("Found ConnectionString in Web.config");
                    return connString.ConnectionString;
                }

                // If still not found, log error and use hardcoded connection string
                System.Diagnostics.Debug.WriteLine("ERROR: Connection string not found in Web.config. Using hardcoded connection.");
                return "User Id=Aaron_IPT;Password=qwen123;Data Source=localhost:1521/xe;";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting connection string: {ex.Message}");
                // Fallback to hardcoded connection string
                return "User Id=Aaron_IPT;Password=qwen123;Data Source=localhost:1521/xe;";
            }
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
            try
            {
                System.Diagnostics.Debug.WriteLine($"Setting active tab to: {tabName}");

                // Check if controls exist before trying to manipulate them
                LinkButton tabBasicInfo = FindControl("tabBasicInfo") as LinkButton;
                LinkButton tabActivities = FindControl("tabActivities") as LinkButton;
                LinkButton tabOrders = FindControl("tabOrders") as LinkButton;

                Panel contentBasicInfo = FindControl("contentBasicInfo") as Panel;
                Panel contentActivities = FindControl("contentActivities") as Panel;
                Panel contentOrders = FindControl("contentOrders") as Panel;

                // Reset all tabs to inactive state - using proper type casting for CssClass
                if (tabBasicInfo != null) tabBasicInfo.CssClass = tabBasicInfo.CssClass.Replace(" active", "");
                if (tabActivities != null) tabActivities.CssClass = tabActivities.CssClass.Replace(" active", "");
                if (tabOrders != null) tabOrders.CssClass = tabOrders.CssClass.Replace(" active", "");

                // Hide all content panels
                if (contentBasicInfo != null) contentBasicInfo.Visible = false;
                if (contentActivities != null) contentActivities.Visible = false;
                if (contentOrders != null) contentOrders.Visible = false;

                // Set the selected tab and content to active
                switch (tabName)
                {
                    case "BasicInfo":
                        if (tabBasicInfo != null) tabBasicInfo.CssClass += " active";
                        if (contentBasicInfo != null) contentBasicInfo.Visible = true;
                        break;
                    case "Activities":
                        if (tabActivities != null) tabActivities.CssClass += " active";
                        if (contentActivities != null) contentActivities.Visible = true;

                        // Load activities data here if needed
                        LoadUserActivities(selectedUserId);
                        break;
                    case "Orders":
                        if (tabOrders != null) tabOrders.CssClass += " active";
                        if (contentOrders != null) contentOrders.Visible = true;

                        // Load orders data here if needed
                        LoadUserOrders(selectedUserId);
                        break;
                    default:
                        // Default to Basic Info
                        if (tabBasicInfo != null) tabBasicInfo.CssClass += " active";
                        if (contentBasicInfo != null) contentBasicInfo.Visible = true;
                        break;
                }

                // As a fallback, use JavaScript to handle tab switching
                string scriptKey = "SetActiveTab_" + tabName;
                string script = $@"
                    if (document.querySelector('.user-tabs .active')) {{
                        document.querySelector('.user-tabs .active').classList.remove('active');
                    }}
                    if (document.querySelector('.tab-content .active')) {{
                        document.querySelector('.tab-content .active').classList.remove('active');
                    }}
                    if (document.getElementById('tab{tabName}')) {{
                        document.getElementById('tab{tabName}').classList.add('active');
                    }}
                    if (document.getElementById('content{tabName}')) {{
                        document.getElementById('content{tabName}').classList.add('active');
                    }}";

                ScriptManager.RegisterStartupScript(this, GetType(), scriptKey, script, true);
                System.Diagnostics.Debug.WriteLine($"Tab {tabName} set to active.");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR in SetActiveTab: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                // Non-critical error, don't show to user
            }
        }

        private void LoadUserActivities(int userId)
        {
            // This method would load user activities data
            // Placeholder for future implementation
            System.Diagnostics.Debug.WriteLine($"Loading activities for user ID: {userId}");
        }

        private void LoadUserOrders(int userId)
        {
            // This method would load user orders data
            // Placeholder for future implementation
            System.Diagnostics.Debug.WriteLine($"Loading orders for user ID: {userId}");
        }

        #endregion

        #region Control Finding Helpers

        // Find the GridView by traversing the page structure (including master page)
        private GridView FindUsersGridView()
        {
            System.Diagnostics.Debug.WriteLine("Finding GridView 'gvUsers' using multiple approaches...");
            GridView result = null;

            // First check our instance variable
            if (gvUsers != null)
            {
                System.Diagnostics.Debug.WriteLine("  Using existing gvUsers instance variable");
                return gvUsers;
            }

            // Then check the master page's content placeholder 
            if (Master != null)
            {
                // Get the content placeholder first
                ContentPlaceHolder contentPlaceHolder = Master.FindControl("AdminContent") as ContentPlaceHolder;
                if (contentPlaceHolder != null)
                {
                    System.Diagnostics.Debug.WriteLine("  Found AdminContent in Master page");

                    // Then find the GridView within the content placeholder
                    result = contentPlaceHolder.FindControl("gvUsers") as GridView;
                    if (result != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"  Found gvUsers in ContentPlaceHolder: {result.ClientID}");
                        gvUsers = result; // Save for later use
                        return result;
                    }
                }
            }

            // Check the entire control hierarchy
            List<Control> allControls = FindAllControlsRecursive(this.Page);
            var gridViews = allControls.OfType<GridView>().ToList();

            System.Diagnostics.Debug.WriteLine($"  Found {gridViews.Count} GridViews in page:");
            foreach (var grid in gridViews)
            {
                System.Diagnostics.Debug.WriteLine($"  - GridView: ID={grid.ID}, ClientID={grid.ClientID}");
                if (grid.ID == "gvUsers")
                {
                    result = grid;
                    System.Diagnostics.Debug.WriteLine("    ^ MATCH for gvUsers");
                }
            }

            if (result != null)
            {
                gvUsers = result; // Save for later use
                return result;
            }

            // Last resort - get ANY GridView and use it
            if (gridViews.Count > 0)
            {
                System.Diagnostics.Debug.WriteLine($"  Using first available GridView as fallback: {gridViews[0].ID}");
                gvUsers = gridViews[0]; // Save for later use
                return gridViews[0];
            }

            System.Diagnostics.Debug.WriteLine("  NO GridViews found in the entire page!");
            return null;
        }

        private void FindDataPagerRemoved()
        {

        }

        private void FindDataPagerRecursiveRemoved()
        {
 
        }

        #endregion

        // Bind users to the ListView
        private void BindUsers()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("BindUsers method called");
                using (OracleConnection conn = new OracleConnection(GetConnectionString()))
                {
                    conn.Open();

                    // Basic query to get all users
                    string query = @"SELECT 
                                    u.USER_ID as USERID, 
                                    u.USERNAME, 
                                    u.FIRST_NAME, 
                                    u.LAST_NAME, 
                                    u.EMAIL, 
                                    u.ROLE_ID, 
                                    r.ROLE_NAME,
                                    u.IS_ACTIVE as ISACTIVE,
                                    u.CREATED_DATE as DATECREATED
                                    FROM USERS u
                                    LEFT JOIN ROLES r ON u.ROLE_ID = r.ROLE_ID
                                    ORDER BY u.USERNAME";

                    using (OracleCommand cmd = new OracleCommand(query, conn))
                    {
                        using (OracleDataAdapter adapter = new OracleDataAdapter(cmd))
                        {
                            DataTable dtUsers = new DataTable();
                            adapter.Fill(dtUsers);

                            if (dtUsers.Rows.Count > 0)
                            {
                                System.Diagnostics.Debug.WriteLine($"Found {dtUsers.Rows.Count} users");
                                lvUsers.DataSource = dtUsers;
                                lvUsers.DataBind();
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine("No users found");
                                lvUsers.DataSource = null;
                                lvUsers.DataBind();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR in BindUsers: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                ShowToast($"<div class='flex items-center'><svg class='w-5 h-5 mr-2' fill='white' viewBox='0 0 20 20' xmlns='http://www.w3.org/2000/svg'><path fill-rule='evenodd' d='M4.293 4.293a1 1 0 011.414 0L10 8.586l4.293-4.293a1 1 0 111.414 1.414L11.414 10l4.293 4.293a1 1 0 01-1.414 1.414L10 11.414l-4.293 4.293a1 1 0 01-1.414-1.414L8.586 10 4.293 5.707a1 1 0 010-1.414z' clip-rule='evenodd'></path></svg><span>Error loading users: {ex.Message}</span></div>", false);
            }
        }

        private void FilterUsers(string searchText, string statusValue)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"FilterUsers called with search: '{searchText}', status: '{statusValue}'");

                using (OracleConnection conn = new OracleConnection(GetConnectionString()))
                {
                    conn.Open();

                    // Build query based on search and status
                    string query = @"SELECT 
                                    u.USER_ID as USERID, 
                                    u.USERNAME, 
                                    u.FIRST_NAME, 
                                    u.LAST_NAME, 
                                    u.EMAIL, 
                                    u.ROLE_ID, 
                                    r.ROLE_NAME,
                                    u.IS_ACTIVE as ISACTIVE,
                                    u.CREATED_DATE as DATECREATED
                                    FROM USERS u
                                    LEFT JOIN ROLES r ON u.ROLE_ID = r.ROLE_ID
                                    WHERE 1=1";

                    // Add parameters
                    OracleCommand cmd = new OracleCommand();

                    // Add status filter if selected
                    if (!string.IsNullOrEmpty(statusValue))
                    {
                        bool isActive = statusValue.ToLower() == "true";
                        query += " AND u.IS_ACTIVE = :isActive";
                        cmd.Parameters.Add(":isActive", OracleDbType.Int32).Value = isActive ? 1 : 0;
                    }

                    // Add search filter if provided
                    if (!string.IsNullOrEmpty(searchText))
                    {
                        query += @" AND (UPPER(u.USERNAME) LIKE UPPER('%' || :searchText || '%') 
                                   OR UPPER(u.EMAIL) LIKE UPPER('%' || :searchText || '%')
                                   OR UPPER(u.FIRST_NAME) LIKE UPPER('%' || :searchText || '%')
                                   OR UPPER(u.LAST_NAME) LIKE UPPER('%' || :searchText || '%'))";
                        cmd.Parameters.Add(":searchText", OracleDbType.Varchar2).Value = searchText;
                    }

                    // Add order by
                    query += " ORDER BY u.USERNAME";

                    cmd.CommandText = query;
                    cmd.Connection = conn;

                    System.Diagnostics.Debug.WriteLine($"Executing query: {query}");

                    using (OracleDataAdapter adapter = new OracleDataAdapter(cmd))
                    {
                        DataTable dtUsers = new DataTable();
                        adapter.Fill(dtUsers);

                        if (dtUsers.Rows.Count > 0)
                        {
                            System.Diagnostics.Debug.WriteLine($"Found {dtUsers.Rows.Count} filtered users");
                            lvUsers.DataSource = dtUsers;
                            lvUsers.DataBind();
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("No users found matching filters");
                            lvUsers.DataSource = null;
                            lvUsers.DataBind();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR in FilterUsers: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                throw; // Rethrow to be caught by calling method
            }
        }

        // Event handler for DataPager initialization
        protected void DataPager_Init_Removed()
        {
            // This method is no longer needed with our custom pagination
        }

        // Event handler for DataPager's page changing event
        protected void DataPager_PagePropertiesChanging_Removed()
        {
            // This method is no longer needed with our custom pagination
        }

        private void BindUserList()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("BindUserList called - rebinding ListView without reloading data");

                // Check if we need to reload users first
                if (lvUsers.DataSource == null)
                {
                    System.Diagnostics.Debug.WriteLine("ListView datasource is null, calling LoadUsers");
                    LoadUsers(false);
                    return;
                }

                // Get current page index from ViewState
                int currentPageIndex = 0;
                if (ViewState["CurrentPageIndex"] != null)
                {
                    currentPageIndex = Convert.ToInt32(ViewState["CurrentPageIndex"]);
                    System.Diagnostics.Debug.WriteLine($"Current page index from ViewState: {currentPageIndex}");
                }

                // No need to find DataPager anymore - we're using custom pagination

                // Just rebind the ListView
                lvUsers.DataBind();

                // Update pagination controls
                int totalPages = ViewState["TotalPages"] != null ? Convert.ToInt32(ViewState["TotalPages"]) : 1;
                UpdatePaginationControls(currentPageIndex + 1, totalPages);

                System.Diagnostics.Debug.WriteLine("BindUserList completed");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR in BindUserList: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                ShowError("Error binding user list: " + ex.Message);
            }
        }

        // Helper method to get a control of a specific type
        private T GetControl<T>(string controlId) where T : Control
        {
            // Try to find the control directly on the page
            T control = Page.FindControl(controlId) as T;
            if (control != null)
                return control;

            // Try to find in the content placeholder
            ContentPlaceHolder contentPlaceHolder = Page.Master.FindControl("AdminContent") as ContentPlaceHolder;
            if (contentPlaceHolder != null)
            {
                control = contentPlaceHolder.FindControl(controlId) as T;
                if (control != null)
                    return control;
            }

            // If not found, search recursively
            return FindControlRecursive(Page, controlId) as T;
        }

        private void ShowUserDetails(string userId)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"Showing details for user ID: {userId}");

                using (OracleConnection conn = new OracleConnection(GetConnectionString()))
                {
                    conn.Open();

                    string query = @"
                        SELECT 
                            USERID, 
                            USERNAME, 
                            EMAIL, 
                            ISACTIVE, 
                            TO_CHAR(DATECREATED, 'YYYY-MM-DD') AS DATECREATED, 
                            ROLE AS USERTYPE 
                        FROM USERS 
                        WHERE USERID = :userId";

                    using (OracleCommand cmd = new OracleCommand(query, conn))
                    {
                        cmd.Parameters.Add("userId", OracleDbType.Varchar2).Value = userId;

                        using (OracleDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // Store userId for use in other operations
                                selectedUserId = Convert.ToInt32(userId);

                                // Load user details into the modal
                                string username = reader["USERNAME"].ToString();
                                string email = reader["EMAIL"].ToString();
                                string userType = reader["USERTYPE"].ToString();
                                string dateCreated = reader["DATECREATED"].ToString();
                                bool isActive = Convert.ToBoolean(reader["ISACTIVE"]);

                                // Find and set values for the detail labels
                                if (lblUserId != null) lblUserId.Text = userId;
                                if (lblUsername != null) lblUsername.Text = username;
                                if (lblEmail != null) lblEmail.Text = email;
                                if (lblDateCreated != null) lblDateCreated.Text = dateCreated;
                                if (lblStatus != null) lblStatus.Text = isActive ? "Active" : "Inactive";

                                // If using a reset password functionality, store username
                                if (lblResetUsername != null) lblResetUsername.Text = username;

                                // Make the user details panel visible
                                if (pnlUserDetails != null)
                                {
                                    pnlUserDetails.Visible = true;

                                    // Make sure "Basic Info" tab is active by default
                                    SetActiveTab("BasicInfo");

                                    System.Diagnostics.Debug.WriteLine($"User details panel made visible for {username}");
                                }
                                else
                                {
                                    // As a fallback, use JavaScript to show a modal if HTML elements exist
                                    ScriptManager.RegisterStartupScript(this, GetType(), "ShowUserDetails",
                                            "if(document.getElementById('userDetailsModal')) document.getElementById('userDetailsModal').classList.remove('hidden');", true);

                                    System.Diagnostics.Debug.WriteLine("User details panel not found, trying JavaScript fallback");
                                }
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
                System.Diagnostics.Debug.WriteLine($"ERROR in ShowUserDetails: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                ShowError("Error showing user details: " + ex.Message);
            }
        }

        private void ShowResetPasswordConfirmation(string userId)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"Showing reset password confirmation for user ID: {userId}");

                // Store the user ID for the reset password confirmation
                selectedUserId = Convert.ToInt32(userId);

                // Get the username for display in the confirmation message
                string username = GetUsernameById(userId);

                // Set the confirmation message if we have a label for it
                if (lblResetConfirmMessage != null)
                {
                    lblResetConfirmMessage.Text = $"Are you sure you want to reset the password for user '{username}'?";
                }

                // Show the reset password confirmation panel if it exists
                if (pnlPasswordReset != null)
                {
                    pnlPasswordReset.Visible = true;
                    System.Diagnostics.Debug.WriteLine($"Password reset confirmation panel made visible for {username}");
                }
                else
                {
                    // As a fallback, use JavaScript to show a modal
                    ScriptManager.RegisterStartupScript(this, GetType(), "ShowResetConfirmation",
                        "if(document.getElementById('resetPasswordConfirmModal')) document.getElementById('resetPasswordConfirmModal').classList.remove('hidden');", true);

                    System.Diagnostics.Debug.WriteLine("Password reset panel not found, trying JavaScript fallback");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR in ShowResetPasswordConfirmation: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                ShowError("Error showing reset password confirmation: " + ex.Message);
            }
        }

        private string GetUsernameById(string userId)
        {
            try
            {
                using (OracleConnection conn = new OracleConnection(GetConnectionString()))
                {
                    conn.Open();

                    string query = "SELECT USERNAME FROM USERS WHERE USERID = :userId";

                    using (OracleCommand cmd = new OracleCommand(query, conn))
                    {
                        cmd.Parameters.Add("userId", OracleDbType.Varchar2).Value = userId;

                        object result = cmd.ExecuteScalar();
                        return result != null ? result.ToString() : "Unknown User";
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR in GetUsernameById: {ex.Message}");
                return "Unknown User";
            }
        }

        private bool IsEventTarget(string targetId)
        {
            try
            {
                // Check if the __EVENTTARGET form value contains the specified control ID
                string eventTarget = Request.Form["__EVENTTARGET"];

                if (string.IsNullOrEmpty(eventTarget))
                {
                    return false;
                }

                // Log the event target for debugging
                System.Diagnostics.Debug.WriteLine($"__EVENTTARGET: '{eventTarget}', checking for: '{targetId}'");

                // Check if the event target contains the specified ID
                if (eventTarget.Contains(targetId))
                {
                    System.Diagnostics.Debug.WriteLine($"Found target ID '{targetId}' in event target");
                    return true;
                }

                // Check if it's a pagination event
                string eventArgument = Request.Form["__EVENTARGUMENT"];
                if (!string.IsNullOrEmpty(eventArgument) && eventArgument.Contains("Page$"))
                {
                    System.Diagnostics.Debug.WriteLine($"Pagination event detected: {eventArgument}");
                    return targetId.Contains("pager");
                }

                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR in IsEventTarget: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Gets the control that triggered the current postback
        /// </summary>
        /// <returns>The control that triggered the postback, or null if not found or not a postback</returns>
        private Control GetPostBackControl()
        {
            try
            {
                if (!IsPostBack)
                    return null;

                // Get the __EVENTTARGET parameter
                string controlName = Request.Form["__EVENTTARGET"];

                // If __EVENTTARGET is empty, look for the control that could have triggered the postback
                if (string.IsNullOrEmpty(controlName))
                {
                    System.Diagnostics.Debug.WriteLine("__EVENTTARGET is empty, checking form elements");

                    // Check for all form elements that could have triggered a postback (like buttons)
                    foreach (string key in Request.Form.AllKeys)
                    {
                        if (key.EndsWith(".x") || key.EndsWith(".y")) // Image buttons
                        {
                            string potentialControlId = key.Substring(0, key.Length - 2);
                            System.Diagnostics.Debug.WriteLine($"Found potential postback control: {potentialControlId} (image button)");
                            Control control = FindControlRecursive(Page, potentialControlId);
                            if (control != null)
                                return control;
                        }

                        if (key != "__VIEWSTATE" && key != "__EVENTVALIDATION" &&
                            key != "__VIEWSTATEGENERATOR" && key != "__SCROLLPOSITIONX" &&
                            key != "__SCROLLPOSITIONY")
                        {
                            Control control = FindControlRecursive(Page, key);
                            if (control is Button || control is ImageButton ||
                                control is LinkButton)
                            {
                                System.Diagnostics.Debug.WriteLine($"Found potential postback control: {key}");
                                return control;
                            }
                        }
                    }

                    return null;
                }

                System.Diagnostics.Debug.WriteLine($"__EVENTTARGET found: {controlName}");
                Control targetControl = FindControlRecursive(Page, controlName);
                return targetControl;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR in GetPostBackControl: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                return null;
            }
        }

        private void ResetPassword(string userId)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"Resetting password for user ID: {userId}");

                string newPassword = GenerateRandomPassword(8);
                string hashedPassword = HashPassword(newPassword);

                using (OracleConnection conn = new OracleConnection(GetConnectionString()))
                {
                    conn.Open();

                    // Fixed column name from PASSWORD to PASSWORDHASH
                    string query = "UPDATE USERS SET PASSWORDHASH = :password WHERE USERID = :userId";

                    using (OracleCommand cmd = new OracleCommand(query, conn))
                    {
                        cmd.Parameters.Add("password", OracleDbType.Varchar2).Value = hashedPassword;
                        cmd.Parameters.Add("userId", OracleDbType.Varchar2).Value = userId;

                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            // Show success message with the new password
                            if (lblNewPassword != null) lblNewPassword.Text = newPassword;

                            // Show the reset password confirmation panel or modal
                            if (pnlResetPasswordSuccess != null)
                            {
                                pnlResetPasswordSuccess.Visible = true;
                                System.Diagnostics.Debug.WriteLine("Password reset success panel made visible");
                            }
                            else
                            {
                                // As a fallback, use JavaScript to show a modal
                                ScriptManager.RegisterStartupScript(this, GetType(), "ShowPasswordReset",
                                    "if(document.getElementById('resetPasswordModal')) document.getElementById('resetPasswordModal').classList.remove('hidden');", true);

                                System.Diagnostics.Debug.WriteLine("Using JavaScript fallback for password reset notification");
                            }

                            ShowToast("Password has been reset successfully.");
                        }
                        else
                        {
                            ShowError("Failed to reset password. User not found.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR in ResetPassword: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                ShowError("Error resetting password: " + ex.Message);
            }
        }

        private string GenerateRandomPassword(int length)
        {
            const string allowedChars = "abcdefghijkmnopqrstuvwxyzABCDEFGHJKLMNOPQRSTUVWXYZ0123456789!@$?_-";
            Random rand = new Random();
            char[] chars = new char[length];

            for (int i = 0; i < length; i++)
            {
                chars[i] = allowedChars[rand.Next(0, allowedChars.Length)];
            }

            return new string(chars);
        }

        private string HashPassword(string password)
        {
            // In a real application, use a proper hashing algorithm like bcrypt
            // This is a simple SHA256 implementation for demonstration
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                byte[] hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        private void ToggleUserStatus(string userId)
        {
            try
            {
                Debug.WriteLine($"Toggling status for user ID: {userId}");

                // First, get the current status
                bool currentStatus = false;
                string username = string.Empty;

                using (OracleConnection conn = new OracleConnection(GetConnectionString()))
                {
                    conn.Open();

                    // Get current status
                    string getStatusQuery = "SELECT USERNAME, ISACTIVE FROM USERS WHERE USERID = :userId";
                    using (OracleCommand cmd = new OracleCommand(getStatusQuery, conn))
                    {
                        cmd.Parameters.Add("userId", OracleDbType.Varchar2).Value = userId;

                        using (OracleDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                username = reader["USERNAME"].ToString();
                                currentStatus = Convert.ToBoolean(Convert.ToInt32(reader["ISACTIVE"]));
                                Debug.WriteLine($"Current status for user {username}: {(currentStatus ? "Active" : "Inactive")}");
                            }
                            else
                            {
                                // If user not found, show error and exit
                                ShowError("User not found.");
                                return;
                            }
                        }
                    }

                    // Update to opposite status
                    bool newStatus = !currentStatus;
                    string updateQuery = "UPDATE USERS SET ISACTIVE = :isActive WHERE USERID = :userId";

                    using (OracleCommand cmd = new OracleCommand(updateQuery, conn))
                    {
                        cmd.Parameters.Add("isActive", OracleDbType.Int32).Value = newStatus ? 1 : 0;
                        cmd.Parameters.Add("userId", OracleDbType.Varchar2).Value = userId;

                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            string action = newStatus ? "activated" : "deactivated";
                            string statusMessage = $"User '{username}' has been {action} successfully.";
                            Debug.WriteLine(statusMessage);

                            // Update ViewState to reflect potential status filter change
                            string statusFilter = ViewState["StatusFilter"]?.ToString() ?? "true";

                            // If we're filtering by a specific status, we may need to refresh
                            if ((newStatus && statusFilter == "false") || (!newStatus && statusFilter == "true"))
                            {
                                // User will disappear from the current view if filter is active
                                Debug.WriteLine("User will disappear from current filtered view");
                            }

                            ShowToast(statusMessage, true);

                            // Reload the user list to reflect the changes
                            LoadUsers(false);
                        }
                        else
                        {
                            Debug.WriteLine($"User status toggle failed - no rows affected");
                            ShowToast($"?? Failed to update status for user '{username}'. No changes were made.", false);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ERROR in ToggleUserStatus: {ex.Message}");
                Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                ShowError("Error updating user status: " + ex.Message);
            }
        }

        /// <summary>
        /// Checks if a user is active based on their ID
        /// </summary>
        /// <param name="userId">The ID of the user to check</param>
        /// <returns>True if the user is active, false otherwise</returns>
        private bool IsUserActive(int userId)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"Checking if user ID {userId} is active");

                using (OracleConnection conn = new OracleConnection(GetConnectionString()))
                {
                    conn.Open();

                    string query = "SELECT ISACTIVE FROM USERS WHERE USERID = :userId";

                    using (OracleCommand cmd = new OracleCommand(query, conn))
                    {
                        cmd.Parameters.Add("userId", OracleDbType.Int32).Value = userId;

                        object result = cmd.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            bool isActive = Convert.ToBoolean(Convert.ToInt32(result));
                            System.Diagnostics.Debug.WriteLine($"User ID {userId} is {(isActive ? "active" : "inactive")}");
                            return isActive;
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"User ID {userId} not found");
                            return false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR in IsUserActive: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                return false;
            }
        }

        /// <summary>
        /// Recursively searches for a control with the specified ID
        /// </summary>
        /// <param name="parent">The parent control to search within</param>
        /// <param name="controlId">The ID of the control to find</param>
        /// <returns>The control if found, null otherwise</returns>
        private Control FindControlRecursive(Control parent, string controlId)
        {
            try
            {
                if (parent == null || string.IsNullOrEmpty(controlId))
                    return null;

                // Check if the current control has the ID we're looking for
                if (parent.ID == controlId)
                    return parent;

                // Check if the current control has the ClientID we're looking for (for unique IDs)
                if (parent.ClientID == controlId)
                    return parent;

                // Recursively search through all child controls
                foreach (Control child in parent.Controls)
                {
                    Control foundControl = FindControlRecursive(child, controlId);
                    if (foundControl != null)
                        return foundControl;
                }

                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR in FindControlRecursive: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                return null;
            }
        }

        /// <summary>
        /// Recursively collects all controls within a parent control
        /// </summary>
        /// <param name="parent">The parent control to search within</param>
        /// <returns>A list of all controls found</returns>
        private List<Control> FindAllControlsRecursive(Control parent)
        {
            try
            {
                List<Control> controls = new List<Control>();

                if (parent == null)
                    return controls;

                // Add the current control to the list
                controls.Add(parent);

                // Recursively add all child controls
                foreach (Control child in parent.Controls)
                {
                    controls.AddRange(FindAllControlsRecursive(child));
                }

                return controls;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR in FindAllControlsRecursive: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                return new List<Control>();
            }
        }

        private void UpdateDataPagerTotalCount_Removed()
        {
            // This method is no longer needed with our custom pagination
        }

        private void UpdateDataPagersAfterBinding_Removed()
        {
            // This method is no longer needed with our custom pagination
        }

        private void FindDataPagingControlsRemoved()
        {
            // This method is no longer needed with our custom pagination
            // Method removed
        }

        #region Pagination Methods

        protected void btnFirst_Click(object sender, EventArgs e)
        {
            try
            {
                Debug.WriteLine("btnFirst_Click called");
                ViewState["CurrentPageIndex"] = 0;
                LoadUsers(false);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ERROR in btnFirst_Click: {ex.Message}");
                Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                ShowError("Error navigating to first page: " + ex.Message);
            }
        }

        // Previous page button click handler
        protected void btnPrev_Click(object sender, EventArgs e)
        {
            try
            {
                Debug.WriteLine("btnPrev_Click called");

                // Get current page index from ViewState (0-based)
                int currentPageIndex = 0;
                if (ViewState["CurrentPageIndex"] != null)
                {
                    currentPageIndex = Convert.ToInt32(ViewState["CurrentPageIndex"]);
                }

                // Check if we can go to previous page
                if (currentPageIndex > 0)
                {
                    // Decrement the page index
                    currentPageIndex--;
                    ViewState["CurrentPageIndex"] = currentPageIndex;
                    Debug.WriteLine($"Moving to previous page: {currentPageIndex + 1}");

                    // Reload users with the new page index
                    LoadUsers(false);
                }
                else
                {
                    Debug.WriteLine("Already on first page, cannot go to previous page");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ERROR in btnPrev_Click: {ex.Message}");
                Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                ShowError("Error navigating to previous page: " + ex.Message);
            }
        }

        // Next page button click handler
        protected void btnNext_Click(object sender, EventArgs e)
        {
            try
            {
                Debug.WriteLine("btnNext_Click called");

                // Get current page index from ViewState (0-based)
                int currentPageIndex = 0;
                if (ViewState["CurrentPageIndex"] != null)
                {
                    currentPageIndex = Convert.ToInt32(ViewState["CurrentPageIndex"]);
                }

                // Get total pages from ViewState
                int totalPages = 1;
                if (ViewState["TotalPages"] != null)
                {
                    totalPages = Convert.ToInt32(ViewState["TotalPages"]);
                }

                // Check if we can go to next page
                if (currentPageIndex < totalPages - 1)
                {
                    // Increment the page index
                    currentPageIndex++;
                    ViewState["CurrentPageIndex"] = currentPageIndex;
                    Debug.WriteLine($"Moving to next page: {currentPageIndex + 1}");

                    // Reload users with the new page index
                    LoadUsers(false);
                }
                else
                {
                    Debug.WriteLine("Already on last page, cannot go to next page");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ERROR in btnNext_Click: {ex.Message}");
                Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                ShowError("Error navigating to next page: " + ex.Message);
            }
        }

        // Handler for numeric page button clicks
        protected void btnPage_Click(object sender, EventArgs e)
        {
            try
            {
                LinkButton btn = sender as LinkButton;
                if (btn != null)
                {
                    // Get the page number from the button's CommandArgument
                    string pageArg = btn.CommandArgument;
                    if (!string.IsNullOrEmpty(pageArg) && int.TryParse(pageArg, out int pageNumber))
                    {
                        Debug.WriteLine($"btnPage_Click called for page {pageNumber}");

                        // Convert from 1-based (display) to 0-based (internal)
                        int pageIndex = pageNumber - 1;

                        // Store in ViewState
                        ViewState["CurrentPageIndex"] = pageIndex;

                        // Reload users with the new page index
                        LoadUsers(false);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ERROR in btnPage_Click: {ex.Message}");
                Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                ShowError("Error navigating to the selected page: " + ex.Message);
            }
        }

        /// <summary>
        /// Updates the pagination controls to reflect the current page and total pages
        /// </summary>
        /// <param name="currentPage">The current page number (1-based for display)</param>
        /// <param name="totalPages">The total number of pages</param>
        private void UpdatePaginationControls(int currentPage, int totalPages)
        {
            try
            {
                Debug.WriteLine($"Updating pagination controls: currentPage={currentPage}, totalPages={totalPages}");

                // Find desktop pagination labels
                Control desktopCurrentPage = FindControl("lblCurrentPage");
                Control desktopTotalPages = FindControl("lblTotalPages");

                // Update the desktop pagination labels
                if (desktopCurrentPage is Label)
                {
                    (desktopCurrentPage as Label).Text = currentPage.ToString();
                }

                if (desktopTotalPages is Label)
                {
                    (desktopTotalPages as Label).Text = totalPages.ToString();
                }

                // Update the mobile pagination labels
                Control mobileCurrentPage = FindControl("lblCurrentPageMobile");
                if (mobileCurrentPage is Label)
                {
                    (mobileCurrentPage as Label).Text = currentPage.ToString();
                }

                Control mobileTotalPages = FindControl("lblTotalPagesMobile");
                if (mobileTotalPages is Label)
                {
                    (mobileTotalPages as Label).Text = totalPages.ToString();
                }

                // Find desktop pagination buttons
                Control prevButton = FindControl("btnPrev");
                Control nextButton = FindControl("btnNext");

                // Update the desktop pagination buttons
                if (prevButton is LinkButton)
                {
                    LinkButton btnPrev = prevButton as LinkButton;
                    btnPrev.Enabled = currentPage > 1;
                    btnPrev.CssClass = currentPage > 1
                        ? "px-4 py-2 text-sm font-medium text-blue-900 bg-white border border-gray-200 rounded-l-lg hover:bg-gray-100 hover:text-blue-700 focus:z-10 focus:ring-2 focus:ring-blue-700 focus:text-blue-700"
                        : "px-4 py-2 text-sm font-medium text-gray-400 bg-white border border-gray-200 rounded-l-lg cursor-not-allowed";
                }

                if (nextButton is LinkButton)
                {
                    LinkButton btnNext = nextButton as LinkButton;
                    btnNext.Enabled = currentPage < totalPages;
                    btnNext.CssClass = currentPage < totalPages
                        ? "px-4 py-2 text-sm font-medium text-blue-900 bg-white border border-gray-200 rounded-r-lg hover:bg-gray-100 hover:text-blue-700 focus:z-10 focus:ring-2 focus:ring-blue-700 focus:text-blue-700"
                        : "px-4 py-2 text-sm font-medium text-gray-400 bg-white border border-gray-200 rounded-r-lg cursor-not-allowed";
                }

                // Update the mobile pagination buttons
                Control prevMobileButton = FindControl("btnPrevMobile");
                if (prevMobileButton is LinkButton)
                {
                    LinkButton btnPrevMobile = prevMobileButton as LinkButton;
                    btnPrevMobile.Enabled = currentPage > 1;
                    btnPrevMobile.CssClass = currentPage > 1
                        ? "px-4 py-2 text-sm font-medium text-blue-900 bg-white border border-gray-200 rounded-l-lg hover:bg-gray-100 hover:text-blue-700 focus:z-10 focus:ring-2 focus:ring-blue-700 focus:text-blue-700"
                        : "px-4 py-2 text-sm font-medium text-gray-400 bg-white border border-gray-200 rounded-l-lg cursor-not-allowed";
                }

                Control nextMobileButton = FindControl("btnNextMobile");
                if (nextMobileButton is LinkButton)
                {
                    LinkButton btnNextMobile = nextMobileButton as LinkButton;
                    btnNextMobile.Enabled = currentPage < totalPages;
                    btnNextMobile.CssClass = currentPage < totalPages
                        ? "px-4 py-2 text-sm font-medium text-blue-900 bg-white border border-gray-200 rounded-r-lg hover:bg-gray-100 hover:text-blue-700 focus:z-10 focus:ring-2 focus:ring-blue-700 focus:text-blue-700"
                        : "px-4 py-2 text-sm font-medium text-gray-400 bg-white border border-gray-200 rounded-r-lg cursor-not-allowed";
                }

                // Create numeric pagination buttons if we have a container panel
                Control numericPaginationPanel = FindControl("pnlNumericPagination");
                if (numericPaginationPanel is Panel)
                {
                    Panel pnlNumericPagination = numericPaginationPanel as Panel;
                    pnlNumericPagination.Controls.Clear();

                    // Only show numeric pagination if we have more than one page
                    if (totalPages > 1)
                    {
                        // Determine range of pages to display (at most 5 pages)
                        int startPage = Math.Max(1, currentPage - 2);
                        int endPage = Math.Min(totalPages, startPage + 4);

                        // Adjust start page if we're at the end of the range
                        if (endPage - startPage < 4 && startPage > 1)
                        {
                            startPage = Math.Max(1, endPage - 4);
                        }

                        // Add numeric buttons
                        for (int i = startPage; i <= endPage; i++)
                        {
                            LinkButton btnPage = new LinkButton();
                            btnPage.ID = "btnPage_" + i.ToString();
                            btnPage.Text = i.ToString();
                            btnPage.CommandArgument = i.ToString();
                            btnPage.Click += new EventHandler(btnPage_Click);

                            // Style the button based on whether it's the current page
                            if (i == currentPage)
                            {
                                btnPage.CssClass = "px-4 py-2 text-sm font-medium text-white bg-blue-700 border border-blue-700 hover:bg-blue-800 focus:z-10 focus:ring-2 focus:ring-blue-700 focus:text-white";
                                btnPage.Enabled = false;
                            }
                            else
                            {
                                btnPage.CssClass = "px-4 py-2 text-sm font-medium text-blue-900 bg-white border border-gray-200 hover:bg-gray-100 hover:text-blue-700 focus:z-10 focus:ring-2 focus:ring-blue-700 focus:text-blue-700";
                            }

                            // Add some margins between buttons
                            btnPage.Style["margin"] = "0 2px";

                            // Add the button to the panel
                            pnlNumericPagination.Controls.Add(btnPage);
                        }
                    }
                }

                // Ensure the pagination panel is visible
                Control paginationPanel = FindControl("pnlPagination");
                if (paginationPanel is Panel)
                {
                    (paginationPanel as Panel).Visible = true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ERROR in UpdatePaginationControls: {ex.Message}");
                Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                // This is a UI update, so we don't want to crash if it fails
            }
        }

        #endregion
    }
}
