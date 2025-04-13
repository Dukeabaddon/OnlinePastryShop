<%@ Page Title="Login" Language="C#" MasterPageFile="~/Pages/Site.Master" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="OnlinePastryShop.Pages.Login" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        /* Modal styles */
        .auth-container {
            min-height: calc(100vh - 200px);
            display: flex;
            align-items: center;
            justify-content: center;
            padding: 2rem 1rem;
        }
        
        .auth-modal {
            background-color: white;
            border-radius: 1rem;
            box-shadow: 0 10px 25px rgba(0, 0, 0, 0.1);
            width: 100%;
            max-width: 500px;
            overflow: hidden;
        }
        
        .auth-header {
            text-align: center;
            padding: 1.5rem 1rem;
        }
        
        .auth-logo {
            max-width: 120px;
            margin: 0 auto 1rem;
        }
        
        .auth-tabs {
            display: flex;
            border-bottom: 1px solid #e5e7eb;
        }
        
        .auth-tab {
            flex: 1;
            text-align: center;
            padding: 1rem;
            cursor: pointer;
            transition: all 0.3s ease;
            font-weight: 500;
            color: #6b7280;
            position: relative;
        }
        
        .auth-tab.active {
            color: #96744F;
        }
        
        .auth-tab.active::after {
            content: '';
            position: absolute;
            bottom: -1px;
            left: 0;
            right: 0;
            height: 2px;
            background-color: #96744F;
        }
        
        .auth-body {
            padding: 2rem;
        }
        
        .auth-form {
            display: none;
        }
        
        .auth-form.active {
            display: block;
        }
        
        .form-group {
            margin-bottom: 1.5rem;
        }
        
        .form-label {
            display: block;
            margin-bottom: 0.5rem;
            font-weight: 500;
            color: #4b5563;
        }
        
        .form-control {
            width: 100%;
            padding: 0.75rem 1rem;
            border: 1px solid #d1d5db;
            border-radius: 0.5rem;
            font-size: 1rem;
            transition: border-color 0.15s ease-in-out;
        }
        
        .form-control:focus {
            outline: none;
            border-color: #96744F;
            box-shadow: 0 0 0 3px rgba(150, 116, 79, 0.2);
        }
        
        .form-control.is-invalid {
            border-color: #ef4444;
        }
        
        .invalid-feedback {
            color: #ef4444;
            font-size: 0.875rem;
            margin-top: 0.25rem;
            display: none;
        }
        
        .is-invalid ~ .invalid-feedback {
            display: block;
        }
        
        .valid-feedback {
            color: #10b981;
            font-size: 0.875rem;
            margin-top: 0.25rem;
            display: none;
        }
        
        .is-valid ~ .valid-feedback {
            display: block;
        }
        
        .form-control.is-valid {
            border-color: #10b981;
        }
        
        .form-check {
            display: flex;
            align-items: center;
            margin-bottom: 1rem;
        }
        
        .form-check-input {
            width: 1rem;
            height: 1rem;
            margin-right: 0.5rem;
            cursor: pointer;
        }
        
        .form-check-label {
            font-size: 0.875rem;
            color: #6b7280;
            cursor: pointer;
        }
        
        .auth-btn {
            display: block;
            width: 100%;
            padding: 0.75rem 1rem;
            font-size: 1rem;
            font-weight: 500;
            text-align: center;
            border: none;
            border-radius: 0.5rem;
            background-color: #96744F;
            color: white;
            cursor: pointer;
            transition: background-color 0.15s ease-in-out;
        }
        
        .auth-btn:hover {
            background-color: #7d6142;
        }
        
        .auth-btn:disabled {
            background-color: #cbd5e0;
            cursor: not-allowed;
        }
        
        .auth-footer {
            text-align: center;
            margin-top: 1.5rem;
            font-size: 0.875rem;
            color: #6b7280;
        }
        
        .auth-link {
            color: #96744F;
            text-decoration: none;
            font-weight: 500;
            transition: color 0.15s ease-in-out;
        }
        
        .auth-link:hover {
            color: #7d6142;
            text-decoration: underline;
        }
        
        .password-toggle {
            position: relative;
        }
        
        .password-toggle-btn {
            position: absolute;
            right: 1rem;
            top: 50%;
            transform: translateY(-50%);
            background: none;
            border: none;
            color: #6b7280;
            cursor: pointer;
        }
        
        /* Loading indicator styles */
        .relative {
            position: relative;
        }
        
        .absolute {
            position: absolute;
        }
        
        .right-3 {
            right: 0.75rem;
        }
        
        .top-1\/2 {
            top: 50%;
        }
        
        .transform {
            transform: translateY(-50%);
        }
        
        .animate-spin {
            animation: spin 1s linear infinite;
        }
        
        /* Form validation animation */
        @keyframes shake {
            0%, 100% { transform: translateX(0); }
            10%, 30%, 50%, 70%, 90% { transform: translateX(-5px); }
            20%, 40%, 60%, 80% { transform: translateX(5px); }
        }
        
        .shake {
            animation: shake 0.6s cubic-bezier(.36,.07,.19,.97) both;
        }
        
        /* Loading spinner */
        .spinner {
            width: 1.5rem;
            height: 1.5rem;
            border: 2px solid rgba(255, 255, 255, 0.3);
            border-radius: 50%;
            border-top-color: white;
            animation: spin 0.8s linear infinite;
            margin-right: 0.5rem;
            display: inline-block;
        }
        
        @keyframes spin {
            to { transform: rotate(360deg); }
        }
        
        .btn-loading {
            display: flex;
            align-items: center;
            justify-content: center;
        }
        
        /* Toast notification */
        .toast {
            position: fixed;
            bottom: 1rem;
            right: 1rem;
            padding: 1rem;
            border-radius: 0.5rem;
            background-color: white;
            box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
            z-index: 50;
            transition: transform 0.3s ease, opacity 0.3s ease;
            transform: translateY(100%);
            opacity: 0;
        }
        
        .toast.show {
            transform: translateY(0);
            opacity: 1;
        }
        
        .toast-success {
            border-left: 4px solid #10b981;
        }
        
        .toast-error {
            border-left: 4px solid #ef4444;
        }
        
        .toast-icon {
            margin-right: 0.5rem;
            display: inline-flex;
        }
        
        .toast-message {
            display: flex;
            align-items: center;
        }
        
        /* Responsive adjustments */
        @media (max-width: 640px) {
            .auth-modal {
                max-width: 100%;
                border-radius: 0;
                box-shadow: none;
            }
            
            .auth-body {
                padding: 1.5rem;
            }
        }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <!-- ScriptManager for AJAX functionality -->
    <asp:ScriptManager ID="ScriptManager1" runat="server" EnablePageMethods="true"></asp:ScriptManager>
    
    <div class="auth-container">
        <div class="auth-modal">
            <!-- Modal Header with Logo -->
            <div class="auth-header">
                <div class="auth-logo">
                    <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="#96744F" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                        <path d="M18 6h-5c-1.1 0-2 .9-2 2v2H6c-1.1 0-2 .9-2 2v2.5c0 .8.7 1.5 1.5 1.5h5c.8 0 1.5-.7 1.5-1.5V11h5c1.1 0 2-.9 2-2V8c0-1.1-.9-2-2-2z"/>
                        <path d="M4 12h16"/>
                        <path d="M11 15h1"/>
                        <path d="M2 20h20"/>
                    </svg>
                </div>
                <h2 style="font-family: 'Playfair Display', serif; color: #96744F;">Pastry Palace</h2>
                <p class="text-gray-600 text-sm">Sign in to access your account</p>
            </div>
            
            <!-- Tabs -->
            <div class="auth-tabs">
                <div class="auth-tab active" data-target="login-form">Login</div>
                <div class="auth-tab" data-target="register-form">Register</div>
            </div>
            
            <!-- Form Container -->
            <div class="auth-body">
                <!-- Login Form -->
                <div id="login-form" class="auth-form active">
                    <div class="form-group">
                        <label for="txtLoginUsername" class="form-label">Username or Email</label>
                        <asp:TextBox ID="txtLoginUsername" runat="server" CssClass="form-control" placeholder="Enter your username or email"></asp:TextBox>
                        <div class="invalid-feedback">Please enter a valid username or email</div>
                    </div>
                    
                    <div class="form-group">
                        <label for="txtLoginPassword" class="form-label">Password</label>
                        <div class="password-toggle">
                            <asp:TextBox ID="txtLoginPassword" runat="server" TextMode="Password" CssClass="form-control" placeholder="Enter your password"></asp:TextBox>
                            <button type="button" class="password-toggle-btn" data-target="txtLoginPassword">
                                <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" class="bi bi-eye" viewBox="0 0 16 16">
                                    <path d="M16 8s-3-5.5-8-5.5S0 8 0 8s3 5.5 8 5.5S16 8 16 8zM1.173 8a13.133 13.133 0 0 1 1.66-2.043C4.12 4.668 5.88 3.5 8 3.5c2.12 0 3.879 1.168 5.168 2.457A13.133 13.133 0 0 1 14.828 8c-.058.087-.122.183-.195.288-.335.48-.83 1.12-1.465 1.755C11.879 11.332 10.119 12.5 8 12.5c-2.12 0-3.879-1.168-5.168-2.457A13.134 13.134 0 0 1 1.172 8z"/>
                                    <path d="M8 5.5a2.5 2.5 0 1 0 0 5 2.5 2.5 0 0 0 0-5zM4.5 8a3.5 3.5 0 1 1 7 0 3.5 3.5 0 0 1-7 0z"/>
                                </svg>
                            </button>
                        </div>
                        <div class="invalid-feedback">Please enter your password</div>
                    </div>
                    
                    <div class="form-check">
                        <asp:CheckBox ID="chkRememberMe" runat="server" CssClass="form-check-input" />
                        <label for="chkRememberMe" class="form-check-label">Remember me</label>
                    </div>
                    
                    <asp:Button ID="LoginButton" runat="server" Text="Login" CssClass="auth-btn" OnClick="LoginButton_Click" />
                    
                    <div class="auth-footer">
                        <a href="#" class="auth-link" id="forgot-password-link">Forgot your password?</a>
                    </div>
                </div>
                
                <!-- Register Form -->
                <div id="register-form" class="auth-form">
                    <div class="form-group">
                        <label for="txtRegisterFirstname" class="form-label">First Name</label>
                        <asp:TextBox ID="txtRegisterFirstname" runat="server" CssClass="form-control" placeholder="Enter your first name"></asp:TextBox>
                        <div class="invalid-feedback">Please enter your first name</div>
                    </div>
                    
                    <div class="form-group">
                        <label for="txtRegisterLastname" class="form-label">Last Name</label>
                        <asp:TextBox ID="txtRegisterLastname" runat="server" CssClass="form-control" placeholder="Enter your last name"></asp:TextBox>
                        <div class="invalid-feedback">Please enter your last name</div>
                    </div>
                    
                    <div class="form-group">
                        <label for="txtRegisterUsername" class="form-label">Username</label>
                        <asp:TextBox ID="txtRegisterUsername" runat="server" CssClass="form-control" placeholder="Choose a username"></asp:TextBox>
                        <div class="invalid-feedback">Please enter a valid username</div>
                        <div class="valid-feedback">Username is available</div>
                    </div>
                    
                    <div class="form-group">
                        <label for="txtRegisterEmail" class="form-label">Email Address</label>
                        <asp:TextBox ID="txtRegisterEmail" runat="server" TextMode="Email" CssClass="form-control" placeholder="Enter your email address"></asp:TextBox>
                        <div class="invalid-feedback">Please enter a valid email address</div>
                        <div class="valid-feedback">Email is available</div>
                    </div>
                    
                    <div class="form-group">
                        <label for="txtRegisterPhone" class="form-label">Phone Number</label>
                        <asp:TextBox ID="txtRegisterPhone" runat="server" TextMode="Phone" CssClass="form-control" placeholder="Enter your phone number"></asp:TextBox>
                        <div class="invalid-feedback">Please enter a valid phone number</div>
                    </div>
                    
                    <div class="form-group">
                        <label for="txtRegisterPassword" class="form-label">Password</label>
                        <div class="password-toggle">
                            <asp:TextBox ID="txtRegisterPassword" runat="server" TextMode="Password" CssClass="form-control" placeholder="Choose a password"></asp:TextBox>
                            <button type="button" class="password-toggle-btn" data-target="txtRegisterPassword">
                                <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" class="bi bi-eye" viewBox="0 0 16 16">
                                    <path d="M16 8s-3-5.5-8-5.5S0 8 0 8s3 5.5 8 5.5S16 8 16 8zM1.173 8a13.133 13.133 0 0 1 1.66-2.043C4.12 4.668 5.88 3.5 8 3.5c2.12 0 3.879 1.168 5.168 2.457A13.133 13.133 0 0 1 14.828 8c-.058.087-.122.183-.195.288-.335.48-.83 1.12-1.465 1.755C11.879 11.332 10.119 12.5 8 12.5c-2.12 0-3.879-1.168-5.168-2.457A13.134 13.134 0 0 1 1.172 8z"/>
                                    <path d="M8 5.5a2.5 2.5 0 1 0 0 5 2.5 2.5 0 0 0 0-5zM4.5 8a3.5 3.5 0 1 1 7 0 3.5 3.5 0 0 1-7 0z"/>
                                </svg>
                            </button>
                        </div>
                        <div class="invalid-feedback">Password must be at least 6 characters</div>
                    </div>
                    
                    <div class="form-group">
                        <label for="txtRegisterConfirmPassword" class="form-label">Confirm Password</label>
                        <div class="password-toggle">
                            <asp:TextBox ID="txtRegisterConfirmPassword" runat="server" TextMode="Password" CssClass="form-control" placeholder="Confirm your password"></asp:TextBox>
                            <button type="button" class="password-toggle-btn" data-target="txtRegisterConfirmPassword">
                                <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" class="bi bi-eye" viewBox="0 0 16 16">
                                    <path d="M16 8s-3-5.5-8-5.5S0 8 0 8s3 5.5 8 5.5S16 8 16 8zM1.173 8a13.133 13.133 0 0 1 1.66-2.043C4.12 4.668 5.88 3.5 8 3.5c2.12 0 3.879 1.168 5.168 2.457A13.133 13.133 0 0 1 14.828 8c-.058.087-.122.183-.195.288-.335.48-.83 1.12-1.465 1.755C11.879 11.332 10.119 12.5 8 12.5c-2.12 0-3.879-1.168-5.168-2.457A13.134 13.134 0 0 1 1.172 8z"/>
                                    <path d="M8 5.5a2.5 2.5 0 1 0 0 5 2.5 2.5 0 0 0 0-5zM4.5 8a3.5 3.5 0 1 1 7 0 3.5 3.5 0 0 1-7 0z"/>
                                </svg>
                            </button>
                        </div>
                        <div class="invalid-feedback">Passwords do not match</div>
                    </div>
                    
                    <asp:Button ID="RegisterButton" runat="server" Text="Create Account" CssClass="auth-btn" OnClick="RegisterButton_Click" />
                    
                    <div class="auth-footer">
                        Already have an account? <a href="#" class="auth-link" id="switch-to-login">Login here</a>
                    </div>
                </div>
            </div>
        </div>
    </div>
    
    <!-- Toast Notification -->
    <div id="toast" class="toast">
        <div class="toast-message">
            <span class="toast-icon"></span>
            <span id="toast-text"></span>
        </div>
    </div>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="ScriptsContent" runat="server">
    <script type="text/javascript">
        document.addEventListener('DOMContentLoaded', function() {
            // Tab switching
            const tabLinks = document.querySelectorAll('.auth-tab');
            const tabContents = document.querySelectorAll('.auth-form');
            
            tabLinks.forEach(tab => {
                tab.addEventListener('click', function() {
                    const target = this.getAttribute('data-target');
                    
                    // Remove active class from all tabs and contents
                    tabLinks.forEach(t => t.classList.remove('active'));
                    tabContents.forEach(c => c.classList.remove('active'));
                    
                    // Add active class to clicked tab and corresponding content
                    this.classList.add('active');
                    document.getElementById(target).classList.add('active');
                });
            });
            
            // Switch to login link
            document.getElementById('switch-to-login').addEventListener('click', function(e) {
                e.preventDefault();
                tabLinks.forEach(t => t.classList.remove('active'));
                tabContents.forEach(c => c.classList.remove('active'));
                
                document.querySelector('[data-target="login-form"]').classList.add('active');
                document.getElementById('login-form').classList.add('active');
            });
            
            // Password toggle
            const passwordToggles = document.querySelectorAll('.password-toggle-btn');
            
            passwordToggles.forEach(btn => {
                btn.addEventListener('click', function() {
                    const target = this.getAttribute('data-target');
                    // Get the actual client ID of the control
                    let inputId = target;
                    
                    // Map the control ID to the client ID for ASP.NET controls
                    if (target === 'txtLoginPassword') {
                        inputId = '<%= txtLoginPassword.ClientID %>';
                    } else if (target === 'txtRegisterPassword') {
                        inputId = '<%= txtRegisterPassword.ClientID %>';
                    } else if (target === 'txtRegisterConfirmPassword') {
                        inputId = '<%= txtRegisterConfirmPassword.ClientID %>';
                    }
                    
                    const input = document.getElementById(inputId);
                    
                    if (input.type === 'password') {
                        input.type = 'text';
                        this.innerHTML = `<svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" class="bi bi-eye-slash" viewBox="0 0 16 16">
                            <path d="M13.359 11.238C15.06 9.72 16 8 16 8s-3-5.5-8-5.5a7.028 7.028 0 0 0-2.79.588l.77.771A5.944 5.944 0 0 1 8 3.5c2.12 0 3.879 1.168 5.168 2.457A13.134 13.134 0 0 1 14.828 8c-.058.087-.122.183-.195.288-.335.48-.83 1.12-1.465 1.755-.165.165-.337.328-.517.486l.708.709z"/>
                            <path d="M11.297 9.176a3.5 3.5 0 0 0-4.474-4.474l.823.823a2.5 2.5 0 0 1 2.829 2.829l.822.822zm-2.943 1.299.822.822a3.5 3.5 0 0 1-4.474-4.474l.823.823a2.5 2.5 0 0 0 2.829 2.829z"/>
                            <path d="M3.35 5.47c-.18.16-.353.322-.518.487A13.134 13.134 0 0 0 1.172 8l.195.288c.335.48.83 1.12 1.465 1.755C4.121 11.332 5.881 12.5 8 12.5c.716 0 1.39-.133 2.02-.36l.77.772A7.029 7.029 0 0 1 8 13.5C3 13.5 0 8 0 8s.939-1.721 2.641-3.238l.708.709zm10.296 8.884-12-12 .708-.708 12 12-.708.708z"/>
                        </svg>`;
                    } else {
                        input.type = 'password';
                        this.innerHTML = `<svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" class="bi bi-eye" viewBox="0 0 16 16">
                            <path d="M16 8s-3-5.5-8-5.5S0 8 0 8s3 5.5 8 5.5S16 8 16 8zM1.173 8a13.133 13.133 0 0 1 1.66-2.043C4.12 4.668 5.88 3.5 8 3.5c2.12 0 3.879 1.168 5.168 2.457A13.133 13.133 0 0 1 14.828 8c-.058.087-.122.183-.195.288-.335.48-.83 1.12-1.465 1.755C11.879 11.332 10.119 12.5 8 12.5c-2.12 0-3.879-1.168-5.168-2.457A13.134 13.134 0 0 1 1.172 8z"/>
                            <path d="M8 5.5a2.5 2.5 0 1 0 0 5 2.5 2.5 0 0 0 0-5zM4.5 8a3.5 3.5 0 1 1 7 0 3.5 3.5 0 0 1-7 0z"/>
                        </svg>`;
                    }
                });
            });
            
            // Registration form validation
            const registerUsername = document.getElementById('<%= txtRegisterUsername.ClientID %>');
            const registerEmail = document.getElementById('<%= txtRegisterEmail.ClientID %>');
            const registerPassword = document.getElementById('<%= txtRegisterPassword.ClientID %>');
            const registerConfirmPassword = document.getElementById('<%= txtRegisterConfirmPassword.ClientID %>');
            const registerFirstname = document.getElementById('<%= txtRegisterFirstname.ClientID %>');
            const registerLastname = document.getElementById('<%= txtRegisterLastname.ClientID %>');
            const registerPhone = document.getElementById('<%= txtRegisterPhone.ClientID %>');
            
            // Username validation
            registerUsername.addEventListener('blur', function() {
                validateUsername(this);
            });
            
            // Email validation
            registerEmail.addEventListener('blur', function() {
                validateEmail(this);
            });
            
            // Password validation
            registerPassword.addEventListener('input', function() {
                validatePassword(this);
                // If confirm password is not empty, validate it too
                if (registerConfirmPassword.value.trim() !== '') {
                    validateConfirmPassword(registerConfirmPassword);
                }
            });
            
            // Confirm password validation
            registerConfirmPassword.addEventListener('input', function() {
                validateConfirmPassword(this);
            });
            
            // First name validation
            registerFirstname.addEventListener('blur', function() {
                validateRequired(this, 'Please enter your first name');
            });
            
            // Last name validation
            registerLastname.addEventListener('blur', function() {
                validateRequired(this, 'Please enter your last name');
            });
            
            // Phone validation
            registerPhone.addEventListener('blur', function() {
                validatePhone(this);
            });
            
            // Login form submission handling
            const loginForm = document.getElementById('login-form');
            const loginButton = document.getElementById('<%= LoginButton.ClientID %>');
            
            // Register form submission handling
            const registerForm = document.getElementById('register-form');
            const registerButton = document.getElementById('<%= RegisterButton.ClientID %>');
            
            // Function to validate username
            function validateUsername(input) {
                if (input.value.trim() === '') {
                    setInvalid(input, 'Please enter a username');
                    return false;
                }
                
                if (input.value.trim().length < 3) {
                    setInvalid(input, 'Username must be at least 3 characters');
                    return false;
                }
                
                // Check if username exists via AJAX
                checkUsernameExists(input.value.trim());
                
                return true;
            }
            
            // Function to validate email
            function validateEmail(input) {
                if (input.value.trim() === '') {
                    setInvalid(input, 'Please enter an email address');
                    return false;
                }
                
                const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
                if (!emailRegex.test(input.value.trim())) {
                    setInvalid(input, 'Please enter a valid email address');
                    return false;
                }
                
                // Check if email exists via AJAX
                checkEmailExists(input.value.trim());
                
                return true;
            }
            
            // Function to validate password
            function validatePassword(input) {
                if (input.value.trim() === '') {
                    setInvalid(input, 'Please enter a password');
                    return false;
                }
                
                if (input.value.trim().length < 6) {
                    setInvalid(input, 'Password must be at least 6 characters');
                    return false;
                }
                
                setValid(input);
                return true;
            }
            
            // Function to validate confirm password
            function validateConfirmPassword(input) {
                if (input.value.trim() === '') {
                    setInvalid(input, 'Please confirm your password');
                    return false;
                }
                
                if (input.value !== registerPassword.value) {
                    setInvalid(input, 'Passwords do not match');
                    return false;
                }
                
                setValid(input);
                return true;
            }
            
            // Function to validate required fields
            function validateRequired(input, message) {
                if (input.value.trim() === '') {
                    setInvalid(input, message);
                    return false;
                }
                
                setValid(input);
                return true;
            }
            
            // Function to validate phone
            function validatePhone(input) {
                if (input.value.trim() === '') {
                    // Phone is optional, so just reset if empty
                    resetValidation(input);
                    return true;
                }
                
                const phoneRegex = /^\+?[0-9]{10,15}$/;
                if (!phoneRegex.test(input.value.trim().replace(/\s+/g, ''))) {
                    setInvalid(input, 'Please enter a valid phone number');
                    return false;
                }
                
                setValid(input);
                return true;
            }
            
            // Helper functions for validation
            function setInvalid(input, message) {
                input.classList.add('is-invalid');
                input.classList.remove('is-valid');
                
                const feedbackElement = input.nextElementSibling;
                if (feedbackElement && feedbackElement.classList.contains('invalid-feedback')) {
                    feedbackElement.textContent = message;
                }
            }
            
            function setValid(input) {
                input.classList.remove('is-invalid');
                input.classList.add('is-valid');
            }
            
            function resetValidation(input) {
                input.classList.remove('is-invalid');
                input.classList.remove('is-valid');
            }
            
            // Function to check if username exists (AJAX)
            function checkUsernameExists(username) {
                if (username.trim() === '') return;
                
                // Add loading indicator
                addLoadingIndicator(registerUsername);
                
                // Call the page method
                PageMethods.CheckUsernameAvailability(username, function(response) {
                    // Remove loading indicator
                    removeLoadingIndicator(registerUsername);
                    
                    if (response.Success) {
                        if (response.IsAvailable) {
                            setValidWithMessage(registerUsername, 'Username is available');
                        } else {
                            setInvalid(registerUsername, 'Username already exists');
                        }
                    } else {
                        // Error occurred
                        resetValidation(registerUsername);
                    }
                }, function(error) {
                    // Remove loading indicator
                    removeLoadingIndicator(registerUsername);
                    console.error('Error checking username:', error);
                    resetValidation(registerUsername);
                });
            }
            
            // Function to check if email exists (AJAX)
            function checkEmailExists(email) {
                if (email.trim() === '') return;
                
                // Add loading indicator
                addLoadingIndicator(registerEmail);
                
                // Call the page method
                PageMethods.CheckEmailAvailability(email, function(response) {
                    // Remove loading indicator
                    removeLoadingIndicator(registerEmail);
                    
                    if (response.Success) {
                        if (response.IsAvailable) {
                            setValidWithMessage(registerEmail, 'Email is available');
                        } else {
                            setInvalid(registerEmail, 'Email already exists');
                        }
                    } else {
                        // Error occurred
                        resetValidation(registerEmail);
                    }
                }, function(error) {
                    // Remove loading indicator
                    removeLoadingIndicator(registerEmail);
                    console.error('Error checking email:', error);
                    resetValidation(registerEmail);
                });
            }
            
            // Function to set valid with custom message
            function setValidWithMessage(input, message) {
                input.classList.remove('is-invalid');
                input.classList.add('is-valid');
                
                const validFeedback = input.nextElementSibling.nextElementSibling;
                if (validFeedback && validFeedback.classList.contains('valid-feedback')) {
                    validFeedback.textContent = message;
                }
            }
            
            // Function to add loading indicator
            function addLoadingIndicator(input) {
                input.parentNode.classList.add('relative');
                
                // Check if loading indicator already exists
                if (input.parentNode.querySelector('.loading-indicator')) return;
                
                // Create loading indicator element
                const loader = document.createElement('div');
                loader.className = 'loading-indicator absolute right-3 top-1/2 transform -translate-y-1/2';
                loader.innerHTML = `
                    <div class="w-5 h-5 border-2 border-gray-200 border-t-[#96744F] rounded-full animate-spin"></div>
                `;
                
                // Insert after input
                input.parentNode.appendChild(loader);
            }
            
            // Function to remove loading indicator
            function removeLoadingIndicator(input) {
                const loader = input.parentNode.querySelector('.loading-indicator');
                if (loader) {
                    loader.remove();
                }
            }
            
            // Register form validation before submit
            if (registerButton) {
                registerButton.addEventListener('click', function(e) {
                    // Validate all fields
                    const isFirstNameValid = validateRequired(registerFirstname, 'Please enter your first name');
                    const isLastNameValid = validateRequired(registerLastname, 'Please enter your last name');
                    const isUsernameValid = validateUsername(registerUsername);
                    const isEmailValid = validateEmail(registerEmail);
                    const isPasswordValid = validatePassword(registerPassword);
                    const isConfirmPasswordValid = validateConfirmPassword(registerConfirmPassword);
                    const isPhoneValid = validatePhone(registerPhone);
                    
                    // Check if all validations passed
                    if (!isFirstNameValid || !isLastNameValid || !isUsernameValid || 
                        !isEmailValid || !isPasswordValid || !isConfirmPasswordValid || !isPhoneValid) {
                        e.preventDefault();
                        
                        // Focus the first invalid field
                        const invalidFields = registerForm.querySelectorAll('.is-invalid');
                        if (invalidFields.length > 0) {
                            invalidFields[0].focus();
                        }
                        
                        // Show error message
                        showToast('Please fix the validation errors before submitting', 'error');
                        
                        // Shake the form
                        registerForm.classList.add('shake');
                        setTimeout(() => {
                            registerForm.classList.remove('shake');
                        }, 600);
                    }
                });
            }
            
            // Login form validation before submit
            if (loginButton) {
                loginButton.addEventListener('click', function(e) {
                    const username = document.getElementById('<%= txtLoginUsername.ClientID %>');
                    const password = document.getElementById('<%= txtLoginPassword.ClientID %>');
                    
                    let isValid = true;
                    
                    // Validate username
                    if (username.value.trim() === '') {
                        setInvalid(username, 'Please enter your username or email');
                        isValid = false;
                    } else {
                        resetValidation(username);
                    }
                    
                    // Validate password
                    if (password.value.trim() === '') {
                        setInvalid(password, 'Please enter your password');
                        isValid = false;
                    } else {
                        resetValidation(password);
                    }
                    
                    if (!isValid) {
                        e.preventDefault();
                        
                        // Focus the first invalid field
                        const invalidFields = loginForm.querySelectorAll('.is-invalid');
                        if (invalidFields.length > 0) {
                            invalidFields[0].focus();
                        }
                        
                        // Shake the form
                        loginForm.classList.add('shake');
                        setTimeout(() => {
                            loginForm.classList.remove('shake');
                        }, 600);
                    }
                });
            }
            
            // Toast notification functions
            window.showToast = function(message, type = 'success') {
                const toast = document.getElementById('toast');
                const toastText = document.getElementById('toast-text');
                const toastIcon = document.querySelector('.toast-icon');
                
                toast.className = 'toast';
                toast.classList.add(`toast-${type}`);
                
                if (type === 'success') {
                    toastIcon.innerHTML = `<svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="#10b981" class="bi bi-check-circle" viewBox="0 0 16 16">
                        <path d="M8 15A7 7 0 1 1 8 1a7 7 0 0 1 0 14zm0 1A8 8 0 1 0 8 0a8 8 0 0 0 0 16z"/>
                        <path d="M10.97 4.97a.235.235 0 0 0-.02.022L7.477 9.417 5.384 7.323a.75.75 0 0 0-1.06 1.06L6.97 11.03a.75.75 0 0 0 1.079-.02l3.992-4.99a.75.75 0 0 0-1.071-1.05z"/>
                    </svg>`;
                } else {
                    toastIcon.innerHTML = `<svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="#ef4444" class="bi bi-exclamation-circle" viewBox="0 0 16 16">
                        <path d="M8 15A7 7 0 1 1 8 1a7 7 0 0 1 0 14zm0 1A8 8 0 1 0 8 0a8 8 0 0 0 0 16z"/>
                        <path d="M7.002 11a1 1 0 1 1 2 0 1 1 0 0 1-2 0zM7.1 4.995a.905.905 0 1 1 1.8 0l-.35 3.507a.552.552 0 0 1-1.1 0L7.1 4.995z"/>
                    </svg>`;
                }
                
                toastText.textContent = message;
                
                // Show the toast
                toast.classList.add('show');
                
                // Hide after 3 seconds
                setTimeout(() => {
                    toast.classList.remove('show');
                }, 3000);
            }
            
            // Handle forgot password link
            const forgotPasswordLink = document.getElementById('forgot-password-link');
            if (forgotPasswordLink) {
                forgotPasswordLink.addEventListener('click', function(e) {
                    e.preventDefault();
                    showToast('Password reset functionality will be implemented in a future update.', 'error');
                });
            }
        });
    </script>
</asp:Content>
