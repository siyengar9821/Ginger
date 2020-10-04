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

using Ginger.SolutionWindows.TreeViewItems;
using GingerWPF.UserControlsLib.UCTreeView;
using GingerCore;
using System;
using System.Collections.Generic;
using System.Windows.Controls;
using Amdocs.Ginger.Common;
using amdocs.ginger.GingerCoreNET;
using GingerCore.ALM.Qtest;

namespace Ginger.ALM.JIRA.TreeViewItems
{
    public class JiraZephyrCycleTreeItem : TreeViewItemBase, ITreeViewItem
    {
        public BusinessFlow QCExplorer { get; set; }
        public List<ITreeViewItem> currentChildrens = null;

        public string Id { get; set; }
        public string Name { get; set; }
        public string VersionId { get; set; }

        Object ITreeViewItem.NodeObject()
        {
            return null;
        }

        public JiraZephyrCycleTreeItem()
        {
           
        }

        StackPanel ITreeViewItem.Header()
        {
            return TreeViewUtils.CreateItemHeader(Name, "@WorkFlow_16x16.png");          
        }

        List<ITreeViewItem> ITreeViewItem.Childrens()
        {
            return currentChildrens;
        }

        bool ITreeViewItem.IsExpandable()
        {
            return false;
        }

        Page ITreeViewItem.EditPage(Amdocs.Ginger.Common.Context mContext)
        {
            return null;
        }

        ContextMenu ITreeViewItem.Menu()
        {
            return null;
        }
        
        void ITreeViewItem.SetTools(ITreeView TV)
        {
            // there is not tools needed at this stage
        }
    }
}
