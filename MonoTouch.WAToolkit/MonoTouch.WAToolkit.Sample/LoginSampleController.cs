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
using System.Collections.Generic;
using System.Globalization;
using MonoTouch.UIKit;

using MonoTouch.WAToolkit.Library.Login;
using MonoTouch.WAToolkit.Library.Utilities;

namespace MonoTouch.WAToolkit.Sample
{
	public class LoginSampleController : UITableViewController
	{
		private LoginSampleDataSource _loginDataSource;
		
		public LoginSampleController ()  : base(UITableViewStyle.Grouped)
		{
		}
		
		public override void LoadView ()
		{
			base.LoadView ();
			
			var token = RequestSecurityTokenResponseStore.Instance.RequestSecurityTokenResponse;
			_loginDataSource = new LoginSampleDataSource(token);
			TableView.DataSource = _loginDataSource;
			Title = "WAToolkit";
		}
		
		public override void ViewWillAppear (bool animated)
		{			
			var loginButton = new UIBarButtonItem("Log In", UIBarButtonItemStyle.Bordered, null);
			loginButton.Clicked += HandleLoginButtonClicked;
			this.NavigationItem.RightBarButtonItem = loginButton;
		}

		void HandleLoginButtonClicked (object sender, EventArgs e)
		{
            // Remember to set namespace and realm here, otherwise no Identity Providers will be shown!
			var login = new AccessControlLoginController("uri://security.noisesentinel.com/Phone7", "bruelandkjaer");
			NavigationController.PushViewController(login, true);
			
		}
	}
			
	public class LoginSampleDataSource : UITableViewDataSource
	{
		private readonly RequestSecurityTokenResponse _data;
		private IList<SectionData> _sectionDataList;
		
		public LoginSampleDataSource(RequestSecurityTokenResponse tokenResponse)
		{
			_data = tokenResponse;
			CreateTable();
		}
		
		private void CreateTable()
		{
			_sectionDataList = new List<SectionData>();

		    var section1 = new SectionData("IdentityProviderInformation")
		    {
		        Title =
		            "Below you should see the token from your Identity Provider. If not press Log In in the Navigation Bar to get a new token."
		    };

		    if (null != _data)
			{
				string[] fields = {"Created", "Expires", "Is Expired", "Token Type", "Token"};
				foreach (var field in fields)
				{
				    var section1Data = new Data
				    {
				        Accessory = UITableViewCellAccessory.None, 
                        Label = field
				    };
				    switch (field)
					{
						case "Created": section1Data.Subtitle = _data.created.ToString(CultureInfo.InvariantCulture); break;
						case "Expires": section1Data.Subtitle = _data.expires.ToString(CultureInfo.InvariantCulture); break;
						case "Is Expired": section1Data.Subtitle = _data.IsExpired.ToString(CultureInfo.InvariantCulture); break;
						case "Token Type": section1Data.Subtitle = _data.tokenType.ToString(CultureInfo.InvariantCulture); break;
						case "Token": section1Data.Subtitle = _data.securityToken; break;
					}

					section1Data.CellStyle = UITableViewCellStyle.Subtitle;
				
					section1.SData.Add(section1Data);
				}
			}
			_sectionDataList.Add(section1);
		}
		
		public override int RowsInSection (UITableView tableView, int section)
		{
			return _sectionDataList[section].SData.Count;
		}
		
		public override int NumberOfSections (UITableView tableView)
		{
			return _sectionDataList == null ? 0 : _sectionDataList.Count;
		}
		
		public override UITableViewCell GetCell (UITableView tableView, Foundation.NSIndexPath indexPath)
		{
			var sectionData = _sectionDataList[indexPath.Section];
			var cellId = sectionData.CellId;
			var row = sectionData.SData[indexPath.Row];
                        
			var cell = tableView.DequeueReusableCell(cellId) ?? new UITableViewCell(row.CellStyle, cellId);

		    cell.TextLabel.Text = row.Label;
			cell.Accessory = row.Accessory;
			if (row.CellStyle == UITableViewCellStyle.Subtitle)
				cell.DetailTextLabel.Text = row.Subtitle;

			return cell;
		}
		
		private class SectionData
        {
			public string Title { private get;set; }
			public string CellId { get; private set; }
			public IList<Data> SData { get; private set; }

			public SectionData(string cellId)
			{
				Title = "";
				CellId = cellId;
				SData = new List<Data>();
			}
        }
                
        private class Data
        {
            public string Label { get; set; }
            public string Subtitle { get; set; }
            public UITableViewCellAccessory Accessory { get; set; }
            public UITableViewCellStyle CellStyle { get; set; }
        }
	}
}

