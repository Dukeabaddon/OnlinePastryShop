<%@ Page Title="Wishlist Management" Language="C#" MasterPageFile="~/Pages/AdminMaster.Master" AutoEventWireup="true" CodeBehind="Wishlist.aspx.cs" Inherits="OnlinePastryShop.Pages.Wishlist" %>

<asp:Content ID="Content1" ContentPlaceHolderID="AdminContent" runat="server">
    <div class="container mx-auto px-4 py-8">
        <div class="flex justify-between items-center mb-6">
            <h1 class="text-2xl font-semibold text-gray-800">Wishlist Management</h1>
            <div class="flex gap-2">
                <asp:TextBox ID="txtSearch" runat="server" placeholder="Search user or product..." CssClass="px-4 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-[#D43B6A] focus:border-transparent"></asp:TextBox>
                <asp:Button ID="btnSearch" runat="server" Text="Search" CssClass="bg-[#D43B6A] hover:bg-[#9B274F] text-white font-bold py-2 px-4 rounded focus:outline-none focus:shadow-outline transition duration-300 ease-in-out" OnClick="btnSearch_Click" />
            </div>
        </div>
        
        <div class="bg-white shadow-md rounded-lg overflow-hidden">
            <div class="flex justify-between items-center p-4 bg-gray-50 border-b border-gray-200">
                <h2 class="text-lg font-medium text-gray-800">Customer Wishlists</h2>
                <div class="flex items-center gap-3">
                    <div class="flex items-center">
                        <span class="text-sm text-gray-600 mr-2">Total Items:</span>
                        <span class="px-2 py-1 bg-gray-200 rounded-md text-sm font-medium"><%= TotalWishlistItems %></span>
                    </div>
                    <div class="flex items-center">
                        <span class="text-sm text-gray-600 mr-2">Unique Users:</span>
                        <span class="px-2 py-1 bg-gray-200 rounded-md text-sm font-medium"><%= UniqueUsersCount %></span>
                    </div>
                </div>
            </div>
            
            <asp:GridView ID="gvWishlist" runat="server" AutoGenerateColumns="False" 
                CssClass="min-w-full divide-y divide-gray-200" 
                EmptyDataText="No wishlist items found." 
                OnRowCommand="gvWishlist_RowCommand"
                AllowPaging="True" 
                PageSize="10" 
                OnPageIndexChanging="gvWishlist_PageIndexChanging">
                <HeaderStyle CssClass="px-6 py-3 bg-gray-50 text-left text-xs font-medium text-gray-500 uppercase tracking-wider" />
                <RowStyle CssClass="px-6 py-4 whitespace-nowrap" />
                <AlternatingRowStyle CssClass="px-6 py-4 whitespace-nowrap bg-gray-50" />
                <Columns>
                    <asp:TemplateField HeaderText="ID">
                        <ItemTemplate>
                            <span class="text-sm text-gray-900"><%# Eval("WishlistId") %></span>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="User">
                        <ItemTemplate>
                            <div class="flex items-center">
                                <div class="ml-4">
                                    <div class="text-sm font-medium text-gray-900"><%# Eval("UserName") %></div>
                                    <div class="text-xs text-gray-500"><%# Eval("Email") %></div>
                                </div>
                            </div>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Product">
                        <ItemTemplate>
                            <div class="flex items-center">
                                <div class="h-10 w-10 flex-shrink-0">
                                    <img class="h-10 w-10 rounded-full object-cover" src='<%# $"~/Images/Products/{Eval("ProductImage")}" %>' alt='<%# Eval("ProductName") %>'>
                                </div>
                                <div class="ml-4">
                                    <div class="text-sm font-medium text-gray-900"><%# Eval("ProductName") %></div>
                                    <div class="text-xs text-gray-500"><%# string.Format("{0:C}", Eval("Price")) %></div>
                                </div>
                            </div>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Date Added">
                        <ItemTemplate>
                            <div class="text-sm text-gray-900"><%# Convert.ToDateTime(Eval("DateAdded")).ToString("MMM dd, yyyy") %></div>
                            <div class="text-xs text-gray-500"><%# Convert.ToDateTime(Eval("DateAdded")).ToString("h:mm tt") %></div>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Status">
                        <ItemTemplate>
                            <span class='<%# Convert.ToBoolean(Eval("InStock")) ? "inline-flex px-2 py-1 text-xs font-semibold rounded-full bg-green-100 text-green-800" : "inline-flex px-2 py-1 text-xs font-semibold rounded-full bg-red-100 text-red-800" %>'>
                                <%# Convert.ToBoolean(Eval("InStock")) ? "In Stock" : "Out of Stock" %>
                            </span>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Actions">
                        <ItemTemplate>
                            <div class="flex space-x-2">
                                <asp:LinkButton ID="lnkViewProduct" runat="server" CommandName="ViewProduct" CommandArgument='<%# Eval("ProductId") %>'
                                    CssClass="text-blue-600 hover:text-blue-900" ToolTip="View Product">
                                    <i class="fas fa-eye"></i>
                                </asp:LinkButton>
                                <asp:LinkButton ID="lnkEmailUser" runat="server" CommandName="EmailUser" CommandArgument='<%# $"{Eval("UserId")}|{Eval("ProductId")}" %>'
                                    CssClass="text-green-600 hover:text-green-900" ToolTip="Email User">
                                    <i class="fas fa-envelope"></i>
                                </asp:LinkButton>
                                <asp:LinkButton ID="lnkDelete" runat="server" CommandName="DeleteWishlistItem" CommandArgument='<%# Eval("WishlistId") %>'
                                    CssClass="text-red-600 hover:text-red-900" ToolTip="Delete Wishlist Item"
                                    OnClientClick="return confirm('Are you sure you want to delete this wishlist item?');">
                                    <i class="fas fa-trash-alt"></i>
                                </asp:LinkButton>
                            </div>
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
                <PagerStyle CssClass="px-3 py-3 border-t border-gray-200 bg-white flex items-center justify-between" />
            </asp:GridView>
        </div>
        
        <!-- Popular Wishlist Items Section -->
        <div class="mt-8 bg-white shadow-md rounded-lg overflow-hidden">
            <div class="p-4 bg-gray-50 border-b border-gray-200">
                <h2 class="text-lg font-medium text-gray-800">Popular Wishlist Items</h2>
            </div>
            
            <div class="p-6 grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6">
                <asp:Repeater ID="rptPopularWishlistItems" runat="server">
                    <ItemTemplate>
                        <div class="flex flex-col bg-white border border-gray-200 rounded-lg overflow-hidden hover:shadow-lg transition-shadow duration-300">
                            <div class="relative h-48 bg-gray-200">
                                <img src='<%# $"~/Images/Products/{Eval("ProductImage")}" %>' alt='<%# Eval("ProductName") %>' class="w-full h-full object-cover">
                                <div class="absolute top-2 right-2 bg-[#D43B6A] text-white text-xs font-bold px-2 py-1 rounded-full">
                                    <%# Eval("WishlistCount") %> users
                                </div>
                            </div>
                            <div class="p-4">
                                <h3 class="font-medium text-gray-900 truncate"><%# Eval("ProductName") %></h3>
                                <p class="text-sm text-gray-500 mt-1 truncate"><%# Eval("Description") %></p>
                                <div class="flex justify-between items-center mt-3">
                                    <span class="text-[#D43B6A] font-bold"><%# string.Format("{0:C}", Eval("Price")) %></span>
                                    <a href='<%# $"Products.aspx?id={Eval("ProductId")}" %>' class="text-xs bg-gray-100 hover:bg-gray-200 text-gray-800 font-medium py-1 px-2 rounded transition duration-300 ease-in-out">
                                        View Details
                                    </a>
                                </div>
                            </div>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>
            </div>
        </div>
    </div>
</asp:Content> 