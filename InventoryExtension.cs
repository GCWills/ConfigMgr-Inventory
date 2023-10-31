using System.Management;
using System.Text.Json;

namespace ConfigurationManager.ManagedObjectFormat
{
    //Container for Extensions. Includes Json deserialisation method.
    public class Schema
    {
        public Extension[] InventoryExtension { get; set; }

        public static Extension[] GetFromJson(string path)
        {
            if (File.Exists(path))
            {
                string jsonData = File.ReadAllText(path);
                return JsonSerializer.Deserialize<Schema>(jsonData).InventoryExtension;
            }
            throw new NotImplementedException("File not found.");
        }
    }

    //Represents a Class such as PMPC_UserApps. All Properties needed to extend Inventory Schema. Methods to Install, Uninstall, Enable and Disable from schema.
    public class Extension
    {
        public string SMSClassID { get; set; }
        public string SMSGroupName { get; set; }
        public string Namespace { get; set; }
        public string ClassName { get; set; }
        public Dictionary<string, int> Properties { get; set; }

        //Adds class to the schema but doesnt enable
        public void Install(ManagementScope scope)
        {
            try
            {
                using (ManagementClass smsInventoryClass = new ManagementClass(scope, new ManagementPath("SMS_InventoryClass"), null))
                {
                    //Define class with instance of object
                    ManagementObject newClass = smsInventoryClass.CreateInstance();
                    newClass["SMSClassID"] = SMSClassID;
                    newClass["ClassName"] = ClassName;
                    newClass["SMSGroupName"] = SMSGroupName;
                    newClass["Namespace"] = Namespace;
                    newClass["IsDeletable"] = true;

                    // Create an array of SMS_InventoryClassProperty Objects
                    ManagementObject[] propertyValues = new ManagementObject[Properties.Keys.Count];
                    ManagementClass propertyClass = new ManagementClass(scope, new ManagementPath("SMS_InventoryClassProperty"), null);

                    //Loop through properties and spawn a SMS_InventoryClassProperty Instance to add to the ManagmentObject Array
                    int i = 0;
                    foreach (KeyValuePair<string, int> property in Properties)
                    {
                        // Create property instances for the class
                        ManagementObject propertyObj = propertyClass.CreateInstance();
                        propertyObj["PropertyName"] = property.Key;
                        propertyObj["Type"] = property.Value;
                        propertyObj["Width"] = 2048;

                        //First property provided will be the key because yes
                        if (i == 0)
                        {
                            propertyObj["IsKey"] = 1;
                        }

                        propertyValues[i] = propertyObj;

                        i++;
                    }

                    //Add the SMS_InventoryClassProperty[] to the SMS_InventoryClass.Properties
                    newClass["Properties"] = propertyValues;
                    newClass.Put();
                    Console.WriteLine($"{ClassName} : INSTALLED");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ClassName} : FAILED TO INSTALL...: {ex.Message} || {ex.StackTrace}");
            }
        }

        //This completely deletes the class from the hardware inventory schema and marks all data related to the class for deletion in the database.
        public void Uninstall(ManagementScope scope)
        {
            try
            {
                //Query to get existing Instance that matches SMSClassID
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, new ObjectQuery($"SELECT * FROM SMS_InventoryClass WHERE SMSClassID = '{SMSClassID}'")))
                {
                    ManagementObject smsInventoryClass = searcher.Get().Cast<ManagementObject>().FirstOrDefault();

                    if (smsInventoryClass == null)
                    {
                        Console.WriteLine($"The {ClassName} provided for uninstall does not exist.");
                        return;
                    }

                    smsInventoryClass.Delete();
                    Console.WriteLine($"{ClassName} : UNINSTALLED");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ClassName} : FAILED TO INSTALL...: {ex.Message} || {ex.StackTrace}");
            }
        }

        // Inventory classes may be "Installed" but not enabled for reporting. This method enables them for reporting on default client settings. This is the equivalent of ticking a class in the Hardware Inventory Client settings page.
        public void Enable(ManagementScope scope)
        {
            try
            {
                //Open Inventory Report for hardware inventory - {00000000-0000-0000-0000-000000000001} is the ID for hwinv report
                using (ManagementObject inventoryReport = new ManagementObject(scope, new ManagementPath("SMS_InventoryReport.InventoryReportID='{00000000-0000-0000-0000-000000000001}'"), null))
                {
                    // Create a new SMS_InventoryReportClass to add to the InventoryReport
                    ManagementBaseObject newReportClass;
                    using (ManagementClass reportClass = new ManagementClass(scope, new ManagementPath("SMS_InventoryReportClass"), null))
                    {
                        newReportClass = reportClass.CreateInstance();
                        newReportClass["SMSClassID"] = SMSClassID;
                        newReportClass["ReportProperties"] = Properties.Keys.ToArray();
                        newReportClass["Timeout"] = 6000;
                    }

                    // create new array and append the new class
                    ManagementBaseObject[] existingReportClasses = inventoryReport["ReportClasses"] as ManagementBaseObject[];
                    ManagementBaseObject[] newReportClasses = new ManagementBaseObject[existingReportClasses.Length + 1];
                    existingReportClasses.CopyTo(newReportClasses, 0);
                    newReportClasses[newReportClasses.Length - 1] = newReportClass;

                    // Update the ReportClasses property of the inventory report with the new array
                    inventoryReport["ReportClasses"] = newReportClasses;
                    inventoryReport.Put();
                    Console.WriteLine($"{ClassName} : ENABLED");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ClassName} : FAILED TO ENABLE...: {ex.Message} || {ex.StackTrace}");
            }
        }

        //Disabling is the equivalent of unticking an "Installed" Inventory extension from the hardware inventory client settings page. Clients will no longer collect the data but existing data will not be deleted and inventory can resume anytime the class is enabled again.
        public void Disable(ManagementScope scope)
        {
            try
            {
                ManagementObject inventoryReport = new ManagementObject(scope, new ManagementPath("SMS_InventoryReport.InventoryReportID='{00000000-0000-0000-0000-000000000001}'"), null);

                // Get existing ReportClasses property
                ManagementBaseObject[] existingReportClasses = inventoryReport["ReportClasses"] as ManagementBaseObject[];

                //Create new array of ReportClasses with the target class removed
                ManagementBaseObject[] newReportClasses = existingReportClasses
                    .Where(inventoryClass => !string.Equals(inventoryClass["SMSClassID"] as string, SMSClassID, StringComparison.OrdinalIgnoreCase))
                    .ToArray();

                // Update the ReportClasses property of the inventory report
                inventoryReport["ReportClasses"] = newReportClasses;
                inventoryReport.Put();
                Console.WriteLine($"{ClassName} : DISABLED");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ClassName} : FAILED TO DISABLE...: {ex.Message} || {ex.StackTrace}");

                // Rethrow a different exception
                throw new InvalidOperationException("Custom error message", ex);
            }
        }
    }
}