# **ConfigurationManager.ManagedObjectFormat**

## **Summary:**
Provides a simple interface to extend and manage ConfigMgr's Hardware Inventory MOF Schema.

## **Usage:**
                
```csharp
///////////////////////////////////////////////////////////////////////////////////////////////////////////

//Define Inventory Extension
Extension inventoryExtension = new InventoryExtension()
{
    SMSClassID   = "MICROSOFT|PMPC_USERAPPS|1.0",
    ClassName    = "PMPC_UserApps",
    SMSGroupName = "PMPC UserApps",
    Namespace    =  "\\\\\\\\.\\\\root\\\\cimv2"
    Properties   = new Dictionary<string, int>
    {
        {"InstallLocation", 8}
        {"DisplayName", 8}
        {"DisplayVersion", 8}
        {"QuietUninstallString", 8}
        {"UninstallString", 8}
        {"Publisher", 8}
        {"InstallDate", 8}
        {"User" 8}
    }
};

//or use schema class to populate Extension Collection
Extension[] inventoryExtensions = Schema.GetFromJson("pathToJson")

///////////////////////////////////////////////////////////////////////////////////////////////////////////

ConnectionOptions options = new ConnectionOptions();
ManagementScope scope = new ManagementScope($@"\\{smsProviderServer}\ROOT\SMS\site_{siteCode}", options);

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

## **Schema Class**

### **Methods**
- **GetFromJson(path)** : Returns a collection of Extensions defined in the json.

## **Extension Class**

### **Methods**

- **Install(scope)** : Installs / updates a custom class in the Hardware Inventory Schema.
- **Uninstall(scope)**: Removes Extension from Hardware Inventory Schema. Deletes related data from database.
- **Enable(scope)**: Enables the class for reporting on default client settings.
- **Disable(scope)**:	Disables the class for reporting on default client settings.

### **Method Arguments**
All Methods must be provided the **ManagementScope** Object. This is the Management Path to the SMS Provider Servers WMI Namespace
```csharp
//define connection options if required
ConnectionOptions options = new ConnectionOptions();
ManagementScope scope     = new ManagementScope($@"\\{smsProviderServer}\ROOT\SMS\site_{siteCode}", options);
```

### **Properties**
- **(String) SMSClassID**: The Class ID that will be used to generate the database table, view, and the UI SDK class.
- **(String) ClassName**: The name of the WMI Class. 
- **(String) SMSGroupName**: The name displayed in the Resource Explorer and Hardware Inventory UI.
- **(String) Namespace**: The WMI namespace
- **(Dictionarty<string,int>) Properties**: Dictionary of properties belonging to the WMI Class to be inventoried. The first property in the dictionary will be used as the key in database table. The int defines the CIM type of the property. See list of Type codes:

| Value | CIM Type |
|----------|----------|
| 8 | String |
| 11 | Boolean |
| 13 | Object |
| 18 | Uint16 |
| 19 | Uint32 |
| 101 | DateTime |
| 8200 | String[] |
| 1210 | Uint16[] |

# **Requirements**
- **System.Management** (Nuget Package)
