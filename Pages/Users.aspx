<%@ Page Title="User Management" Language="C#" MasterPageFile="~/Pages/AdminMaster.Master" AutoEventWireup="true" CodeBehind="Users.aspx.cs" Inherits="OnlinePastryShop.Pages.Users" %>

<asp:Content ID="AdminContent" ContentPlaceHolderID="AdminContent" runat="server">
    <form runat="server" DefaultButton="btnSearch">
        <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>

        <div class="container mx-auto px-4 py-8">
            <h1 class="text-3xl font-bold text-gray-800 mb-6">User Management</h1>
            
            <!-- Success and Error Messages -->
            <asp:Panel ID="pnlSuccess" runat="server" CssClass="bg-green-100 border border-green-400 text-green-700 px-4 py-3 rounded mb-4" Visible="false">
                <strong class="font-bold">Success!</strong>
                <span class="block sm:inline"> <asp:Literal ID="litSuccessMessage" runat="server"></asp:Literal></span>
            </asp:Panel>
            
            <asp:Panel ID="pnlError" runat="server" CssClass="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded mb-4" Visible="false">
                <strong class="font-bold">Error!</strong>
                <span class="block sm:inline"> <asp:Literal ID="litErrorMessage" runat="server"></asp:Literal></span>
            </asp:Panel>
            
            <!-- Success Notification Popup (Hidden by default) -->
            <div id="successPopup" class="fixed bottom-4 right-4 bg-green-100 border border-green-400 text-green-700 px-4 py-3 rounded shadow-lg hidden z-50">
                <div class="flex items-center">
                    <svg class="h-5 w-5 text-green-600 mr-2" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7" />
                    </svg>
                    <div>
                        <span class="font-bold">Success!</span>
                        <span class="block sm:inline" id="successMessage"></span>
                    </div>
                    <button id="closeSuccessPopup" class="ml-4 bg-green-500 hover:bg-green-700 text-white font-bold py-1 px-3 rounded">
                        OK
                    </button>
                </div>
            </div>
            
            <!-- User Stats Cards -->
            <div class="grid grid-cols-1 md:grid-cols-2 gap-6 mb-8">
                <!-- Total Users Card -->
                <div class="bg-white rounded-lg shadow-md p-6 border-l-4 border-[#D43B6A]">
                    <div class="flex items-center">
                        <div class="p-3 rounded-full bg-pink-100 text-[#D43B6A] mr-4">
                            <svg xmlns="http://www.w3.org/2000/svg" class="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4.354a4 4 0 110 5.292M15 21H3v-1a6 6 0 0112 0v1zm0 0h6v-1a6 6 0 00-9-5.197M13 7a4 4 0 11-8 0 4 4 0 018 0z" />
                            </svg>
                        </div>
                        <div>
                            <p class="text-gray-500 text-sm">Total Users</p>
                            <div class="flex items-center">
                                <asp:Literal ID="litTotalUsers" runat="server">0</asp:Literal>
                                <span class="text-xl font-bold"></span>
                            </div>
                        </div>
                    </div>
                </div>
                
                <!-- New Users Card -->
                <div class="bg-white rounded-lg shadow-md p-6 border-l-4 border-green-500">
                    <div class="flex items-center">
                        <div class="p-3 rounded-full bg-green-100 text-green-600 mr-4">
                            <svg xmlns="http://www.w3.org/2000/svg" class="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M18 9v3m0 0v3m0-3h3m-3 0h-3m-2-5a4 4 0 11-8 0 4 4 0 018 0zM3 20a6 6 0 0112 0v1H3v-1z" />
                            </svg>
                        </div>
                        <div>
                            <p class="text-gray-500 text-sm">New Users (Last 30 Days)</p>
                            <div class="flex items-center">
                                <asp:Literal ID="litNewUsers" runat="server">0</asp:Literal>
                                <span class="text-xl font-bold"></span>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
            
            <!-- Search and Filter Controls -->
            <div class="bg-white rounded-lg shadow-md p-6 mb-8">
                <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
                    <div class="col-span-1 md:col-span-2">
                        <label for="txtSearch" class="block text-sm font-medium text-gray-700 mb-1">Search</label>
                        <asp:TextBox ID="txtSearch" runat="server" CssClass="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-[#D43B6A] focus:border-[#D43B6A]" placeholder="Search by username or email"></asp:TextBox>
                    </div>
                    <div>
                        <label for="ddlStatus" class="block text-sm font-medium text-gray-700 mb-1">Status</label>
                        <asp:DropDownList ID="ddlStatus" runat="server" CssClass="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-[#D43B6A] focus:border-[#D43B6A]" AutoPostBack="true" OnSelectedIndexChanged="ddlStatus_SelectedIndexChanged">
                            <asp:ListItem Text="Active" Value="true" Selected="True"></asp:ListItem>
                            <asp:ListItem Text="Inactive" Value="false"></asp:ListItem>
                        </asp:DropDownList>
                    </div>
                </div>
                <div class="mt-4 flex justify-end">
                    <asp:Button ID="btnReset" runat="server" Text="Reset" OnClick="btnReset_Click" CssClass="bg-gray-200 text-gray-700 px-4 py-2 rounded-md hover:bg-gray-300 mr-2 transition-colors" />
                    <asp:Button ID="btnSearch" runat="server" Text="Search" OnClick="btnSearch_Click" CssClass="bg-[#D43B6A] text-white px-4 py-2 rounded-md hover:bg-pink-700 transition-colors" />
                </div>
            </div>
            
            <!-- Users List -->
            <div class="bg-white rounded-lg shadow-md overflow-hidden mb-8">
                <asp:Literal ID="litNoUsers" runat="server" Visible="false"></asp:Literal>
                <asp:ListView ID="lvUsers" runat="server" OnItemCommand="lvUsers_ItemCommand" ItemPlaceholderID="itemPlaceholder">
                    <LayoutTemplate>
                        <table class="min-w-full divide-y divide-gray-200">
                            <thead class="bg-gray-50">
                                <tr>
                                    <th scope="col" class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">ID</th>
                                    <th scope="col" class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Username</th>
                                    <th scope="col" class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Email</th>
                                    <th scope="col" class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Role</th>
                                    <th scope="col" class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Date Created</th>
                                    <th scope="col" class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Status</th>
                                    <th scope="col" class="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">Actions</th>
                                </tr>
                            </thead>
                            <tbody class="bg-white divide-y divide-gray-200">
                                <asp:PlaceHolder ID="itemPlaceholder" runat="server"></asp:PlaceHolder>
                            </tbody>
                        </table>
                        <!-- Empty Data Template -->
                        <asp:Panel ID="emptyPanel" runat="server" CssClass="py-8 text-center" Visible="false">
                            <p class="text-gray-500">No users found matching the criteria.</p>
                        </asp:Panel>
                    </LayoutTemplate>
                    <ItemTemplate>
                        <tr>
                            <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500"><%# Eval("UserId") %></td>
                            <td class="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900"><%# Eval("Username") %></td>
                            <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500"><%# Eval("Email") %></td>
                            <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500"><%# Eval("Role") %></td>
                            <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500"><%# Eval("DateCreated", "{0:MM/dd/yyyy}") %></td>
                            <td class="px-6 py-4 whitespace-nowrap">
                                <span class='<%# Convert.ToInt32(Eval("IsActive")) == 1 ? "px-2 inline-flex text-xs leading-5 font-semibold rounded-full bg-green-100 text-green-800" : "px-2 inline-flex text-xs leading-5 font-semibold rounded-full bg-red-100 text-red-800" %>'>
                                        <%# Convert.ToInt32(Eval("IsActive")) == 1 ? "Active" : "Inactive" %>
                                    </span>
                            </td>
                            <td class="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                                    <div class="flex justify-end space-x-2">
                                    <asp:LinkButton ID="btnViewDetails" runat="server" CommandName="ViewDetails" CommandArgument='<%# Eval("UserId") %>' CssClass="text-indigo-600 hover:text-indigo-900" ToolTip="View Details">
                                            <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
                                                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z" />
                                            </svg>
                                        </asp:LinkButton>
                                    <asp:LinkButton ID="btnResetPassword" runat="server" CommandName="ResetPassword" CommandArgument='<%# Eval("UserId") %>' CssClass="text-blue-600 hover:text-blue-900" ToolTip="Reset Password">
                                            <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 7a2 2 0 012 2m4 0a6 6 0 01-7.743 5.743L11 17H9v2H7v2H4a1 1 0 01-1-1v-2.586a1 1 0 01.293-.707l5.964-5.964A6 6 0 1121 9z" />
                                            </svg>
                                        </asp:LinkButton>
                                    <asp:LinkButton ID="btnToggleStatus" runat="server" CommandName="ToggleStatus" CommandArgument='<%# Eval("UserId") %>' CssClass='<%# Convert.ToInt32(Eval("IsActive")) == 1 ? "text-red-600 hover:text-red-900" : "text-green-600 hover:text-green-900" %>' ToolTip='<%# Convert.ToInt32(Eval("IsActive")) == 1 ? "Deactivate User" : "Activate User" %>'>
                                            <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d='<%# Convert.ToInt32(Eval("IsActive")) == 1 ? "M18.364 18.364A9 9 0 005.636 5.636m12.728 12.728A9 9 0 015.636 5.636m12.728 12.728L5.636 5.636" : "M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" %>' />
                                            </svg>
                                        </asp:LinkButton>
                                </div>
                            </td>
                        </tr>
                    </ItemTemplate>
                    <EmptyDataTemplate>
                        <div class="py-8 text-center">
                            <p class="text-gray-500">No users found matching the criteria.</p>
                        </div>
                    </EmptyDataTemplate>
                </asp:ListView>
            </div>
            
            <!-- Pagination -->
            <div class="bg-white rounded-lg shadow-md p-4 flex items-center justify-between">
                <div class="text-sm text-gray-700">
                    Page <asp:Label ID="lblCurrentPage" runat="server" CssClass="font-medium">1</asp:Label> of <asp:Label ID="lblTotalPages" runat="server" CssClass="font-medium">1</asp:Label>
                    </div>
                <div class="flex items-center space-x-2">
                    <asp:LinkButton ID="btnFirst" runat="server" OnClientClick="return changePage(1);" OnClick="Page_Click" CommandArgument="first" CssClass="px-3 py-1 rounded-md text-sm font-medium bg-white text-gray-700 hover:bg-gray-50 border border-gray-300">
                        <i class="fas fa-angle-double-left"></i> First
                    </asp:LinkButton>
                    <asp:LinkButton ID="btnPrev" runat="server" OnClientClick="return changePage(Math.max(1, parseInt(document.getElementById('<%=hdnCurrentPage.ClientID%>').value) - 1));" OnClick="Page_Click" CommandArgument="prev" CssClass="px-3 py-1 rounded-md text-sm font-medium bg-white text-gray-700 hover:bg-gray-50 border border-gray-300">
                        <i class="fas fa-angle-left"></i> Prev
                    </asp:LinkButton>
                    
                    <!-- Numeric pagination will be dynamically generated here -->
                    <asp:PlaceHolder ID="phPageNumbers" runat="server"></asp:PlaceHolder>
                    <asp:Literal ID="litPagination" runat="server"></asp:Literal>
                    
                    <asp:LinkButton ID="btnNext" runat="server" OnClientClick="return changePage(Math.min(parseInt(document.getElementById('<%=hdnTotalPages.ClientID%>').value), parseInt(document.getElementById('<%=hdnCurrentPage.ClientID%>').value) + 1));" OnClick="Page_Click" CommandArgument="next" CssClass="px-3 py-1 rounded-md text-sm font-medium bg-white text-gray-700 hover:bg-gray-50 border border-gray-300">
                        Next <i class="fas fa-angle-right"></i>
                    </asp:LinkButton>
                    <asp:LinkButton ID="btnLast" runat="server" OnClientClick="return changePage(parseInt(document.getElementById('<%=hdnTotalPages.ClientID%>').value));" OnClick="Page_Click" CommandArgument="last" CssClass="px-3 py-1 rounded-md text-sm font-medium bg-white text-gray-700 hover:bg-gray-50 border border-gray-300">
                        Last <i class="fas fa-angle-double-right"></i>
                    </asp:LinkButton>
                </div>
            </div>
            
            <!-- Hidden Fields for State Management -->
            <asp:HiddenField ID="hdnCurrentPage" runat="server" Value="1" />
            <asp:HiddenField ID="hdnTotalPages" runat="server" Value="1" />
            <asp:HiddenField ID="hdnStatus" runat="server" Value="true" />
            <asp:HiddenField ID="hdnSearch" runat="server" Value="" />
            <asp:HiddenField ID="hdnSelectedUserId" runat="server" Value="0" />
            <asp:Button ID="btnChangePage" runat="server" Style="display:none;" OnClick="Page_Click" />
            
            <!-- User Details Modal -->
            <asp:Panel ID="pnlUserDetails" runat="server" CssClass="fixed inset-0 bg-gray-600 bg-opacity-50 overflow-y-auto h-full w-full z-50" Visible="false">
                <div class="relative top-20 mx-auto p-5 border w-11/12 md:w-2/3 lg:w-1/2 shadow-lg rounded-md bg-white">
                    <div class="flex justify-between items-center border-b pb-3">
                            <h3 class="text-xl font-medium text-gray-900">User Details</h3>
                            <asp:LinkButton ID="btnCloseDetails" runat="server" OnClick="btnCloseDetails_Click" CssClass="text-gray-400 hover:text-gray-500">
                            <svg class="h-6 w-6" fill="none" stroke="currentColor" viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg">
                                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"></path>
                                </svg>
                            </asp:LinkButton>
                        </div>
                        
                    <!-- Tabs -->
                    <div class="border-b border-gray-200 my-4">
                        <nav class="-mb-px flex space-x-8">
                            <asp:LinkButton ID="tabBasicInfo" runat="server" OnClick="tabBasicInfo_Click" CssClass="border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300 whitespace-nowrap py-4 px-1 border-b-2 font-medium text-sm" data-tab="basicInfo">
                                Basic Information
                                </asp:LinkButton>
                            <asp:LinkButton ID="tabOrderHistory" runat="server" OnClick="tabOrderHistory_Click" CssClass="border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300 whitespace-nowrap py-4 px-1 border-b-2 font-medium text-sm" data-tab="orderHistory">
                                    Order History
                                </asp:LinkButton>
                            </nav>
                        </div>
                        
                    <!-- Basic Info Tab Content -->
                    <asp:Panel ID="pnlBasicInfo" runat="server" Visible="true">
                        <div class="grid grid-cols-2 gap-4 mb-4">
                                        <div>
                                <label class="block text-sm font-medium text-gray-700">User ID</label>
                                <asp:Label ID="lblUserId" runat="server" CssClass="mt-1 block w-full text-sm text-gray-900"></asp:Label>
                                        </div>
                                        <div>
                                <label class="block text-sm font-medium text-gray-700">Username</label>
                                <asp:Label ID="lblUsername" runat="server" CssClass="mt-1 block w-full text-sm text-gray-900"></asp:Label>
                                        </div>
                                        <div>
                                <label class="block text-sm font-medium text-gray-700">Email</label>
                                <asp:Label ID="lblEmail" runat="server" CssClass="mt-1 block w-full text-sm text-gray-900"></asp:Label>
                                        </div>
                            <div>
                                <label class="block text-sm font-medium text-gray-700">Role</label>
                                <asp:Label ID="lblRole" runat="server" CssClass="mt-1 block w-full text-sm text-gray-900"></asp:Label>
                                    </div>
                                        <div>
                                <label class="block text-sm font-medium text-gray-700">Status</label>
                                <asp:Label ID="lblStatus" runat="server" CssClass="mt-1 block w-full text-sm text-gray-900"></asp:Label>
                                        </div>
                                        <div>
                                <label class="block text-sm font-medium text-gray-700">Date Created</label>
                                <asp:Label ID="lblDateCreated" runat="server" CssClass="mt-1 block w-full text-sm text-gray-900"></asp:Label>
                                        </div>
                                        <div>
                                <label class="block text-sm font-medium text-gray-700">Last Modified</label>
                                <asp:Label ID="lblDateModified" runat="server" CssClass="mt-1 block w-full text-sm text-gray-900"></asp:Label>
                                        </div>
                                        <div>
                                <label class="block text-sm font-medium text-gray-700">Last Login</label>
                                <asp:Label ID="lblLastLogin" runat="server" CssClass="mt-1 block w-full text-sm text-gray-900"></asp:Label>
                                    </div>
                                </div>
                            </asp:Panel>
                            
                    <!-- Order History Tab Content -->
                    <asp:Panel ID="pnlOrderHistory" runat="server" Visible="false">
                        <asp:GridView ID="gvOrderHistory" runat="server" AutoGenerateColumns="false" CssClass="min-w-full divide-y divide-gray-200" 
                            EmptyDataText="No orders found for this user." GridLines="None">
                            <HeaderStyle CssClass="bg-gray-50 text-left text-xs font-medium text-gray-500 uppercase tracking-wider px-6 py-3" />
                            <RowStyle CssClass="bg-white text-sm px-6 py-4" />
                            <AlternatingRowStyle CssClass="bg-gray-50 text-sm px-6 py-4" />
                                        <Columns>
                                <asp:BoundField DataField="ORDERID" HeaderText="Order ID" ItemStyle-CssClass="px-6 py-4 whitespace-nowrap" />
                                <asp:BoundField DataField="ORDERDATE" HeaderText="Order Date" DataFormatString="{0:MM/dd/yyyy}" ItemStyle-CssClass="px-6 py-4 whitespace-nowrap" />
                                <asp:BoundField DataField="TOTALAMOUNT" HeaderText="Total Amount" DataFormatString="{0:C}" ItemStyle-CssClass="px-6 py-4 whitespace-nowrap" />
                                <asp:TemplateField HeaderText="Status" ItemStyle-CssClass="px-6 py-4 whitespace-nowrap">
                                                <ItemTemplate>
                                        <span class='<%# GetOrderStatusClass(Eval("STATUS").ToString()) %>'>
                                                        <%# Eval("STATUS") %>
                                                    </span>
                                                </ItemTemplate>
                                            </asp:TemplateField>
                                <asp:BoundField DataField="ITEMCOUNT" HeaderText="Items" ItemStyle-CssClass="px-6 py-4 whitespace-nowrap" />
                                        </Columns>
                                    </asp:GridView>
                        <asp:Label ID="lblOrderHistoryDebug" runat="server" CssClass="mt-4 block text-sm text-red-500" Visible="false"></asp:Label>
                            </asp:Panel>
                </div>
            </asp:Panel>
            
            <!-- Reset Password Modal -->
            <asp:Panel ID="pnlPasswordReset" runat="server" CssClass="fixed inset-0 bg-gray-600 bg-opacity-50 overflow-y-auto h-full w-full z-50" Visible="false">
                <div class="relative top-20 mx-auto p-5 border w-11/12 max-w-md shadow-lg rounded-md bg-white">
                    <div class="mt-3 text-center">
                        <h3 class="text-lg leading-6 font-medium text-gray-900">Reset Password</h3>
                        <div class="mt-2 px-7 py-3">
                            <asp:Label ID="lblResetConfirmMessage" runat="server" CssClass="text-sm text-gray-500">
                                Are you sure you want to reset this user's password?
                            </asp:Label>
                            <asp:Literal ID="ltrResetPasswordResult" runat="server"></asp:Literal>
                        </div>
                        
                        <asp:Panel ID="pnlResetConfirm" runat="server" Visible="true">
                            <div class="items-center px-4 py-3">
                                <asp:Button ID="btnCancelReset" runat="server" Text="Cancel" OnClick="btnCancelReset_Click" CssClass="px-4 py-2 bg-gray-200 text-gray-800 text-base font-medium rounded-md w-1/3 shadow-sm hover:bg-gray-300 mr-2" />
                                <asp:Button ID="btnConfirmReset" runat="server" Text="Reset" OnClick="btnConfirmReset_Click" CssClass="px-4 py-2 bg-red-500 text-white text-base font-medium rounded-md w-1/3 shadow-sm hover:bg-red-600" />
                </div>
            </asp:Panel>
            
                        <asp:Panel ID="pnlResetComplete" runat="server" Visible="false">
                            <div class="mt-4">
                                <label class="block text-sm font-medium text-gray-700">New Password</label>
                                <asp:TextBox ID="txtNewPassword" runat="server" ReadOnly="true" CssClass="mt-1 block w-full py-2 px-3 border border-gray-300 bg-gray-100 rounded-md shadow-sm focus:outline-none focus:ring-indigo-500 focus:border-indigo-500 sm:text-sm"></asp:TextBox>
                    </div>
                            <div class="mt-4">
                                <asp:Button ID="btnCopyPassword" runat="server" OnClientClick="copyPassword(); return false;" Text="Copy Password" CssClass="px-4 py-2 bg-blue-500 text-white text-base font-medium rounded-md shadow-sm hover:bg-blue-600 mr-2" />
                                <asp:Button ID="btnCloseReset" runat="server" Text="Close" OnClick="btnCloseReset_Click" CssClass="px-4 py-2 bg-gray-200 text-gray-800 text-base font-medium rounded-md shadow-sm hover:bg-gray-300" />
                            </div>
                        </asp:Panel>
                    </div>
                </div>
            </asp:Panel>
            
            <!-- Toggle Status Modal -->
            <asp:Panel ID="pnlToggleStatus" runat="server" CssClass="fixed inset-0 bg-gray-600 bg-opacity-50 overflow-y-auto h-full w-full z-50" Visible="false">
                <div class="relative top-20 mx-auto p-5 border w-11/12 max-w-md shadow-lg rounded-md bg-white">
                    <div class="mt-3 text-center">
                        <h3 class="text-lg leading-6 font-medium text-gray-900">Confirm Status Change</h3>
                        <div class="mt-2 px-7 py-3">
                            <asp:Literal ID="litStatusMessage" runat="server"></asp:Literal>
                        </div>
                        <div class="items-center px-4 py-3">
                            <asp:Button ID="btnCancelStatus" runat="server" Text="Cancel" OnClick="btnCancelStatus_Click" CssClass="px-4 py-2 bg-gray-200 text-gray-800 text-base font-medium rounded-md w-1/3 shadow-sm hover:bg-gray-300 mr-2" />
                            <asp:Button ID="btnConfirmStatus" runat="server" Text="Confirm" OnClick="btnConfirmStatus_Click" CssClass="px-4 py-2 bg-blue-500 text-white text-base font-medium rounded-md w-1/3 shadow-sm hover:bg-blue-600" />
                        </div>
                    </div>
                </div>
            </asp:Panel>
        </div>
    </form>
    
    <!-- JavaScript for Tab Switching and Password Copy -->
   <script type="text/javascript">
        function showUserTab(tabName) {
            // Get all tab buttons and content panels
            var tabButtons = document.querySelectorAll("[data-tab]");
            var tabContents = document.querySelectorAll("[id$='pnlBasicInfo'], [id$='pnlOrderHistory']");
            
            // Remove active class from all tabs
            tabButtons.forEach(function(btn) {
                btn.classList.remove("border-pink-500", "text-pink-600");
                btn.classList.add("border-transparent", "text-gray-500");
            });
            
            // Hide all tab contents
            tabContents.forEach(function(content) {
                content.style.display = "none";
            });
            
            // Add active class to selected tab
            var selectedTab = document.querySelector("[data-tab='" + tabName + "']");
            if (selectedTab) {
                selectedTab.classList.add("border-pink-500", "text-pink-600");
                selectedTab.classList.remove("border-transparent", "text-gray-500");
            }
            
            // Show selected tab content
            var selectedContent = document.getElementById('<%=pnlBasicInfo.ClientID%>');
            if (tabName === "orderHistory") {
                selectedContent = document.getElementById('<%=pnlOrderHistory.ClientID%>');
            }
            
            if (selectedContent) {
                selectedContent.style.display = "block";
            }
        }
        
        function copyPassword() {
            var passwordField = document.getElementById('<%=txtNewPassword.ClientID%>');
            passwordField.select();
            document.execCommand("copy");
            alert("Password copied to clipboard");
        }
        
        function changePage(pageNumber) {
            // Update UI to show loading state
            var paginationButtons = document.querySelectorAll("[id$='btnFirst'], [id$='btnPrev'], [id$='btnNext'], [id$='btnLast']");
            paginationButtons.forEach(function(btn) {
                btn.classList.add("opacity-50", "cursor-not-allowed");
            });
            
            // Simple approach: Update the hidden field and click the button directly
            document.getElementById('<%=hdnCurrentPage.ClientID%>').value = pageNumber;
            
            // Click the actual button element
            document.getElementById('<%=btnChangePage.ClientID%>').click();
            
            return false; // Prevent default behavior
        }
        
        // Initialize pagination on page load
        document.addEventListener("DOMContentLoaded", function() {
            initializePagination();
            
            // Set up success popup close button
            var closeBtn = document.getElementById('closeSuccessPopup');
            if (closeBtn) {
                closeBtn.addEventListener('click', function() {
                    hideSuccessPopup();
                });
            }
        });
        
        function initializePagination() {
            // Get pagination values from hidden fields
            var currentPage = parseInt(document.getElementById('<%=hdnCurrentPage.ClientID%>').value) || 1;
            var totalPages = parseInt(document.getElementById('<%=hdnTotalPages.ClientID%>').value) || 1;
            
            // Enable/disable navigation buttons based on current page
            document.getElementById('<%=btnFirst.ClientID%>').disabled = (currentPage <= 1);
            document.getElementById('<%=btnPrev.ClientID%>').disabled = (currentPage <= 1);
            document.getElementById('<%=btnNext.ClientID%>').disabled = (currentPage >= totalPages);
            document.getElementById('<%=btnLast.ClientID%>').disabled = (currentPage >= totalPages);
            
            // Apply visual styling for disabled buttons
            applyButtonStyles('<%=btnFirst.ClientID%>', currentPage <= 1);
            applyButtonStyles('<%=btnPrev.ClientID%>', currentPage <= 1);
            applyButtonStyles('<%=btnNext.ClientID%>', currentPage >= totalPages);
            applyButtonStyles('<%=btnLast.ClientID%>', currentPage >= totalPages);
        }
        
        function applyButtonStyles(buttonId, isDisabled) {
            var button = document.getElementById(buttonId);
            if (button) {
                if (isDisabled) {
                    button.classList.add("opacity-50", "cursor-not-allowed");
                    button.classList.remove("hover:bg-gray-50");
           } else {
                    button.classList.remove("opacity-50", "cursor-not-allowed");
                    button.classList.add("hover:bg-gray-50");
                }
            }
        }
        
        // Success popup functions
        function showSuccessPopup(message) {
            var popup = document.getElementById('successPopup');
            var messageElement = document.getElementById('successMessage');
            
            if (popup && messageElement) {
                messageElement.textContent = ' ' + message;
                popup.classList.remove('hidden');
                
                // Auto-hide after 5 seconds
                setTimeout(function() {
                    hideSuccessPopup();
                }, 5000);
            }
        }
        
        function hideSuccessPopup() {
            var popup = document.getElementById('successPopup');
            if (popup) {
                popup.classList.add('hidden');
            }
       }
   </script>
</asp:Content>