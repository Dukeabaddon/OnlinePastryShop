<%@ Page Title="" Language="C#" MasterPageFile="~/Pages/AdminMaster.Master" AutoEventWireup="true" CodeBehind="Orders.aspx.cs" Inherits="OnlinePastryShop.Pages.Orders" %>
<asp:Content ID="Content1" ContentPlaceHolderID="AdminContent" runat="server">

    <div class="container mx-auto">
        <h1 class="text-2xl font-bold text-gray-800 mb-6">Order Management</h1>
        
        <!-- Filter Section -->
        <div class="bg-white p-4 rounded-lg shadow mb-6">
            <div class="grid grid-cols-1 md:grid-cols-4 gap-4">
                <!-- Status Filter -->
                <div>
                    <label for="ddlStatus" class="block text-sm font-medium text-gray-700 mb-1">Status</label>
                    <asp:DropDownList ID="ddlStatus" runat="server" CssClass="w-full rounded-md border-gray-300 shadow-sm focus:border-pink-500 focus:ring focus:ring-pink-200" AutoPostBack="true" OnSelectedIndexChanged="ddlStatus_SelectedIndexChanged">
                        <asp:ListItem Text="All" Value=""></asp:ListItem>
                        <asp:ListItem Text="Pending" Value="Pending"></asp:ListItem>
                        <asp:ListItem Text="Processing" Value="Processing"></asp:ListItem>
                        <asp:ListItem Text="Shipped" Value="Shipped"></asp:ListItem>
                        <asp:ListItem Text="Delivered" Value="Delivered"></asp:ListItem>
                        <asp:ListItem Text="Cancelled" Value="Cancelled"></asp:ListItem>
                    </asp:DropDownList>
                </div>
                
                <!-- Date Range - Start Date -->
                <div>
                    <label for="txtStartDate" class="block text-sm font-medium text-gray-700 mb-1">Start Date</label>
                    <asp:TextBox ID="txtStartDate" runat="server" CssClass="w-full rounded-md border-gray-300 shadow-sm focus:border-pink-500 focus:ring focus:ring-pink-200" placeholder="MM/DD/YYYY" TextMode="Date"></asp:TextBox>
                </div>
                
                <!-- Date Range - End Date -->
                <div>
                    <label for="txtEndDate" class="block text-sm font-medium text-gray-700 mb-1">End Date</label>
                    <asp:TextBox ID="txtEndDate" runat="server" CssClass="w-full rounded-md border-gray-300 shadow-sm focus:border-pink-500 focus:ring focus:ring-pink-200" placeholder="MM/DD/YYYY" TextMode="Date"></asp:TextBox>
                </div>
                
                <!-- Filter Button -->
                <div class="flex items-end">
                    <asp:Button ID="btnFilter" runat="server" Text="Apply Filters" CssClass="w-full bg-pink-600 hover:bg-pink-700 text-white font-medium py-2 px-4 rounded-md focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-pink-500" OnClick="btnFilter_Click" />
                </div>
            </div>
            
            <!-- Export to CSV -->
            <div class="mt-4 flex justify-end">
                <asp:Button ID="btnExportCsv" runat="server" Text="Export to CSV" CssClass="bg-gray-600 hover:bg-gray-700 text-white font-medium py-2 px-4 rounded-md focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-gray-500" OnClick="btnExportCsv_Click" />
            </div>
        </div>
        
        <!-- Orders GridView -->
        <div class="bg-white p-4 rounded-lg shadow">
            <asp:UpdatePanel ID="UpdatePanel1" runat="server">
                <ContentTemplate>
                    <!-- Loading Spinner -->
                    <div id="loadingSpinner" runat="server" class="hidden">
                        <div class="flex justify-center items-center p-4">
                            <div class="animate-spin rounded-full h-8 w-8 border-b-2 border-pink-600"></div>
                        </div>
                    </div>
                    
                    <!-- No Orders Message -->
                    <asp:Panel ID="pnlNoOrders" runat="server" CssClass="py-8 text-center" Visible="false">
                        <p class="text-gray-600">No orders found</p>
                    </asp:Panel>
                    
                    <!-- Orders GridView -->
                    <asp:GridView ID="gvOrders" runat="server" AutoGenerateColumns="False" 
                        CssClass="table-auto w-full" 
                        AllowPaging="True" 
                        AllowSorting="True"
                        PageSize="10"
                        OnPageIndexChanging="gvOrders_PageIndexChanging"
                        OnSorting="gvOrders_Sorting"
                        OnRowCommand="gvOrders_RowCommand"
                        OnRowDataBound="gvOrders_RowDataBound"
                        DataKeyNames="OrderID"
                        EmptyDataText="No orders found">
                        <Columns>
                            <asp:TemplateField>
                                <HeaderTemplate>
                                    <asp:CheckBox ID="chkSelectAll" runat="server" onclick="toggleAllCheckboxes(this);" />
                                </HeaderTemplate>
                                <ItemTemplate>
                                    <asp:CheckBox ID="chkSelect" runat="server" />
                                </ItemTemplate>
                                <ItemStyle HorizontalAlign="Center" />
                            </asp:TemplateField>
                            
                            <asp:BoundField DataField="OrderID" HeaderText="Order ID" SortExpression="OrderID" />
                            <asp:BoundField DataField="Username" HeaderText="Customer" SortExpression="Username" />
                            <asp:BoundField DataField="OrderDate" HeaderText="Date" SortExpression="OrderDate" DataFormatString="{0:MM/dd/yyyy}" />
                            <asp:BoundField DataField="TotalAmount" HeaderText="Total" SortExpression="TotalAmount" DataFormatString="{0:C}" />
                            
                            <asp:TemplateField HeaderText="Status" SortExpression="Status">
                                <ItemTemplate>
                                    <span class='<%# GetStatusCssClass(Eval("Status").ToString()) %>'>
                                        <%# Eval("Status") %>
                                    </span>
                                </ItemTemplate>
                            </asp:TemplateField>
                            
                            <asp:TemplateField HeaderText="Address" SortExpression="ShippingAddress">
                                <ItemTemplate>
                                    <span title='<%# Eval("ShippingAddress") %>'>
                                        <%# TruncateAddress(Eval("ShippingAddress").ToString()) %>
                                    </span>
                                </ItemTemplate>
                            </asp:TemplateField>
                            
                            <asp:BoundField DataField="PaymentMethod" HeaderText="Payment" SortExpression="PaymentMethod" />
                            
                            <asp:TemplateField HeaderText="Actions">
                                <ItemTemplate>
                                    <div class="flex space-x-2">
                                        <asp:LinkButton ID="btnViewDetails" runat="server" 
                                            CssClass="text-blue-600 hover:text-blue-800"
                                            CommandName="ViewDetails" 
                                            CommandArgument='<%# Eval("OrderID") %>'>
                                            <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" viewBox="0 0 20 20" fill="currentColor">
                                                <path d="M10 12a2 2 0 100-4 2 2 0 000 4z" />
                                                <path fill-rule="evenodd" d="M.458 10C1.732 5.943 5.522 3 10 3s8.268 2.943 9.542 7c-1.274 4.057-5.064 7-9.542 7S1.732 14.057.458 10zM14 10a4 4 0 11-8 0 4 4 0 018 0z" clip-rule="evenodd" />
                                            </svg>
                                        </asp:LinkButton>
                                        
                                        <asp:LinkButton ID="btnDelete" runat="server" 
                                            CssClass="text-red-600 hover:text-red-800"
                                            CommandName="DeleteOrder" 
                                            CommandArgument='<%# Eval("OrderID") %>'
                                            OnClientClick="return confirm('Are you sure you want to delete this order?');">
                                            <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" viewBox="0 0 20 20" fill="currentColor">
                                                <path fill-rule="evenodd" d="M9 2a1 1 0 00-.894.553L7.382 4H4a1 1 0 000 2v10a2 2 0 002 2h8a2 2 0 002-2V6a1 1 0 100-2h-3.382l-.724-1.447A1 1 0 0011 2H9zM7 8a1 1 0 012 0v6a1 1 0 11-2 0V8zm5-1a1 1 0 00-1 1v6a1 1 0 102 0V8a1 1 0 00-1-1z" clip-rule="evenodd" />
                                            </svg>
                                        </asp:LinkButton>
                                    </div>
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                        <HeaderStyle CssClass="bg-gray-100 text-left text-xs font-medium text-gray-700 uppercase tracking-wider py-3 px-4" />
                        <RowStyle CssClass="border-b border-gray-200 hover:bg-gray-50" />
                        <AlternatingRowStyle CssClass="border-b border-gray-200 bg-gray-50 hover:bg-gray-100" />
                        <PagerSettings Mode="NextPreviousFirstLast" Position="Bottom" />
                        <PagerTemplate>
                            <div class="bg-white border-t border-gray-200 px-4 py-3 flex items-center justify-between">
                                <div class="flex-1 flex justify-between sm:hidden">
                                    <asp:LinkButton ID="btnPrevMobile" runat="server" CommandName="Page" CommandArgument="Prev"
                                        CssClass="relative inline-flex items-center px-4 py-2 border border-gray-300 text-sm font-medium rounded-md text-gray-700 bg-white hover:bg-gray-50">
                                        &laquo; Previous
                                    </asp:LinkButton>
                                    <asp:LinkButton ID="btnNextMobile" runat="server" CommandName="Page" CommandArgument="Next"
                                        CssClass="ml-3 relative inline-flex items-center px-4 py-2 border border-gray-300 text-sm font-medium rounded-md text-gray-700 bg-white hover:bg-gray-50">
                                        Next &raquo;
                                    </asp:LinkButton>
                                </div>
                                <div class="hidden sm:flex-1 sm:flex sm:items-center sm:justify-between">
                                    <div>
                                        <p class="text-sm text-gray-700">
                                            <span>Showing page </span>
                                            <span id="currentPageDisplay" class="font-medium"><%# ((GridView)Container.Parent.Parent).PageIndex + 1 %></span>
                                            <span> of </span>
                                            <span id="totalPagesDisplay" class="font-medium"><%# Math.Ceiling((double)GetTotalRowCount() / ((GridView)Container.Parent.Parent).PageSize) %></span>
                                        </p>
                                    </div>
                                    <div>
                                        <nav class="relative z-0 inline-flex rounded-md shadow-sm -space-x-px" aria-label="Pagination">
                                            <!-- First Page Button -->
                                            <asp:LinkButton ID="lnkFirst" runat="server" CommandName="Page" CommandArgument="First"
                                                CssClass="relative inline-flex items-center px-2 py-2 rounded-l-md border border-gray-300 bg-white text-sm font-medium text-gray-500 hover:bg-gray-50">
                                                <span class="sr-only">First</span>
                                                <span>&laquo;</span>
                                            </asp:LinkButton>
                                            
                                            <!-- Previous Page Button -->
                                            <asp:LinkButton ID="lnkPrevious" runat="server" CommandName="Page" CommandArgument="Prev"
                                                CssClass="relative inline-flex items-center px-2 py-2 border border-gray-300 bg-white text-sm font-medium text-gray-500 hover:bg-gray-50">
                                                <span class="sr-only">Previous</span>
                                                <svg class="h-5 w-5" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 20 20" fill="currentColor" aria-hidden="true">
                                                    <path fill-rule="evenodd" d="M12.707 5.293a1 1 0 010 1.414L9.414 10l3.293 3.293a1 1 0 01-1.414 1.414l-4-4a1 1 0 010-1.414l4-4a1 1 0 011.414 0z" clip-rule="evenodd" />
                                                </svg>
                                            </asp:LinkButton>
                                            
                                            <!-- Page Numbers -->
                                            <asp:DataList ID="dlPaging" runat="server" RepeatDirection="Horizontal" OnItemCommand="dlPaging_ItemCommand" OnItemDataBound="dlPaging_ItemDataBound">
                                                <ItemTemplate>
                                                    <asp:LinkButton ID="lnkPage" runat="server" Text='<%# Eval("Text") %>' CommandName="Page" CommandArgument='<%# Eval("Value") %>'
                                                        CssClass='<%# Convert.ToBoolean(Eval("Selected")) ? "relative inline-flex items-center px-4 py-2 border border-pink-600 bg-pink-600 text-sm font-medium text-white" : "relative inline-flex items-center px-4 py-2 border border-gray-300 bg-white text-sm font-medium text-gray-700 hover:bg-gray-50" %>' />
                                                </ItemTemplate>
                                            </asp:DataList>
                                            
                                            <!-- Next Page Button -->
                                            <asp:LinkButton ID="lnkNext" runat="server" CommandName="Page" CommandArgument="Next"
                                                CssClass="relative inline-flex items-center px-2 py-2 border border-gray-300 bg-white text-sm font-medium text-gray-500 hover:bg-gray-50">
                                                <span class="sr-only">Next</span>
                                                <svg class="h-5 w-5" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 20 20" fill="currentColor" aria-hidden="true">
                                                    <path fill-rule="evenodd" d="M7.293 14.707a1 1 0 010-1.414L10.586 10 7.293 6.707a1 1 0 011.414-1.414l4 4a1 1 0 010 1.414l-4 4a1 1 0 01-1.414 0z" clip-rule="evenodd" />
                                                </svg>
                                            </asp:LinkButton>
                                            
                                            <!-- Last Page Button -->
                                            <asp:LinkButton ID="lnkLast" runat="server" CommandName="Page" CommandArgument="Last"
                                                CssClass="relative inline-flex items-center px-2 py-2 rounded-r-md border border-gray-300 bg-white text-sm font-medium text-gray-500 hover:bg-gray-50">
                                                <span class="sr-only">Last</span>
                                                <span>&raquo;</span>
                                            </asp:LinkButton>
                                        </nav>
                                    </div>
                                </div>
                            </div>
                        </PagerTemplate>
                    </asp:GridView>
                    
                    <!-- Bulk Actions -->
                    <div class="mt-4 flex items-center space-x-2">
                        <asp:DropDownList ID="ddlBulkAction" runat="server" CssClass="rounded-md border-gray-300 shadow-sm focus:border-pink-500 focus:ring focus:ring-pink-200">
                            <asp:ListItem Text="Bulk Actions" Value=""></asp:ListItem>
                            <asp:ListItem Text="Mark as Processing" Value="Processing"></asp:ListItem>
                            <asp:ListItem Text="Mark as Shipped" Value="Shipped"></asp:ListItem>
                            <asp:ListItem Text="Mark as Delivered" Value="Delivered"></asp:ListItem>
                            <asp:ListItem Text="Mark as Cancelled" Value="Cancelled"></asp:ListItem>
                            <asp:ListItem Text="Delete Selected" Value="Delete"></asp:ListItem>
                        </asp:DropDownList>
                        <asp:Button ID="btnApplyBulk" runat="server" Text="Apply" 
                            CssClass="bg-pink-600 hover:bg-pink-700 text-white font-medium py-2 px-4 rounded-md focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-pink-500"
                            OnClick="btnApplyBulk_Click" />
                    </div>
                </ContentTemplate>
                <Triggers>
                    <asp:AsyncPostBackTrigger ControlID="btnFilter" EventName="Click" />
                    <asp:AsyncPostBackTrigger ControlID="ddlStatus" EventName="SelectedIndexChanged" />
                    <asp:PostBackTrigger ControlID="btnExportCsv" />
                </Triggers>
            </asp:UpdatePanel>
        </div>
    </div>
    
    <!-- Order Details Modal -->
    <div id="orderDetailsModal" class="hidden fixed z-10 inset-0 overflow-y-auto" aria-labelledby="modal-title" role="dialog" aria-modal="true">
        <div class="flex items-end justify-center min-h-screen pt-4 px-4 pb-20 text-center sm:block sm:p-0">
            <!-- Background overlay -->
            <div class="fixed inset-0 bg-gray-500 bg-opacity-75 transition-opacity" aria-hidden="true"></div>
            
            <!-- Modal panel -->
            <div class="inline-block align-bottom bg-white rounded-lg text-left overflow-hidden shadow-xl transform transition-all sm:my-8 sm:align-middle sm:max-w-3xl sm:w-full">
                <div class="bg-white px-4 pt-5 pb-4 sm:p-6 sm:pb-4">
                    <div class="sm:flex sm:items-start">
                        <div class="mt-3 text-center sm:mt-0 sm:ml-4 sm:text-left w-full">
                            <h3 class="text-lg leading-6 font-medium text-gray-900" id="modal-title">
                                Order Details
                            </h3>
                            
                            <div class="mt-4">
                                <asp:UpdatePanel ID="UpdatePanel2" runat="server">
                                    <ContentTemplate>
                                        <div class="grid grid-cols-1 sm:grid-cols-2 gap-4 mb-4">
                                            <div>
                                                <p class="text-sm text-gray-500">Order ID: <span id="spanOrderId" runat="server" class="font-medium text-gray-900"></span></p>
                                                <p class="text-sm text-gray-500">Customer: <span id="spanCustomer" runat="server" class="font-medium text-gray-900"></span></p>
                                                <p class="text-sm text-gray-500">Date: <span id="spanOrderDate" runat="server" class="font-medium text-gray-900"></span></p>
                                            </div>
                                            <div>
                                                <p class="text-sm text-gray-500">Total: <span id="spanTotal" runat="server" class="font-medium text-gray-900"></span></p>
                                                <p class="text-sm text-gray-500">Payment: <span id="spanPayment" runat="server" class="font-medium text-gray-900"></span></p>
                                                <p class="text-sm text-gray-500">Address: <span id="spanAddress" runat="server" class="font-medium text-gray-900"></span></p>
                                            </div>
                                        </div>
                                        
                                        <div class="mb-4">
                                            <label for="ddlOrderStatus" class="block text-sm font-medium text-gray-700 mb-1">Status</label>
                                            <div class="flex space-x-2">
                                                <asp:DropDownList ID="ddlOrderStatus" runat="server" CssClass="rounded-md border-gray-300 shadow-sm focus:border-pink-500 focus:ring focus:ring-pink-200">
                                                    <asp:ListItem Text="Pending" Value="Pending"></asp:ListItem>
                                                    <asp:ListItem Text="Processing" Value="Processing"></asp:ListItem>
                                                    <asp:ListItem Text="Shipped" Value="Shipped"></asp:ListItem>
                                                    <asp:ListItem Text="Delivered" Value="Delivered"></asp:ListItem>
                                                    <asp:ListItem Text="Cancelled" Value="Cancelled"></asp:ListItem>
                                                </asp:DropDownList>
                                                <asp:Button ID="btnUpdateStatus" runat="server" Text="Update Status" 
                                                    CssClass="bg-pink-600 hover:bg-pink-700 text-white font-medium py-2 px-4 rounded-md focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-pink-500"
                                                    OnClick="btnUpdateStatus_Click" />
                                                <asp:HiddenField ID="hfOrderId" runat="server" />
                                            </div>
                                        </div>
                                        
                                        <h4 class="font-medium text-gray-900 mb-2">Order Items</h4>
                                        
                                        <!-- Order Items GridView -->
                                        <asp:GridView ID="gvOrderItems" runat="server" AutoGenerateColumns="False" 
                                            CssClass="table-auto w-full" 
                                            EmptyDataText="No items found">
                                            <Columns>
                                                <asp:BoundField DataField="ProductID" HeaderText="Product ID" />
                                                <asp:BoundField DataField="Name" HeaderText="Product" />
                                                <asp:BoundField DataField="Quantity" HeaderText="Quantity" />
                                                <asp:BoundField DataField="Price" HeaderText="Price" DataFormatString="{0:C}" />
                                                <asp:TemplateField HeaderText="Subtotal">
                                                    <ItemTemplate>
                                                        <%# String.Format("{0:C}", Convert.ToDecimal(Eval("Price")) * Convert.ToInt32(Eval("Quantity"))) %>
                                                    </ItemTemplate>
                                                </asp:TemplateField>
                                            </Columns>
                                            <HeaderStyle CssClass="bg-gray-100 text-left text-xs font-medium text-gray-700 uppercase tracking-wider py-3 px-4" />
                                            <RowStyle CssClass="border-b border-gray-200" />
                                            <AlternatingRowStyle CssClass="border-b border-gray-200 bg-gray-50" />
                                        </asp:GridView>
                                    </ContentTemplate>
                                </asp:UpdatePanel>
                            </div>
                        </div>
                    </div>
                </div>
                <div class="bg-gray-50 px-4 py-3 sm:px-6 sm:flex sm:flex-row-reverse">
                    <button type="button" id="btnCloseModal" class="mt-3 w-full inline-flex justify-center rounded-md border border-gray-300 shadow-sm px-4 py-2 bg-white text-base font-medium text-gray-700 hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-pink-500 sm:mt-0 sm:ml-3 sm:w-auto sm:text-sm">
                        Close
                    </button>
                </div>
            </div>
        </div>
    </div>
    
    <!-- JavaScript for Modal -->
    <script type="text/javascript">
        var prm = Sys.WebForms.PageRequestManager.getInstance();
        
        prm.add_endRequest(function() {
            attachEvents();
        });
        
        document.addEventListener('DOMContentLoaded', function() {
            attachEvents();
        });
        
        function attachEvents() {
            // Handle Order Details buttons
            document.querySelectorAll('[id*=btnViewDetails]').forEach(function(btn) {
                btn.addEventListener('click', function() {
                    showOrderDetailsModal();
                });
            });
            
            // Close modal button
            document.getElementById('btnCloseModal').addEventListener('click', function() {
                hideOrderDetailsModal();
            });
            
            // Close modal when clicking outside
            document.getElementById('orderDetailsModal').addEventListener('click', function(e) {
                if (e.target === this) {
                    hideOrderDetailsModal();
                }
            });
            
            // "Select All" checkbox
            var selectAllCheckbox = document.querySelector('[id*=chkSelectAll]');
            if (selectAllCheckbox) {
                selectAllCheckbox.addEventListener('change', function() {
                    toggleAllCheckboxes(this);
                });
            }
        }
        
        function toggleAllCheckboxes(source) {
            var checkboxes = document.querySelectorAll('[id*=chkSelect]');
            for (var i = 0; i < checkboxes.length; i++) {
                checkboxes[i].checked = source.checked;
            }
        }
        
        function showOrderDetailsModal() {
            document.getElementById('orderDetailsModal').classList.remove('hidden');
        }
        
        function hideOrderDetailsModal() {
            document.getElementById('orderDetailsModal').classList.add('hidden');
        }
    </script>
</asp:Content>
