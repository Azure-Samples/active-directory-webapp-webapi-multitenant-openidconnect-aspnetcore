---
services: active-directory
platforms: dotnet
author: dstrockis
---

# Build a multi-tenant SaaS web application that calls a web API using Azure AD

This sample shows how to build a multi-tenant ASP.NET Core web application that uses OpenID Connect to sign up and sign in users from any Azure Active Directory (AD) tenant, using the ASP.Net OpenID Connect OWIN middleware and the Active Directory Authentication Library (ADAL) for .NET. The sample also demonstrates how to leverage the authorization code received at sign in time to invoke the Graph API.

For more information about how the protocols work in this scenario and other scenarios, see the [Authentication Scenarios for Azure AD](https://azure.microsoft.com/documentation/articles/active-directory-authentication-scenarios/) document.

> This sample has finally been updated to ASP.NET Core RC2.  Looking for previous versions of this code sample? Check out the tags on the [releases](../../releases) GitHub page.

## How To Run This Sample

Getting started is simple!  To run this sample you will need:
- [.NET Core & .NET Core SDK RC2 releases](https://www.microsoft.com/net/download)
- [ASP.NET Core RC2 release](https://blogs.msdn.microsoft.com/webdev/2016/05/16/announcing-asp-net-core-rc2/)
- [Visual Studio 2015 Update 2](https://www.visualstudio.com/en-us/downloads/visual-studio-2015-downloads-vs.aspx)
- An Internet connection
- An Azure subscription (a free trial is sufficient)

Every Azure subscription has an associated Azure Active Directory tenant, which provides authentication and authorization services for the subscription. You can easily create new tenants within your subscription for testing purposes. For details on the relationship between an Azure subscription and Azure AD tenant, see [How Azure subscriptions are associated with Azure AD](https://azure.microsoft.com/documentation/articles/active-directory-how-subscriptions-associated-directory/#manage-the-directory-for-your-office-365-subscription-in-azure).

If you don't already have an Azure subscription, you can get a free subscription by signing up at [http://wwww.windowsazure.com](http://www.windowsazure.com). All of the Azure AD features used by this sample are available free of charge. 

### Step 1:  Clone or download this repository

From your shell or command line:

`git clone https://github.com/Azure-Samples/active-directory-dotnet-webapp-webapi-multitenant-openidconnect-aspnetcore.git`

### Step 2:  Create an Organizational user account in your Azure Active Directory tenant

If you already have an Organizational user account in your Azure Active Directory tenant that you would like to use for consent and authentication, you can skip to the next step.  This sample will not work with a Microsoft account, so if you signed in to the Azure portal with a Microsoft account and have never created a user account in your directory before, you need to do that now. You can find instructions to do that [here](http://www.cloudidentity.com/blog/2013/12/11/setting-up-an-asp-net-project-with-organizational-authentication-requires-an-organizational-account/). 

If you want to test both the Administrator and User consent flows discussed below, you will want to create two Organizational accounts: one assigned to the "User" role and one assigned to the ["Global Administrator"](https://azure.microsoft.com/documentation/articles/active-directory-assign-admin-roles/) role. If you want to use the Organizational account to sign-in to the Azure portal, don't forget to also add it as a co-administrator of your Azure subscription.

### Step 3:  Register the sample with your Azure Active Directory tenant

1. Sign in to the [Azure management portal](https://manage.windowsazure.com).
2. Click on Active Directory in the left hand nav.
3. Click the directory tenant where you wish to register the sample application.
4. Click the Applications tab.
5. Click the Add option at the bottom of the page.
6. Click "Add an application my organization is developing".
7. Enter a friendly name for the application, for example "TodoListWebApp_MT", select "Web Application and/or Web API", and click the Next arrow on the lower right.
8. For the Sign-on URL, enter the base URL for the sample, which is by default `https://localhost:44376/`.
9. For the App ID URI, enter `https://<your_tenant_domain>/TodoListWebApp_MT`, replacing `<your_tenant_domain>` with the domain of your Azure AD tenant (either in the form `<tenant_name>.onmicrosoft.com` or your own custom domain if you registered it in Azure Active Directory).
10. Click the Complete checkmark on the lower right corner. Your application is now provisioned and you will be returned to it's main application page.
11. Click on the Configure tab of your application.
12. Find "the application is multi-tenant" switch and flip it to "yes", then click the Save option at the bottom of the page.
12. Scroll down to the "Permissions to other applications" section. On the Windows Azure Active Directory row, click on the "Delegated Permissions" combo box and note that the "Sign-on and read users profile" permission is already enabled, which is assigned by default whenever a new application is registered. This will allow our application to receive delegated permission to authenticate and read user profile data, for a given user account. The list of permissions provided here are known as permissions scopes, some of which require Administrator consent. See the [Graph API Permissions Scopes](https://msdn.microsoft.com/Library/Azure/Ad/Graph/api/graph-api-permission-scopes) article for more information.

Don't close the browser yet, as we will still need to work with the portal for few more steps. 

### Step 4:  Provision a key for your app in your Azure Active Directory tenant

The application will need to authenticate with Azure AD in order to participate in the OAuth2 flow, which requires you to associate a private key with the application you registered in your tenant. In order to do this:
 
1. Click the Configure tab of your application.
2. Scroll down to the "Keys" section. In the Select Duration dropdown, pick a value (either is fine for the demo).
3. Click the Save option at the bottom of the page.
4. Once the Save operation has completed, the value of the key appears. Copy the key to the clipboard. **Important: this is the only opportunity you have to access the value of the key, if you don't use it now you'll have to create a new one.**

Leave the browser open to this page. 

### Step 5:  Configure the sample to use your Azure Active Directory tenant

At this point we are ready to paste the configuration settings into the VS project that will tie it to its entry in your Azure AD tenant. 

1. Open the solution in Visual Studio 2015.
2. Open the `appsettings.json` file.
4. Find the `ClientId` property and replace the value with the Client ID for the TodoListWebApp from the Azure portal.
5. Find the `ClientSecret` and replace the value with the key for the TodoListWebApp from the Azure portal.
6. If you changed the base URL of the TodoListWebApp sample, find the `RedirectUri` property and replace the value with the new base URL of the sample.

### Step 6:  [optional] Create an Azure Active Directory test tenant 

This sample shows how to take advantage of the consent framework in Azure AD to enable an application to be multi-tenant aware, which allows authentication by user accounts from any Azure AD tenant. To see that part of the sample in action, you need to have access to user accounts from a tenant that is different from the one you used for registering the application. A great example of this type of scenario, is an application that needs to allow Office365 user accounts (which are homed in a separate Azure AD) to authenticate and consent access to their Office365 tenant. The simplest way of doing this is to create a new directory tenant in your Azure subscription (just navigate to the main Active Directory page in the portal and click Add) and add test users.
This step is optional as you can also use accounts from the same directory, but if you do you will not see the consent prompts as the app is already approved. 

### Step 5:  Run the sample

The sample implements two distinct tasks: the onboarding of a new customer (aka: Sign up), and regular sign in & use of the application.

####  Sign up
1. Start the application. Click on Sign Up.
2. You will be presented with a form that simulates an onboarding process. Here you can choose whether you want to follow the "admin consent" flow (the app gets provisioned for all the users in one organization - requires you to sign up using an administrator), or the "user consent" flow (the app gets provisioned for your user only).
3. Click the SignUp button. You'll be transferred to the Azure AD portal. Sign in as the user you want to use for consenting. 4. If the user is from a tenant that is different from the one where the app was developed, you will be presented with a consent page. Click OK. You will be transported back to the app, where your registration will be finalized.

####  Sign in
Once you signed up, you can either click on the Todo tab or the sign in link to gain access to the application. Note that if you are doing this in the same session in which you signed up, you will automatically sign in with the same account you used for signing up. If you are signing in during a new session, you will be presented with Azure AD's credentials prompt: sign in using an account compatible with the sign up option you chose earlier (the exact same account if you used user consent, any user form the same tenant if you used admin consent). 