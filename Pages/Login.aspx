<%@ Page Title="" Language="C#" MasterPageFile="~/Pages/Site.Master" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="OnlinePastryShop.Pages.Login" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        /* Custom styles for login/register */
        .password-requirement {
            transition: all 0.3s ease;
        }
        .requirement-met {
            color: #10B981;
            text-decoration: line-through;
        }
        .requirement-not-met {
            color: #6B7280;
        }
        .input-focus-effect:focus {
            transform: scale(1.01);
            transition: all 0.3s ease;
        }
        .tab-btn {
            transition: all 0.2s ease;
            position: relative;
        }
        .tab-btn:hover {
            background-color: rgba(150, 116, 79, 0.05);
        }
        .tab-btn::after {
            content: '';
            position: absolute;
            width: 0;
            height: 2px;
            bottom: -1px;
            left: 50%;
            background-color: #96744F;
            transition: all 0.2s ease;
        }
        .tab-btn:hover::after {
            width: 100%;
            left: 0;
        }
        .tab-content {
            transition: opacity 0.2s ease, transform 0.2s ease;
        }
        .pastry-pattern {
            background-image: url("data:image/svg+xml,%3Csvg width='60' height='60' viewBox='0 0 60 60' xmlns='http://www.w3.org/2000/svg'%3E%3Cpath d='M54.627 0l.83.828-1.415 1.415L51.8 0h2.827zM5.373 0l-.83.828L5.96 2.243 8.2 0H5.374zM48.97 0l3.657 3.657-1.414 1.414L46.143 0h2.828zM11.03 0L7.372 3.657 8.787 5.07 13.857 0H11.03zm32.284 0L49.8 6.485 48.384 7.9l-7.9-7.9h2.83zM16.686 0L10.2 6.485 11.616 7.9l7.9-7.9h-2.83zm20.97 0l9.315 9.314-1.414 1.414L34.828 0h2.83zM22.344 0L13.03 9.314l1.414 1.414L25.172 0h-2.83zM32 0l12.142 12.142-1.414 1.414L30 1.828 17.272 14.556l-1.414-1.414L28 0h4zM.284 0l28 28-1.414 1.414L0 2.544V0h.284zM0 5.373l25.456 25.455-1.414 1.415L0 8.2V5.374zm0 5.656l22.627 22.627-1.414 1.414L0 13.86v-2.83zm0 5.656l19.8 19.8-1.415 1.413L0 19.514v-2.83zm0 5.657l16.97 16.97-1.414 1.415L0 25.172v-2.83zM0 28l14.142 14.142-1.414 1.414L0 30.828V28zm0 5.657L11.314 44.97 9.9 46.386 0 36.485v-2.83zm0 5.657l8.485 8.485-1.414 1.414L0 42.143v-2.83zm0 5.657l5.657 5.657-1.414 1.415L0 47.8v-2.83zm0 5.657l2.828 2.83-1.414 1.413L0 53.458v-2.83zM54.627 60L30 35.373 5.373 60H8.2L30 38.2 51.8 60h2.827zm-5.656 0L30 41.03 11.03 60h2.828L30 43.858 46.142 60h2.83zm-5.656 0L30 46.686 13.314 60h2.83L30 49.515 46.485 60h2.83zm-5.657 0L30 52.343 16.343 60h2.83L30 55.172 41.17 60h2.83zm-5.657 0L30 58l-8 2h2.83L30 60h5.657z' fill='%2396744F' fill-opacity='0.05'/%3E%3C/svg%3E");
        }
        .strength-meter {
            height: 8px;
            border-radius: 4px;
            transition: all 0.3s ease;
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="min-h-screen pastry-pattern flex items-center justify-center p-4 sm:p-6 lg:p-8">
        <div class="w-full max-w-lg bg-white rounded-xl shadow-2xl overflow-hidden">
            <!-- Company Name Header -->
            <div class="p-6 bg-[#96744F] text-white text-center">
                <h1 class="text-4xl font-bold" style="font-family: 'Playfair Display', serif;">Royal Pastries</h1>
                <p class="mt-2 text-white/80">Your Filipino Pastry Destination</p>
            </div>

            <!-- Tab Navigation -->
            <div class="flex border-b border-gray-200">
                <button type="button" id="loginTabBtn" class="tab-btn w-1/2 py-4 text-center font-medium text-[#96744F] border-b-2 border-[#96744F]">
                    Login
                </button>
                <button type="button" id="registerTabBtn" class="tab-btn w-1/2 py-4 text-center font-medium text-gray-500">
                    Register
                </button>
            </div>

            <!-- Login Form -->
            <div id="loginTab" class="tab-content p-6">
                <asp:Panel ID="pnlLoginError" runat="server" CssClass="mb-5 p-4 bg-red-50 text-red-500 rounded-md" Visible="false">
                    <asp:Literal ID="litLoginError" runat="server"></asp:Literal>
                </asp:Panel>

                <div class="mb-4">
                    <label for="<%= txtLoginEmail.ClientID %>" class="block text-sm font-medium text-gray-700 mb-1">Email or Username</label>
                    <asp:TextBox ID="txtLoginEmail" runat="server" CssClass="input-focus-effect w-full px-4 py-3 rounded-md border border-gray-300 focus:outline-none focus:ring-2 focus:ring-[#96744F] focus:border-[#96744F]" 
                        placeholder="Enter your email or username" autocomplete="username"></asp:TextBox>
                    <asp:RequiredFieldValidator ID="rfvLoginEmail" runat="server" ControlToValidate="txtLoginEmail" 
                        ErrorMessage="Email or username is required" CssClass="mt-1 text-sm text-red-500" Display="Dynamic" 
                        ValidationGroup="LoginGroup"></asp:RequiredFieldValidator>
                </div>

                <div class="mb-5">
                    <label for="<%= txtLoginPassword.ClientID %>" class="block text-sm font-medium text-gray-700 mb-1">Password</label>
                    <div class="relative">
                        <asp:TextBox ID="txtLoginPassword" runat="server" TextMode="Password" CssClass="input-focus-effect w-full px-4 py-3 rounded-md border border-gray-300 focus:outline-none focus:ring-2 focus:ring-[#96744F] focus:border-[#96744F]" 
                            placeholder="Enter your password" autocomplete="current-password"></asp:TextBox>
                        <button type="button" class="toggle-password absolute inset-y-0 right-0 pr-3 flex items-center text-gray-500 hover:text-gray-700 cursor-pointer">
                            <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
                                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z" />
                            </svg>
                        </button>
                    </div>
                    <asp:RequiredFieldValidator ID="rfvLoginPassword" runat="server" ControlToValidate="txtLoginPassword" 
                        ErrorMessage="Password is required" CssClass="mt-1 text-sm text-red-500" Display="Dynamic" 
                        ValidationGroup="LoginGroup"></asp:RequiredFieldValidator>
                </div>

                <div class="mb-5">
                    <asp:Button ID="btnLogin" runat="server" Text="Log In" CssClass="w-full bg-[#96744F] hover:bg-[#7d603f] text-white font-medium py-3 px-4 rounded-md focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-[#96744F] transition-all duration-200" 
                        ValidationGroup="LoginGroup" OnClick="btnLogin_Click" />
                </div>

                <p class="text-center text-sm text-gray-600">
                    Don't have an account? 
                    <a href="javascript:void(0)" id="switchToRegister" class="text-[#96744F] hover:text-[#7d603f] font-medium">Create one here</a>
                </p>
            </div>

            <!-- Register Form -->
            <div id="registerTab" class="tab-content p-6 hidden opacity-0">
                <asp:Panel ID="pnlRegisterError" runat="server" CssClass="mb-5 p-4 bg-red-50 text-red-500 rounded-md" Visible="false">
                    <asp:Literal ID="litRegisterError" runat="server"></asp:Literal>
                </asp:Panel>

                <div class="grid grid-cols-1 sm:grid-cols-2 gap-4 mb-4">
                    <div>
                        <label for="<%= txtFirstName.ClientID %>" class="block text-sm font-medium text-gray-700 mb-1">First Name</label>
                        <asp:TextBox ID="txtFirstName" runat="server" CssClass="input-focus-effect w-full px-4 py-3 rounded-md border border-gray-300 focus:outline-none focus:ring-2 focus:ring-[#96744F] focus:border-[#96744F]" 
                            placeholder="Enter your first name"></asp:TextBox>
                        <asp:RequiredFieldValidator ID="rfvFirstName" runat="server" ControlToValidate="txtFirstName" 
                            ErrorMessage="First name is required" CssClass="mt-1 text-sm text-red-500" Display="Dynamic" 
                            ValidationGroup="RegisterGroup"></asp:RequiredFieldValidator>
                    </div>
                    <div>
                        <label for="<%= txtLastName.ClientID %>" class="block text-sm font-medium text-gray-700 mb-1">Last Name</label>
                        <asp:TextBox ID="txtLastName" runat="server" CssClass="input-focus-effect w-full px-4 py-3 rounded-md border border-gray-300 focus:outline-none focus:ring-2 focus:ring-[#96744F] focus:border-[#96744F]" 
                            placeholder="Enter your last name"></asp:TextBox>
                        <asp:RequiredFieldValidator ID="rfvLastName" runat="server" ControlToValidate="txtLastName" 
                            ErrorMessage="Last name is required" CssClass="mt-1 text-sm text-red-500" Display="Dynamic" 
                            ValidationGroup="RegisterGroup"></asp:RequiredFieldValidator>
                    </div>
                </div>

                <div class="mb-4">
                    <label for="<%= txtUsername.ClientID %>" class="block text-sm font-medium text-gray-700 mb-1">Username</label>
                    <asp:TextBox ID="txtUsername" runat="server" CssClass="input-focus-effect w-full px-4 py-3 rounded-md border border-gray-300 focus:outline-none focus:ring-2 focus:ring-[#96744F] focus:border-[#96744F]" 
                        placeholder="Choose a username"></asp:TextBox>
                    <asp:RequiredFieldValidator ID="rfvUsername" runat="server" ControlToValidate="txtUsername" 
                        ErrorMessage="Username is required" CssClass="mt-1 text-sm text-red-500" Display="Dynamic" 
                        ValidationGroup="RegisterGroup"></asp:RequiredFieldValidator>
                    <asp:RegularExpressionValidator ID="revUsername" runat="server" ControlToValidate="txtUsername"
                        ValidationExpression="^[a-zA-Z0-9_]{4,20}$"
                        ErrorMessage="Username must be 4-20 characters and may include letters, numbers, and underscores"
                        CssClass="mt-1 text-sm text-red-500" Display="Dynamic" ValidationGroup="RegisterGroup"></asp:RegularExpressionValidator>
                </div>

                <div class="mb-4">
                    <label for="<%= txtEmail.ClientID %>" class="block text-sm font-medium text-gray-700 mb-1">Email Address</label>
                    <asp:TextBox ID="txtEmail" runat="server" TextMode="Email" CssClass="input-focus-effect w-full px-4 py-3 rounded-md border border-gray-300 focus:outline-none focus:ring-2 focus:ring-[#96744F] focus:border-[#96744F]" 
                        placeholder="Enter your email"></asp:TextBox>
                    <asp:RequiredFieldValidator ID="rfvEmail" runat="server" ControlToValidate="txtEmail" 
                        ErrorMessage="Email is required" CssClass="mt-1 text-sm text-red-500" Display="Dynamic" 
                        ValidationGroup="RegisterGroup"></asp:RequiredFieldValidator>
                    <asp:RegularExpressionValidator ID="revEmail" runat="server" ControlToValidate="txtEmail"
                        ValidationExpression="^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$"
                        ErrorMessage="Please enter a valid email address"
                        CssClass="mt-1 text-sm text-red-500" Display="Dynamic" ValidationGroup="RegisterGroup"></asp:RegularExpressionValidator>
                </div>

                <div class="mb-4">
                    <label for="<%= txtPhoneNumber.ClientID %>" class="block text-sm font-medium text-gray-700 mb-1">Phone Number</label>
                    <div class="relative">
                        <asp:TextBox ID="txtPhoneNumber" runat="server" CssClass="input-focus-effect w-full px-4 py-3 rounded-md border border-gray-300 focus:outline-none focus:ring-2 focus:ring-[#96744F] focus:border-[#96744F]" 
                            placeholder="Enter your phone number (e.g., 09123456789)"></asp:TextBox>
                    </div>
                    <p class="mt-1 text-xs text-gray-500">Format: 09XX-XXX-XXXX, +63 9XX-XXX-XXXX, or 639XX-XXX-XXXX</p>
                    <asp:RequiredFieldValidator ID="rfvPhoneNumber" runat="server" ControlToValidate="txtPhoneNumber" 
                        ErrorMessage="Phone number is required" CssClass="mt-1 text-sm text-red-500" Display="Dynamic" 
                        ValidationGroup="RegisterGroup"></asp:RequiredFieldValidator>
                    <asp:RegularExpressionValidator ID="revPhoneNumber" runat="server" ControlToValidate="txtPhoneNumber"
                        ValidationExpression="^(\+63|63|0)9\d{9}$"
                        ErrorMessage="Please enter a valid Philippine phone number"
                        CssClass="mt-1 text-sm text-red-500" Display="Dynamic" ValidationGroup="RegisterGroup"></asp:RegularExpressionValidator>
                </div>

                <div class="mb-2">
                    <label for="<%= txtPassword.ClientID %>" class="block text-sm font-medium text-gray-700 mb-1">Password</label>
                    <div class="relative">
                        <asp:TextBox ID="txtPassword" runat="server" TextMode="Password" CssClass="input-focus-effect w-full px-4 py-3 rounded-md border border-gray-300 focus:outline-none focus:ring-2 focus:ring-[#96744F] focus:border-[#96744F]" 
                            placeholder="Create a password"></asp:TextBox>
                        <button type="button" class="toggle-password absolute inset-y-0 right-0 pr-3 flex items-center text-gray-500 hover:text-gray-700 cursor-pointer">
                            <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
                                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z" />
                            </svg>
                        </button>
                    </div>
                </div>

                <!-- Password Strength Meter -->
                <div class="mb-2">
                    <div class="flex justify-between items-center mb-1">
                        <span class="text-xs text-gray-500">Password Strength:</span>
                        <span id="strengthText" class="text-xs font-medium text-gray-500">None</span>
                    </div>
                    <div class="bg-gray-200 rounded-full">
                        <div id="strengthMeter" class="strength-meter bg-gray-400 w-0"></div>
                    </div>
                </div>

                <div class="mb-5">
                    <label for="<%= txtConfirmPassword.ClientID %>" class="block text-sm font-medium text-gray-700 mb-1">Confirm Password</label>
                    <div class="relative">
                        <asp:TextBox ID="txtConfirmPassword" runat="server" TextMode="Password" CssClass="input-focus-effect w-full px-4 py-3 rounded-md border border-gray-300 focus:outline-none focus:ring-2 focus:ring-[#96744F] focus:border-[#96744F]" 
                            placeholder="Confirm your password"></asp:TextBox>
                        <button type="button" class="toggle-password absolute inset-y-0 right-0 pr-3 flex items-center text-gray-500 hover:text-gray-700 cursor-pointer">
                            <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
                                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z" />
                            </svg>
                        </button>
                    </div>
                    <asp:CompareValidator ID="cvConfirmPassword" runat="server" ControlToValidate="txtConfirmPassword" 
                        ControlToCompare="txtPassword" ErrorMessage="Passwords do not match" 
                        CssClass="mt-1 text-sm text-red-500" Display="Dynamic" ValidationGroup="RegisterGroup"></asp:CompareValidator>
                </div>

                <!-- Password Requirements -->
                <div class="mb-4">
                    <ul class="text-xs space-y-1 pl-5 list-disc">
                        <li id="req-length" class="password-requirement requirement-not-met">At least 8 characters long</li>
                        <li id="req-uppercase" class="password-requirement requirement-not-met">At least one uppercase letter</li>
                        <li id="req-lowercase" class="password-requirement requirement-not-met">At least one lowercase letter</li>
                        <li id="req-number" class="password-requirement requirement-not-met">At least one number</li>
                        <li id="req-special" class="password-requirement requirement-not-met">At least one special character</li>
                    </ul>
                </div>

                <div class="mb-5">
                    <asp:Button ID="btnRegister" runat="server" Text="Create Account" CssClass="w-full bg-[#96744F] hover:bg-[#7d603f] text-white font-medium py-3 px-4 rounded-md focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-[#96744F] transition-all duration-200" 
                        ValidationGroup="RegisterGroup" OnClick="btnRegister_Click" />
                </div>

                <p class="text-center text-sm text-gray-600">
                    Already have an account? 
                    <a href="javascript:void(0)" id="switchToLogin" class="text-[#96744F] hover:text-[#7d603f] font-medium">Log in here</a>
                </p>
            </div>
        </div>
    </div>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="ScriptsContent" runat="server">
    <script>
        document.addEventListener('DOMContentLoaded', function() {
            // Tab switching
            const loginTab = document.getElementById('loginTab');
            const registerTab = document.getElementById('registerTab');
            const loginTabBtn = document.getElementById('loginTabBtn');
            const registerTabBtn = document.getElementById('registerTabBtn');
            const switchToRegister = document.getElementById('switchToRegister');
            const switchToLogin = document.getElementById('switchToLogin');

            // Set autofocus on first input field
            document.getElementById('<%= txtLoginEmail.ClientID %>').focus();

            function showLoginTab() {
                // Update tabs
                loginTabBtn.classList.add('text-[#96744F]', 'border-b-2', 'border-[#96744F]');
                loginTabBtn.classList.remove('text-gray-500');
                registerTabBtn.classList.add('text-gray-500');
                registerTabBtn.classList.remove('text-[#96744F]', 'border-b-2', 'border-[#96744F]');
                
                // Hide register tab with animation
                registerTab.style.opacity = '0';
                registerTab.style.transform = 'translateY(5px)';
                
                setTimeout(() => {
                    registerTab.classList.add('hidden');
                    loginTab.classList.remove('hidden');
                    
                    // Show login tab with animation
                    setTimeout(() => {
                        loginTab.style.opacity = '1';
                        loginTab.style.transform = 'translateY(0)';
                        document.getElementById('<%= txtLoginEmail.ClientID %>').focus();
                    }, 20);
                }, 200);
            }

            function showRegisterTab() {
                // Update tabs
                registerTabBtn.classList.add('text-[#96744F]', 'border-b-2', 'border-[#96744F]');
                registerTabBtn.classList.remove('text-gray-500');
                loginTabBtn.classList.add('text-gray-500');
                loginTabBtn.classList.remove('text-[#96744F]', 'border-b-2', 'border-[#96744F]');
                
                // Hide login tab with animation
                loginTab.style.opacity = '0';
                loginTab.style.transform = 'translateY(5px)';
                
                setTimeout(() => {
                    loginTab.classList.add('hidden');
                    registerTab.classList.remove('hidden');
                    
                    // Show register tab with animation
                    setTimeout(() => {
                        registerTab.style.opacity = '1';
                        registerTab.style.transform = 'translateY(0)';
                        document.getElementById('<%= txtFirstName.ClientID %>').focus();
                    }, 20);
                }, 200);
            }

            // Add hover effect for tab buttons
            const tabButtons = document.querySelectorAll('.tab-btn');
            tabButtons.forEach(button => {
                button.addEventListener('mouseenter', function() {
                    if (!this.classList.contains('border-[#96744F]')) {
                        this.classList.add('text-[#96744F]/70');
                    }
                });
                
                button.addEventListener('mouseleave', function() {
                    if (!this.classList.contains('border-[#96744F]')) {
                        this.classList.remove('text-[#96744F]/70');
                    }
                });
            });

            loginTabBtn.addEventListener('click', showLoginTab);
            registerTabBtn.addEventListener('click', showRegisterTab);
            switchToRegister.addEventListener('click', showRegisterTab);
            switchToLogin.addEventListener('click', showLoginTab);

            // Password visibility toggle
            const toggleButtons = document.querySelectorAll('.toggle-password');
            toggleButtons.forEach(button => {
                button.addEventListener('click', function() {
                    const input = this.previousElementSibling;
                    const type = input.getAttribute('type') === 'password' ? 'text' : 'password';
                    input.setAttribute('type', type);
                    
                    // Change the eye icon
                    if (type === 'text') {
                        this.innerHTML = `
                            <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13.875 18.825A10.05 10.05 0 0112 19c-4.478 0-8.268-2.943-9.543-7a9.97 9.97 0 011.563-3.029m5.858.908a3 3 0 114.243 4.243M9.878 9.878l4.242 4.242M9.88 9.88l-3.29-3.29m7.532 7.532l3.29 3.29M3 3l3.59 3.59m0 0A9.953 9.953 0 0112 5c4.478 0 8.268 2.943 9.543 7a10.025 10.025 0 01-4.132 5.411m0 0L21 21" />
                            </svg>
                        `;
                    } else {
                        this.innerHTML = `
                            <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
                                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z" />
                            </svg>
                        `;
                    }
                });
            });

            // Password strength and requirements validation
            const passwordInput = document.getElementById('<%= txtPassword.ClientID %>');
            const strengthMeter = document.getElementById('strengthMeter');
            const strengthText = document.getElementById('strengthText');

            // Password requirement elements
            const reqLength = document.getElementById('req-length');
            const reqUppercase = document.getElementById('req-uppercase');
            const reqLowercase = document.getElementById('req-lowercase');
            const reqNumber = document.getElementById('req-number');
            const reqSpecial = document.getElementById('req-special');

            passwordInput.addEventListener('input', function() {
                const password = this.value;
                
                // Check requirements
                const hasLength = password.length >= 8;
                const hasUppercase = /[A-Z]/.test(password);
                const hasLowercase = /[a-z]/.test(password);
                const hasNumber = /[0-9]/.test(password);
                const hasSpecial = /[^A-Za-z0-9]/.test(password);
                
                // Update requirement indicators
                updateRequirement(reqLength, hasLength);
                updateRequirement(reqUppercase, hasUppercase);
                updateRequirement(reqLowercase, hasLowercase);
                updateRequirement(reqNumber, hasNumber);
                updateRequirement(reqSpecial, hasSpecial);
                
                // Calculate strength
                let strength = 0;
                if (hasLength) strength += 20;
                if (hasUppercase) strength += 20;
                if (hasLowercase) strength += 20;
                if (hasNumber) strength += 20;
                if (hasSpecial) strength += 20;
                
                // Update strength meter
                strengthMeter.style.width = strength + '%';
                
                // Set color based on strength
                if (strength <= 20) {
                    strengthMeter.className = 'strength-meter bg-red-500 w-0';
                    strengthText.textContent = 'Very Weak';
                    strengthText.className = 'text-xs font-medium text-red-500';
                } else if (strength <= 40) {
                    strengthMeter.className = 'strength-meter bg-orange-500';
                    strengthText.textContent = 'Weak';
                    strengthText.className = 'text-xs font-medium text-orange-500';
                } else if (strength <= 60) {
                    strengthMeter.className = 'strength-meter bg-yellow-500';
                    strengthText.textContent = 'Medium';
                    strengthText.className = 'text-xs font-medium text-yellow-600';
                } else if (strength <= 80) {
                    strengthMeter.className = 'strength-meter bg-blue-500';
                    strengthText.textContent = 'Strong';
                    strengthText.className = 'text-xs font-medium text-blue-600';
                } else {
                    strengthMeter.className = 'strength-meter bg-green-500';
                    strengthText.textContent = 'Very Strong';
                    strengthText.className = 'text-xs font-medium text-green-600';
                }
            });

            function updateRequirement(element, isMet) {
                if (isMet) {
                    element.classList.remove('requirement-not-met');
                    element.classList.add('requirement-met');
                } else {
                    element.classList.remove('requirement-met');
                    element.classList.add('requirement-not-met');
                }
            }

            // Phone number formatting and validation
            const phoneInput = document.getElementById('<%= txtPhoneNumber.ClientID %>');
            
            phoneInput.addEventListener('input', function(e) {
                let value = e.target.value.replace(/\D/g, ''); // Remove non-digits
                
                // Handle different phone formats
                if (value.startsWith('63')) {
                    if (value.length > 12) value = value.substr(0, 12);
                } else if (value.startsWith('0')) {
                    if (value.length > 11) value = value.substr(0, 11);
                } else {
                    // If doesn't start with 0 or 63, assume it's a 9XXXXXXXXX format
                    if (value.length > 10) value = value.substr(0, 10);
                    value = '0' + value;
                }
                
                // Format with hyphens if enough digits (optional)
                /*
                if (value.startsWith('63') && value.length > 5) {
                    value = value.replace(/^(\d{2})(\d{3})(\d{3})(\d{4})$/, "$1-$2-$3-$4");
                } else if (value.length > 4) {
                    value = value.replace(/^(\d{4})(\d{3})(\d{4})$/, "$1-$2-$3");
                }
                */
                
                e.target.value = value;
            });
        });
    </script>
</asp:Content>
