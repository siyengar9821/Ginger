#region License
/*
Copyright © 2014-2020 European Support Limited

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
using GingerCore;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using Amdocs.Ginger.Common;
using System.Windows.Data;
using System.Linq;

namespace Ginger.ALM
{
    /// <summary>
    /// Interaction logic for ALMConnectionPage.xaml
    /// </summary>
    public partial class ALMConnectionPage : Page
    {
        //Show as connection or settings window
        bool isConnWin;
        bool isServerDetailsCorrect;
        bool isProjectMappingCorrect;
        GenericWindow _pageGenericWin;
        ALMIntegration.eALMConnectType almConectStyle;

        private void Bind()
        {
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(ServerURLTextBox, TextBox.TextProperty, CurrentAlmConfigurations, nameof(CurrentAlmConfigurations.ALMServerURL));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(RestAPICheckBox, CheckBox.IsCheckedProperty, CurrentAlmConfigurations, nameof(CurrentAlmConfigurations.UseRest));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(UserNameTextBox, TextBox.TextProperty, CurrentAlmUserConfigurations, nameof(CurrentAlmUserConfigurations.ALMUserName));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(DomainComboBox, ComboBox.SelectedValueProperty, CurrentAlmConfigurations, nameof(CurrentAlmConfigurations.ALMDomain));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(ProjectComboBox, ComboBox.SelectedValueProperty, CurrentAlmConfigurations, nameof(CurrentAlmConfigurations.ALMProjectKey));
            PasswordTextBox.Password = CurrentAlmUserConfigurations.ALMPassword; //can't do regular binding with PasswordTextBox control for security reasons
        }
        public ALMConnectionPage(ALMIntegration.eALMConnectType almConnectStyle, bool isConnWin = false)
        {
            CurrentAlmConfigurations = ALMIntegration.Instance.GetDefaultAlmConfig();
            CurrentAlmUserConfigurations = ALMIntegration.Instance.GetCurrentAlmUserConfig(CurrentAlmConfigurations.AlmType);
            ALMIntegration.Instance.UpdateALMType(CurrentAlmConfigurations.AlmType);

            InitializeComponent();
            this.isConnWin = isConnWin;
            this.almConectStyle = almConnectStyle;

            Bind();

            if (!WorkSpace.Instance.BetaFeatures.Rally)
            {
                RallyRadioButton.Visibility = Visibility.Hidden;
                if (CurrentAlmConfigurations.AlmType == GingerCoreNET.ALMLib.ALMIntegration.eALMType.RALLY)
                    CurrentAlmConfigurations.AlmType = GingerCoreNET.ALMLib.ALMIntegration.eALMType.QC;
            }

            if (almConnectStyle != ALMIntegration.eALMConnectType.Silence)
            {
                if (GetProjectsDetails())
                    ConnectProject();
            }
            
            SetControls();
            StyleRadioButtons();
            ChangeALMType();
        }

        private GingerCoreNET.ALMLib.ALMConfig CurrentAlmConfigurations { get; set; }
        private GingerCoreNET.ALMLib.ALMUserConfig CurrentAlmUserConfigurations { get; set; }
        
        private void SetControls()
        {
            bool ServerDetailsSelected = false;

            if (!string.IsNullOrEmpty(ServerURLTextBox.Text) && !string.IsNullOrEmpty(UserNameTextBox.Text) && !string.IsNullOrEmpty(PasswordTextBox.Password))
                ServerDetailsSelected = true;

            ALMSelectPanel.Visibility = Visibility.Visible;

            ALMServerDetailsPanel.Visibility = Visibility.Visible;
            if (ServerDetailsSelected)
                LoginServerButton.IsEnabled = true;
            else LoginServerButton.IsEnabled = false;


            if (isServerDetailsCorrect)
            {
                QCRadioButton.IsEnabled = false;
                RestAPICheckBox.IsEnabled = false;
                RQMRadioButton.IsEnabled = false;
                RallyRadioButton.IsEnabled = false;
                JiraRadioButton.IsEnabled = false;
                qTestRadioButton.IsEnabled = false;
                RQMLoadConfigPackageButton.IsEnabled = false;
                ServerURLTextBox.IsEnabled = false;
                UserNameTextBox.IsEnabled = false;
                PasswordTextBox.IsEnabled = false;

                LoginServerButton.Content = "Change Server Details";
                ALMProjectDetailsPanel.Visibility = Visibility.Visible;
                if (CurrentAlmConfigurations.AlmType == GingerCoreNET.ALMLib.ALMIntegration.eALMType.RQM)
                {
                    ALMDomainSelectionPanel.Visibility = Visibility.Collapsed;
                    LoginServerButton.Content = "Get Projects Details";
                }
                else if (CurrentAlmConfigurations.AlmType == GingerCoreNET.ALMLib.ALMIntegration.eALMType.Qtest)
                {
                    ALMDomainSelectionPanel.Visibility = Visibility.Collapsed;
                }
                else ALMDomainSelectionPanel.Visibility = Visibility.Visible;
            }
            else
            {
                QCRadioButton.IsEnabled = true;
                RestAPICheckBox.IsEnabled = true;
                RQMRadioButton.IsEnabled = true;
                RallyRadioButton.IsEnabled = true;
                JiraRadioButton.IsEnabled = true;
                RQMLoadConfigPackageButton.IsEnabled = true;
                qTestRadioButton.IsEnabled = true;
                if (CurrentAlmConfigurations.AlmType == GingerCoreNET.ALMLib.ALMIntegration.eALMType.RQM)
                    ServerURLTextBox.IsEnabled = false;
                else
                {
                    ServerURLTextBox.IsEnabled = true;
                    ServerURLTextBox.IsReadOnly = false;
                }
                UserNameTextBox.IsEnabled = true;
                PasswordTextBox.IsEnabled = true;
                if (isConnWin)
                    LoginServerButton.Content = "Connect ALM Server";
                else LoginServerButton.Content = "Get Projects Details";
                ALMProjectDetailsPanel.Visibility = Visibility.Collapsed;
            }

            if (isProjectMappingCorrect)
            {
                DomainComboBox.IsEnabled = false;
                ProjectComboBox.IsEnabled = false;
                ConnectProjectButton.Content = "Change Project Mapping";
            }
            else
            {
                DomainComboBox.IsEnabled = true;
                ProjectComboBox.IsEnabled = true;
                if (isConnWin)
                    ConnectProjectButton.Content = "Connect";
                else ConnectProjectButton.Content = "Save Project Mapping";
            }
        }

        private void ChangeALMType()
        {
            if (CurrentAlmConfigurations.AlmType == GingerCoreNET.ALMLib.ALMIntegration.eALMType.QC && QCRadioButton.IsChecked == false)
            {
                QCRadioButton.IsChecked = true;
            }
            else if (CurrentAlmConfigurations.AlmType == GingerCoreNET.ALMLib.ALMIntegration.eALMType.RQM && RQMRadioButton.IsChecked == false)
            {
                RQMRadioButton.IsChecked = true;
            }
            else if (CurrentAlmConfigurations.AlmType == GingerCoreNET.ALMLib.ALMIntegration.eALMType.Jira && JiraRadioButton.IsChecked == false)
            {
                JiraRadioButton.IsChecked = true;
            }
            else if (CurrentAlmConfigurations.AlmType == GingerCoreNET.ALMLib.ALMIntegration.eALMType.RALLY && RallyRadioButton.IsChecked == false)
            {
                RallyRadioButton.IsChecked = true;
            }
            else if (CurrentAlmConfigurations.AlmType == GingerCoreNET.ALMLib.ALMIntegration.eALMType.Qtest && qTestRadioButton.IsChecked == false)
            {
                qTestRadioButton.IsChecked = true;
            }
        }

        private bool GetProjectsDetails()
        {
            Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;

            bool almConn = false;
            ALMIntegration.Instance.UpdateALMType(CurrentAlmConfigurations.AlmType);

            if (LoginServerButton.Content.ToString() == "Get Projects Details" || LoginServerButton.Content.ToString() == "Connect ALM Server")
            {
                almConn = ALMIntegration.Instance.TestALMServerConn(almConectStyle);
                if (almConn)
                {
                    RefreshDomainList(almConectStyle);
                    RefreshProjectsList();
                }

            }

            isServerDetailsCorrect = almConn;
            if (!isServerDetailsCorrect)
                isProjectMappingCorrect = false;

            SetControls();
            Mouse.OverrideCursor = null;
            return almConn;
        }

        private void GetProjectsDetails_Clicked(object sender, RoutedEventArgs e)
        {
            almConectStyle = ALMIntegration.eALMConnectType.Manual;
            GetProjectsDetails();
        }

        private void RefreshDomainList(ALMIntegration.eALMConnectType userMsgStyle)
        {
            List<string> Domains = ALMIntegration.Instance.GetALMDomains(userMsgStyle);

            string currDomain = CurrentAlmConfigurations.ALMDomain;
            DomainComboBox.Items.Clear();
            foreach (string domain in Domains)
                DomainComboBox.Items.Add(domain);

            if (DomainComboBox.Items.Count > 0)
            {
                if (string.IsNullOrEmpty(currDomain) == false)
                {
                    if (DomainComboBox.Items.Contains(currDomain))
                    {
                        CurrentAlmConfigurations.ALMDomain = currDomain;
                        DomainComboBox.SelectedIndex = DomainComboBox.Items.IndexOf(CurrentAlmConfigurations.ALMDomain);
                    }
                }
                if (DomainComboBox.SelectedIndex == -1)
                    DomainComboBox.SelectedIndex = 0;
            }
        }

        private void DomainComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender == null || ALMSettingsPannel == null || DomainComboBox.SelectedItem == null)
                return;

            RefreshProjectsList();
        }

        private void RefreshProjectsList()
        {
            if (ALMIntegration.Instance.TestALMServerConn(almConectStyle))
            {
                string currSelectedDomain = CurrentAlmConfigurations.ALMDomain;
                if (CurrentAlmConfigurations.AlmType != GingerCoreNET.ALMLib.ALMIntegration.eALMType.Qtest)
                {
                    if (string.IsNullOrEmpty(currSelectedDomain))
                    {
                        if (string.IsNullOrEmpty(CurrentAlmConfigurations.ALMDomain))
                            return;

                        currSelectedDomain = CurrentAlmConfigurations.ALMDomain;
                        DomainComboBox.SelectedItem = currSelectedDomain;
                    }
                }
                Dictionary<string,string> lstProjects = ALMIntegration.Instance.GetALMDomainProjects(currSelectedDomain, almConectStyle);

                KeyValuePair<string, string> currSavedProj = new KeyValuePair<string, string>(CurrentAlmConfigurations.ALMProjectKey, CurrentAlmConfigurations.ALMProjectName);
                ProjectComboBox.Items.Clear();
                foreach (KeyValuePair<string,string> project in lstProjects)
                {
                    ProjectComboBox.Items.Add(new KeyValuePair<string, string>(project.Key, project.Value));
                }

                if (ProjectComboBox.Items.Count > 0)
                {
                    if (string.IsNullOrEmpty(currSavedProj.Key) == false)
                    {
                        if (ProjectComboBox.Items.Contains(currSavedProj))
                        {
                            ProjectComboBox.SelectedIndex = ProjectComboBox.Items.IndexOf(currSavedProj);
                            CurrentAlmConfigurations.ALMProjectName = currSavedProj.Value;
                            CurrentAlmConfigurations.ALMProjectKey = currSavedProj.Key;
                        }
                    }
                    if (ProjectComboBox.SelectedIndex == -1)
                    {
                        ProjectComboBox.SelectedIndex = 0;
                        CurrentAlmConfigurations.ALMProjectName = ProjectComboBox.Text;
                        CurrentAlmConfigurations.ALMProjectKey = ProjectComboBox.SelectedValuePath;
                    }

                }
            }
        }

        private void ConnectProject()
        {
            if (ConnectProjectButton.Content.ToString() == "Save Project Mapping" || ConnectProjectButton.Content.ToString() == "Connect")
            {
                Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
                if (ALMIntegration.Instance.TestALMProjectConn(almConectStyle))
                {
                    isProjectMappingCorrect = true;
                }
                Mouse.OverrideCursor = null;
            }
            else
            {
                ALMIntegration.Instance.DisconnectALMProjectStayLoggedIn();
                isProjectMappingCorrect = false;
            }
            SetControls();
        }

        private void ConnectProjectButton_Click(object sender, RoutedEventArgs e)
        {
            if(ConnectProjectButton.Content.ToString() == "Save Project Mapping")
            {
                SaveConnectionDetails();
                return;
            }
            ConnectProject();
        }

        private void SaveConnectionDetails()
        {
            ALMIntegration.Instance.SetALMCoreConfigurations(CurrentAlmConfigurations.AlmType);
            Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
            if (ALMIntegration.Instance.TestALMProjectConn(almConectStyle))
            {
                if ((almConectStyle == ALMIntegration.eALMConnectType.Manual) || (almConectStyle == ALMIntegration.eALMConnectType.SettingsPage))
                {
                    SaveALMConfigs();
                }
                isProjectMappingCorrect = true;
            }
            Mouse.OverrideCursor = null;
            SetControls();
        }

        public void ShowAsWindow()
        {
            GingerCore.General.LoadGenericWindow(ref _pageGenericWin, App.MainWindow, eWindowShowStyle.Dialog, this.Title, this, closeEventHandler: CloseWindow);
        }

        private void CloseWindow(object sender, EventArgs e)
        {
            _pageGenericWin.Close();
        }

        private void StyleRadioButtons()
        {
            switch (CurrentAlmConfigurations.AlmType)
            {
                case GingerCoreNET.ALMLib.ALMIntegration.eALMType.QC:
                    QCRadioButton.FontWeight = FontWeights.ExtraBold;
                    QCRadioButton.Foreground = (SolidColorBrush)FindResource("$SelectionColor_Pink");
                    RQMLoadConfigPackageButton.Visibility = Visibility.Collapsed;
                    DownloadPackageLink.Visibility = Visibility.Collapsed;
                    Grid.SetColumnSpan(ServerURLTextBox, 2);
                    ExampleURLHint.Content = "Example: http://server:8080/almbin";
                    if (!isServerDetailsCorrect)
                    {
                        ServerURLTextBox.IsEnabled = true;
                        ServerURLTextBox.IsReadOnly = false;
                    }
                    else
                    {
                        ServerURLTextBox.IsEnabled = false;
                        ServerURLTextBox.IsReadOnly = true;
                    }
                    ServerURLTextBox.Cursor = null;
                    RQMRadioButton.FontWeight = FontWeights.Regular;
                    RQMRadioButton.Foreground = Brushes.Black;
                    RallyRadioButton.FontWeight = FontWeights.Regular;
                    RallyRadioButton.Foreground = Brushes.Black;                  
                    RestAPICheckBox.Visibility = Visibility.Visible;
                    JiraRadioButton.FontWeight = FontWeights.Regular;
                    JiraRadioButton.Foreground = Brushes.Black;
                    qTestRadioButton.FontWeight = FontWeights.Regular;
                    qTestRadioButton.Foreground = Brushes.Black;
                    break;

                case GingerCoreNET.ALMLib.ALMIntegration.eALMType.RQM:
                    RQMRadioButton.FontWeight = FontWeights.ExtraBold;
                    RQMRadioButton.Foreground = (SolidColorBrush)FindResource("$SelectionColor_Pink");
                    RQMLoadConfigPackageButton.Visibility = Visibility.Visible;
                    DownloadPackageLink.Visibility = Visibility.Visible;
                    Grid.SetColumnSpan(ServerURLTextBox, 1);
                    SetLoadPackageButtonContent();
                    ServerURLTextBox.IsReadOnly = true;
                    ServerURLTextBox.IsEnabled = false;
                    ServerURLTextBox.Cursor = Cursors.Arrow;
                    QCRadioButton.FontWeight = FontWeights.Regular;
                    QCRadioButton.Foreground = Brushes.Black;
                    RallyRadioButton.FontWeight = FontWeights.Regular;
                    RallyRadioButton.Foreground = Brushes.Black;
                    RestAPICheckBox.Visibility = Visibility.Hidden;
                    JiraRadioButton.FontWeight = FontWeights.Regular;
                    JiraRadioButton.Foreground = Brushes.Black;
                    qTestRadioButton.FontWeight = FontWeights.Regular;
                    qTestRadioButton.Foreground = Brushes.Black;
                    break;
                case GingerCoreNET.ALMLib.ALMIntegration.eALMType.RALLY:
                    RallyRadioButton.FontWeight = FontWeights.ExtraBold;
                    RallyRadioButton.Foreground = (SolidColorBrush)FindResource("$SelectionColor_Pink");
                    RQMLoadConfigPackageButton.Visibility = Visibility.Collapsed;
                    DownloadPackageLink.Visibility = Visibility.Collapsed;
                    Grid.SetColumnSpan(ServerURLTextBox, 2);
                    ExampleURLHint.Content = "Example: http://server:8080/almbin";
                    if (!isServerDetailsCorrect)
                    {
                        ServerURLTextBox.IsEnabled = true;
                        ServerURLTextBox.IsReadOnly = false;
                    }
                    else
                    {
                        ServerURLTextBox.IsEnabled = false;
                        ServerURLTextBox.IsReadOnly = true;
                    }
                    ServerURLTextBox.Cursor = null;
                    QCRadioButton.FontWeight = FontWeights.Regular;
                    QCRadioButton.Foreground = Brushes.Black;
                    RQMRadioButton.FontWeight = FontWeights.Regular;
                    RQMRadioButton.Foreground = Brushes.Black;
                    RestAPICheckBox.Visibility = Visibility.Hidden;
                    JiraRadioButton.FontWeight = FontWeights.Regular;
                    JiraRadioButton.Foreground = Brushes.Black;
                    qTestRadioButton.FontWeight = FontWeights.Regular;
                    qTestRadioButton.Foreground = Brushes.Black;
                    break;
                case GingerCoreNET.ALMLib.ALMIntegration.eALMType.Jira:
                    JiraRadioButton.FontWeight = FontWeights.ExtraBold;
                    JiraRadioButton.Foreground = (SolidColorBrush)FindResource("$SelectionColor_Pink");
                    RQMLoadConfigPackageButton.Visibility = Visibility.Visible;
                    DownloadPackageLink.Visibility = Visibility.Visible;
                    Grid.SetColumnSpan(ServerURLTextBox, 2);
                    SetLoadJiraPackageButtonContent();
                    if (!isServerDetailsCorrect)
                    {
                        ServerURLTextBox.IsEnabled = true;
                        ServerURLTextBox.IsReadOnly = false;
                    }
                    else
                    {
                        ServerURLTextBox.IsEnabled = false;
                        ServerURLTextBox.IsReadOnly = true;
                    }
                    ServerURLTextBox.Cursor = null;
                    QCRadioButton.FontWeight = FontWeights.Regular;
                    QCRadioButton.Foreground = Brushes.Black;
                    RQMRadioButton.FontWeight = FontWeights.Regular;
                    RQMRadioButton.Foreground = Brushes.Black;
                    RestAPICheckBox.Visibility = Visibility.Hidden;
                    RallyRadioButton.FontWeight = FontWeights.Regular;
                    RallyRadioButton.Foreground = Brushes.Black;
                    qTestRadioButton.FontWeight = FontWeights.Regular;
                    qTestRadioButton.Foreground = Brushes.Black;
                    break;
                case GingerCoreNET.ALMLib.ALMIntegration.eALMType.Qtest:
                    qTestRadioButton.FontWeight = FontWeights.ExtraBold;
                    qTestRadioButton.Foreground = (SolidColorBrush)FindResource("$SelectionColor_Pink");
                    RQMLoadConfigPackageButton.Visibility = Visibility.Hidden;
                    DownloadPackageLink.Visibility = Visibility.Collapsed;
                    Grid.SetColumnSpan(ServerURLTextBox, 2);
                    ExampleURLHint.Content = "Example: https://qtest-stage.t-mobile.com/ ";
                    if (!isServerDetailsCorrect)
                    {
                        ServerURLTextBox.IsEnabled = true;
                        ServerURLTextBox.IsReadOnly = false;
                    }
                    else
                    {
                        ServerURLTextBox.IsEnabled = false;
                        ServerURLTextBox.IsReadOnly = true;
                    }
                    ServerURLTextBox.Cursor = null;
                    QCRadioButton.FontWeight = FontWeights.Regular;
                    QCRadioButton.Foreground = Brushes.Black;
                    RQMRadioButton.FontWeight = FontWeights.Regular;
                    RQMRadioButton.Foreground = Brushes.Black;
                    RestAPICheckBox.Visibility = Visibility.Hidden;
                    RallyRadioButton.FontWeight = FontWeights.Regular;
                    RallyRadioButton.Foreground = Brushes.Black;
                    JiraRadioButton.FontWeight = FontWeights.Regular;
                    JiraRadioButton.Foreground = Brushes.Black;
                    break;
            }
        }

        private void SetLoadJiraPackageButtonContent()
        {
            if (!string.IsNullOrEmpty(ServerURLTextBox.Text))
            {
                RQMLoadConfigPackageButton.Content = "Replace";
                ExampleURLHint.Content = "and click Replace to change Jira Configuration Package";

            }
            else
            {
                RQMLoadConfigPackageButton.Content = "Load";
                ExampleURLHint.Content = "and Load Jira Configuration Package";
            }
        }

        private void SaveALMConfigs()
        {
            WorkSpace.Instance.UserProfile.SaveUserProfile();
            WorkSpace.Instance.Solution.SaveSolution(true, SolutionGeneral.Solution.eSolutionItemToSave.ALMSettings);
        }

        private void ALMRadioButton_Checked_Changed(object sender, RoutedEventArgs e)
        {
            if (sender == null || ALMSettingsPannel == null)
            {
                return;
            }
            if (CurrentAlmConfigurations != null)
            {
                ALMIntegration.Instance.SetALMCoreConfigurations(CurrentAlmConfigurations.AlmType);
            }
            GingerCoreNET.ALMLib.ALMIntegration.eALMType almType = GingerCoreNET.ALMLib.ALMIntegration.eALMType.QC;
            RadioButton rBtn = sender as RadioButton;
            if ((bool)rBtn.IsChecked)
            {
                switch (rBtn.Name)
                {
                    case "QCRadioButton":
                        if (almType != GingerCoreNET.ALMLib.ALMIntegration.eALMType.QC)
                        {
                            almType = GingerCoreNET.ALMLib.ALMIntegration.eALMType.QC;
                        }
                        break;
                    case "RQMRadioButton":
                        if (almType != GingerCoreNET.ALMLib.ALMIntegration.eALMType.RQM)
                        {
                            almType = GingerCoreNET.ALMLib.ALMIntegration.eALMType.RQM;
                            ALMIntegration.Instance.SetALMCoreConfigurations(almType); //Because RQM need to update the server field from existing package
                            SetLoadPackageButtonContent();
                        }
                        break;
                    case "RallyRadioButton":
                        if (almType != GingerCoreNET.ALMLib.ALMIntegration.eALMType.RALLY)
                        {
                            almType = GingerCoreNET.ALMLib.ALMIntegration.eALMType.RALLY;
                        }
                        break;
                    case "JiraRadioButton":
                        if (almType != GingerCoreNET.ALMLib.ALMIntegration.eALMType.Jira)
                        {
                            almType = GingerCoreNET.ALMLib.ALMIntegration.eALMType.Jira;
                        }
                        break;
                    case "qTestRadioButton":
                        if (almType != GingerCoreNET.ALMLib.ALMIntegration.eALMType.Qtest)
                        {
                            almType = GingerCoreNET.ALMLib.ALMIntegration.eALMType.Qtest;
                        }
                        break;
                }
                //Clear bindings
                BindingOperations.ClearAllBindings(ServerURLTextBox);
                BindingOperations.ClearAllBindings(RestAPICheckBox);
                BindingOperations.ClearAllBindings(UserNameTextBox);
                BindingOperations.ClearAllBindings(PasswordTextBox);
                BindingOperations.ClearAllBindings(DomainComboBox);
                BindingOperations.ClearAllBindings(ProjectComboBox);

                ALMIntegration.Instance.SetDefaultAlmConfig(almType);
                ALMIntegration.Instance.UpdateALMType(almType);
                CurrentAlmConfigurations = ALMIntegration.Instance.GetCurrentAlmConfig(almType);
                CurrentAlmUserConfigurations = ALMIntegration.Instance.GetCurrentAlmUserConfig(almType);
                StyleRadioButtons();
                SetControls();
                //Bind again as we changed the objects
                Bind();
            }
        }

        private void SetLoadPackageButtonContent()
        {
            if (!string.IsNullOrEmpty(ServerURLTextBox.Text))
            {
                RQMLoadConfigPackageButton.Content = "Replace";
                ExampleURLHint.Content = "and click Replace to change RQM Configuration Package";

            }
            else
            {
                RQMLoadConfigPackageButton.Content = "Load";
                ExampleURLHint.Content = "and Load RQM Configuration Package";
            }
        }

        private void ServerURLTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

            if (ServerURLTextBox.Text.ToLower().Contains("qcbin"))
            {
                //remove rest of URL
                ServerURLTextBox.Text = ServerURLTextBox.Text.Substring(0,ServerURLTextBox.Text.ToLower().IndexOf("qcbin") + 5);
            }

            SetControls();
        }

        private void UserNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            SetControls();
        }

        private void PasswordTextBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            CurrentAlmUserConfigurations.ALMPassword = PasswordTextBox.Password;
            SetControls();
        }

        private void ProjectComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ProjectComboBox != null && ProjectComboBox.SelectedItem != null)
            {
                KeyValuePair<string, string> newProject = (KeyValuePair<string, string>)ProjectComboBox.SelectedItem;
                CurrentAlmConfigurations.ALMProjectName = newProject.Value;
                CurrentAlmConfigurations.ALMProjectKey = newProject.Key;
            }
        }

        private void TestALMConnectionButton_Click(object sender, RoutedEventArgs e)
        {
            ALMIntegration.Instance.SetALMCoreConfigurations(CurrentAlmConfigurations.AlmType);
            Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
            bool connectionSucc = false;
            try { connectionSucc = ALMIntegration.Instance.TestALMProjectConn(almConectStyle); }
            catch (Exception) { }

            if (connectionSucc)
                Reporter.ToUser(eUserMsgKey.StaticInfoMessage, "Passed! ALM connection test passed successfully");
            else
                Reporter.ToUser(eUserMsgKey.StaticInfoMessage, "Failed! ALM connection test failed, Please check ALM connection details");
            Mouse.OverrideCursor = null;
        }

        private void LoadRQMConfigPackageButton_Click(object sender, RoutedEventArgs e)
        {
            if (ALMIntegration.Instance.LoadALMConfigurations())
                SetLoadPackageButtonContent();
        }

        private void HandleLinkClick(object sender, RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(@"http://ginger/Downloads/Other");
            e.Handled = true;
        }

        private void RestAPICheckBox_Checked(object sender, RoutedEventArgs e)
        {
            ExampleURLHint.Content = "Example: http://server:8080/";
        }

        private void RestAPICheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            ExampleURLHint.Content = "Example: http://server:8080/almbin";
        }
    }
}
