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
using Amdocs.Ginger.Repository;
using Ginger.Run;
using GingerCore.Environments;
using System;
using System.Linq;
using System.Threading;

namespace Amdocs.Ginger.CoreNET.RosLynLib
{
    // This is Roslyn global file which is passed to the script code
    // DO NOT put here general code unless it is intended to be used from script
    // DO NOT change functions signature once published as we might have script which use them, or make sure it is backward compatible

    public class GingerScriptGlobals
    {
        //TODO: we can put global vars too
        // TODO: Just example delete me later
        public int X = 1;
        public int Y;

        PluginPackage P;

        public void LoadPluginPackage(string folder)
        {
            Console.WriteLine("* Loading Plugin - " + folder);
            P = new PluginPackage(folder);
            Console.WriteLine("* Plugin Loaded");
        }


        public void Sleep(int millisecondsTimeout)
        {
            Thread.Sleep(millisecondsTimeout);
        }



        public void OpenSolution(string solutionFolder)
        {
            WorkSpace.Instance.OpenSolution(solutionFolder);
        }

        public void OpenRunSet(string runSetName, string envName)
        {
            SolutionRepository SR = WorkSpace.Instance.SolutionRepository;
            var envs = SR.GetAllRepositoryItems<ProjEnvironment>();
            ProjEnvironment projEnvironment = (from x in SR.GetAllRepositoryItems<ProjEnvironment>() where x.Name == envName select x).SingleOrDefault();
            RunSetConfig runSetConfig = (from x in SR.GetAllRepositoryItems<RunSetConfig>() where x.Name == runSetName select x).SingleOrDefault();
            RunsetExecutor runsetExecutor = new RunsetExecutor();
            WorkSpace.Instance.RunsetExecutor = runsetExecutor;
            runsetExecutor.RunSetConfig = runSetConfig;
            runsetExecutor.RunsetExecutionEnvironment = projEnvironment;
            runsetExecutor.InitRunners();
            runsetExecutor.RunRunset();

            
        }

        /// <summary>
        /// Create summary json of the execution
        /// </summary>
        /// <param name="fileName"></param>
        public void CreateExecutionSummaryJSON(string fileName)
        {            
            string s = WorkSpace.Instance.RunsetExecutor.CreateSummary();
            System.IO.File.WriteAllText(fileName, s);
        }



    }
}