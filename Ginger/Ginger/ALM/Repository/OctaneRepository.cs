﻿using ALM_Common.DataContracts;
using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using Ginger.ALM.QC;
using Ginger.ALM.QC.TreeViewItems;
using Ginger.ALM.Repository;
using GingerCore;
using GingerCore.Activities;
using GingerCore.ALM;
using GingerCore.Platforms;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using QCRestClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Ginger.ALM.Repository
{
    class OctaneRepository : ALMRepository
    {
        OctaneCore octaneCore;

        QCTestCase matchingTC = null;
        public OctaneRepository()
        {
            octaneCore = new OctaneCore();
        }
        public override bool ConnectALMServer(ALMIntegration.eALMConnectType userMsgStyle)
        {
            try
            {
                Reporter.ToLog(eLogLevel.DEBUG, "Connecting to Octane server");
                if (ALMIntegration.Instance.AlmCore.ConnectALMServer())
                {
                    return true;
                }
                else
                {
                    Reporter.ToUser(eUserMsgKey.ALMConnectFailureWithCurrSettings, "Bad credentials");
                    return false;
                }
            }
            catch (Exception e)
            {
                if (userMsgStyle == ALMIntegration.eALMConnectType.Manual)
                {
                    Reporter.ToUser(eUserMsgKey.QcConnectFailure, e.Message); //TODO: Fix message
                }
                else if (userMsgStyle == ALMIntegration.eALMConnectType.Auto)
                {
                    Reporter.ToUser(eUserMsgKey.ALMConnectFailureWithCurrSettings, e.Message);
                }
                Reporter.ToLog(eLogLevel.WARN, "Error connecting to QTest server", e);
                return false;
            }
        }


        public override bool ExportActivitiesGroupToALM(ActivitiesGroup activtiesGroup, string uploadPath = null, bool performSaveAfterExport = false, BusinessFlow businessFlow = null)
        {
            if (activtiesGroup == null) { return false; }
            //if it is called from shared repository need to select path
            if (uploadPath == null)
            {
                QCTestPlanExplorerPage win = new QCTestPlanExplorerPage();
                win.xCreateBusinessFlowFolder.Visibility = Visibility.Collapsed;//no need to create separate folder
                uploadPath = win.ShowAsWindow(eWindowShowStyle.Dialog);
            }
            //upload the Activities Group
            Reporter.ToStatus(eStatusMsgKey.ExportItemToALM, null, activtiesGroup.Name);
            string res = string.Empty;

            ObservableList<ExternalItemFieldBase> allFields = new ObservableList<ExternalItemFieldBase>(WorkSpace.Instance.Solution.ExternalItemsFields);
            
            ALMIntegration.Instance.RefreshALMItemFields(allFields, true, null);        

            ObservableList<ExternalItemFieldBase> testCaseFields = CleanUnrelvantFields(allFields, "Test Case");

            bool exportRes = ((OctaneCore)ALMIntegration.Instance.AlmCore).ExportActivitiesGroupToALM(activtiesGroup, matchingTC, uploadPath, testCaseFields, null, null, ref res);

            Reporter.HideStatusMessage();
            if (exportRes)
            {
                if (performSaveAfterExport)
                {
                    Reporter.ToStatus(eStatusMsgKey.SaveItem, null, activtiesGroup.Name, GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup));
                    WorkSpace.Instance.SolutionRepository.SaveRepositoryItem(activtiesGroup);
                    Reporter.HideStatusMessage();
                }
                return true;
            }
            else
            {
                Reporter.ToUser(eUserMsgKey.ExportItemToALMFailed, GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup), activtiesGroup.Name, res);
            }

            return false;
        }

        public override void ExportBfActivitiesGroupsToALM(BusinessFlow businessFlow, ObservableList<ActivitiesGroup> grdActivitiesGroups)
        {
            throw new NotImplementedException();
        }

        public override bool ExportBusinessFlowToALM(BusinessFlow businessFlow, bool performSaveAfterExport = false, ALMIntegration.eALMConnectType almConectStyle = ALMIntegration.eALMConnectType.Manual, string testPlanUploadPath = null, string testLabUploadPath = null)
        {
            if (businessFlow == null)
            {
                return false;
            }

            if (businessFlow.ActivitiesGroups.Count == 0)
            {
                Reporter.ToUser(eUserMsgKey.StaticInfoMessage, "The " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " do not include " + GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroups) + " which supposed to be mapped to ALM Test Cases, please add at least one " + GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup) + " before doing export.");
                return false;
            }

            QCTestSet matchingTS = null;

            Amdocs.Ginger.Common.eUserMsgSelection userSelec;
            //TO DO MaheshK : check if the businessFlow already mapped to Octane Test Suite
            if (!String.IsNullOrEmpty(businessFlow.ExternalID))
            {
                matchingTS = ((OctaneCore)ALMIntegration.Instance.AlmCore).GetTestSuiteById(businessFlow.ExternalID);
                if (matchingTS != null)
                {
                    //ask user if want to continute
                    userSelec = Reporter.ToUser(eUserMsgKey.BusinessFlowAlreadyMappedToTC, businessFlow.Name, matchingTS.Name);
                    if (userSelec == Amdocs.Ginger.Common.eUserMsgSelection.Cancel)
                    {
                        return false;
                    }
                    else if (userSelec == Amdocs.Ginger.Common.eUserMsgSelection.No)
                    {
                        matchingTS = null;
                        testPlanUploadPath = SelectALMTestPlanPath();
                        if (String.IsNullOrEmpty(testPlanUploadPath))
                        {
                            //no path to upload to
                            return false;
                        }
                        //create upload path if checked to create separete folder
                        if (QCTestPlanFolderTreeItem.IsCreateBusinessFlowFolder)
                        {
                            try
                            {
                                string folderId = octaneCore.GetLastTestPlanIdFromPath(testPlanUploadPath).ToString();
                                folderId = octaneCore.CreateApplicationModule(businessFlow.Name, businessFlow.Description, folderId);
                                testPlanUploadPath = folderId;
                            }
                            catch (Exception ex)
                            {
                                Reporter.ToLog(eLogLevel.ERROR, "Failed to get create folder for Test Plan with Octane REST API", ex);
                            }
                        }
                        else
                        {
                            testPlanUploadPath = octaneCore.GetLastTestPlanIdFromPath(testPlanUploadPath).ToString();
                        }
                    }
                    else
                    {
                        if (String.IsNullOrEmpty(testPlanUploadPath))
                        {
                            testPlanUploadPath = matchingTS.ParentId;
                        }
                    }
                }
            }

            testLabUploadPath = testPlanUploadPath;
            bool performSave = false;

            //just to check if new TC needs to be created or update has to be done
            if (matchingTS == null)
            {
                matchingTC = null;
            }
            else
            {
                matchingTC = new QCTestCase();
            } 
            //check if all of the business flow activities groups already exported to Octane and export the ones which not
            foreach (ActivitiesGroup ag in businessFlow.ActivitiesGroups)
            {
                ExportActivitiesGroupToALM(ag, testPlanUploadPath, performSave, businessFlow);
            }

            //upload the business flow
            Reporter.ToStatus(eStatusMsgKey.ExportItemToALM, null, businessFlow.Name);
            string res = string.Empty;

            ObservableList<ExternalItemFieldBase> allFields = new ObservableList<ExternalItemFieldBase>(WorkSpace.Instance.Solution.ExternalItemsFields);
            ALMIntegration.Instance.RefreshALMItemFields(allFields, true, null);

            ObservableList<ExternalItemFieldBase> testSetFieldsFields = CleanUnrelvantFields(allFields, "Test Suite");

            bool exportRes = ((OctaneCore)ALMIntegration.Instance.AlmCore).ExportBusinessFlow(businessFlow, matchingTS, testLabUploadPath, testSetFieldsFields, null, ref res);
            Reporter.HideStatusMessage();
            if (exportRes)
            {
                if (performSaveAfterExport)
                {
                    Reporter.ToStatus(eStatusMsgKey.SaveItem, null, businessFlow.Name, GingerDicser.GetTermResValue(eTermResKey.BusinessFlow));
                    WorkSpace.Instance.SolutionRepository.SaveRepositoryItem(businessFlow);
                    Reporter.HideStatusMessage();
                }
                if (almConectStyle != ALMIntegration.eALMConnectType.Auto)
                {
                    Reporter.ToUser(eUserMsgKey.ExportItemToALMSucceed);
                }
                return true;
            }
            else
                if (almConectStyle != ALMIntegration.eALMConnectType.Auto)
            {
                Reporter.ToUser(eUserMsgKey.ExportItemToALMFailed, GingerDicser.GetTermResValue(eTermResKey.BusinessFlow), businessFlow.Name, res);
            }

            return false;
        }

        public override eUserMsgKey GetDownloadPossibleValuesMessage()
        {
            return eUserMsgKey.AskIfToDownloadPossibleValuesShortProcesss;
        }

        public override List<string> GetTestLabExplorer(string path)
        {
            return octaneCore.GetTestLabExplorer(path);
        }

        public override List<string> GetTestPlanExplorer(string path)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<object> GetTestSetExplorer(string path)
        {
            return octaneCore.GetTestSetExplorer(path);
        }

        public override object GetTSRunStatus(object tsItem)
        {
            return octaneCore.GetTSRunStatus(tsItem);
        }

        public override void ImportALMTests(string importDestinationFolderPath)
        {
            Reporter.ToLog(eLogLevel.DEBUG, "Start importing from QC");
            //set path to import to               
            if (importDestinationFolderPath == "")
            {
                importDestinationFolderPath = WorkSpace.Instance.Solution.BusinessFlowsMainFolder;
            }
            //show Test Lab browser for selecting the Test Set/s to import
            QCTestLabExplorerPage win = new QCTestLabExplorerPage(QCTestLabExplorerPage.eExplorerTestLabPageUsageType.Import, importDestinationFolderPath);
            win.ShowAsWindow(eWindowShowStyle.Dialog);
        }

        public override void ImportALMTestsById(string importDestinationFolderPath)
        {
            throw new NotImplementedException();
        }

        public override bool ImportSelectedTests(string importDestinationPath, IEnumerable<object> selectedTestSets)
        {
            if (selectedTestSets != null && selectedTestSets.Count() > 0)
            {
                ObservableList<QCTestSetTreeItem> testSetsItemsToImport = new ObservableList<QCTestSetTreeItem>();
                foreach (QCTestSetTreeItem testSetItem in selectedTestSets)
                {
                    //check if some of the Test Set was already imported                
                    if (testSetItem.AlreadyImported)
                    {
                        Amdocs.Ginger.Common.eUserMsgSelection userSelection = Reporter.ToUser(eUserMsgKey.TestSetExists, testSetItem.TestSetName);
                        if (userSelection == Amdocs.Ginger.Common.eUserMsgSelection.Yes)
                        {
                            //Delete the mapped BF
                            File.Delete(testSetItem.MappedBusinessFlow.FileName);
                            testSetsItemsToImport.Add(testSetItem);
                        }
                    }
                    else
                    {
                        testSetsItemsToImport.Add(testSetItem);
                    }
                }

                if (testSetsItemsToImport.Count == 0) { return false; } //noting to import

                //Refresh Ginger repository and allow GingerQC to use it                
                ALMIntegration.Instance.AlmCore.GingerActivitiesGroupsRepo = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ActivitiesGroup>();
                ALMIntegration.Instance.AlmCore.GingerActivitiesRepo = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Activity>();

                foreach (QCTestSetTreeItem testSetItemtoImport in testSetsItemsToImport)
                {
                    try
                    {
                        //import test set data
                        Reporter.ToStatus(eStatusMsgKey.ALMTestSetImport, null, testSetItemtoImport.TestSetName);
                        GingerCore.ALM.QC.QCTestSet TS = new GingerCore.ALM.QC.QCTestSet();
                        TS.TestSetID = testSetItemtoImport.TestSetID;
                        TS.TestSetName = testSetItemtoImport.TestSetName;
                        TS.TestSetPath = testSetItemtoImport.Path;
                        TS = ((OctaneCore)ALMIntegration.Instance.AlmCore).ImportTestSetData(TS);

                        //convert test set into BF
                        BusinessFlow tsBusFlow = ((OctaneCore)ALMIntegration.Instance.AlmCore).ConvertQCTestSetToBF(TS);

                        if (WorkSpace.Instance.Solution.MainApplication != null)
                        {
                            //add the applications mapped to the Activities
                            foreach (Activity activ in tsBusFlow.Activities)
                            {
                                if (string.IsNullOrEmpty(activ.TargetApplication))
                                {
                                    if (tsBusFlow.TargetApplications.Where(x => x.Name == activ.TargetApplication).FirstOrDefault() == null)
                                    {
                                        ApplicationPlatform appAgent = WorkSpace.Instance.Solution.ApplicationPlatforms.Where(x => x.AppName == activ.TargetApplication).FirstOrDefault();
                                        if (appAgent != null)
                                        {
                                            tsBusFlow.TargetApplications.Add(new TargetApplication() { AppName = appAgent.AppName });
                                        }
                                    }
                                }
                            }
                            //handle non mapped Activities
                            if (tsBusFlow.TargetApplications.Count == 0)
                            {
                                tsBusFlow.TargetApplications.Add(new TargetApplication() { AppName = WorkSpace.Instance.Solution.MainApplication });
                            }
                            foreach (Activity activ in tsBusFlow.Activities)
                            {
                                if (string.IsNullOrEmpty(activ.TargetApplication))
                                {
                                    activ.TargetApplication = tsBusFlow.MainApplication;
                                }
                            }
                        }
                        else
                        {
                            foreach (Activity activ in tsBusFlow.Activities)
                            {
                                activ.TargetApplication = null; // no app configured on solution level
                            }
                        }

                        WorkSpace.Instance.SolutionRepository.AddRepositoryItem(tsBusFlow);
                        Reporter.HideStatusMessage();
                    }
                    catch (Exception ex)
                    {
                        Reporter.ToUser(eUserMsgKey.ErrorInTestsetImport, testSetItemtoImport.TestSetName, ex.Message);
                        Reporter.ToLog(eLogLevel.ERROR, "Error importing from QC", ex);

                    }
                }

                Reporter.ToUser(eUserMsgKey.TestSetsImportedSuccessfully);
                Reporter.ToLog(eLogLevel.DEBUG, "Imported from QC successfully");
                return true;
            }
            Reporter.ToLog(eLogLevel.ERROR, "Error importing from QC");
            return false;
        }

        public override bool LoadALMConfigurations()
        {
            throw new NotImplementedException();
        }

        public override string SelectALMTestLabPath()
        {
            QCTestLabExplorerPage win = new QCTestLabExplorerPage(QCTestLabExplorerPage.eExplorerTestLabPageUsageType.BrowseFolders);
            return (string)win.ShowAsWindow(eWindowShowStyle.Dialog);
        }

        public override string SelectALMTestPlanPath()
        {
            QCTestLabExplorerPage win = new QCTestLabExplorerPage(QCTestLabExplorerPage.eExplorerTestLabPageUsageType.BrowseFolders);
            return (string)win.ShowAsWindow(eWindowShowStyle.Dialog);
        }

        public override IEnumerable<object> SelectALMTestSets()
        {
            throw new NotImplementedException();
        }

        public override bool ShowImportReviewPage(string importDestinationPath, object selectedTestPlan = null)
        {
            throw new NotImplementedException();
        }

        public override void UpdateActivitiesGroup(ref BusinessFlow businessFlow, List<Tuple<string, string>> TCsIDs)
        {
            throw new NotImplementedException();
        }

        public override void UpdateBusinessFlow(ref BusinessFlow businessFlow)
        {
            throw new NotImplementedException();
        }
        private ObservableList<ExternalItemFieldBase> CleanUnrelvantFields(ObservableList<ExternalItemFieldBase> fields, string resourceType)
        {
            ObservableList<ExternalItemFieldBase> fieldsToReturn = new ObservableList<ExternalItemFieldBase>();

            //Going through the fields to leave only Test Set fields
            for (int indx = 0; indx < fields.Count; indx++)
            {
                if (fields[indx].ItemType == resourceType)
                {
                    fieldsToReturn.Add(fields[indx]);
                }
            }

            return fieldsToReturn;
        }

    }
}
