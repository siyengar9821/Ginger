﻿using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.Plugin.Core;
using Amdocs.Ginger.Repository;
using GingerCore;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.BusinessFlowsLibNew.AddActionMenu
{
    /// <summary>
    /// Interaction logic for MainAddActionsNavigationPage.xaml
    /// </summary>
    public partial class MainAddActionsNavigationPage : Page
    {
        Context mContext;
        RecordNavPage mRecordPage = null;
        SharedRepositoryNavPage mSharedRepositoryNavPage = null;
        POMNavPage mPOMNavPage = null;
        ActionsLibraryNavPage mActionsLibraryNavPage = null;
        LiveSpyNavPage mLiveSpyNavPage = null;
        WindowsExplorerNavPage mWindowsExplorerNavPage = null;
        APINavPage mAPINavPage = null;
        private bool applicationModelView;

        public MainAddActionsNavigationPage(Context context)
        {
            mContext = context;
            InitializeComponent();
            context.PropertyChanged += Context_PropertyChanged;
            xNavigationBarPnl.Visibility = Visibility.Collapsed;
            xSelectedItemFrame.ContentRendered += NavPnlActionFrame_ContentRendered;
            ToggleApplicatoinModels();
            ToggleRecordLiveSpyAndWindowsExplorer();
            xApplicationModelsPnl.Visibility = Visibility.Collapsed;
        }

        private void Context_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName is nameof(mContext.Agent) || e.PropertyName is nameof(mContext.AgentStatus))
            {
                ToggleRecordLiveSpyAndWindowsExplorer();
            }
            if (e.PropertyName == nameof(BusinessFlow) || e.PropertyName == nameof(mContext.Platform))
            {
                ToggleApplicatoinModels();
                LoadActionFrame(null);
            }
        }

        void ToggleRecordLiveSpyAndWindowsExplorer()
        {
            if (mContext.Agent != null && mContext.Agent.Driver != null)
            {
                if (mContext.Agent.Driver is IWindowExplorer)
                {
                    xWindowExplorerItemBtn.Visibility = Visibility.Visible;
                    xLiveSpyItemBtn.Visibility = Visibility.Visible;
                }
                if(mContext.Agent.Driver is IRecord)
                {
                    xRecordItemBtn.Visibility = Visibility.Visible;
                }
            }
            else
            {
                xWindowExplorerItemBtn.Visibility = Visibility.Collapsed;
                xLiveSpyItemBtn.Visibility = Visibility.Collapsed;
                xRecordItemBtn.Visibility = Visibility.Collapsed;
            }
        }

        void ToggleApplicatoinModels()
        {
            bool POMCompliantPlatform = ApplicationPOMModel.PomSupportedPlatforms.Contains(mContext.Platform);
            bool APICompliantPlatform = mContext.Platform == ePlatformType.WebServices;

            if (POMCompliantPlatform)
            {
                xApplicationPOMItemBtn.Visibility = Visibility.Visible;
            }
            else
            {
                xApplicationPOMItemBtn.Visibility = Visibility.Collapsed;
            }

            if (APICompliantPlatform)
            {
                xAPIBtn.Visibility = Visibility.Visible;
            }
            else
            {
                xAPIBtn.Visibility = Visibility.Collapsed;
            }

            if (APICompliantPlatform || POMCompliantPlatform)
            {
                xApplicationModelsBtn.Visibility = Visibility.Visible;
            }
            else
            {
                xApplicationModelsBtn.Visibility = Visibility.Collapsed;
            }

        }

        public void ResetAddActionPages()
        {
            mRecordPage = null;
            mSharedRepositoryNavPage = null;
            mPOMNavPage = null;
            mActionsLibraryNavPage = null;
            mLiveSpyNavPage = null;
            mWindowsExplorerNavPage = null;
            mAPINavPage = null;
        }

        private void NavPnlActionFrame_ContentRendered(object sender, EventArgs e)
        {
            if ((sender as Frame).Content == null)
            {
                if(applicationModelView)
                {
                    (sender as Frame).Visibility = Visibility.Collapsed;
                    xNavigationBarPnl.Visibility = Visibility.Visible;
                    xAddActionsOptionsPnl.Visibility = Visibility.Collapsed;
                    xApplicationModelsPnl.Visibility = Visibility.Visible;
                }
                else
                {
                    (sender as Frame).Visibility = Visibility.Collapsed;
                    xNavigationBarPnl.Visibility = Visibility.Collapsed;
                    xAddActionsOptionsPnl.Visibility = Visibility.Visible;
                    xApplicationModelsPnl.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                (sender as Frame).Visibility = Visibility.Visible;
                xNavigationBarPnl.Visibility = Visibility.Visible;
                xAddActionsOptionsPnl.Visibility = Visibility.Collapsed;
                xApplicationModelsPnl.Visibility = Visibility.Collapsed;
            }
        }

        private void XNavSharedRepo_Click(object sender, RoutedEventArgs e)
        {
            if(mSharedRepositoryNavPage == null)
            {
                mSharedRepositoryNavPage = new SharedRepositoryNavPage(mContext);
            }
            LoadActionFrame(mSharedRepositoryNavPage, "Shared Repository", eImageType.SharedRepositoryItem); // WorkSpace.Instance.SolutionRepository.GetRepositoryItemRootFolder<Act>()));
        }

        private void XNavPOM_Click(object sender, RoutedEventArgs e)
        {
            if(mPOMNavPage == null)
            {
                mPOMNavPage = new POMNavPage(mContext);
            }
            LoadActionFrame(mPOMNavPage, "Page Objects Model", eImageType.ApplicationPOMModel);
        }

        private void XRecord_Click(object sender, RoutedEventArgs e)
        {
            if (mRecordPage == null)
            {
                mRecordPage = new RecordNavPage(mContext);
            }

            LoadActionFrame(mRecordPage, "Record", eImageType.Camera);
        }

        private void XNavActLib_Click(object sender, RoutedEventArgs e)
        {
            if(mActionsLibraryNavPage == null)
            {
                mActionsLibraryNavPage = new ActionsLibraryNavPage(mContext);
            }
            LoadActionFrame(mActionsLibraryNavPage, "Actions Library", eImageType.Action);
        }

        private void XNavSpy_Click(object sender, RoutedEventArgs e)
        {
            if (mLiveSpyNavPage == null)
            {
                mLiveSpyNavPage = new LiveSpyNavPage(mContext);
            }
            LoadActionFrame(mLiveSpyNavPage, "Live Spy", eImageType.Spy);
        }

        private void XNavWinExp_Click(object sender, RoutedEventArgs e)
        {
            if (mWindowsExplorerNavPage == null)
            {
                mWindowsExplorerNavPage = new WindowsExplorerNavPage(mContext);
            }
            LoadActionFrame(mWindowsExplorerNavPage, "Explorer", eImageType.Window);
        }

        private void XAPIBtn_Click(object sender, RoutedEventArgs e)
        {
            if(mAPINavPage == null)
            {
                mAPINavPage = new APINavPage(mContext);
            }
            LoadActionFrame(mAPINavPage, "API Models", eImageType.APIModel);
        }

        private void xGoBackBtn_Click(object sender, RoutedEventArgs e)
        {
            if(xSelectedItemFrame.Content is APINavPage || xSelectedItemFrame.Content is POMNavPage)
            {
                applicationModelView = true;
                LoadActionFrame(null, "Application Models", eImageType.ApplicationModel);
            }
            else if(xSelectedItemFrame.Content is null)
            {
                applicationModelView = false;
                xNavigationBarPnl.Visibility = Visibility.Collapsed;
                xAddActionsOptionsPnl.Visibility = Visibility.Visible;
                xApplicationModelsPnl.Visibility = Visibility.Collapsed;
            }
            else
                LoadActionFrame(null);
        }

        private void LoadActionFrame(Page navigationPage, string titleText = "", eImageType titleImage = eImageType.Empty)
        {
            xSelectedItemFrame.Content = navigationPage;

            if (navigationPage != null || titleImage is eImageType.ApplicationModel)
            {
                xNavigationBarPnl.Visibility = Visibility.Visible;
                xSelectedItemTitlePnl.Visibility = Visibility.Visible;
                xSelectedItemTitleImage.ImageType = titleImage;
                xSelectedItemTitleText.Content = titleText;
            }
            else
            {
                xSelectedItemTitlePnl.Visibility = Visibility.Collapsed;
            }
        }

        private void XApplicationModelsBtn_Click(object sender, RoutedEventArgs e)
        {
            xApplicationModelsPnl.Visibility = Visibility.Visible;
            xAddActionsOptionsPnl.Visibility = Visibility.Collapsed;

            LoadActionFrame(null, "Application Models", eImageType.ApplicationModel);
        }
    }
}
