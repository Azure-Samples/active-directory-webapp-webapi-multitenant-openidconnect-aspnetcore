---
services: active-directory
platforms: dotnet
author: dstrockis
---

# Build a multi-tenant SaaS web application that calls a web API using Azure AD

This sample shows how to build a multi-tenant ASP.NET Core web application that uses OpenID Connect to sign up and sign in users from any Azure Active Directory (AD) tenant, using the ASP.Net OpenID Connect middleware and the Active Directory Authentication Library (ADAL) for .NET. The sample also demonstrates how to leverage the authorization code received at sign in time to invoke the Graph API.

For more information about how the protocols work in this scenario and other scenarios, see the [Authentication Scenarios for Azure AD](https://azure.microsoft.com/documentation/articles/active-directory-authentication-scenarios/) document.

> This sample has been updated to ASP.NET Core 1.0.  Looking for previous versions of this code sample? Check out the tags on the [releases](../../releases) GitHub page.

## How To Run This Sample

Getting started is simple!  To run this sample you will need:
- To install .NET Core for Windows by following the instructions at [dot.net/core](https://dot.net/core), which will include Visual Studio 2015 Update 3.
- An Internet connection
- An Azure Active Directory (Azure AD) tenant. For more information on how to get an Azure AD tenant, please see [How to get an Azure AD tenant](https://azure.microsoft.com/en-us/documentation/articles/active-directory-howto-tenant/) 
- A user account in your Azure AD tenant. This sample will not work with a Microsoft account, so if you signed in to the Azure portal with a Microsoft account and have never created a user account in your directory before, you need to do that now.

### Step 1:  Clone or download this repository

From your shell or command line:

`git clone https://github.com/Azure-Samples/active-directory-dotnet-webapp-webapi-multitenant-openidconnect-aspnetcore.git`

### Step 2:  Create an Organizational user account in your Azure Active Directory tenant

If you already have an Organizational user account in your Azure Active Directory tenant that you would like to use for consent and authentication, you can skip to the next step.  This sample will not work with a Microsoft account, so if you signed in to the Azure portal with a Microsoft account and have never created a user account in your directory before, you need to do that now. You can find instructions to do that [here](http://www.cloudidentity.com/blog/2013/12/11/setting-up-an-asp-net-project-with-organizational-authentication-requires-an-organizational-account/). 

If you want to test both the Administrator and User consent flows discussed below, you will want to create two Organizational accounts: one assigned to the "User" role and one assigned to the ["Global Administrator"](https://azure.microsoft.com/documentation/articles/active-directory-assign-admin-roles/) role.

### Step 3:  Register the sample with your Azure Active Directory tenant

1. Sign in to the [Azure portal](https://portal.azure.com).
2. On the top bar, click on your account and under the **Directory** list, choose the Active Directory tenant where you wish to register your application.
3. Click on **More Services** in the left hand nav, and choose **Azure Active Directory**.
4. Click on **App registrations** and choose **Add**.
5. Enter a friendly name for the application, for example 'TodoListWebApp_MT' and select 'Web Application and/or Web API' as the Application Type. For the sign-on URL, enter the base URL for the sample, which is by default `https://localhost:44376/`. Click on **Create** to create the application.
6. While still in the Azure portal, choose your application, click on **Settings** and choose **Properties**.
7. Find the Application ID value and copy it to the clipboard.
8. In the same page, change the `Logout Url` property to `https://localhost:44376/Account/EndSession`.  This is the default single sign out URL for this sample.
9. Find "Multi-tenanted" switch and flip it to "yes", then click the Save option at the bottom of the page.
10. For the App ID URI, enter `https://<your_tenant_domain>/TodoListWebApp_MT`, replacing `<your_tenant_domain>` with the domain of your Azure AD tenant (either in the form `<tenant_name>.onmicrosoft.com` or your own custom domain if you registered it in Azure Active Directory). 
11. From the Settings menu, choose **Keys** and add a key - select a key duration of either 1 year or 2 years. When you save this page, the key value will be displayed, copy and save the value in a safe location - you will need this key later to configure the project in Visual Studio - this key value will not be displayed again, nor retrievable by any other means, so please record it as soon as it is visible from the Azure Portal.
12. Configure Permissions for your application - in the Settings menu, choose the 'Required permissions' section, click on **Add**, then **Select an API**, and select 'Microsoft Graph' (this is the Graph API). Then, click on  **Select Permissions** and select 'Sign in and Read User Profile'. This will allow our application to receive delegated permission to authenticate and read user profile data, for a given user account. The list of permissions provided here are known as permissions scopes, some of which require Administrator consent. See the [Graph API Permissions Scopes](https://msdn.microsoft.com/Library/Azure/Ad/Graph/api/graph-api-permission-scopes) article for more information.

### Step 5:  Configure the sample to use your Azure Active Directory tenant

At this point we are ready to paste the configuration settings into the VS project that will tie it to its entry in your Azure AD tenant. 

1. Open the solution in Visual Studio 2015.
2. Open the `appsettings.json` file.
4. Find the `ClientId` property and replace the value with the Application ID for the TodoListWebApp from the Azure portal.
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

### Troubleshooting

This sample uses a SQLite database located at `TodoListWebApp\App_Data\TodoListWebApp.db`.  The initial Entity Framework migration has been included in the `TodoListWebApp\Migrations` folder.  If you encounter database-related exceptions in the app, take the following steps:

- Ensure that the `TodoListWebApp.db` file exists.  An empty instance is included in the github repo, and the `.db` file should be created during first app run if it's not already there.
- Ensure that the tables have been created in the SQLite db.  You should be able to run `dotnet ef database update` in the `TodoListWebApp` directory to create the tables.  If that doesn't work, you can also run `dotnet ef migrations script` to generate the SQL for creating the tables.  Execute that SQL against the `.db` file using a SQLite db explorer (there are many out there), and you should be good to go.