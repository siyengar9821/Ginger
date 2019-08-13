#region License
/*
Copyright © 2014-2019 European Support Limited

Licensed under the Apache License, Version 2.0 (the "License")
you may not use this file except in compliance with the License.
You may obtain a copy of the License at 

http://www.apache.org/licenses/LICENSE-2.0 

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS, 
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
See the License for the specific language governing permissions and 
limitations under the License. 
*/
#endregion

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Repository;
using Ginger.UserControls;
using GingerCore;
using GingerCore.Platforms;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Ginger.BusinessFlowWindows
{
    /// <summary>
    /// Interaction logic for EditBusinessFlowAppsPage.xaml
    /// </summary>
    public partial class EditBusinessFlowAppsPage : Page
    {
         BusinessFlow mBusinessFlow;
         ObservableList<TargetBase> mApplications = new ObservableList<TargetBase>();
         GenericWindow _pageGenericWin = null;
         private bool IsNewBusinessflow = false;
        public EditBusinessFlowAppsPage(BusinessFlow BizFlow, bool IsNewBF = false)
         {
            
             InitializeComponent();

             this.Title = "Edit " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " Target Application(s)";

             mBusinessFlow = BizFlow;
            IsNewBusinessflow = IsNewBF;
             SetGridView();
         }

         private void SetGridView()
         {
             GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
             view.GridColsView = new ObservableList<GridColView>();

             view.GridColsView.Add(new GridColView() { Field = nameof(TargetBase.Selected), WidthWeight = 20, StyleType = GridColView.eGridColStyleType.CheckBox });
             view.GridColsView.Add(new GridColView() { Field = nameof(TargetBase.Name), Header = "Application Name", WidthWeight = 50, ReadOnly=true, BindingMode=BindingMode.OneWay});
             view.GridColsView.Add(new GridColView() { Field = nameof(TargetBase.Platform), Header = "Platform", WidthWeight = 30, ReadOnly = true, BindingMode = BindingMode.OneWay });

             AppsGrid.SetAllColumnsDefaultView(view);
             AppsGrid.InitViewItems();

             foreach (TargetBase AP in  WorkSpace.Instance.Solution.TargetApplications)
             {
                TargetApplication AP1 = new TargetApplication();
                 AP1.AppName = AP.Name;
                 AP1.Platform = AP.Platform;

                 // If this App was selected before then mark it 
                 if ((from x in mBusinessFlow.TargetApplicationsKeys where x.Guid == AP.Guid select x).FirstOrDefault() != null)
                 {
                     AP1.Selected = true;
                 }

                 mApplications.Add(AP1);
             }

             AppsGrid.DataSourceList = mApplications;
         }

        private void OKButton_Click(object sender, RoutedEventArgs e)
       {
            if (IsNewBusinessflow == true)
            {
                SetTargetApplications();
                if (mBusinessFlow.TargetApplicationsKeys?.Count != 0)
                {
                    mBusinessFlow.CurrentActivity.TargetApplicationKey = mBusinessFlow.TargetApplicationsKeys[0];
                }
            }
            else
            {               
                SetTargetApplications();
                if (mBusinessFlow.TargetApplicationsKeys.Count == 1)
                {
                    foreach (Activity activity in mBusinessFlow.Activities)
                    {
                        activity.TargetApplicationKey = mBusinessFlow.TargetApplicationsKeys[0];
                    }
                }
            }
            if (mBusinessFlow.TargetApplicationsKeys.Count > 0 || mApplications.Count==0)
            {
                _pageGenericWin.Close();
            }
            else
            {
                Reporter.ToUser(eUserMsgKey.BusinessFlowNeedTargetApplication);
            }
        }

        public void ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Dialog,bool ShowCancelButton=true)
        {
            Button okBtn = new Button();
            okBtn.Content = "Ok";
            okBtn.Click += new RoutedEventHandler(OKButton_Click);
            ObservableList<Button> winButtons = new ObservableList<Button>();
            winButtons.Add(okBtn);

            GingerCore.General.LoadGenericWindow(ref _pageGenericWin, App.MainWindow, windowStyle, this.Title, this, winButtons, ShowCancelButton, "Cancel");
        }

        internal void ResetPlatformSelection()
        {
            foreach (var item in mApplications)
            {
                item.Selected=false;
            }
        }

        public void SetTargetApplications()
        {
            //mBusinessFlow.TargetApplications.Clear();
            //remove deleted
            for(int indx=0;indx< mBusinessFlow.TargetApplicationsKeys.Count;indx++)
            {
                TargetBase taregtApp = mApplications.Where(x => x.Selected && x.Guid == mBusinessFlow.TargetApplicationsKeys[indx].Guid).FirstOrDefault();
                if (taregtApp == null)
                {
                    mBusinessFlow.TargetApplicationsKeys.RemoveAt(indx);
                    indx--;
                }
                else
                {
                    mBusinessFlow.TargetApplicationsKeys[0].ItemName = taregtApp.Name;//making sure name is in sync
                }
            }

            //add new
            foreach (TargetBase TA in mApplications.Where(x=>x.Selected).ToList())
            {                
                if (mBusinessFlow.TargetApplicationsKeys.Where(x=>x.Guid == TA.Guid).FirstOrDefault() == null)
                {
                    mBusinessFlow.TargetApplicationsKeys.Add(TA.Key);
                }
            }
        }
    }
}
