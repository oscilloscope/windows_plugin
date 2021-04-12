using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic.Devices;
namespace ComputerProperties
{
    public class DeviceProperties
    {

        public static string getComputerName()
        {
            return Environment.MachineName.ToString();
        }

        public static string getOSVersion()
        {
            return new ComputerInfo().OSVersion;
        }
        public static string getOSFullName()
        {
            return new ComputerInfo().OSFullName;
        }
        public static string getPhysicalMemory()
        {
            return new ComputerInfo().TotalPhysicalMemory.ToString();
        }

       
        public static string getCPUName()
        {
            ManagementObjectSearcher mos = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_Processor");
            foreach (ManagementObject mo in mos.Get())
            {                
                return mo["Name"].ToString();
            }

            return "Cannot obtain CPU";            
        }
        public static string getGPUName()
        {
            ManagementObjectSearcher objvide = new ManagementObjectSearcher("select * from Win32_VideoController");
            List<string> gpuItems= new List<string>();
            foreach (ManagementObject mo in objvide.Get())
            {
                gpuItems.Add("\"" + mo["Name"].ToString() + "\"");
            }
            string asd = string.Join(", ", gpuItems);
            return asd;

        }       
    }
}
