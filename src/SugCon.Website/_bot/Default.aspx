<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="SugCon.Website.Bot.Default" %>

<%@ Import Namespace="Sitecore.Configuration" %>
<%@ Import Namespace="Sitecore.SecurityModel.License" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>Welcome to the Sitecore Bot Authenticator</title>
    <link rel="shortcut icon" href="/sitecore/images/favicon.ico" />
    <meta name="robots" content="noindex, nofollow" />
    <meta http-equiv="X-UA-Compatible" content="IE=edge">
    <link href="https://fonts.googleapis.com/css?family=Open+Sans:400,300italic,400italic,600italic,700italic,300,600,700,800" rel="stylesheet" type="text/css" />
    <script type="text/javascript">
        if (window != top) {
            var urlParams = encodeURIComponent(top.location.pathname + top.location.search + top.location.hash);
            if (urlParams) {
                top.location.href = '<%# GetLoginPageUrl()%>' + '?returnUrl=' + (top.location.pathname[0] == '/' ? '' : '/') + urlParams;
            }
        }
    </script>

    <!-- Bootstrap for testing -->
    <link href="Login.css" rel="stylesheet" />

    <style>
        .login-outer {
            background: url('<%#GetBackgroundImageUrl() %>') no-repeat center center fixed;
            background-size: cover;
        }
    </style>
</head>
<body class="sc">
    <div class="login-outer">
        <div class="login-main-wrap">
            <div class="login-box">
                <div class="logo-wrap">
                    <img src="/_bot/logo_new.png" alt="Sitecore logo" />
                    <img src="/_bot/logo_bot.png" alt="Sitecore logo" />
                </div>

                <form id="LoginForm" runat="server" class="form-signin" role="form">
                    <div id="login">

                        <div class="scLoginFailedMessagesContainer">
                            <div id="credentialsError" class="scMessageBar scWarning" style="display: none">
                                <i class="scMessageBarIcon"></i>
                                <div class="scMessageBarTextContainer">
                                    <div class="scMessageBarText">
                                        <asp:Literal Text="Please enter your login credentials." runat="server"></asp:Literal>
                                    </div>
                                </div>
                            </div>
                            <asp:PlaceHolder runat="server" ID="FailureHolder" Visible="False">
                                <div id="loginFailedMessage" class="scMessageBar scWarning">
                                    <i class="scMessageBarIcon"></i>
                                    <div class="scMessageBarTextContainer">
                                        <div class="scMessageBarText">
                                            <asp:Literal ID="FailureText" Text="Login failed" runat="server"></asp:Literal>
                                        </div>
                                    </div>
                                </div>
                            </asp:PlaceHolder>
                        </div>

                        <asp:PlaceHolder runat="server" ID="SuccessHolder" Visible="False">
                            <div class="sc-messageBar">
                                <div class="sc-messageBar-head alert alert-info">
                                    <i class="alert-ico"></i>
                                    <span class="sc-messageBar-messageText">
                                        <asp:Literal runat="server" ID="SuccessText" />
                                    </span>
                                </div>
                            </div>
                        </asp:PlaceHolder>

                        <asp:PlaceHolder runat="server" ID="FormHolder" Visible="true">
                            <div class="form-wrap">
                                <asp:Label runat="server" ID="loginLbl" CssClass="login-label">User name:</asp:Label>
                                <asp:TextBox ID="UserName" CssClass="form-control" placeholder="Enter user name" autofocus runat="server" ValidationGroup="Login" />
                                <asp:RequiredFieldValidator ID="UserNameRequired" runat="server" ControlToValidate="UserName" ValidationGroup="Login" />
                                <asp:Label runat="server" ID="passLabel" CssClass="login-label">Password:</asp:Label>
                                <asp:TextBox ID="Password" CssClass="form-control" placeholder="Enter password" runat="server" TextMode="Password" ValidationGroup="Login" />
                                <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ControlToValidate="Password" ValidationGroup="Login" />
                            </div>

                            <asp:Button runat="server" ValidationGroup="Login" UseSubmitBehavior="True" CssClass="btn btn-primary btn-block" OnClick="LoginClicked" Text="Log in" />
                        </asp:PlaceHolder>
                    </div>
                </form>
            </div>
        </div>
    </div>
<%--    <script type="text/javascript" src="/_bot/login.js"></script>--%>
</body>
</html>