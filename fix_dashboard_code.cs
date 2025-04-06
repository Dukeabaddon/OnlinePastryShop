// Update the LoadLowStockItems method to use the new procedure name
private void LoadLowStockItems(OracleConnection connection)
{
    // Add a null check for LowStockRepeater before proceeding
    // This prevents the NullReferenceException if the control is missing or not rendered
    if (LowStockRepeater == null)
    {
        System.Diagnostics.Debug.WriteLine("LowStockRepeater control not found. Skipping low stock data binding.");
        // Optionally hide the placeholder or show a message
        if (EmptyLowStockMessage != null)
        {
            EmptyLowStockMessage.Visible = true;
        }
        // Ensure the KPI card value is handled gracefully
        LowStockCount = "N/A"; // Or "0" or some other indicator
        return; // Exit the method as we cannot bind data
    }

    try
    {
        using (OracleCommand command = new OracleCommand("GET_INVENTORY_ALERTS_V2", connection))
        {
            command.CommandType = CommandType.StoredProcedure;

            // Define output parameter for the cursor
            OracleParameter outputCursor = new OracleParameter("p_low_stock_cursor", OracleDbType.RefCursor);
            outputCursor.Direction = ParameterDirection.Output;
            command.Parameters.Add(outputCursor);

            // Define output parameter for low stock count
            OracleParameter lowStockCountParam = new OracleParameter("p_low_stock_count", OracleDbType.Int32);
            lowStockCountParam.Direction = ParameterDirection.Output;
            command.Parameters.Add(lowStockCountParam);

            OracleDataAdapter adapter = new OracleDataAdapter(command);
            DataTable lowStockTable = new DataTable();
            adapter.Fill(lowStockTable);

            // Data binding for Low Stock Items
            LowStockRepeater.DataSource = lowStockTable;
            LowStockRepeater.DataBind();

            // Update the Low Stock Count KPI
            // Check if the output parameter value is DBNull before converting
            if (lowStockCountParam.Value != DBNull.Value && lowStockCountParam.Value != null)
            {
                try
                {
                    // Use Convert.ToString for safer conversion
                    LowStockCount = Convert.ToString(lowStockCountParam.Value);
                }
                catch (InvalidCastException ice)
                {
                    System.Diagnostics.Debug.WriteLine($"Error casting LowStockCount: {ice.Message}");
                    LowStockCount = "Err"; // Indicate an error occurred
                }
                catch (Exception ex) // Catch any other potential conversion issues
                {
                    System.Diagnostics.Debug.WriteLine($"Error getting LowStockCount: {ex.Message}");
                    LowStockCount = "Err";
                }
            }
            else
            {
                LowStockCount = "0"; // Default to 0 if no count is returned
            }

            // Handle empty data message visibility
            if (EmptyLowStockMessage != null)
            {
                EmptyLowStockMessage.Visible = (lowStockTable.Rows.Count == 0);
            }
            LowStockRepeater.Visible = (lowStockTable.Rows.Count > 0);
        }
    }
    catch (Exception ex)
    {
        System.Diagnostics.Debug.WriteLine($"Error loading low stock items: {ex.Message}");
        ShowError("Error loading low stock items.");
        // Ensure UI elements are handled gracefully on error
        if (EmptyLowStockMessage != null) EmptyLowStockMessage.Visible = true;
        LowStockRepeater.Visible = false; // Hide repeater on error
        LowStockCount = "Err"; // Indicate error on KPI
    }
}
