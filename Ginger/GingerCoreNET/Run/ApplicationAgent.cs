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
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.Common.Repository;
using Amdocs.Ginger.Repository;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace GingerCore.Platforms
{
    public class ApplicationAgent : RepositoryItemBase
    {        
        //private string mAppName;
        //[IsSerializedForLocalRepository]
        //public string AppName {
        //    get
        //    {
        //        return mAppName;
        //    }
        //    set
        //    {
        //        if (mAppName != value)
        //        {
        //            mAppName = value;
        //            OnPropertyChanged(Fields.AppName);
        //        }
        //    }
        //}
        private RepositoryItemKey mAppKey;
        [IsSerializedForLocalRepository]
        public RepositoryItemKey AppKey
        {
            get
            {
                return mAppKey;
            }
            set
            {
                if (mAppKey != value)
                {
                    mAppKey = value;
                    OnPropertyChanged(nameof(AppKey));
                }
            }
        }

        // No need to serialized as it used only in runtime   
        private Agent mAgent;
        public Agent Agent 
        {
            get { return mAgent; }
            set
            {
                if (mAgent != null) mAgent.PropertyChanged -= Agent_OnPropertyChange;
                mAgent =(Agent) value;
                if (mAgent != null)
                {
                    AgentKey = mAgent.Key;
                    mAgent.PropertyChanged += Agent_OnPropertyChange;
                }
                OnPropertyChanged(nameof(Agent));  
                OnPropertyChanged(nameof(AgentKey));  
                //OnPropertyChanged(Fields.AppAndAgent);                
            }
        }


        //private string mAgentName;
        //[IsSerializedForLocalRepository]
        //public string AgentName
        //{
        //    get
        //    {
        //        if (Agent != null)
        //        {
        //            if (mAgentName != Agent.Name)
        //                mAgentName = Agent.Name;
        //        }
        //        else if (string.IsNullOrEmpty(mAgentName))
        //        {
        //            mAgentName = string.Empty;
        //        }
        //        return mAgentName;
        //    }
        //    set
        //    {
        //        mAgentName = value;
        //        OnPropertyChanged(Fields.AgentName);
        //    }
        //}
        private RepositoryItemKey mAgentKey;
        [IsSerializedForLocalRepository]
        public RepositoryItemKey AgentKey
        {
            get
            {
                if (Agent != null)
                {
                    if (mAgentKey.Guid != Agent.Guid)
                        mAgentKey = Agent.Key;
                }

                return mAgentKey;
            }
            set
            {
                mAgentKey = value;
                OnPropertyChanged(nameof(AgentKey));
            }
        }

        //public string AppAndAgent
        //{
        //    get
        //    {
        //        string s = AppName + ":" ;
        //        if (mAgent != null)
        //        {
        //            s += mAgent.Name;
        //        }
        //        else
        //        {
        //            s += " Agent not defined";
        //        }
        //        return s;
        //    }
        //}

        private void Agent_OnPropertyChange(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == GingerCore.Agent.Fields.Name)
            {
               OnPropertyChanged(nameof(AgentKey));
            }
        }

        public override string ItemName
        {
            get
            {
                return string.Empty;
            }
            set
            {
                return;
            }
        }

        //IAgent IApplicationAgent.Agent
        //{
        //    get { return Agent; }
        //    set { Agent = (Agent)value; }
        //}

        public List<IAgent> PossibleAgents
        {
            get
            {
                List<IAgent> possibleAgents = new List<IAgent>();

                //find out the target application platform
                TargetBase ap = WorkSpace.Instance.Solution.TargetApplications.Where(x => x.Guid == AppKey.Guid).FirstOrDefault();
                if (ap != null)
                {
                    ePlatformType appPlatform = ap.Platform;

                    //get the solution Agents which match to this platform                     
                    List<Agent> agents = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Agent>().Where(x => x.Platform == appPlatform || x.ServiceId == AppKey.ItemName).ToList();
                    if (agents != null)
                    {
                        foreach(IAgent agent in agents)
                        {
                            possibleAgents.Add(agent);
                        }                        
                    }
                }
                return possibleAgents;
            }
        }
    }
}
