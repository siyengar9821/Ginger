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

using Amdocs.Ginger.Common;
using Ginger.ALM.QC.TreeViewItems;
using GingerWPF.UserControlsLib.UCTreeView;
using GingerCore;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GingerCore.ALM;
using Ginger.ALM.JIRA.TreeViewItems;
using amdocs.ginger.GingerCoreNET;
using JiraRepository.Data_Contracts;
using GingerCore.ALM.JIRA;

namespace Ginger.ALM.JIRA
{
    public partial class JiraZephyrCyclesExplorerPage : Page
    {
        public enum eExplorerTestLabPageUsageType { Import, Select, BrowseFolders }

        ObservableList<BusinessFlow> mBizFlows;
        private List<string[]> mExecDetailNames;
        JiraZephyrCyclesCollection treeData;

        private ITreeViewItem mCurrentSelectedTreeItem = null;
        public string CurrentSelectedPath { get; set; }
        ObservableList<JiraZephyrVersionTreeItem> mCurrentSelectedVersions = new ObservableList<JiraZephyrVersionTreeItem>();
        ObservableList<JiraZephyrCycleTreeItem> mCurrentSelectedCycles = new ObservableList<JiraZephyrCycleTreeItem>();
        object mCurrentSelectedObject = new object();
        private string mImportDestinationPath = string.Empty;
        private bool mParentSelectionMode = false;
        GenericWindow _GenericWin = null;

        public ObservableList<JiraZephyrVersionTreeItem> CurrentSelectedVersions
        {
            get
            {
                return mCurrentSelectedVersions;
            }
            set
            {
                mCurrentSelectedVersions = value;
            }
        }

        public ObservableList<JiraZephyrCycleTreeItem> CurrentSelectedCycles
        {
            get
            {
                return mCurrentSelectedCycles;
            }
            set
            {
                mCurrentSelectedCycles = value;
            }
        }

        public JiraZephyrCyclesExplorerPage(string importDestinationPath = "", bool parentSelectionMode = false)
        {
            InitializeComponent();

            mImportDestinationPath = importDestinationPath;
            mParentSelectionMode = parentSelectionMode;

            GetTreeData();

            JiraZephyrCyclesExplorerTreeView.TreeTitle = "'" + ALMCore.DefaultAlmConfig.ALMDomain + " \\ " + ALMCore.DefaultAlmConfig.ALMProjectName + "' - Test Cycles Explorer";
            JiraZephyrCyclesExplorerTreeView.TreeTitleStyle = (Style)TryFindResource("@ucTitleStyle_3");
            JiraZephyrCyclesExplorerTreeView.Tree.ItemSelected += TestLabExplorerTreeView_ItemSelected;
            foreach (JiraZephyrRelease version in treeData.projectsReleasesList)
            {
                JiraZephyrVersionTreeItem tvv = new JiraZephyrVersionTreeItem(version.releasesCycles);
                tvv.Name = version.versionName.ToString();
                JiraZephyrCyclesExplorerTreeView.Tree.AddItem(tvv);
            }

            LoadDataBizFlows();

            // mExecDetailNames = new List<string[]>();

            ShowCycleDetailsPanel(false);
        }

        private void GetTreeData()
        {
            Mouse.OverrideCursor = Cursors.Wait;
            treeData = (JiraZephyrCyclesCollection)ALMIntegration.Instance.GetZephyrCycles();
            Mouse.OverrideCursor = null;
        }

        private void LoadExecutionDetails()
        {
            TSExecDetails.Children.Clear();
            //Total TC's
            int total = 0;
            foreach (string[] detail in mExecDetailNames)
            {
                short number;
                if (Int16.TryParse(detail[1], out number))
                {
                    total += number;
                }
            }
            Label totalTcsNum = new Label();
            totalTcsNum.Content = "Total Number of TC's: " + total;
            totalTcsNum.Style = this.FindResource("@SmallerInputFieldLabelStyle") as Style;
            totalTcsNum.Margin = new Thickness(0, 0, 0, 0);
            TSExecDetails.Children.Add(totalTcsNum);

            foreach (string[] detail in mExecDetailNames)
            {
                Label StatusName = new Label();
                StatusName.Content = detail[1] + " TC's in Status '" + detail[0] + "'";
                StatusName.Style = this.FindResource("@SmallerInputFieldLabelStyle") as Style;
                StatusName.Margin = new Thickness(0, 0, 0, 0);
                TSExecDetails.Children.Add(StatusName);
            }
        }

        /// <summary>
        /// load BF for Import ExternalID check if exists
        /// </summary>
        private void LoadDataBizFlows()
        {
            mBizFlows = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<BusinessFlow>();
        }

        private void TestLabExplorerTreeView_ItemSelected(object sender, EventArgs e)
        {
            mCurrentSelectedVersions.Clear();
            mCurrentSelectedCycles.Clear();

            TreeViewItem item = (TreeViewItem)sender;
            mCurrentSelectedTreeItem = (ITreeViewItem)item.Tag;

            if (mParentSelectionMode)
            {
                mCurrentSelectedObject = mCurrentSelectedTreeItem;
            }
            else
            {
                if (mCurrentSelectedTreeItem is JiraZephyrVersionTreeItem)
                {
                    mCurrentSelectedVersions.Add((JiraZephyrVersionTreeItem)mCurrentSelectedTreeItem);
                    ((JiraZephyrVersionTreeItem)mCurrentSelectedTreeItem).CurrentChildrens.ForEach(z => mCurrentSelectedCycles.Add((JiraZephyrCycleTreeItem)z));
                }
                if (mCurrentSelectedTreeItem is JiraZephyrCycleTreeItem)
                {
                    mCurrentSelectedCycles.Add((JiraZephyrCycleTreeItem)mCurrentSelectedTreeItem);
                }
            }
        }

        private void GetTestSetDetails(QCTestSetTreeItem testSetItem)
        {
            mExecDetailNames = testSetItem.TestSetStatuses;
            LoadExecutionDetails();
            if (testSetItem.AlreadyImported)
            {
                lblTSAlreadyImported.Content = "Yes";
                txtBoxTSMappedBF.Text = "'" + testSetItem.MappedBusinessFlowPath + "'";
            }
            else
            {
                lblTSAlreadyImported.Content = "No";
                txtBoxTSMappedBF.Text = "None";
            }
        }

        private void ShowCycleDetailsPanel(bool toShow)
        {
            if (toShow)
            {
                CycleDetailsPanel.Visibility = System.Windows.Visibility.Visible;
                testSetDetailsColumn.Width = new GridLength(200);
            }
            else
            {
                CycleDetailsPanel.Visibility = System.Windows.Visibility.Collapsed;
                testSetDetailsColumn.Width = new GridLength(0);
            }
        }

        public object ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Dialog)
        {
            if (mParentSelectionMode)
            {
                Button importBtn = new Button();
                importBtn.Content = "Select";
                importBtn.Click += new RoutedEventHandler(SelectFolder);
                GingerCore.General.LoadGenericWindow(ref _GenericWin, App.MainWindow, windowStyle, "Select Path For Export", this, new ObservableList<Button> { importBtn }, true, "Cancel", Cancel_Clicked);
                return mCurrentSelectedObject;
            }
            else
            {
                Button importBtn = new Button();
                importBtn.Content = "Import Selected";
                importBtn.Click += new RoutedEventHandler(ImportSelected);
                GingerCore.General.LoadGenericWindow(ref _GenericWin, App.MainWindow, windowStyle, "Browse JiraZephyr Cycles", this, new ObservableList<Button> { importBtn }, true, "Cancel", Cancel_Clicked);
                return CurrentSelectedVersions;
            }
        }

        private void SelectFolder(object sender, RoutedEventArgs e)
        {
            if (mCurrentSelectedTreeItem == null)
            {
                Reporter.ToUser(eUserMsgKey.StaticWarnMessage, "Please select a folder item");
            }
            else
            {
                _GenericWin.Close();
            }
        }

        private void ImportSelected(object sender, RoutedEventArgs e)
        {
            if (mCurrentSelectedTreeItem != null)
            {
                if (ALMIntegration.Instance.ImportZephyrCycle(mImportDestinationPath, (IEnumerable<object>)CurrentSelectedCycles));
                {
                    LoadDataBizFlows();
                    ShowCycleDetailsPanel(false);
                    _GenericWin.Close();
                }
            }
            else
            {
                Reporter.ToUser(eUserMsgKey.NoItemWasSelected);
            }
        }

        private void Cancel_Clicked(object sender, EventArgs e)
        {
            CurrentSelectedVersions = null;
            CurrentSelectedPath = null;
            _GenericWin.Close();
        }
    }
}
