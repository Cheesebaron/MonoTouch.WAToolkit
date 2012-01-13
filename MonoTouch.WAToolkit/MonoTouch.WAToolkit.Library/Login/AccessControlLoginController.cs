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
using System.Web;
using System.Linq;

using MonoTouch.UIKit;
using MonoTouch.Foundation;

using MonoTouch.WAToolkit.Library.Utilities;
using MonoTouch.WAToolkit.Library.EventArguments;
using System.Collections.Generic;

namespace MonoTouch.WAToolkit.Library.Login
{
	public class AccessControlLoginController : UITableViewController
	{
		private Uri _identityProviderDiscoveryService = null;
		private string realm = null;
		private string acsNamespace = null;
		
		public AccessControlLoginController(string realm, string acsNamespace) : base(UITableViewStyle.Grouped)
		{
			this.realm = realm;
			this.acsNamespace = acsNamespace;
		}
		
		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			
			this.Title = "Log in";
			
			TableView.DataSource = new AccessControlLoginDataSource(null);
			TableView.Delegate = new AccessControlLoginDelegate(this, null);
			
			GetSecurityToken();
		}
		
		public void GetSecurityToken(Uri identityProviderDiscoveryService)
        {
            _identityProviderDiscoveryService = identityProviderDiscoveryService;
			
			JSONIdentityProviderDiscoveryClient jsonClient = new JSONIdentityProviderDiscoveryClient();
            jsonClient.GetIdentityProviderListCompleted += new EventHandler<GetIdentityProviderListEventArgs>(IdentityProviderList_RefreshCompleted);
            jsonClient.GetIdentityProviderListAsync(_identityProviderDiscoveryService);
        }

        public void GetSecurityToken()
        {
            if (null == realm)
            {
                throw new InvalidOperationException("Realm was not set");
            }

            if (null == acsNamespace)
            {
                throw new InvalidOperationException("ServiceNamespace was not set");
            }

            Uri identityProviderDiscovery = new Uri(
                string.Format(
                    "https://{0}.accesscontrol.windows.net/v2/metadata/IdentityProviders.js?protocol=javascriptnotify&realm={1}&version=1.0",
                    acsNamespace,
                    HttpUtility.UrlEncode(realm)),
                UriKind.Absolute
                );

            GetSecurityToken(identityProviderDiscovery);
        }
		
		private void IdentityProviderList_RefreshCompleted(object sender, GetIdentityProviderListEventArgs e)
        {
			InvokeOnMainThread(() => {
            	if (null == e.Error)
            	{
					TableView.DataSource = new AccessControlLoginDataSource(e.Result);
					TableView.Delegate = new AccessControlLoginDelegate(this, e.Result);
					TableView.ReloadData();
            	}
            	else
            	{
            	}
			});
        }
	}
	
	public class AccessControlLoginDataSource : UITableViewDataSource
	{
		private IList<SectionData> _sectionDataList;
		private IEnumerable<IdentityProviderInformation> _providerInformation;
		
		public AccessControlLoginDataSource (IEnumerable<IdentityProviderInformation> providerInformation)
		{
			this._providerInformation = providerInformation;
			
			CreateTableData();
		}
		
		private void CreateTableData()
		{
			_sectionDataList = new List<SectionData>();
			
			SectionData section1 = new SectionData("IdentityProviderInformation");
			section1.Footer = "Log in the application with your account of choice.";
			
			if (null != _providerInformation)
			{
				foreach (IdentityProviderInformation provider in _providerInformation)
				{
					Data section1Data = new Data();
					section1Data.Accessory = UITableViewCellAccessory.DisclosureIndicator;
					section1Data.Label = provider.Name;
					section1Data.CellStyle = UITableViewCellStyle.Default;
					if (provider.ImageUrl != null || provider.ImageUrl != "")
						provider.LoadImageFromImageUrl();
					section1Data.Image = provider.Image;
					
					section1.sData.Add(section1Data);
				}
			}
			
			_sectionDataList.Add(section1);
		}
		
		public override int NumberOfSections (UITableView tableView)
		{
			return _sectionDataList == null ? 0 : _sectionDataList.Count;
		}
		
		public override string TitleForHeader (UITableView tableView, int section)
        {
			return _sectionDataList == null ? "" : _sectionDataList[section].Title;
        }
                
		public override string TitleForFooter (UITableView tableView, int section)
		{
			return _sectionDataList == null ? "" : _sectionDataList[section].Footer;
		}
                
        public override int RowsInSection (UITableView tableview, int section)
        {
                return _sectionDataList[section].sData.Count;
        }
		
		public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
        {       
			SectionData sectionData = _sectionDataList[indexPath.Section];
			string cellId = sectionData.CellId;
			Data row = sectionData.sData[indexPath.Row];
                        
			UITableViewCell cell = tableView.DequeueReusableCell(cellId);

			if (cell == null)
				cell = new UITableViewCell(row.CellStyle, cellId);
                        
			cell.TextLabel.Text = row.Label;
			cell.Accessory = row.Accessory;
			if (row.Image != null)
				cell.ImageView.Image = row.Image;
			if (row.CellStyle == UITableViewCellStyle.Subtitle)
				cell.DetailTextLabel.Text = row.Subtitle;

			return cell; 
        }
		
		private class SectionData
        {
			public string Title { get;set; }
			public string Footer { get;set; }
			public string CellId { get;set; }
			public IList<Data> sData { get;set; }

			public SectionData(string cellId)
			{
				Title = "";
				Footer = "";
				CellId = cellId;
				sData = new List<Data>();
			}
        }
                
        private class Data
        {
            public string Label { get; set; }
            public string Subtitle { get; set; }
            public UITableViewCellAccessory Accessory { get; set; }
            public UITableViewCellStyle CellStyle { get; set; }
            public UIImage Image { get; set; }
        }
	}
	
	public class AccessControlLoginDelegate: UITableViewDelegate
	{
		private AccessControlLoginController controller;
		private IEnumerable<IdentityProviderInformation> providerInformation;
		
		public AccessControlLoginDelegate (AccessControlLoginController controller, IEnumerable<IdentityProviderInformation> providerInformation)
		{
			this.controller = controller;
			this.providerInformation = providerInformation;
		}
		
		public override void RowSelected (UITableView tableView, NSIndexPath indexPath)
        {
			UIViewController nextController = null;
			
			if (providerInformation != null)
			{
				string loginUrl = providerInformation.ElementAt(indexPath.Row).LoginUrl;
				nextController = new AccessControlWebAuthController(loginUrl);
				nextController.Title = providerInformation.ElementAt(indexPath.Row).Name;
			}
			
			if (nextController != null)
				controller.NavigationController.PushViewController(nextController, true); //NavigationController er null!
		}
	}
}

