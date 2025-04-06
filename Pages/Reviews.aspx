<%@ Page Title="Product Reviews" Language="C#" MasterPageFile="~/Pages/AdminMaster.Master" AutoEventWireup="true" CodeBehind="Reviews.aspx.cs" Inherits="OnlinePastryShop.Pages.Reviews" %>

<asp:Content ID="Content1" ContentPlaceHolderID="AdminContent" runat="server">
    <div class="container mx-auto px-4 py-8">
        <div class="flex justify-between items-center mb-6">
            <h1 class="text-2xl font-semibold text-gray-800">Product Reviews</h1>
            <div class="flex gap-2">
                <asp:DropDownList ID="ddlFilterStatus" runat="server" CssClass="px-4 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-[#D43B6A] focus:border-transparent" AutoPostBack="true" OnSelectedIndexChanged="ddlFilterStatus_SelectedIndexChanged">
                    <asp:ListItem Text="All Reviews" Value="All" Selected="True"></asp:ListItem>
                    <asp:ListItem Text="Pending Approval" Value="Pending"></asp:ListItem>
                    <asp:ListItem Text="Approved" Value="Approved"></asp:ListItem>
                    <asp:ListItem Text="Rejected" Value="Rejected"></asp:ListItem>
                </asp:DropDownList>
                <asp:TextBox ID="txtSearch" runat="server" placeholder="Search reviews..." CssClass="px-4 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-[#D43B6A] focus:border-transparent"></asp:TextBox>
                <asp:Button ID="btnSearch" runat="server" Text="Search" CssClass="bg-[#D43B6A] hover:bg-[#9B274F] text-white font-bold py-2 px-4 rounded focus:outline-none focus:shadow-outline transition duration-300 ease-in-out" OnClick="btnSearch_Click" />
            </div>
        </div>

        <div class="bg-white shadow-md rounded-lg overflow-hidden">
            <asp:GridView ID="gvReviews" runat="server" AutoGenerateColumns="False" 
                CssClass="min-w-full divide-y divide-gray-200" 
                EmptyDataText="No reviews found." 
                OnRowCommand="gvReviews_RowCommand"
                OnRowDataBound="gvReviews_RowDataBound"
                AllowPaging="True" 
                PageSize="10" 
                OnPageIndexChanging="gvReviews_PageIndexChanging">
                <HeaderStyle CssClass="px-6 py-3 bg-gray-50 text-left text-xs font-medium text-gray-500 uppercase tracking-wider" />
                <RowStyle CssClass="px-6 py-4 whitespace-nowrap" />
                <AlternatingRowStyle CssClass="px-6 py-4 whitespace-nowrap bg-gray-50" />
                <Columns>
                    <asp:TemplateField HeaderText="ID">
                        <ItemTemplate>
                            <span class="text-sm text-gray-900"><%# Eval("ReviewId") %></span>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Product">
                        <ItemTemplate>
                            <div class="flex items-center">
                                <div class="h-10 w-10 flex-shrink-0">
                                    <img class="h-10 w-10 rounded-full object-cover" src='<%# Eval("ProductImage") %>' alt='<%# Eval("ProductName") %>'>
                                </div>
                                <div class="ml-4">
                                    <div class="text-sm font-medium text-gray-900"><%# Eval("ProductName") %></div>
                                </div>
                            </div>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="User">
                        <ItemTemplate>
                            <div class="text-sm text-gray-900"><%# Eval("UserName") %></div>
                            <div class="text-sm text-gray-500"><%# Eval("Email") %></div>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Rating">
                        <ItemTemplate>
                            <div class="flex items-center">
                                <%# GetStarRating(Convert.ToInt32(Eval("Rating"))) %>
                                <span class="ml-2 text-sm text-gray-600"><%# Eval("Rating") %>/5</span>
                            </div>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Review">
                        <ItemTemplate>
                            <p class="text-sm text-gray-700"><%# Eval("ReviewText") %></p>
                            <p class="text-xs text-gray-500 mt-1"><%# Convert.ToDateTime(Eval("ReviewDate")).ToString("MMM dd, yyyy") %></p>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Status">
                        <ItemTemplate>
                            <span class='<%# GetStatusCssClass(Convert.ToBoolean(Eval("IsApproved"))) %>'>
                                <%# GetStatusText(Convert.ToBoolean(Eval("IsApproved"))) %>
                            </span>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Actions">
                        <ItemTemplate>
                            <div class="flex space-x-2">
                                <asp:LinkButton ID="lnkApprove" runat="server" CommandName="ApproveReview" CommandArgument='<%# Eval("ReviewId") %>'
                                    CssClass='<%# Convert.ToBoolean(Eval("IsApproved")) ? "hidden" : "text-green-600 hover:text-green-900" %>'
                                    ToolTip="Approve Review">
                                    <i class="fas fa-check-circle"></i>
                                </asp:LinkButton>
                                <asp:LinkButton ID="lnkReject" runat="server" CommandName="RejectReview" CommandArgument='<%# Eval("ReviewId") %>'
                                    CssClass='<%# Convert.ToBoolean(Eval("IsApproved")) ? "text-red-600 hover:text-red-900" : "hidden" %>'
                                    ToolTip="Reject Review">
                                    <i class="fas fa-times-circle"></i>
                                </asp:LinkButton>
                                <asp:LinkButton ID="lnkDelete" runat="server" CommandName="DeleteReview" CommandArgument='<%# Eval("ReviewId") %>'
                                    CssClass="text-red-600 hover:text-red-900" ToolTip="Delete Review"
                                    OnClientClick="return confirm('Are you sure you want to delete this review?');">
                                    <i class="fas fa-trash-alt"></i>
                                </asp:LinkButton>
                            </div>
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
                <PagerStyle CssClass="px-3 py-3 border-t border-gray-200 bg-white flex items-center justify-between" />
            </asp:GridView>
        </div>
    </div>
</asp:Content> 