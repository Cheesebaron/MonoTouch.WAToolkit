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
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Xml;
using System.Web;

namespace MonoTouch.WAToolkit.Library.Utilities
{
    /// <summary>
    /// Contains the data returned in a RequestSecurityTokenResponse
    /// </summary>
    [DataContract]
    public class RequestSecurityTokenResponse
    {
        private const string WsSecuritySecExtNamespace = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd";
        private const string BinarySecurityTokenName = "BinarySecurityToken";

        string _token;
        string _tokenType;
        long _tokenExpiration;
        long _tokenCreated;

        /// <summary>
        /// The raw string value of the security token contained in the RequestSecurityTokenResponse
        /// </summary>
        [DataMember]
        public string securityToken
        {
            get
            {
               return _token;
            }
            set
            {
                _token = value;
            }
        }

        /// <summary>
        /// The uri which uniquely identifies the type of token contained in the RequestSecurityTokenResponse
        /// </summary>
        [DataMember]
        public string tokenType
        {
            get
            {
                return _tokenType;
            }
            set
            {
                _tokenType = value;
            }
        }

        /// <summary>
        /// The expiration time of the token in the RequestSecurityTokenResponse
        /// </summary>
        [DataMember]
        public long expires
        {
            get
            {
                return _tokenExpiration;
            }
            set
            {
                _tokenExpiration = value;
            }
        }

        /// <summary>
        /// The creation time of the token in the RequestSecurityTokenResponse
        /// </summary>
        [DataMember]
        public long created
        {
            get
            {
                return _tokenCreated;
            }
            set
            {
                _tokenCreated = value;
            }
        }

        internal static RequestSecurityTokenResponse FromJSON(string jsonRequestSecurityTokenService)
        {
            using (var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(jsonRequestSecurityTokenService)))
            {
                var serializer = new DataContractJsonSerializer(typeof(RequestSecurityTokenResponse));
                var returnToken = serializer.ReadObject(memoryStream) as RequestSecurityTokenResponse;

                if (null != returnToken)
                {
                    returnToken.securityToken = HttpUtility.HtmlDecode( returnToken.securityToken );

                    using ( var sr = new StringReader( returnToken.securityToken ) )
                    {
                        using ( var reader = XmlReader.Create( sr ) )
                        {
                            reader.MoveToContent();                    
                            var binaryToken = reader.ReadElementContentAsString( BinarySecurityTokenName, WsSecuritySecExtNamespace );
                            var tokenBytes = Convert.FromBase64String(binaryToken);
                            returnToken._token = Encoding.UTF8.GetString(tokenBytes, 0, tokenBytes.Length);
                        }
                    }
                }
                return returnToken;
            }
        }

        public bool IsExpired
        {
            get
            {
                var result = true;
                if (expires > 0)
                {
                    var now = ConvertToUnixTimestamp(DateTime.UtcNow);
                    var diff = now - expires;

                    result = diff >= 0;
                }
                return result;
            }
        }

        static long ConvertToUnixTimestamp(DateTime date)
        {
            var origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            var diff = date - origin;
            return (long)Math.Floor(diff.TotalSeconds);
        }

        public override string ToString()
        {
            return string.Format("Created {0}\nExpires {1}\nIsExpired {2}\nType {3}\nToken {4}", created, expires, IsExpired, tokenType, securityToken);
        }
    }
}