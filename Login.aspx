<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Login.aspx.cs" Inherits="Login" MasterPageFile="~/MasterPage.master" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
    <title>Login - Online Pastry Shop</title>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
    <div class="container my-5">
        <div class="row justify-content-center">
            <div class="col-md-6">
                <div class="card shadow-lg">
                    <div class="card-header bg-primary text-white">
                        <h2 class="text-center mb-0">Login</h2>
                    </div>
                    <div class="card-body p-4">
                        <div class="text-center mb-4">
                            <p>Please enter your credentials to access your account</p>
                        </div>
                        
                        <asp:Panel ID="pnlError" runat="server" CssClass="alert alert-danger" Visible="false">
                            <asp:Literal ID="litError" runat="server"></asp:Literal>
                        </asp:Panel>
                        
                        <div class="form-group mb-3">
                            <label for="txtEmail" class="form-label">Email Address</label>
                            <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control" placeholder="Enter your email" TextMode="Email" required></asp:TextBox>
                            <asp:RequiredFieldValidator ID="rfvEmail" runat="server" 
                                ControlToValidate="txtEmail" 
                                ErrorMessage="Email is required" 
                                CssClass="text-danger"
                                Display="Dynamic">
                            </asp:RequiredFieldValidator>
                        </div>
                        
                        <div class="form-group mb-3">
                            <label for="txtPassword" class="form-label">Password</label>
                            <asp:TextBox ID="txtPassword" runat="server" CssClass="form-control" placeholder="Enter your password" TextMode="Password" required></asp:TextBox>
                            <asp:RequiredFieldValidator ID="rfvPassword" runat="server" 
                                ControlToValidate="txtPassword" 
                                ErrorMessage="Password is required" 
                                CssClass="text-danger"
                                Display="Dynamic">
                            </asp:RequiredFieldValidator>
                        </div>
                        
                        <div class="form-group mb-4">
                            <div class="form-check">
                                <asp:CheckBox ID="chkRememberMe" runat="server" CssClass="form-check-input" />
                                <label class="form-check-label" for="chkRememberMe">Remember Me</label>
                            </div>
                        </div>
                        
                        <div class="d-grid gap-2">
                            <asp:Button ID="btnLogin" runat="server" Text="Login" CssClass="btn btn-primary btn-lg" OnClick="btnLogin_Click" />
                        </div>
                        
                        <div class="mt-4 text-center">
                            <p>Don't have an account? <a href="Register.aspx" class="text-primary">Register here</a></p>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</asp:Content> 