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

using System.Runtime.Serialization;
using System.Collections.Generic;

using MonoTouch.UIKit;
using MonoTouch.Foundation;

namespace MonoTouch.WAToolkit.Library.Utilities
{
    /// <summary>
    /// DataContract for IdentityProviderInformation returned by the Identity Provider Discover Service
    /// </summary>
    [DataContract]
    public class IdentityProviderInformation
    {
        private UIImage _image;

        /// <summary>
        /// The display name for the identity provider.
        /// </summary>
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// The url used for Login to the identity provider.
        /// </summary>
        [DataMember]
        public string LoginUrl { get; set; }

        /// <summary>
        /// The url used for Login out the identity provider.
        /// </summary>
        [DataMember]
        public string LogoutUrl { get; set; }

        /// <summary>
        /// The url that is used to retrieve the image for the identity provider
        /// </summary>
        [DataMember]
        public string ImageUrl { get; set; }

        /// <summary>
        /// A list fo email address suffixes configured for the identity provider.
        /// </summary>
        [DataMember]
        public IEnumerable<string> EmailAddressSuffixes { get; set; }

        /// <summary>
        /// The image populated by calling LoadImageFromImageUrl
        /// </summary>
        public UIImage Image
        {
            get
            {
                return _image;
            }
        }

        /// <summary>
        /// Retieves the image from ImageUrl
        /// </summary>
        /// <returns>The image from the url as a BitmapImage</returns>
        public UIImage LoadImageFromImageUrl()
        {
            _image = null;

            if (string.Empty != ImageUrl)
            {
                _image = UIImage.LoadFromData(NSData.FromUrl(new NSUrl(ImageUrl)));
            }

            return _image;
        }

        public override string ToString()
        {
            return string.Format("Name: {0}, LoginUrl: {1}, LogoutUrl: {2}, ImageUrl: {3}, EmailAddressSuffixes: {4}", Name, LoginUrl, LogoutUrl, ImageUrl, EmailAddressSuffixes);
        }
    }
}