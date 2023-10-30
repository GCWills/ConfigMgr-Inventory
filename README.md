# **ConfigMgr.Inventory**

## **Summary:**
Provides a simple interface to extend and manage Configuration Managers Hardware Inventory Schema through the Schema and Extension classes.

## **Usage:**
                
```csharp
///////////////////////////////////////////////////////////////////////////////////////////////////////////

//Define Inventory Extension
Extension inventoryExtension = new InventoryExtension()
{
    SMSClassID = "MICROSOFT|PMPC_USERAPPS|1.0",
    ClassName = "PMPC_UserApps",
    SMSGroupName = "PMPC UserApps",
    Properties = new string[]
    {
        "InstallLocation",
        "DisplayName",
        "DisplayVersion",
        "QuietUninstallString",
        "UninstallString",
        "Publisher",
        "InstallDate",
        "User"
    }
};

//or use schema class to populate Extension Collection
Extension[] inventoryExtensions = Schema.GetFromJson("pathToJson")

///////////////////////////////////////////////////////////////////////////////////////////////////////////

//define connection options if required
ConnectionOptions options = new ConnectionOptions();
ManagementScope scope = new ManagementScope($@"\\{smsProviderServer}\ROOT\SMS\site_{siteCode}", options);

//Or define scope as simple string
string scope = $@"\\{smsProviderServer}\ROOT\SMS\site_{siteCode}";

///////////////////////////////////////////////////////////////////////////////////////////////////////////

foreach ( Extension inventoryExtension in inventoryExtensions )
{
  //Install
  inventoryExtension.Install(scope)  
  
  //Enable
  inventoryExtension.Enable(scope);
}

///////////////////////////////////////////////////////////////////////////////////////////////////////////
```

## **ConfigMgr.Inventory.Schema**

### **Methods**
- **GetFromJson(path)** : Returns a collection of Extensions defined in the json.

## **ConfigMgr.Inventory.Extension**

### **Methods**

- **Install(scope)** : Installs / updates a custom class in the Hardware Inventory Schema.
- **Uninstall(scope)**: Removes Extension from Hardware Inventory Schema. Deletes related data from database.
- **Enable(scope)**: Enables the class for reporting on default client settings.
- **Disable(scope)**:	Disables the class for reporting on default client settings.

### **Method Arguments**
All Methods must be provided the **ManagementScope**. This is the Management Path to the SMS Provider Servers WMI Namespace:
```csharp
//define connection options if required
ConnectionOptions options = new ConnectionOptions();
ManagementScope scope = new ManagementScope($@"\\{smsProviderServer}\ROOT\SMS\site_{siteCode}", options);

//Or define scope as simple string
string scope = $@"\\{smsProviderServer}\ROOT\SMS\site_{siteCode}";
```

### **Properties**
- **(String) SMSClassID**: The Class ID that will be used to generate the database table, view, and the UI SDK class.
- **(String) ClassName**: The name of the WMI Class. 
- **(String) SMSGroupName**: The default class name displayed in the MOF editor and Resource Explorer if no localized resources are provided.
- **(String[]) Properties**: The list of properties belonging to the WMI Class to be inventoried. It is assumed that the first property in the array will be the key property in the database.

# **Requirements**
- **System.Management** Namespace (available as a Nuget Package in .NET Core)