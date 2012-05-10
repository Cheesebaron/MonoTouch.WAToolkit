// Modified by Tomasz Cielecki (tomasz@ostebaronen.dk) 2012

// ----------------------------------------------------------------------------------
// Microsoft Developer & Platform Evangelism
// 
// Copyright (c) Microsoft Corporation. All rights reserved.
// 
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, 
// EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES 
// OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
// ----------------------------------------------------------------------------------
// The example companies, organizations, products, domain names,
// e-mail addresses, logos, people, places, and events depicted
// herein are fictitious.  No association with any real company,
// organization, product, domain name, email address, logo, person,
// places, or events is intended or should be inferred.
// ----------------------------------------------------------------------------------

//---------------------------------------------------------------------------------
// Copyright 2010 Microsoft Corporation
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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Linq;
using System.Globalization;

using MonoTouch.WAToolkit.Library.EventArguments;
using MonoTouch.UIKit;

namespace MonoTouch.WAToolkit.Library.Utilities
{
    internal class JSONIdentityProviderDiscoveryClient
    {
        internal event EventHandler<GetIdentityProviderListEventArgs> GetIdentityProviderListCompleted;

        private class StateBag
        {
            public HttpWebRequest Request { get; set; }
            public Action<Exception, string> Callback { get; set; }
        }

        internal void GetIdentityProviderListAsync(Uri identityProviderListServiceEndpoint)
        {
            var request = (HttpWebRequest)WebRequest.Create(identityProviderListServiceEndpoint);
            var state = new StateBag { Request = request, Callback = ParseJson };
            request.BeginGetResponse(DownloadStreamCompleted, state);
			
			var app = UIApplication.SharedApplication;
			app.NetworkActivityIndicatorVisible = true;
        }

        private static void DownloadStreamCompleted(IAsyncResult result)
        {
            var state = (StateBag)result.AsyncState;
            try
            {
                var response = state.Request.EndGetResponse(result);
                using (var stream = response.GetResponseStream())
                {
                    using (var reader = new StreamReader(stream))
                    {
                        var text = reader.ReadToEnd();
                        state.Callback(null, text);
                    }
                }
            }
            catch (Exception ex)
            {
                state.Callback(ex, null);
            }
			finally
			{
				var app = UIApplication.SharedApplication;
				app.NetworkActivityIndicatorVisible = false;
			}
        }

        private void ParseJson(Exception e, string json)
        {
            IEnumerable<IdentityProviderInformation> identityProviders = null;
            var error = e;

            if (null == e)
            {
                try
                {
                    using (var ms = new MemoryStream(Encoding.Unicode.GetBytes(json)))
                    {
                        var serializer = new DataContractJsonSerializer(typeof(IEnumerable<IdentityProviderInformation>));
                        identityProviders = serializer.ReadObject(ms) as IEnumerable<IdentityProviderInformation>;

                        var windowsLiveId = identityProviders.FirstOrDefault(i => i.Name.Equals("Windows Live™ ID", StringComparison.InvariantCultureIgnoreCase));
                        if (windowsLiveId != null)
                        {
                            var separator = windowsLiveId.LoginUrl.Contains("?") ? "&" : "?";
                            windowsLiveId.LoginUrl = string.Format(CultureInfo.InvariantCulture, "{0}{1}pcexp=false", windowsLiveId.LoginUrl, separator);
                        }
                    }
                }
                catch(Exception ex)
                {
                    error = ex;
                }
            }

            if (null != GetIdentityProviderListCompleted)
            {
                GetIdentityProviderListCompleted(this, new GetIdentityProviderListEventArgs( identityProviders, error ));
            }
        }
    }
}