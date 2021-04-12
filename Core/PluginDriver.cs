/*
	Copyright (c) 2011, pGina Team
	All rights reserved.

	Redistribution and use in source and binary forms, with or without
	modification, are permitted provided that the following conditions are met:
		* Redistributions of source code must retain the above copyright
		  notice, this list of conditions and the following disclaimer.
		* Redistributions in binary form must reproduce the above copyright
		  notice, this list of conditions and the following disclaimer in the
		  documentation and/or other materials provided with the distribution.
		* Neither the name of the pGina Team nor the names of its contributors 
		  may be used to endorse or promote products derived from this software without 
		  specific prior written permission.

	THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
	ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
	WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
	DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY
	DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
	(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
	LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
	ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
	(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
	SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Xml.Schema;
using ComputerProperties;
using log4net;
using Newtonsoft.Json.Linq;
using pGina.Plugin.Securify;
using pGina.Shared.Interfaces;
using pGina.Shared.Types;
using System.Web.Script.Serialization;
using System.Text.RegularExpressions;
using System.Linq;

namespace pGina.Core
{
    public class PluginDriver
    {
        private Guid m_sessionId = Guid.NewGuid();
        private SessionProperties m_properties = null;
        private ILog m_logger = null;


        private string computerName;
        private string cpuName;
        private string gpuName;
        private string osFullName;
        private string osVersion;
        private string totalPhysicalMemory;
        private string platform = "windows";


        // prop json
        private string propJson;

        // Plugin results
        Dictionary<string, string> pluginResults = new Dictionary<string, string>();

        // userid variable for securifyid plugin
        private string securifyUserID;


        public Guid SessionId
        {
            get { return m_sessionId; }
            set
            {
                m_sessionId = value;
                m_properties.Id = value;
                m_properties.AddTrackedObject("SessionId", new Guid(m_sessionId.ToString()));
            }
        }

        public PluginDriver()
        {
            m_logger = LogManager.GetLogger(string.Format("PluginDriver:{0}", m_sessionId));

            m_properties = new SessionProperties(m_sessionId);

            // Add the user information object we'll be using for this session
            UserInformation userInfo = new UserInformation();
            m_properties.AddTrackedSingle<UserInformation>(userInfo);

            // Add the plugin tracking object we'll be using for this session
            PluginActivityInformation pluginInfo = new PluginActivityInformation();
            pluginInfo.LoadedAuthenticationGatewayPlugins = PluginLoader.GetOrderedPluginsOfType<IPluginAuthenticationGateway>();
            pluginInfo.LoadedAuthenticationPlugins = PluginLoader.GetOrderedPluginsOfType<IPluginAuthentication>();
            pluginInfo.LoadedAuthorizationPlugins = PluginLoader.GetOrderedPluginsOfType<IPluginAuthorization>();
            m_properties.AddTrackedSingle<PluginActivityInformation>(pluginInfo);

            m_logger.DebugFormat("New PluginDriver created");
        }

        public UserInformation UserInformation
        {
            get { return m_properties.GetTrackedSingle<UserInformation>(); }
        }

        public List<GroupInformation> GroupInformation
        {
            get { return m_properties.GetTrackedSingle<UserInformation>().Groups; }
        }

        public SessionProperties SessionProperties
        {
            get { return m_properties; }
        }

        public static void Starting()
        {
            foreach (IPluginBase plugin in PluginLoader.AllPlugins)
                plugin.Starting();
        }

        public static void Stopping()
        {
            foreach (IPluginBase plugin in PluginLoader.AllPlugins)
                plugin.Stopping();
        }
        public string DictionaryToString(Dictionary<string, string> dictionary)
        {
            string dictionaryString = "";
            foreach (KeyValuePair<string, string> keyValues in dictionary)
            {

                //var json = new JavaScriptSerializer().Serialize(keyValues.Value);
                //JObject o = JObject.Parse(json);
                //string result = (string)o["Result"];
                //string message = (string)o["Message"];


                dictionaryString += keyValues.Key + " : " + keyValues.Value + ", ";
               // dictionaryString += keyValues.Key + " : " + "{" + "\"" + result +"\":" + "\"" + message + "\"" + "}"+ ", ";
            }
            // return dictionaryString.TrimEnd(',', ' ').Substring(dictionaryString.Length - 3);
            return dictionaryString.Remove(dictionaryString.Length - 2);
        }
        public BooleanResult PerformLoginProcess()
        {
            try
            {
                // Set the original username to gputhe current username if not already set
                UserInformation userInfo = m_properties.GetTrackedSingle<UserInformation>();
                if (string.IsNullOrEmpty(userInfo.OriginalUsername))
                    userInfo.OriginalUsername = userInfo.Username;

                // Execute login
                BeginChain();
                BooleanResult result = ExecuteLoginChain();
                EndChain();

                return result;
            }
            catch (Exception e)
            {
                // We catch exceptions at a high level here and report failure in these cases,
                //  with the exception details as our message for now
                m_logger.ErrorFormat("Exception during login process: {0}", e);
                return new BooleanResult() { Success = false, Message = string.Format("Unhandled exception during login: {0}", e) };
            }
        }
        public void sendLog2(string _url, string platform, string log_type, bool result, string _tenant, string _tenantapikey, string _userid, string width, string height, string ipaddress, string __deviceid, string __notification_type, string __notification_create_date, string __sessionid, string __phonenumber, string token)
        {
            try
            {
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(_url);

                var response = "";
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "POST";
                httpWebRequest.Accept = "*/*";



                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    string json = "{\"tenantid\":\"" + _tenant + "\"," +
                                  "\"email\":\"" + _userid.ToLower() + "\"," +
                                  "\"userid\":\"" + _userid.ToLower() + "\"," +
                                  "\"log_type\":\"" + log_type + "\"," +
                                  "\"ipaddress\":\"" + ipaddress + "\"," +
                                  "\"screen_width\":\"" + width + "\"," +
                                  "\"screen_height\":\"" + height + "\"," +
                                  "\"device_type_name\":\"" + "" + "\"," +
                                  "\"platform\":\"" + platform + "\"," +
                                  "\"tenantapikey\":\"" + _tenantapikey.ToLower() + "\"," +
                                  "\"sessionid\":\"" + __sessionid + "\"," +
                                  "\"phone_number\":\"" + __phonenumber + "\"," +
                                  (token == "" ? "" : "\"token\":\"" + token + "\",").ToString() +
                                  //"\"device_id\":\"" + __deviceid.Replace("\"", "") + "\"," +
                                  "\"device_id\":" + __deviceid + "," +
                                  "\"notification_type\":\"" + __notification_type + "\"," +
                                  "\"notification_create_date\":\"" + __notification_create_date + "\"," +
                                  "\"result\":\"" + result.ToString().ToLower() + "\"}";

                    streamWriter.Write(json);
                }
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    response = streamReader.ReadToEnd();
                }


                // Console.WriteLine("Return response: " + response);

            }
            catch
            {
                //Console.WriteLine("Network is unreachable");
            }
        }
        private BooleanResult ExecuteLoginChain()
        {
            m_logger.DebugFormat("Performing login process");
            UserInformation userInfo = m_properties.GetTrackedSingle<UserInformation>();

            computerName = DeviceProperties.getComputerName().ToLower();
            cpuName = DeviceProperties.getCPUName().ToLower();
            gpuName = DeviceProperties.getGPUName().ToLower();
            osFullName = DeviceProperties.getOSFullName().ToLower();
            osVersion = DeviceProperties.getOSVersion().ToLower();
            totalPhysicalMemory = DeviceProperties.getPhysicalMemory().ToLower();

            BooleanResult result = AuthenticateUser();

            

            

            string[] screenSize;
            PluginImpl securifyPlugin = new PluginImpl();
            dynamic securifySetting = securifyPlugin.getSecurifySetting;
            string ipaddress = obtainIPAddress((string)securifySetting.url + "public_ip");
                                    
            try
            {
                screenSize = DeviceProp.ScreenSize();
            }
            catch
            {
                screenSize = new string[2];
                // Console.WriteLine("Cannot obtain screen size");
            }



            if (!result.Success) {

                propJson = "{\"pluginResults\":{" + DictionaryToString(pluginResults) + "}," +
                        "\"computer_name\":\"" + computerName + "\"," +
                                  "\"cpu_name\":\"" + cpuName + "\"," +
                                  "\"gpu_name\":[" + gpuName + "]," +
                                  "\"os_fullname\":\"" + osFullName + "\"," +
                                  "\"os_version\":\"" + osVersion + "\"," +
                                  "\"total_physical_memory\":\"" + totalPhysicalMemory + "\"}";
                propJson = propJson.ToLower().Replace("ı", "i");
                //sendLog((string)securifySetting.log, pluginName, pluginResult.Success, (string)securifySetting.tenant, userInfo.Username, screenSize[0], screenSize[1], ipaddress);
                sendLog2((string)securifySetting.log, platform, "windows_fail_login", false, (string)securifySetting.tenant, (string)securifySetting.apiKey,
                    securifyUserID, screenSize[0], screenSize[1], ipaddress, propJson, "", "", "", "", (string)securifySetting.token);
                //result.Message = "Invalid Credentials";
                // send windows fail log
                return result;
            }

            result = AuthorizeUser();
            if (!result.Success)
                return result;

            string temp_username = userInfo.Username;

            result = GatewayProcess();

            temp_username = userInfo.Username;

           
            if (result.Success)
            {
                bool allPluginsSuccess = !(result.Message == "One of the plugins failed.");
                pluginResults.Add("\"" + "SingleUser" + "\"", "{\"Success \": \"" + allPluginsSuccess + " ( Last username: <" + temp_username + ">)" + "\", \"Message\": \"" + (allPluginsSuccess ? "windows_login_success" : "One of the plugins failed") + "\"}");

                propJson = "{\"pluginResults\":{" + DictionaryToString(pluginResults) + "}," +
                       "\"computer_name\":\"" + computerName + "\"," +
                                 "\"cpu_name\":\"" + cpuName + "\"," +
                                 "\"gpu_name\":[" + gpuName + "]," +
                                 "\"os_fullname\":\"" + osFullName + "\"," +
                                 "\"os_version\":\"" + osVersion + "\"," +
                                 "\"total_physical_memory\":\"" + totalPhysicalMemory + "\"}";
                propJson = propJson.ToLower().Replace("ı", "i");

                sendLog2((string)securifySetting.log, platform, allPluginsSuccess? "windows_login_success":"One of the plugins failed", allPluginsSuccess, (string)securifySetting.tenant, (string)securifySetting.apiKey,
                  securifyUserID, screenSize[0], screenSize[1], ipaddress, propJson, "", "", "", "", (string)securifySetting.token);
            }
            else
            {

                if(result.Message != null) 
                { 
                    if (result.Message.Contains("rejected"))
                    {
                        pluginResults.Add("\"" + "SingleUser" + "\"", "{\"Success \": \"" + result.Success + " ( Last username: <" + temp_username + ">)" + "\", \"Message\": \"" + "confirmation_fail" + "\"}");


                        propJson = "{\"pluginResults\":{" + DictionaryToString(pluginResults) + "}," +
                       "\"computer_name\":\"" + computerName + "\"," +
                                 "\"cpu_name\":\"" + cpuName + "\"," +
                                 "\"gpu_name\":[" + gpuName + "]," +
                                 "\"os_fullname\":\"" + osFullName + "\"," +
                                 "\"os_version\":\"" + osVersion + "\"," +
                                 "\"total_physical_memory\":\"" + totalPhysicalMemory + "\"}";
                        propJson = propJson.ToLower().Replace("ı", "i");







                        sendLog2((string)securifySetting.log, platform, "confirmation_fail", false, (string)securifySetting.tenant, (string)securifySetting.apiKey,
                     securifyUserID, screenSize[0], screenSize[1], ipaddress, propJson, "", "", "", "", (string)securifySetting.token);
                    }
                    else
                    {
                        pluginResults.Add("\"" + "SingleUser" + "\"", "{\"Success \": \"" + result.Success + " ( Last username: <" + temp_username + ">)" + "\", \"Message\": \"" + "windows_fail_login" + "\"}");

                        propJson = "{\"pluginResults\":{" + DictionaryToString(pluginResults) + "}," +
                       "\"computer_name\":\"" + computerName + "\"," +
                                 "\"cpu_name\":\"" + cpuName + "\"," +
                                 "\"gpu_name\":[" + gpuName + "]," +
                                 "\"os_fullname\":\"" + osFullName + "\"," +
                                 "\"os_version\":\"" + osVersion + "\"," +
                                 "\"total_physical_memory\":\"" + totalPhysicalMemory + "\"}";
                        propJson = propJson.ToLower().Replace("ı", "i");


                        sendLog2((string)securifySetting.log, platform, "windows_fail_login", false, (string)securifySetting.tenant, (string)securifySetting.apiKey,
                         securifyUserID, screenSize[0], screenSize[1], ipaddress, propJson, "", "", "", "", (string)securifySetting.token);
                    }
                }
            }
            //Console.WriteLine(result.Message);
            //Console.WriteLine(result.Success);
            return result;
        }

        public void BeginChain()
        {
            List<IStatefulPlugin> plugins = PluginLoader.GetEnabledStatefulPlugins();
            m_logger.DebugFormat("Begin login chain, {0} stateful plugin(s).", plugins.Count);
            foreach (IStatefulPlugin plugin in plugins)
            {
                plugin.BeginChain(m_properties);
            }
        }

        public void EndChain()
        {
            List<IStatefulPlugin> plugins = PluginLoader.GetEnabledStatefulPlugins();
            m_logger.DebugFormat("End login chain, {0} stateful plugin(s).", plugins.Count);
            foreach (IStatefulPlugin plugin in plugins)
            {
                plugin.EndChain(m_properties);
            }
        }


        
        public string obtainIPAddress(string ipCheck)
        {
            string publicIp = "";

            var httpPublicIP = (HttpWebRequest)WebRequest.Create(ipCheck);

            publicIp = "";
            httpPublicIP.ContentType = "application/json";
            httpPublicIP.Method = "GET";
            httpPublicIP.Accept = "*/*";

            string ipaddress2 = "";
            try
            {


                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                var httpResponse = (HttpWebResponse)httpPublicIP.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    publicIp = streamReader.ReadToEnd();
                }



                // Console.WriteLine(publicIp);
                ipaddress2 = publicIp;
                try
                {
                JObject joResponse = JObject.Parse(ipaddress2);
                string ip4 = (string)joResponse["ip"];
                    return ip4;
                }
                catch
                {                    
                    return "";
                }
                
            }
            catch
            {
                return "Could not obtain IP address";
            }
        }



        public BooleanResult AuthenticateUser()
        {
            PluginActivityInformation pluginInfo = m_properties.GetTrackedSingle<PluginActivityInformation>();
            List<IPluginAuthentication> plugins = PluginLoader.GetOrderedPluginsOfType<IPluginAuthentication>();

            m_logger.DebugFormat("Authenticating user {0}, {1} plugins available", m_properties.GetTrackedSingle<UserInformation>().Username, plugins.Count);
            
            
            // At least one must succeed
            BooleanResult finalResult = new BooleanResult() { Success = false };
            string pluginName = "";

           

            

            

            foreach (IPluginAuthentication plugin in plugins)
            {
                m_logger.DebugFormat("Calling {0}", plugin.Uuid);
                

                BooleanResult pluginResult = new BooleanResult() { Message = null, Success = false };

                try
                {
                    pluginResult = plugin.AuthenticateUser(m_properties);
                    UserInformation userInfo = m_properties.GetTrackedSingle<UserInformation>();
                    pluginName = plugin.Name;
                    if (pluginName.Contains("SecurifyID") == true)
                    {

                        string temp_username = UserInformation.Username;
                        string logMessage = pluginResult.Message;
                        logMessage = logMessage.Replace("__________", temp_username);

                        string output = String.Join(";", Regex.Matches(logMessage, @"\<(.+?)\>")
                                  .Cast<Match>()
                                  .Select(m => m.Groups[1].Value));


                        string[] IDs = output.Split(';');
                        try
                        {

                            securifyUserID = IDs[1];
                        }
                        catch {
                            securifyUserID = "";
                        }

                        pluginResults.Add("\"" + pluginName + "\"", "{\"Success \": \"" + pluginResult.Success.ToString() + " (" + logMessage + ")" + "\", \"Message\": \"" + pluginResult.Message + "\"}");
                    }
                    else {
                        pluginResults.Add("\"" + pluginName + "\"", "{\"Success \": \"" + pluginResult.Success.ToString() + "\", \"Message\": \"" + pluginResult.Message + "\"}");
                    }

                    //sendLog((string)securifySetting.log, pluginName, pluginResult.Success, (string)securifySetting.tenant, userInfo.Username, screenSize[0], screenSize[1], ipaddress);


                    pluginInfo.AddAuthenticateResult(plugin.Uuid, pluginResult);
                    m_logger.DebugFormat("Calling {0}", plugin.Uuid);                   

                    if (pluginResult.Success)
                    {
                        m_logger.DebugFormat("{0} Succeeded", plugin.Uuid);
                        finalResult.Success = true;
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(pluginResult.Message))
                        {
                            m_logger.WarnFormat("{0} Failed with Message: {1}", plugin.Uuid, pluginResult.Message);
                            finalResult.Message = pluginResult.Message;
                        }
                        else
                        {
                            m_logger.WarnFormat("{0} Failed without a message", plugin.Uuid);
                        }
                    }
                }
                catch (Exception e)
                {
                    m_logger.ErrorFormat("{0} Threw an unexpected exception, assuming failure: {1}", plugin.Uuid, e);
                }
            }

            if (finalResult.Success)
            {
                // Clear any errors from plugins if we did succeed
                finalResult.Message = null;
                m_logger.InfoFormat("Successfully authenticated {0}", m_properties.GetTrackedSingle<UserInformation>().Username);
            }
            else
            {
                m_logger.ErrorFormat("Failed to authenticate {0}, Message: {1}", m_properties.GetTrackedSingle<UserInformation>().Username, finalResult.Message);
            }

            return finalResult;
        }


        public void sendLog(string _url, string platform, bool result, string _tenant, string _userid, string width, string height, string ipaddress)
        {
            try
            {
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(_url);

                var response = "";
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "POST";
                httpWebRequest.Accept = "*/*";



                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    string json = "{\"tenantid\":\"" + _tenant + "\"," +
                                  "\"email\":\"" + _userid + "\"," +
                                  "\"userid\":\"" + _userid + "\"," +
                                  "\"ipaddress\":\"" + ipaddress + "\"," +
                                  "\"screen_width\":\"" + width + "\"," +
                                  "\"screen_height\":\"" + height + "\"," +
                                  "\"device_type_name\":\"" + "" + "\"," +
                                  "\"platform\":\"" + platform + "\"," +
                                  "\"result\":\"" + result + "\"}";
                                        
                    streamWriter.Write(json);
                }
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    response = streamReader.ReadToEnd();
                }

                
                //Console.WriteLine("Return response: " + response);

            }
            catch
            {
                // Console.WriteLine("Network is unreachable");
            }
        }



        



        public BooleanResult AuthorizeUser()
        {
            PluginActivityInformation pluginInfo = m_properties.GetTrackedSingle<PluginActivityInformation>();
            List<IPluginAuthorization> plugins = PluginLoader.GetOrderedPluginsOfType<IPluginAuthorization>();

            m_logger.DebugFormat("Authorizing user {0}, {1} plugins available", m_properties.GetTrackedSingle<UserInformation>().Username, plugins.Count);

            foreach (IPluginAuthorization plugin in plugins)
            {
                m_logger.DebugFormat("Calling {0}", plugin.Uuid);

                BooleanResult pluginResult = new BooleanResult() { Message = null, Success = false };

                try
                {
                    pluginResult = plugin.AuthorizeUser(m_properties);
                    pluginInfo.AddAuthorizationResult(plugin.Uuid, pluginResult);

                    // All must succeed, fail = total fail
                    if (!pluginResult.Success)
                    {
                        m_logger.ErrorFormat("{0} Failed to authorize {1} message: {2}", plugin.Uuid, m_properties.GetTrackedSingle<UserInformation>().Username, pluginResult.Message);
                        return pluginResult;
                    }
                }
                catch (Exception e)
                {
                    m_logger.ErrorFormat("{0} Threw an unexpected exception, treating this as failure: {1}", plugin.Uuid, e);
                    return pluginResult;
                }
            }

            m_logger.InfoFormat("Successfully authorized {0}", m_properties.GetTrackedSingle<UserInformation>().Username);
            return new BooleanResult() { Success = true };
        }

        public BooleanResult GatewayProcess()
        {
            PluginActivityInformation pluginInfo = m_properties.GetTrackedSingle<PluginActivityInformation>();
            List<IPluginAuthenticationGateway> plugins = PluginLoader.GetOrderedPluginsOfType<IPluginAuthenticationGateway>();

            m_logger.DebugFormat("Processing gateways for user {0}, {1} plugins available", m_properties.GetTrackedSingle<UserInformation>().Username, plugins.Count);
            string pluginMessage = "";
            foreach (IPluginAuthenticationGateway plugin in plugins)
            {
                m_logger.DebugFormat("Calling {0}", plugin.Uuid);

                BooleanResult pluginResult = new BooleanResult() { Message = null, Success = false };

                try
                {
                    pluginResult = plugin.AuthenticatedUserGateway(m_properties);
                    pluginInfo.AddGatewayResult(plugin.Uuid, pluginResult);
                    pluginMessage = (pluginResult.Message.Contains("One of the plugins failed.") ? "One of the plugins failed." : null);


                    
                    

                    // All must succeed, fail = total fail
                    if (!pluginResult.Success)
                    {
                        m_logger.ErrorFormat("{0} Failed to process gateway for {1} message: {2}", plugin.Uuid, m_properties.GetTrackedSingle<UserInformation>().Username, pluginResult.Message);
                        return pluginResult;
                    }
                }
                catch (Exception e)
                {
                    m_logger.ErrorFormat("{0} Threw an unexpected exception, treating this as failure: {1}", plugin.Uuid, e);
                    return pluginResult;
                }
            }

            m_logger.InfoFormat("Successfully processed gateways for {0}", m_properties.GetTrackedSingle<UserInformation>().Username);
            
            return new BooleanResult() { Success = true, Message = pluginMessage };
        }
    }
}
