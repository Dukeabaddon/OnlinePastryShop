<%@ Page Title="Newsletter Management" Language="C#" MasterPageFile="~/Pages/AdminMaster.Master" AutoEventWireup="true" CodeBehind="Newsletter.aspx.cs" Inherits="OnlinePastryShop.Pages.Newsletter" %>

<asp:Content ID="Content1" ContentPlaceHolderID="AdminContent" runat="server">
    <div class="container mx-auto px-4 py-8">
        <div class="flex justify-between items-center mb-6">
            <h1 class="text-2xl font-semibold text-gray-800">Newsletter Management</h1>
            <div class="flex gap-2">
                <asp:DropDownList ID="ddlFilterStatus" runat="server" CssClass="px-4 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-[#D43B6A] focus:border-transparent" AutoPostBack="true" OnSelectedIndexChanged="ddlFilterStatus_SelectedIndexChanged">
                    <asp:ListItem Text="All Subscribers" Value="All" Selected="True"></asp:ListItem>
                    <asp:ListItem Text="Active" Value="Active"></asp:ListItem>
                    <asp:ListItem Text="Inactive" Value="Inactive"></asp:ListItem>
                </asp:DropDownList>
                <asp:TextBox ID="txtSearch" runat="server" placeholder="Search email..." CssClass="px-4 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-[#D43B6A] focus:border-transparent"></asp:TextBox>
                <asp:Button ID="btnSearch" runat="server" Text="Search" CssClass="bg-[#D43B6A] hover:bg-[#9B274F] text-white font-bold py-2 px-4 rounded focus:outline-none focus:shadow-outline transition duration-300 ease-in-out" OnClick="btnSearch_Click" />
            </div>
        </div>
        
        <!-- Statistics Cards -->
        <div class="grid grid-cols-1 md:grid-cols-3 gap-6 mb-6">
            <div class="bg-white rounded-lg shadow p-6">
                <div class="flex justify-between items-start">
                    <div>
                        <h3 class="text-sm font-medium text-gray-500">Total Subscribers</h3>
                        <p class="text-2xl font-bold text-gray-800"><%= TotalSubscribers %></p>
                    </div>
                    <div class="p-3 bg-blue-100 rounded-full">
                        <i class="fas fa-users text-blue-600"></i>
                    </div>
                </div>
                <div class="mt-2">
                    <span class="text-sm text-gray-500">Since inception</span>
                </div>
            </div>
            
            <div class="bg-white rounded-lg shadow p-6">
                <div class="flex justify-between items-start">
                    <div>
                        <h3 class="text-sm font-medium text-gray-500">Active Subscribers</h3>
                        <p class="text-2xl font-bold text-gray-800"><%= ActiveSubscribers %></p>
                    </div>
                    <div class="p-3 bg-green-100 rounded-full">
                        <i class="fas fa-envelope-open text-green-600"></i>
                    </div>
                </div>
                <div class="mt-2">
                    <span class="text-sm text-gray-500"><%= ActivePercentage %>% of total</span>
                </div>
            </div>
            
            <div class="bg-white rounded-lg shadow p-6">
                <div class="flex justify-between items-start">
                    <div>
                        <h3 class="text-sm font-medium text-gray-500">New Subscribers</h3>
                        <p class="text-2xl font-bold text-gray-800"><%= NewSubscribers %></p>
                    </div>
                    <div class="p-3 bg-purple-100 rounded-full">
                        <i class="fas fa-user-plus text-purple-600"></i>
                    </div>
                </div>
                <div class="mt-2">
                    <span class="text-sm text-gray-500">Last 30 days</span>
                </div>
            </div>
        </div>
        
        <!-- Action Buttons -->
        <div class="flex gap-4 mb-6">
            <asp:Button ID="btnExport" runat="server" Text="Export Subscribers" OnClick="btnExport_Click" CssClass="px-4 py-2 bg-gray-200 hover:bg-gray-300 text-gray-800 rounded-md transition duration-300 ease-in-out flex items-center gap-2" />
            <asp:Button ID="btnCreateCampaign" runat="server" Text="Create Campaign" OnClick="btnCreateCampaign_Click" CssClass="px-4 py-2 bg-[#D43B6A] hover:bg-[#9B274F] text-white rounded-md transition duration-300 ease-in-out flex items-center gap-2" />
        </div>
        
        <!-- Subscribers List -->
        <div class="bg-white shadow-md rounded-lg overflow-hidden">
            <div class="p-4 bg-gray-50 border-b border-gray-200">
                <h2 class="text-lg font-medium text-gray-800">Subscribers</h2>
            </div>
            
            <asp:GridView ID="gvSubscribers" runat="server" AutoGenerateColumns="False" 
                CssClass="min-w-full divide-y divide-gray-200" 
                EmptyDataText="No subscribers found." 
                OnRowCommand="gvSubscribers_RowCommand"
                AllowPaging="True" 
                PageSize="10" 
                OnPageIndexChanging="gvSubscribers_PageIndexChanging">
                <HeaderStyle CssClass="px-6 py-3 bg-gray-50 text-left text-xs font-medium text-gray-500 uppercase tracking-wider" />
                <RowStyle CssClass="px-6 py-4 whitespace-nowrap" />
                <AlternatingRowStyle CssClass="px-6 py-4 whitespace-nowrap bg-gray-50" />
                <Columns>
                    <asp:TemplateField HeaderText="ID">
                        <ItemTemplate>
                            <span class="text-sm text-gray-900"><%# Eval("SubscriberId") %></span>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Email">
                        <ItemTemplate>
                            <span class="text-sm text-gray-900"><%# Eval("Email") %></span>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Name">
                        <ItemTemplate>
                            <span class="text-sm text-gray-900"><%# Eval("Name") %></span>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Subscription Date">
                        <ItemTemplate>
                            <span class="text-sm text-gray-900"><%# Convert.ToDateTime(Eval("SubscriptionDate")).ToString("MMM dd, yyyy") %></span>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Status">
                        <ItemTemplate>
                            <span class='<%# Convert.ToBoolean(Eval("IsActive")) ? "inline-flex px-2 py-1 text-xs font-semibold rounded-full bg-green-100 text-green-800" : "inline-flex px-2 py-1 text-xs font-semibold rounded-full bg-red-100 text-red-800" %>'>
                                <%# Convert.ToBoolean(Eval("IsActive")) ? "Active" : "Inactive" %>
                            </span>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Actions">
                        <ItemTemplate>
                            <div class="flex space-x-2">
                                <asp:LinkButton ID="lnkToggleStatus" runat="server" CommandName="ToggleStatus" CommandArgument='<%# Eval("SubscriberId") %>'
                                    CssClass='<%# Convert.ToBoolean(Eval("IsActive")) ? "text-red-600 hover:text-red-900" : "text-green-600 hover:text-green-900" %>'
                                    ToolTip='<%# Convert.ToBoolean(Eval("IsActive")) ? "Deactivate" : "Activate" %>'>
                                    <i class='<%# Convert.ToBoolean(Eval("IsActive")) ? "fas fa-ban" : "fas fa-check-circle" %>'></i>
                                </asp:LinkButton>
                                <asp:LinkButton ID="lnkDelete" runat="server" CommandName="DeleteSubscriber" CommandArgument='<%# Eval("SubscriberId") %>'
                                    CssClass="text-red-600 hover:text-red-900" ToolTip="Delete Subscriber"
                                    OnClientClick="return confirm('Are you sure you want to delete this subscriber?');">
                                    <i class="fas fa-trash-alt"></i>
                                </asp:LinkButton>
                            </div>
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
                <PagerStyle CssClass="px-3 py-3 border-t border-gray-200 bg-white flex items-center justify-between" />
            </asp:GridView>
        </div>
        
        <!-- Add New Subscriber Section -->
        <div class="mt-8 bg-white shadow-md rounded-lg overflow-hidden">
            <div class="p-4 bg-gray-50 border-b border-gray-200">
                <h2 class="text-lg font-medium text-gray-800">Add New Subscriber</h2>
            </div>
            
            <div class="p-6">
                <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
                    <div>
                        <label for="<%= txtNewEmail.ClientID %>" class="block text-sm font-medium text-gray-700 mb-1">Email Address</label>
                        <asp:TextBox ID="txtNewEmail" runat="server" CssClass="w-full px-4 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-[#D43B6A] focus:border-transparent" placeholder="email@example.com"></asp:TextBox>
                        <asp:RequiredFieldValidator ID="rfvEmail" runat="server" ControlToValidate="txtNewEmail" 
                            ErrorMessage="Email is required" ValidationGroup="AddSubscriber"
                            CssClass="text-red-500 text-xs mt-1" Display="Dynamic"></asp:RequiredFieldValidator>
                        <asp:RegularExpressionValidator ID="revEmail" runat="server" ControlToValidate="txtNewEmail"
                            ValidationExpression="^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$"
                            ErrorMessage="Invalid email format" ValidationGroup="AddSubscriber"
                            CssClass="text-red-500 text-xs mt-1" Display="Dynamic"></asp:RegularExpressionValidator>
                    </div>
                    <div>
                        <label for="<%= txtNewName.ClientID %>" class="block text-sm font-medium text-gray-700 mb-1">Name (Optional)</label>
                        <asp:TextBox ID="txtNewName" runat="server" CssClass="w-full px-4 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-[#D43B6A] focus:border-transparent" placeholder="Full Name"></asp:TextBox>
                    </div>
                </div>
                
                <div class="mt-6">
                    <asp:Button ID="btnAddSubscriber" runat="server" Text="Add Subscriber" ValidationGroup="AddSubscriber"
                        CssClass="px-6 py-2 bg-[#D43B6A] hover:bg-[#9B274F] text-white font-medium rounded-md transition duration-300 ease-in-out"
                        OnClick="btnAddSubscriber_Click" />
                    <asp:Label ID="lblAddResult" runat="server" CssClass="ml-4 text-sm"></asp:Label>
                </div>
            </div>
        </div>
    </div>
</asp:Content> 