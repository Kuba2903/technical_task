using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml;

class Program
{
    static void Main()
    {

        string xmlContent = "input_file_example.xml";
        
        // parse to xml
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.Load(xmlContent);
 
        // build employee hierarchy
        var employees = ParseEmployees(xmlDoc);

        var hierarchy = BuildHierarchy(employees);

        // convert to json output
        string json = JsonSerializer.Serialize(hierarchy, new JsonSerializerOptions { WriteIndented = true });
        
        File.WriteAllText("output.json", json);

        Console.WriteLine(json);
    }


    /// <summary>
    /// Creates objects based on xml data
    /// </summary>
    /// <param name="xmlDoc"></param>
    /// <returns></returns>

    static Dictionary<string, Employee> ParseEmployees(XmlDocument xmlDoc)
    {
        var employees = new Dictionary<string, Employee>();

        foreach (XmlNode node in xmlDoc.SelectNodes("//employee"))
        {
            string email = node.SelectSingleNode("field[@id='email']").InnerText;
            string manager = node.SelectSingleNode("field[@id='manager']")?.InnerText;

            if (!employees.ContainsKey(email))
            {
                employees[email] = new Employee { Email = email, DirectReports = new List<WrappedEmployee>(),
                    ManagerEmail = manager};
            }
        }

        return employees;
    }

    /// <summary>
    /// Iterates through every employee and if the employee doesn't have a manager email
    /// he goes to the top of hierarchy, if he has a manager we get the value of this manager
    /// and add that employee to the list of manager direct reports
    /// </summary>
    /// <param name="employees"></param>
    /// <returns></returns>
    static List<WrappedEmployee> BuildHierarchy(Dictionary<string, Employee> employees)
    {
        var hierarchy = new List<WrappedEmployee>();

        foreach (var employee in employees.Values)
        {
            var wrappedEmployee = new WrappedEmployee { Employee = employee };

            if (string.IsNullOrEmpty(employee.ManagerEmail))
            {
                hierarchy.Add(wrappedEmployee);
            }
            else if (employees.TryGetValue(employee.ManagerEmail, out var manager))
            {
                manager.DirectReports.Add(wrappedEmployee);
            }
        }

        return hierarchy;
    }
}

class Employee
{
    public string Email { get; set; }
    [JsonIgnore]
    public string ManagerEmail { get; set; }
    public List<WrappedEmployee> DirectReports { get; set; }
}


class WrappedEmployee
{
    public Employee Employee { get; set; }
}