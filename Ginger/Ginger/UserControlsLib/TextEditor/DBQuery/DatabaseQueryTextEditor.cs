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

using Amdocs.Ginger.Plugin.Core;
using Ginger.UserControlsLib.TextEditor;
using Ginger.UserControlsLib.TextEditor.Common;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Highlighting;
using System;
using System.Collections.Generic;
using System.Windows.Controls;


namespace Ginger.Actions.ActionEditPages.DatabaseLib
{
    public class DatabaseQueryTextEditor : TextEditorBase
    {
        public override string Descritpion { get { throw new NotImplementedException(); } }
        public override Image Icon { get { throw new NotImplementedException(); } }

        public override List<string> Extensions
        {
            get
            {
                if (mExtensions.Count == 0)
                {
                    mExtensions.Add(".sql");                    
                }
                return mExtensions;
            }
        }

        public override IHighlightingDefinition HighlightingDefinition
        {
            get
            {                                
                return GetHighlightingDefinitionFromResource(Properties.Resources.DatabaseQuery);             
            }
        }


         
        public override List<ICompletionData> GetCompletionData(string txt, SelectedContentArgs SelectedContentArgs)
        {
            List<ICompletionData> list = new List<ICompletionData>();
            //list.Add(new GherkinTextCompletionData("Given"));
            //list.Add(new GherkinTextCompletionData("When"));
            //list.Add(new GherkinTextCompletionData("Then"));
            //list.Add(new GherkinTextCompletionData("And"));

            return null;
        }

        public override Page GetSelectedContentPageEditor(SelectedContentArgs SelectedContentArgs)
        {
            return null;
        }
        public override void UpdateSelectedContent()
        {
        }

        // if we want to add tool bar item and handler this is the place
        public override List<ITextEditorToolBarItem> Tools
        {
            get { return null; }
        }

        public override UserControlsLib.TextEditor.IFoldingStrategy FoldingStrategy => null;
    }
}