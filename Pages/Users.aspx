<%@ Page Title="User Management" Language="C#" MasterPageFile="~/Pages/AdminMaster.Master" AutoEventWireup="true" CodeBehind="Users.aspx.cs" Inherits="OnlinePastryShop.Pages.Users" %>

<asp:Content ID="Content1" ContentPlaceHolderID="AdminContent" runat="server">
    <form runat="server" DefaultButton="btnSearch">
        <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>

        <div class="container mx-auto px-4 py-8">
            <h1 class="text-3xl font-bold text-gray-800 mb-6">User Management</h1>
            
            <asp:Label ID="lblMessage" runat="server" Visible="false" CssClass="text-red-500 mb-4"></asp:Label>
            
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
                        <label for="<%= txtSearch.ClientID %>" class="block text-sm font-medium text-gray-700 mb-1">Search</label>
                        <asp:TextBox ID="txtSearch" runat="server" CssClass="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-[#D43B6A] focus:border-[#D43B6A]" placeholder="Search by username or email"></asp:TextBox>
                    </div>
                    <div>
                        <label for="<%= ddlStatus.ClientID %>" class="block text-sm font-medium text-gray-700 mb-1">Status</label>
                        <asp:DropDownList ID="ddlStatus" runat="server" AutoPostBack="true" OnSelectedIndexChanged="btnSearch_Click" CssClass="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-[#D43B6A] focus:border-[#D43B6A]">
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
            
            <!-- Users Table -->
            <div class="bg-white rounded-lg shadow-md overflow-hidden mb-8">
                <asp:GridView ID="gvUsers" runat="server" AutoGenerateColumns="false" 
                    CssClass="min-w-full divide-y divide-gray-200"
                    GridLines="None"
                    AlternatingRowStyle-CssClass="bg-gray-50"
                    RowStyle-CssClass="bg-white"
                    HeaderStyle-CssClass="bg-gray-50"
                    OnRowCommand="gvUsers_RowCommand"
                    OnRowDataBound="gvUsers_RowDataBound"
                    AllowPaging="true"
                    PageSize="10"
                    OnPageIndexChanging="gvUsers_PageIndexChanging"
                    PagerStyle-CssClass="bg-white border-t border-gray-200 px-4 py-3 sm:px-6 text-center"
                    PagerSettings-Mode="NumericFirstLast"
                    PagerSettings-FirstPageText="First"
                    PagerSettings-LastPageText="Last"
                    PagerSettings-PageButtonCount="5">
                    <Columns>
                        <asp:BoundField DataField="UserId" HeaderText="ID" 
                            HeaderStyle-CssClass="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider"
                            ItemStyle-CssClass="px-6 py-4 whitespace-nowrap text-sm text-gray-500" />
                            
                        <asp:BoundField DataField="Username" HeaderText="Username" 
                            HeaderStyle-CssClass="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider"
                            ItemStyle-CssClass="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900" />
                            
                        <asp:BoundField DataField="Email" HeaderText="Email" 
                            HeaderStyle-CssClass="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider"
                            ItemStyle-CssClass="px-6 py-4 whitespace-nowrap text-sm text-gray-500" />
                            
                        <asp:BoundField DataField="DateCreated" HeaderText="Created On" DataFormatString="{0:MM/dd/yyyy}"
                            HeaderStyle-CssClass="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider"
                            ItemStyle-CssClass="px-6 py-4 whitespace-nowrap text-sm text-gray-500" />
                            
                        <asp:TemplateField HeaderText="Status" 
                            HeaderStyle-CssClass="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider"
                            ItemStyle-CssClass="px-6 py-4 whitespace-nowrap">
                            <ItemTemplate>
                                <span id="statusBadge" runat="server">
                                    <%# Convert.ToBoolean(Eval("IsActive")) ? "Active" : "Inactive" %>
                                </span>
                            </ItemTemplate>
                        </asp:TemplateField>
                        
                        <asp:TemplateField HeaderText="Actions" 
                            HeaderStyle-CssClass="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider"
                            ItemStyle-CssClass="px-6 py-4 whitespace-nowrap text-sm font-medium text-right">
                            <ItemTemplate>
                                <div class="flex justify-end space-x-2">
                                    <asp:LinkButton ID="btnViewDetails" runat="server" 
                                        CommandName="ViewDetails" 
                                        CommandArgument='<%# Eval("UserId") %>'
                                        CssClass="text-indigo-600 hover:text-indigo-900"
                                        ToolTip="View Details">
                                        <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
                                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z" />
                                        </svg>
                                    </asp:LinkButton>
                                    
                                    <asp:LinkButton ID="btnResetPassword" runat="server" 
                                        CommandName="ResetPassword" 
                                        CommandArgument='<%# Eval("UserId") %>'
                                        CssClass="text-blue-600 hover:text-blue-900"
                                        ToolTip="Reset Password">
                                        <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 7a2 2 0 012 2m4 0a6 6 0 01-7.743 5.743L11 17H9v2H7v2H4a1 1 0 01-1-1v-2.586a1 1 0 01.293-.707l5.964-5.964A6 6 0 1121 9z" />
                                        </svg>
                                    </asp:LinkButton>
                                    
                                    <asp:LinkButton ID="btnToggleStatus" runat="server" 
                                        CommandName="ToggleStatus" 
                                        CommandArgument='<%# Eval("UserId") %>'
                                        CssClass='<%# Convert.ToBoolean(Eval("IsActive")) ? "text-red-600 hover:text-red-900" : "text-green-600 hover:text-green-900" %>'
                                        ToolTip='<%# Convert.ToBoolean(Eval("IsActive")) ? "Deactivate User" : "Activate User" %>'>
                                        <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d='<%# Convert.ToBoolean(Eval("IsActive")) 
                                                ? "M18.364 18.364A9 9 0 005.636 5.636m12.728 12.728A9 9 0 015.636 5.636m12.728 12.728L5.636 5.636" 
                                                : "M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" %>' />
                                        </svg>
                                    </asp:LinkButton>
                                </div>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                    <EmptyDataTemplate>
                        <div class="text-center py-6 px-4">
                            <p class="text-gray-500">No users found matching the criteria.</p>
                        </div>
                    </EmptyDataTemplate>
                </asp:GridView>
            </div>
            
            <!-- User Details Modal -->
            <asp:Panel ID="pnlUserDetails" runat="server" Visible="false" CssClass="fixed inset-0 z-50 overflow-y-auto" DefaultButton="">
                <div class="flex items-center justify-center min-h-screen pt-4 px-4 pb-20 text-center">
                    <div class="fixed inset-0 transition-opacity" aria-hidden="true">
                        <div class="absolute inset-0 bg-gray-500 opacity-75"></div>
                    </div>
                    
                    <div class="inline-block align-bottom bg-white rounded-lg text-left overflow-hidden shadow-xl transform transition-all sm:my-8 sm:align-middle sm:max-w-4xl sm:w-full">
                        <div class="flex justify-between items-center px-6 py-4 bg-gray-50">
                            <h3 class="text-xl font-medium text-gray-900">User Details</h3>
                            <asp:LinkButton ID="btnCloseDetails" runat="server" OnClick="btnCloseDetails_Click" CssClass="text-gray-400 hover:text-gray-500">
                                <svg class="h-6 w-6" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
                                </svg>
                            </asp:LinkButton>
                        </div>
                        
                        <!-- Tabs -->
                        <div class="border-b border-gray-200">
                            <div class="flex flex-wrap">
                                <asp:LinkButton ID="tabBasicInfo" runat="server" OnClick="tabBasicInfo_Click" 
                                    CssClass="inline-block p-4 text-[#D43B6A] border-b-2 border-[#D43B6A] rounded-t-lg">
                                    Basic Info
                                </asp:LinkButton>
                                <asp:LinkButton ID="tabOrderHistory" runat="server" OnClick="tabOrderHistory_Click" 
                                    CssClass="inline-block p-4 border-b-2 border-transparent rounded-t-lg hover:text-gray-600 hover:border-gray-300">
                                    Order History
                                </asp:LinkButton>
                            </div>
                        </div>
                        
                        <!-- Tab Content -->
                        <div class="p-6">
                            <!-- Basic Info Panel -->
                            <asp:Panel ID="pnlBasicInfo" runat="server" Visible="true">
                                <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
                                    <div class="space-y-4">
                                        <div>
                                            <h4 class="text-sm font-medium text-gray-500">User ID</h4>
                                            <p class="mt-1 text-gray-900"><asp:Label ID="lblUserId" runat="server"></asp:Label></p>
                                        </div>
                                        <div>
                                            <h4 class="text-sm font-medium text-gray-500">Username</h4>
                                            <p class="mt-1 text-gray-900"><asp:Label ID="lblUsername" runat="server"></asp:Label></p>
                                        </div>
                                        <div>
                                            <h4 class="text-sm font-medium text-gray-500">Email</h4>
                                            <p class="mt-1 text-gray-900"><asp:Label ID="lblEmail" runat="server"></asp:Label></p>
                                        </div>
                                    </div>
                                    <div class="space-y-4">
                                        <div>
                                            <h4 class="text-sm font-medium text-gray-500">Account Status</h4>
                                            <p class="mt-1 text-gray-900"><asp:Label ID="lblStatus" runat="server"></asp:Label></p>
                                        </div>
                                        <div>
                                            <h4 class="text-sm font-medium text-gray-500">Created On</h4>
                                            <p class="mt-1 text-gray-900"><asp:Label ID="lblDateCreated" runat="server"></asp:Label></p>
                                        </div>
                                        <div>
                                            <h4 class="text-sm font-medium text-gray-500">Last Modified</h4>
                                            <p class="mt-1 text-gray-900"><asp:Label ID="lblDateModified" runat="server"></asp:Label></p>
                                        </div>
                                        <div>
                                            <h4 class="text-sm font-medium text-gray-500">Last Login</h4>
                                            <p class="mt-1 text-gray-900"><asp:Label ID="lblLastLogin" runat="server"></asp:Label></p>
                                        </div>
                                    </div>
                                </div>
                            </asp:Panel>
                            
                            <!-- Order History Panel -->
                            <asp:Panel ID="pnlOrderHistory" runat="server" Visible="false">
                                <div class="overflow-x-auto">
                                    <asp:GridView ID="gvOrderHistory" runat="server" AutoGenerateColumns="false" 
                                        CssClass="min-w-full divide-y divide-gray-200"
                                        GridLines="None"
                                        AlternatingRowStyle-CssClass="bg-gray-50"
                                        RowStyle-CssClass="bg-white"
                                        HeaderStyle-CssClass="bg-gray-50">
                                        <Columns>
                                            <asp:BoundField DataField="OrderId" HeaderText="Order ID" 
                                                HeaderStyle-CssClass="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider"
                                                ItemStyle-CssClass="px-6 py-4 whitespace-nowrap text-sm text-gray-900" />
                                                
                                            <asp:BoundField DataField="OrderDate" HeaderText="Order Date" DataFormatString="{0:MM/dd/yyyy hh:mm tt}"
                                                HeaderStyle-CssClass="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider"
                                                ItemStyle-CssClass="px-6 py-4 whitespace-nowrap text-sm text-gray-500" />
                                                
                                            <asp:BoundField DataField="TotalAmount" HeaderText="Total Amount" DataFormatString="{0:C2}" 
                                                HeaderStyle-CssClass="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider"
                                                ItemStyle-CssClass="px-6 py-4 whitespace-nowrap text-sm text-gray-500" />
                                                
                                            <asp:TemplateField HeaderText="Status" 
                                                HeaderStyle-CssClass="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider"
                                                ItemStyle-CssClass="px-6 py-4 whitespace-nowrap">
                                                <ItemTemplate>
                                                    <span class='<%# GetStatusClass(Eval("Status") != null ? Eval("Status").ToString() : "") %>'>
                                                        <%# Eval("Status") %>
                                                    </span>
                                                </ItemTemplate>
                                            </asp:TemplateField>
                                        </Columns>
                                        <EmptyDataTemplate>
                                            <div class="text-center py-6 px-4">
                                                <p class="text-gray-500">No order history found for this user.</p>
                                            </div>
                                        </EmptyDataTemplate>
                                    </asp:GridView>
                                </div>
                            </asp:Panel>
                        </div>
                    </div>
                </div>
            </asp:Panel>
            
            <!-- Password Reset Confirmation Modal -->
            <asp:Panel ID="pnlPasswordReset" runat="server" Visible="false" CssClass="fixed inset-0 z-50 overflow-y-auto">
                <div class="flex items-center justify-center min-h-screen pt-4 px-4 pb-20 text-center">
                    <div class="fixed inset-0 transition-opacity" aria-hidden="true">
                        <div class="absolute inset-0 bg-gray-500 opacity-75"></div>
                    </div>
                    
                    <div class="inline-block align-bottom bg-white rounded-lg text-left overflow-hidden shadow-xl transform transition-all sm:my-8 sm:align-middle sm:max-w-lg sm:w-full">
                        <div class="bg-white px-4 pt-5 pb-4 sm:p-6 sm:pb-4">
                            <div class="sm:flex sm:items-start">
                                <div class="mx-auto flex-shrink-0 flex items-center justify-center h-12 w-12 rounded-full bg-yellow-100 sm:mx-0 sm:h-10 sm:w-10">
                                    <svg class="h-6 w-6 text-yellow-600" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
                                    </svg>
                                </div>
                                <div class="mt-3 text-center sm:mt-0 sm:ml-4 sm:text-left">
                                    <h3 class="text-lg leading-6 font-medium text-gray-900">Reset User Password</h3>
                                    <div class="mt-2">
                                        <p class="text-sm text-gray-500">
                                            Are you sure you want to reset the password for user <strong><asp:Label ID="lblResetUsername" runat="server"></asp:Label></strong>? A new random password will be generated and can be shared with the user.
                                        </p>
                                    </div>
                                </div>
                            </div>
                        </div>
                        <div class="bg-gray-50 px-4 py-3 sm:px-6 sm:flex sm:flex-row-reverse">
                            <asp:Button ID="btnConfirmReset" runat="server" Text="Reset Password" OnClick="btnConfirmReset_Click"
                                CssClass="w-full inline-flex justify-center rounded-md border border-transparent shadow-sm px-4 py-2 bg-yellow-600 text-base font-medium text-white hover:bg-yellow-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-yellow-500 sm:ml-3 sm:w-auto sm:text-sm" />
                            <asp:Button ID="btnClosePasswordReset" runat="server" Text="Cancel" OnClick="btnClosePasswordReset_Click"
                                CssClass="mt-3 w-full inline-flex justify-center rounded-md border border-gray-300 shadow-sm px-4 py-2 bg-white text-base font-medium text-gray-700 hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 sm:mt-0 sm:ml-3 sm:w-auto sm:text-sm" />
                        </div>
                    </div>
                </div>
            </asp:Panel>
            
            <!-- Status Toggle Confirmation Modal -->
            <asp:Panel ID="pnlToggleStatus" runat="server" Visible="false" CssClass="fixed inset-0 z-50 overflow-y-auto">
                <div class="flex items-center justify-center min-h-screen pt-4 px-4 pb-20 text-center">
                    <div class="fixed inset-0 transition-opacity" aria-hidden="true">
                        <div class="absolute inset-0 bg-gray-500 opacity-75"></div>
                    </div>
                    
                    <div class="inline-block align-bottom bg-white rounded-lg text-left overflow-hidden shadow-xl transform transition-all sm:my-8 sm:align-middle sm:max-w-lg sm:w-full">
                        <div class="bg-white px-4 pt-5 pb-4 sm:p-6 sm:pb-4">
                            <div class="sm:flex sm:items-start">
                                <div class="mx-auto flex-shrink-0 flex items-center justify-center h-12 w-12 rounded-full bg-red-100 sm:mx-0 sm:h-10 sm:w-10">
                                    <svg class="h-6 w-6 text-red-600" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
                                    </svg>
                                </div>
                                <div class="mt-3 text-center sm:mt-0 sm:ml-4 sm:text-left">
                                    <h3 class="text-lg leading-6 font-medium text-gray-900">
                                        <asp:Literal ID="litStatusAction" runat="server">Toggle User Status</asp:Literal>
                                    </h3>
                                    <div class="mt-2">
                                        <p class="text-sm text-gray-500">
                                            <asp:Literal ID="litStatusMessage" runat="server">Are you sure you want to change this user's status?</asp:Literal>
                                        </p>
                                    </div>
                                </div>
                            </div>
                        </div>
                        <div class="bg-gray-50 px-4 py-3 sm:px-6 sm:flex sm:flex-row-reverse">
                            <asp:Button ID="btnConfirmToggle" runat="server" Text="Toggle Status" OnClick="btnConfirmToggle_Click"
                                CssClass="w-full inline-flex justify-center rounded-md border border-transparent shadow-sm px-4 py-2 bg-red-600 text-base font-medium text-white hover:bg-red-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-red-500 sm:ml-3 sm:w-auto sm:text-sm" />
                            <asp:Button ID="btnCloseToggleStatus" runat="server" Text="Cancel" OnClick="btnCloseToggleStatus_Click"
                                CssClass="mt-3 w-full inline-flex justify-center rounded-md border border-gray-300 shadow-sm px-4 py-2 bg-white text-base font-medium text-gray-700 hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 sm:mt-0 sm:ml-3 sm:w-auto sm:text-sm" />
                        </div>
                    </div>
                </div>
            </asp:Panel>
            
            <!-- Toast Message -->
            <asp:Panel ID="pnlToast" runat="server" CssClass="fixed bottom-4 right-4 px-6 py-4 rounded-lg shadow-lg bg-green-500" Style="display: none;">
                <div class="flex items-center text-white">
                    <span><asp:Label ID="lblToastMessage" runat="server"></asp:Label></span>
                    <button type="button" class="ml-4 text-white hover:text-gray-100" onclick="this.parentElement.parentElement.style.display='none'">
                        <svg class="h-4 w-4" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
                        </svg>
                    </button>
                </div>
            </asp:Panel>
        </div>
    </form>
   <script type="text/javascript">
    // Global variables
    let searchTimeout;
    let currentUsersLoading = false;

    // Initialize event listeners when DOM is loaded
    document.addEventListener('DOMContentLoaded', function() {
        console.log('DOM Loaded - Setting up search functionality');
        setupSearchEvents();
    });

    // Setup event listeners for search functionality
    function setupSearchEvents() {
        // Get the search text box
        var searchBox = document.getElementById('<%= txtSearch.ClientID %>');
        var statusDropdown = document.getElementById('<%= ddlStatus.ClientID %>');
        var searchButton = document.getElementById('<%= btnSearch.ClientID %>');
        var resetButton = document.getElementById('<%= btnReset.ClientID %>');
        
        if (searchBox) {
            console.log('Found search box: ' + searchBox.id);
            // Add event listener for Enter key only (no live search)
            searchBox.addEventListener('keydown', function(e) {
                // If user presses Enter, trigger search immediately
                if (e.key === 'Enter') {
                    console.log('Enter key pressed - triggering search');
                    e.preventDefault(); // Prevent form submission
                    searchUsers();
                    return;
                }
            });
        } else {
            console.error('Search box not found');
        }
        
        // Remove the live search functionality for status dropdown
        // Only keep the search button functionality
        
        if (searchButton) {
            console.log('Found search button: ' + searchButton.id);
            // Replace default click behavior with our client-side handler
            searchButton.addEventListener('click', function(e) {
                console.log('Search button clicked');
                e.preventDefault(); // Prevent default form submission
                searchUsers();
            });
        } else {
            console.error('Search button not found');
        }
        
        if (resetButton) {
            console.log('Found reset button: ' + resetButton.id);
            resetButton.addEventListener('click', function(e) {
                console.log('Reset button clicked');
                // Allow default behavior for reset
            });
        }
    }

    // Function to search users
    function searchUsers() {
        if (currentUsersLoading) {
            console.log('Search already in progress, ignoring this request');
            return;
        }
        
        currentUsersLoading = true;
        console.log('Performing user search');
        
        var searchText = document.getElementById('<%= txtSearch.ClientID %>').value || '';
        var statusValue = document.getElementById('<%= ddlStatus.ClientID %>').value || 'true';
        
        console.log('Search params:', { searchText, statusValue });
        
        // Show loading message in GridView
        var gridView = document.querySelector('[id$="gvUsers"]');
        if (gridView) {
            // Set a loading indicator for the grid
            if (gridView.getElementsByTagName('tbody').length > 0) {
                var tbody = gridView.getElementsByTagName('tbody')[0];
                tbody.innerHTML = '<tr><td colspan="6" class="text-center py-4">Loading users...</td></tr>';
            }
        }
        
        // Call server-side search using __doPostBack
        __doPostBack('<%= btnSearch.UniqueID %>', '');

           // Reset loading flag after a timeout
           setTimeout(function () {
               currentUsersLoading = false;
           }, 5000); // 5 second timeout
       }
   </script>
 
</asp:Content>

