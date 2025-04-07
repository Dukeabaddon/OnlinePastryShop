﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;

namespace OnlinePastryShop.Pages
{
    public partial class Orders : System.Web.UI.Page
    {
        // Connection string
        private string connectionString = ConfigurationManager.ConnectionStrings["OracleConnection"].ConnectionString;

        // Current sorting expression and direction
        private string SortExpression
        {
            get { return ViewState["SortExpression"] as string ?? "OrderID"; }
            set { ViewState["SortExpression"] = value; }
        }

        private string SortDirection
        {
            get { return ViewState["SortDirection"] as string ?? "DESC"; }
            set { ViewState["SortDirection"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                BindOrders();
            }
        }

        /// <summary>
        /// Binds orders to the GridView with optional filtering
        /// </summary>
        private void BindOrders()
        {
            try
            {
                using (OracleConnection conn = new OracleConnection(connectionString))
                {
                    using (OracleCommand cmd = new OracleCommand())
                    {
                        cmd.Connection = conn;

                        // Build the base SQL query with joins
                        StringBuilder query = new StringBuilder();
                        query.Append(@"SELECT o.OrderID, u.Username, o.OrderDate, o.TotalAmount, 
                                      o.Status, o.ShippingAddress, o.PaymentMethod 
                                      FROM AARON_IPT.Orders o 
                                      INNER JOIN AARON_IPT.Users u ON o.UserID = u.UserID 
                                      WHERE o.IsActive = 1");

                        // Apply status filter if selected
                        if (!string.IsNullOrEmpty(ddlStatus.SelectedValue))
                        {
                            query.Append(" AND o.Status = :status");
                            cmd.Parameters.Add(new OracleParameter("status", ddlStatus.SelectedValue));
                        }

                        // Apply date range filter if both dates are provided
                        if (!string.IsNullOrEmpty(txtStartDate.Text) && !string.IsNullOrEmpty(txtEndDate.Text))
                        {
                            query.Append(" AND o.OrderDate BETWEEN :startDate AND :endDate");

                            DateTime startDate = DateTime.Parse(txtStartDate.Text);
                            DateTime endDate = DateTime.Parse(txtEndDate.Text).AddDays(1).AddSeconds(-1); // Set to end of day

                            cmd.Parameters.Add(new OracleParameter("startDate", OracleDbType.Date, startDate, ParameterDirection.Input));
                            cmd.Parameters.Add(new OracleParameter("endDate", OracleDbType.Date, endDate, ParameterDirection.Input));
                        }

                        // Apply sorting
                        query.Append($" ORDER BY {SortExpression} {SortDirection}");

                        cmd.CommandText = query.ToString();

                        // Create data adapter and fill dataset
                        OracleDataAdapter adapter = new OracleDataAdapter(cmd);
                        DataTable dt = new DataTable();

                        conn.Open();
                        adapter.Fill(dt);

                        // Store the total number of records for pagination
                        int totalRecords = dt.Rows.Count;
                        int totalPages = (int)Math.Ceiling((double)totalRecords / gvOrders.PageSize);

                        // Set up page number buttons for DataList
                        if (dt.Rows.Count > 0)
                        {
                            DataTable dtPaging = new DataTable();
                            dtPaging.Columns.Add("Text", typeof(string));
                            dtPaging.Columns.Add("Value", typeof(string));
                            dtPaging.Columns.Add("Selected", typeof(bool));

                            // Get the current page index
                            int currentPage = gvOrders.PageIndex + 1;

                            // Determine range of page numbers to show
                            int startPage = Math.Max(1, currentPage - 2);
                            int endPage = Math.Min(totalPages, startPage + 4);

                            // Adjust if we're near the end
                            if (endPage - startPage < 4 && totalPages > 5)
                            {
                                startPage = Math.Max(1, endPage - 4);
                            }

                            // Add page number buttons
                            for (int i = startPage; i <= endPage; i++)
                            {
                                DataRow dr = dtPaging.NewRow();
                                dr["Text"] = i.ToString();
                                dr["Value"] = i.ToString();
                                dr["Selected"] = (i == currentPage);
                                dtPaging.Rows.Add(dr);
                            }

                            // Find the DataList in the GridView's PagerTemplate
                            GridViewRow pagerRow = gvOrders.BottomPagerRow;
                            if (pagerRow != null)
                            {
                                DataList dlPaging = (DataList)pagerRow.FindControl("dlPaging");
                                if (dlPaging != null)
                                {
                                    // Bind data to the DataList for server-side functionality
                                    dlPaging.DataSource = dtPaging;
                                    dlPaging.DataBind();
                                    
                                    // Ensure the DataList is visible
                                    dlPaging.Visible = true;
                                }

                                // Set the page information for client-side JavaScript
                                Label currentPageLabel = (Label)pagerRow.FindControl("currentPageDisplay");
                                if (currentPageLabel != null)
                                {
                                    currentPageLabel.Text = currentPage.ToString();
                                }
                                
                                Label totalPageLabel = (Label)pagerRow.FindControl("totalPagesDisplay");
                                if (totalPageLabel != null)
                                {
                                    totalPageLabel.Text = totalPages.ToString();
                                }
                            }
                        }

                        // Bind data to GridView
                        gvOrders.DataSource = dt;
                        gvOrders.DataBind();

                        // Show/hide no orders message
                        pnlNoOrders.Visible = dt.Rows.Count == 0;
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the error
                LogError(ex);

                // Show error message
                ScriptManager.RegisterStartupScript(this, GetType(), "error",
                    "alert('Error loading orders: " + ex.Message.Replace("'", "\\'") + "');", true);
            }
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the status dropdown
        /// </summary>
        protected void ddlStatus_SelectedIndexChanged(object sender, EventArgs e)
        {
            BindOrders();
        }

        /// <summary>
        /// Handles the Click event of the filter button
        /// </summary>
        protected void btnFilter_Click(object sender, EventArgs e)
        {
            BindOrders();
        }

        /// <summary>
        /// Handles the PageIndexChanging event of the orders GridView
        /// </summary>
        protected void gvOrders_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvOrders.PageIndex = e.NewPageIndex;
            BindOrders();
        }

        /// <summary>
        /// Handles the Sorting event of the orders GridView
        /// </summary>
        protected void gvOrders_Sorting(object sender, GridViewSortEventArgs e)
        {
            // Toggle sort direction if same column is clicked
            if (e.SortExpression == SortExpression)
            {
                SortDirection = SortDirection == "ASC" ? "DESC" : "ASC";
            }
            else
            {
                SortExpression = e.SortExpression;
                SortDirection = "ASC";
            }

            BindOrders();
        }

        /// <summary>
        /// Handles the RowCommand event of the orders GridView
        /// </summary>
        protected void gvOrders_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "ViewDetails")
            {
                // Get the OrderID
                int orderId = Convert.ToInt32(e.CommandArgument);

                // Populate order details
                PopulateOrderDetails(orderId);

                // Show the modal
                ScriptManager.RegisterStartupScript(this, GetType(), "showModal", "showModal();", true);
            }
            else if (e.CommandName == "DeleteOrder")
            {
                // Get the OrderID
                int orderId = Convert.ToInt32(e.CommandArgument);

                // Delete the order
                DeleteOrder(orderId);

                // Refresh the GridView
                BindOrders();
            }
        }

        /// <summary>
        /// Handles the RowDataBound event of the orders GridView
        /// </summary>
        protected void gvOrders_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                // Get data item
                DataRowView rowView = (DataRowView)e.Row.DataItem;
                string status = rowView["Status"].ToString();

                // Apply row styling based on order status
                if (status == "Approved" || status == "Delivered")
                {
                    e.Row.CssClass += " bg-green-100";
                }
                else if (status == "Cancelled")
                {
                    e.Row.CssClass += " bg-red-100";
                }

                // Add tooltip for payment method abbreviations
                TableCell paymentCell = e.Row.Cells[7]; // Payment Method column
                string paymentMethod = rowView["PaymentMethod"].ToString();

                if (paymentMethod == "COD")
                {
                    paymentCell.ToolTip = "Cash on Delivery";
                }
                else if (paymentMethod == "CC")
                {
                    paymentCell.ToolTip = "Credit Card";
                }
            }
        }

        /// <summary>
        /// Populates the order details modal
        /// </summary>
        private void PopulateOrderDetails(int orderId)
        {
            try
            {
                using (OracleConnection conn = new OracleConnection(connectionString))
                {
                    conn.Open();

                    // Get order details
                    using (OracleCommand cmd = new OracleCommand())
                    {
                        cmd.Connection = conn;
                        cmd.CommandText = @"SELECT o.OrderID, u.Username, o.OrderDate, o.TotalAmount, 
                                          o.Status, o.ShippingAddress, o.PaymentMethod 
                                          FROM AARON_IPT.Orders o 
                                          INNER JOIN AARON_IPT.Users u ON o.UserID = u.UserID 
                                          WHERE o.OrderID = :orderId";
                        cmd.Parameters.Add(new OracleParameter("orderId", orderId));

                        using (OracleDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // Populate order summary fields
                                spanOrderId.InnerText = reader["OrderID"].ToString();
                                spanCustomer.InnerText = reader["Username"].ToString();
                                spanOrderDate.InnerText = Convert.ToDateTime(reader["OrderDate"]).ToString("MM/dd/yyyy");
                                spanTotal.InnerText = string.Format("{0:C}", reader["TotalAmount"]);
                                spanPayment.InnerText = reader["PaymentMethod"].ToString();
                                spanAddress.InnerText = reader["ShippingAddress"].ToString();

                                // Set current status in dropdown
                                ddlOrderStatus.SelectedValue = reader["Status"].ToString();

                                // Store OrderID in hidden field
                                hfOrderId.Value = orderId.ToString();
                            }
                        }
                    }

                    // Get order items
                    using (OracleCommand cmd = new OracleCommand())
                    {
                        cmd.Connection = conn;
                        cmd.CommandText = @"SELECT od.ProductID, p.Name, od.Quantity, od.Price 
                                          FROM AARON_IPT.OrderDetails od 
                                          INNER JOIN AARON_IPT.Products p ON od.ProductID = p.ProductID 
                                          WHERE od.OrderID = :orderId AND od.IsActive = 1";
                        cmd.Parameters.Add(new OracleParameter("orderId", orderId));

                        OracleDataAdapter adapter = new OracleDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);

                        // Bind order items to GridView
                        gvOrderItems.DataSource = dt;
                        gvOrderItems.DataBind();
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the error
                LogError(ex);

                // Show error message
                ScriptManager.RegisterStartupScript(this, GetType(), "error",
                    "alert('Error loading order details: " + ex.Message.Replace("'", "\\'") + "');", true);
            }
        }

        /// <summary>
        /// Updates the order status
        /// </summary>
        protected void btnUpdateStatus_Click(object sender, EventArgs e)
        {
            try
            {
                int orderId = Convert.ToInt32(hfOrderId.Value);
                string newStatus = ddlOrderStatus.SelectedValue;
                string notes = $"Status updated to {newStatus} via Orders management page";

                // Get current user ID
                int userId = 1; // Default to admin user ID if not available
                if (Session["UserID"] != null)
                {
                    userId = Convert.ToInt32(Session["UserID"]);
                }

                using (OracleConnection conn = new OracleConnection(connectionString))
                {
                    using (OracleCommand cmd = new OracleCommand())
                    {
                        cmd.Connection = conn;
                        cmd.CommandText = "AARON_IPT.PRC_UPDATE_ORDER_STATUS";
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        
                        // Add parameters
                        cmd.Parameters.Add("p_order_id", OracleDbType.Int32).Value = orderId;
                        cmd.Parameters.Add("p_new_status", OracleDbType.Varchar2).Value = newStatus;
                        cmd.Parameters.Add("p_user_id", OracleDbType.Int32).Value = userId;
                        cmd.Parameters.Add("p_notes", OracleDbType.Varchar2).Value = notes;

                        conn.Open();
                        cmd.ExecuteNonQuery();
                        
                        // Still log the change client-side for redundancy
                        LogOrderStatusChange(orderId, newStatus);

                        // Refresh the GridView
                        BindOrders();

                        // Show success message
                        ScriptManager.RegisterStartupScript(this, GetType(), "success",
                            "alert('Order status updated successfully.');", true);
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the error
                LogError(ex);

                // Show error message
                ScriptManager.RegisterStartupScript(this, GetType(), "error",
                    "alert('Error updating order status: " + ex.Message.Replace("'", "\\'") + "');", true);
            }
        }

        /// <summary>
        /// Deletes an order (soft delete)
        /// </summary>
        private void DeleteOrder(int orderId)
        {
            try
            {
                using (OracleConnection conn = new OracleConnection(connectionString))
                {
                    using (OracleCommand cmd = new OracleCommand())
                    {
                        cmd.Connection = conn;
                        cmd.CommandText = "UPDATE AARON_IPT.Orders SET IsActive = 0 WHERE OrderID = :orderId";
                        cmd.Parameters.Add(new OracleParameter("orderId", orderId));

                        conn.Open();
                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            // Log the deletion in audit log
                            LogOrderDeletion(orderId);

                            // Show success message
                            ScriptManager.RegisterStartupScript(this, GetType(), "success",
                                "alert('Order deleted successfully.');", true);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the error
                LogError(ex);

                // Show error message
                ScriptManager.RegisterStartupScript(this, GetType(), "error",
                    "alert('Error deleting order: " + ex.Message.Replace("'", "\\'") + "');", true);
            }
        }

        /// <summary>
        /// Handles bulk actions on selected orders
        /// </summary>
        protected void btnApplyBulk_Click(object sender, EventArgs e)
        {
            try
            {
                string action = ddlBulkAction.SelectedValue;

                if (string.IsNullOrEmpty(action))
                {
                    // No action selected
                    ScriptManager.RegisterStartupScript(this, GetType(), "warning",
                        "alert('Please select an action to apply.');", true);
                    return;
                }

                // Get selected order IDs
                List<int> selectedOrderIds = new List<int>();

                foreach (GridViewRow row in gvOrders.Rows)
                {
                    CheckBox chkSelect = (CheckBox)row.FindControl("chkSelect");

                    if (chkSelect.Checked)
                    {
                        int orderId = Convert.ToInt32(gvOrders.DataKeys[row.RowIndex].Value);
                        selectedOrderIds.Add(orderId);
                    }
                }

                if (selectedOrderIds.Count == 0)
                {
                    // No orders selected
                    ScriptManager.RegisterStartupScript(this, GetType(), "warning",
                        "alert('Please select at least one order.');", true);
                    return;
                }

                // Get current user ID
                int userId = 1; // Default to admin user ID if not available
                if (Session["UserID"] != null)
                {
                    userId = Convert.ToInt32(Session["UserID"]);
                }

                using (OracleConnection conn = new OracleConnection(connectionString))
                {
                    conn.Open();

                    foreach (int orderId in selectedOrderIds)
                    {
                        using (OracleCommand cmd = new OracleCommand())
                        {
                            cmd.Connection = conn;

                            if (action == "Delete")
                            {
                                // Soft delete
                                cmd.CommandText = "UPDATE AARON_IPT.Orders SET IsActive = 0 WHERE OrderID = :orderId";
                                cmd.Parameters.Add(new OracleParameter("orderId", orderId));

                                cmd.ExecuteNonQuery();

                                // Log the deletion
                                LogOrderDeletion(orderId);
                            }
                            else
                            {
                                // Update status using stored procedure
                                cmd.CommandText = "AARON_IPT.PRC_UPDATE_ORDER_STATUS";
                                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                                
                                // Add parameters
                                cmd.Parameters.Add("p_order_id", OracleDbType.Int32).Value = orderId;
                                cmd.Parameters.Add("p_new_status", OracleDbType.Varchar2).Value = action;
                                cmd.Parameters.Add("p_user_id", OracleDbType.Int32).Value = userId;
                                cmd.Parameters.Add("p_notes", OracleDbType.Varchar2).Value = $"Bulk status update to {action}";

                                cmd.ExecuteNonQuery();

                                // Log the status change client-side as well
                                LogOrderStatusChange(orderId, action);
                            }

                            cmd.Parameters.Clear();
                        }
                    }
                }

                // Refresh the GridView
                BindOrders();

                // Show success message
                ScriptManager.RegisterStartupScript(this, GetType(), "success",
                    $"alert('Successfully applied {action} to {selectedOrderIds.Count} order(s).');", true);
            }
            catch (Exception ex)
            {
                // Log the error
                LogError(ex);

                // Show error message
                ScriptManager.RegisterStartupScript(this, GetType(), "error",
                    "alert('Error applying bulk action: " + ex.Message.Replace("'", "\\'") + "');", true);
            }
        }

        /// <summary>
        /// Exports the current orders to CSV
        /// </summary>
        protected void btnExportCsv_Click(object sender, EventArgs e)
        {
            try
            {
                // Build CSV content
                StringBuilder csv = new StringBuilder();

                // Add headers
                csv.AppendLine("OrderID,Customer,Date,TotalAmount,Status,ShippingAddress,PaymentMethod");

                using (OracleConnection conn = new OracleConnection(connectionString))
                {
                    using (OracleCommand cmd = new OracleCommand())
                    {
                        cmd.Connection = conn;

                        // Build the base SQL query with joins
                        StringBuilder query = new StringBuilder();
                        query.Append(@"SELECT o.OrderID, u.Username, o.OrderDate, o.TotalAmount, 
                                      o.Status, o.ShippingAddress, o.PaymentMethod 
                                      FROM AARON_IPT.Orders o 
                                      INNER JOIN AARON_IPT.Users u ON o.UserID = u.UserID 
                                      WHERE o.IsActive = 1");

                        // Apply status filter if selected
                        if (!string.IsNullOrEmpty(ddlStatus.SelectedValue))
                        {
                            query.Append(" AND o.Status = :status");
                            cmd.Parameters.Add(new OracleParameter("status", ddlStatus.SelectedValue));
                        }

                        // Apply date range filter if both dates are provided
                        if (!string.IsNullOrEmpty(txtStartDate.Text) && !string.IsNullOrEmpty(txtEndDate.Text))
                        {
                            query.Append(" AND o.OrderDate BETWEEN :startDate AND :endDate");

                            DateTime startDate = DateTime.Parse(txtStartDate.Text);
                            DateTime endDate = DateTime.Parse(txtEndDate.Text).AddDays(1).AddSeconds(-1); // Set to end of day

                            cmd.Parameters.Add(new OracleParameter("startDate", OracleDbType.Date, startDate, ParameterDirection.Input));
                            cmd.Parameters.Add(new OracleParameter("endDate", OracleDbType.Date, endDate, ParameterDirection.Input));
                        }

                        // Apply sorting
                        query.Append($" ORDER BY {SortExpression} {SortDirection}");

                        cmd.CommandText = query.ToString();

                        conn.Open();

                        using (OracleDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                // Add each row to CSV
                                csv.AppendLine(string.Format("{0},{1},{2},{3},{4},{5},{6}",
                                    reader["OrderID"],
                                    EscapeCsvField(reader["Username"].ToString()),
                                    Convert.ToDateTime(reader["OrderDate"]).ToString("MM/dd/yyyy"),
                                    Convert.ToDecimal(reader["TotalAmount"]).ToString("0.00"),
                                    EscapeCsvField(reader["Status"].ToString()),
                                    EscapeCsvField(reader["ShippingAddress"].ToString()),
                                    EscapeCsvField(reader["PaymentMethod"].ToString())
                                ));
                            }
                        }
                    }
                }

                // Send CSV file to the client
                Response.Clear();
                Response.Buffer = true;
                Response.AddHeader("content-disposition", "attachment;filename=Orders_" + DateTime.Now.ToString("yyyyMMdd") + ".csv");
                Response.Charset = "";
                Response.ContentType = "application/text";
                Response.Output.Write(csv.ToString());
                Response.Flush();
                Response.End();
            }
            catch (Exception ex)
            {
                // Log the error
                LogError(ex);

                // Show error message
                ScriptManager.RegisterStartupScript(this, GetType(), "error",
                    "alert('Error exporting to CSV: " + ex.Message.Replace("'", "\\'") + "');", true);
            }
        }

        /// <summary>
        /// Escapes special characters in CSV fields
        /// </summary>
        private string EscapeCsvField(string field)
        {
            if (string.IsNullOrEmpty(field))
            {
                return string.Empty;
            }

            // Check if the field contains commas, quotes, or newlines
            bool containsSpecialChars = field.Contains(",") || field.Contains("\"") || field.Contains("\n") || field.Contains("\r");

            if (containsSpecialChars)
            {
                // Replace double quotes with two double quotes
                field = field.Replace("\"", "\"\"");

                // Wrap in quotes
                return "\"" + field + "\"";
            }

            return field;
        }

        /// <summary>
        /// Returns the CSS class for an order status
        /// </summary>
        public string GetStatusCssClass(string status)
        {
            switch (status)
            {
                case "Pending":
                    return "px-2 py-1 rounded-full text-xs bg-yellow-100 text-yellow-800";
                case "Processing":
                    return "px-2 py-1 rounded-full text-xs bg-blue-100 text-blue-800";
                case "Shipped":
                    return "px-2 py-1 rounded-full text-xs bg-indigo-100 text-indigo-800";
                case "Delivered":
                    return "px-2 py-1 rounded-full text-xs bg-green-100 text-green-800";
                case "Cancelled":
                    return "px-2 py-1 rounded-full text-xs bg-red-100 text-red-800";
                default:
                    return "px-2 py-1 rounded-full text-xs bg-gray-100 text-gray-800";
            }
        }

        /// <summary>
        /// Truncates a long shipping address for display
        /// </summary>
        public string TruncateAddress(string address)
        {
            if (string.IsNullOrEmpty(address))
            {
                return string.Empty;
            }

            if (address.Length > 30)
            {
                return address.Substring(0, 27) + "...";
            }

            return address;
        }

        /// <summary>
        /// Logs an order status change to the audit log
        /// </summary>
        private void LogOrderStatusChange(int orderId, string newStatus)
        {
            try
            {
                // Get the current user
                string username = User.Identity.Name;

                // Log to a text file (in a real application, this would go to a database table)
                string logEntry = string.Format("{0} - Order {1} status changed to {2} by {3}",
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    orderId,
                    newStatus,
                    username);

                string logPath = Server.MapPath("~/App_Data/AuditLog.txt");
                File.AppendAllText(logPath, logEntry + Environment.NewLine);
            }
            catch (Exception ex)
            {
                // Just log to debug - don't disrupt the user experience for a logging error
                System.Diagnostics.Debug.WriteLine("Error logging status change: " + ex.Message);
            }
        }

        /// <summary>
        /// Logs an order deletion to the audit log
        /// </summary>
        private void LogOrderDeletion(int orderId)
        {
            try
            {
                // Get the current user
                string username = User.Identity.Name;

                // Log to a text file (in a real application, this would go to a database table)
                string logEntry = string.Format("{0} - Order {1} deleted by {2}",
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    orderId,
                    username);

                string logPath = Server.MapPath("~/App_Data/AuditLog.txt");
                File.AppendAllText(logPath, logEntry + Environment.NewLine);
            }
            catch (Exception ex)
            {
                // Just log to debug - don't disrupt the user experience for a logging error
                System.Diagnostics.Debug.WriteLine("Error logging order deletion: " + ex.Message);
            }
        }

        /// <summary>
        /// Logs an error to the error log
        /// </summary>
        private void LogError(Exception ex)
        {
            try
            {
                // Log to a text file (in a real application, this would go to a database or logging system)
                string logEntry = string.Format("{0} - ERROR: {1}\r\nStack Trace: {2}",
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    ex.Message,
                    ex.StackTrace);

                string logPath = Server.MapPath("~/App_Data/ErrorLog.txt");
                File.AppendAllText(logPath, logEntry + Environment.NewLine + Environment.NewLine);
            }
            catch
            {
                // Fail silently - this is just logging
                System.Diagnostics.Debug.WriteLine("Error logging exception: " + ex.Message);
            }
        }

        /// <summary>
        /// Gets the total count of rows for pagination
        /// </summary>
        public int GetTotalRowCount()
        {
            int count = 0;
            
            try
            {
                using (OracleConnection conn = new OracleConnection(connectionString))
                {
                    using (OracleCommand cmd = new OracleCommand())
                    {
                        cmd.Connection = conn;

                        // Build the base SQL query with joins
                        StringBuilder query = new StringBuilder();
                        query.Append(@"SELECT COUNT(*) 
                                      FROM AARON_IPT.Orders o 
                                      INNER JOIN AARON_IPT.Users u ON o.UserID = u.UserID 
                                      WHERE o.IsActive = 1");

                        // Apply status filter if selected
                        if (!string.IsNullOrEmpty(ddlStatus.SelectedValue))
                        {
                            query.Append(" AND o.Status = :status");
                            cmd.Parameters.Add(new OracleParameter("status", ddlStatus.SelectedValue));
                        }

                        // Apply date range filter if both dates are provided
                        if (!string.IsNullOrEmpty(txtStartDate.Text) && !string.IsNullOrEmpty(txtEndDate.Text))
                        {
                            query.Append(" AND o.OrderDate BETWEEN :startDate AND :endDate");

                            DateTime startDate = DateTime.Parse(txtStartDate.Text);
                            DateTime endDate = DateTime.Parse(txtEndDate.Text).AddDays(1).AddSeconds(-1); // Set to end of day

                            cmd.Parameters.Add(new OracleParameter("startDate", OracleDbType.Date, startDate, ParameterDirection.Input));
                            cmd.Parameters.Add(new OracleParameter("endDate", OracleDbType.Date, endDate, ParameterDirection.Input));
                        }

                        cmd.CommandText = query.ToString();

                        conn.Open();
                        count = Convert.ToInt32(cmd.ExecuteScalar());
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the error
                LogError(ex);
                return 0;
            }
            
            return count;
        }

        /// <summary>
        /// Handles the ItemCommand event of the pagination DataList
        /// </summary>
        protected void dlPaging_ItemCommand(object source, DataListCommandEventArgs e)
        {
            if (e.CommandName == "Page")
            {
                // Convert the CommandArgument to integer
                int pageNumber;
                if (int.TryParse(e.CommandArgument.ToString(), out pageNumber))
                {
                    // GridView's PageIndex is 0-based, but our visible page numbers are 1-based
                    gvOrders.PageIndex = pageNumber - 1;
                    BindOrders();
                }
            }
        }

        /// <summary>
        /// Handles the ItemDataBound event of the pagination DataList
        /// </summary>
        protected void dlPaging_ItemDataBound(object sender, DataListItemEventArgs e)
        {
            LinkButton lnkPage = (LinkButton)e.Item.FindControl("lnkPage");
            if (lnkPage != null)
            {
                // Highlight the current page button
                if (Convert.ToBoolean(DataBinder.Eval(e.Item.DataItem, "Selected")))
                {
                    lnkPage.CssClass = "relative inline-flex items-center px-4 py-2 border border-pink-600 bg-pink-600 text-sm font-medium text-white";
                }
                else
                {
                    lnkPage.CssClass = "relative inline-flex items-center px-4 py-2 border border-gray-300 bg-white text-sm font-medium text-gray-700 hover:bg-gray-50";
                }
            }
        }
    }
}