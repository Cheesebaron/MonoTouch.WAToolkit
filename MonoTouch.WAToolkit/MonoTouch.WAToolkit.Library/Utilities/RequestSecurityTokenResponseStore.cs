//---------------------------------------------------------------------------------
// Copyright 2012 Tomasz Cielecki (tomasz@ostebaronen.dk)
// Licensed under the Apache License, Version 2.0 (the "License"); 
// You may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0 

// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, 
// INCLUDING WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR 
// CONDITIONS OF TITLE, FITNESS FOR A PARTICULAR PURPOSE, 
// MERCHANTABLITY OR NON-INFRINGEMENT. 

// See the Apache 2 License for the specific language governing 
// permissions and limitations under the License.
//---------------------------------------------------------------------------------

using System;
using System.IO;

namespace MonoTouch.WAToolkit.Library.Utilities
{
    /// <summary>
    /// Provides a class for storing a RequestSecurityTokenResponse to isolatedStorage
    /// </summary>
    public sealed class RequestSecurityTokenResponseStore
    {
        private static readonly RequestSecurityTokenResponseStore instance = new RequestSecurityTokenResponseStore();

        private static readonly string SettingsDir = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
        private const string SettingsFile = "monodroid.watoolkit.library.RequestSecurityTokenResponse.xml";

        private RequestSecurityTokenResponseStore() { }

        public static RequestSecurityTokenResponseStore Instance
        {
            get
            {
                return instance;
            }
        }

        /// <summary>
        /// Gets or sets the configured RequestSecurityTokenResponse
        /// </summary>
        /// <remarks>Get returns null if no RequestSecurityTokenResponse has been configured. </remarks>
        public RequestSecurityTokenResponse RequestSecurityTokenResponse
        {
            get
            {
                return RetrieveSettingFromFile<RequestSecurityTokenResponse>(SettingsDir, SettingsFile);
            }
            set
            {
                SaveSettingToFile(SettingsDir, SettingsFile, value);
            }
        }

        /// <summary>
        /// Gets or sets the security token from the configured RequestSecurityTokenResponse
        /// </summary>
        /// <remarks>Get returns null if no RequestSecurityTokenResponse has been configured. </remarks>
        public string SecurityToken
        {
            get
            {
                return null == RequestSecurityTokenResponse ? "" : RequestSecurityTokenResponse.securityToken;
            }
        }

        /// <summary>
        /// Checks if there is a valid RequestSecurityTokenResponse currrenlty in the store.
        /// </summary>
        /// <remarks>Returns true if there is a RequestSecurityTokenResponse in the store and it has not expired,
        /// otherwise retruns false</remarks>
        public bool ContainsValidRequestSecurityTokenResponse()
        {
            if ( null == RequestSecurityTokenResponse )
            {
                return false;
            }            

            return true;
        }

        private T RetrieveSettingFromFile<T>(string dir, string file) where T : class
        {
            var fn = Path.Combine(dir, file);

            if (File.Exists(fn))
            {
                try
                {
                    using (var stream = File.Open(fn, FileMode.Open, FileAccess.Read))
                    {
                        return (T)SerializationHelper.DeserializeData<T>(stream);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Could not retrieve file " + fn + ". With Exception: " + ex.Message);
                }
            }
            return null;
        }

        private void SaveSettingToFile<T>(string dir, string file, T data)
        {
            var fn = Path.Combine(dir, file);

            if (null == data)
            {
                try
                {
                    if (File.Exists(fn)) File.Delete(fn);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Could not delete file " + fn + ". With Exception: " + ex.Message);
                }
            }
            else
            {
                try
                {
                    if (File.Exists(fn)) File.Delete(fn);

                    using (var stream = File.Open(fn, FileMode.CreateNew, FileAccess.Write))
                    {
                        SerializationHelper.SerializeData<T>(data, stream);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Could not save file " + fn + ". With Exception: " + ex.Message);
                }
            }
        }
    }
}