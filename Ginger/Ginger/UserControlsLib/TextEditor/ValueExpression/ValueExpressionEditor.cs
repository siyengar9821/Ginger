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
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Plugin.Core;
using Ginger.Actions;
using Ginger.UserControlsLib.TextEditor.Common;
using GingerCore.Variables;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Highlighting;
using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Ginger.UserControlsLib.TextEditor.ValueExpression
{
    public class ValueExpressionEditor : TextEditorBase
    {
        public override string Descritpion { get { return ""; } }
        public override Image Icon { get { throw new NotImplementedException(); } }

        private Page p=null;
        public override List<string> Extensions
        {
            get
            {
                return null;
            }
        }
        
        public override IHighlightingDefinition HighlightingDefinition
        {
            get
            {
                return GetHighlightingDefinitionFromResource(Properties.Resources.ValueExpressionHighlighting);                
            }
        }

        public override IFoldingStrategy FoldingStrategy
        {
            get
            {
                return null;

            }
        }

        Context mContext;
        public ValueExpressionEditor(Context context)
        {
            mContext = context;
        }
        public override Page GetSelectedContentPageEditor(SelectedContentArgs SelectedContentArgs)
        {
            // SelectedContentArgs.TextEditor.Document.
            //TODO: use SelectedContentArgs.TextEditor.Document  - documents function is better

            //start by searching the closest { to the caret offset
            int i1 = SelectedContentArgs.TextEditor.CaretOffset;            
            int closestpos = -1;
            while (i1 >= 0 && i1< SelectedContentArgs.TextEditor.Text.Length)
            {
                string c = SelectedContentArgs.TextEditor.Text.Substring(i1, 1);
                if (c == "{")
                {
                    closestpos = i1;
                    break;
                }
                if (c == "}" && i1 != SelectedContentArgs.TextEditor.CaretOffset)  // we stop if we found } - except for the first char - in case the user click on the closing } of exp
                {                    
                    break;
                }
                i1--;
            }

            if (closestpos == -1) return null;

            // find the = after the closest { pos
            
            int i0 = SelectedContentArgs.TextEditor.Text.IndexOf("}", closestpos);

            // Verify caret are in between { }
            if (closestpos < SelectedContentArgs.TextEditor.CaretOffset && i0 >= SelectedContentArgs.TextEditor.CaretOffset)
            {
                int i2= SelectedContentArgs.TextEditor.Text.IndexOf("=", closestpos);
                if (i2>=0)
                {                    
                    SelectedContentArgs.StartPos = closestpos;
                    SelectedContentArgs.EndPos = i0;
                    Page p = GetPageFor(SelectedContentArgs);
                    return p;
                }                
            }
            return null;              
        }    
        
        public override List<ICompletionData> GetCompletionData(string txt, SelectedContentArgs SelectedContentArgs)
        {
            List<ICompletionData> list = new List<ICompletionData>();
            if(txt == "{")
            {
                list.Add(new TextCompletionData("{Var Name=}" , "Variable"));
                list.Add(new TextCompletionData("{VBS Eval=}" , "VB Script Evaluate"));

                AddVars(list);
            }
            return list;
        }

        void AddVars(List<ICompletionData> list)
        {            
            foreach (VariableBase v in  WorkSpace.Instance.Solution.Variables)
            {
                list.Add(GETVariableTCD(v));
            }

            foreach (VariableBase v in mContext.BusinessFlow.Variables)
            {
                list.Add(GETVariableTCD(v));
            }

            foreach (VariableBase v in mContext.BusinessFlow.CurrentActivity.Variables)
            {
                list.Add(GETVariableTCD(v));
            }
        }

        private TextCompletionData GETVariableTCD(VariableBase v)
        {
            TextCompletionData TCD = new TextCompletionData("{Var Name=" + v.Name + "}");
            TCD.Description = "Variable " + v.Name + " " + v.VariableType;


            //TODO: replace with General.GetImage("@Variable_32x32.png")
            BitmapImage b = new BitmapImage();
            b.BeginInit();
            b.UriSource = new Uri("pack://application:,,,/Ginger;component/Images/@Variable_32x32.png"); 
            b.EndInit();                             

            TCD.Image = b;
            return TCD;
        }

        private Page GetPageFor(SelectedContentArgs SelectedContentArgs)
        {            
            string txt = SelectedContentArgs.TextEditor.Text.Substring(SelectedContentArgs.StartPos, SelectedContentArgs.Length);

            if (txt.StartsWith("{Var Name="))
            {
                p= new ValueExpressionVariableEditorPage(mContext, SelectedContentArgs);
            }
            
            if (txt.StartsWith("{DS Name="))
            {
               p= new ActDataSourcePage(SelectedContentArgs);
            }

            if (txt.StartsWith("{FD Object="))
            {
                string obj = string.Empty;
                string field = string.Empty;
                int FieldStartIndex = txt.IndexOf("Field");              
                obj = txt.Substring(11, FieldStartIndex - 12).Trim();
                field = txt.Substring(FieldStartIndex + 6, txt.Length - (FieldStartIndex + 7)).Trim();
                p = new ValueExpressionFlowDetailsEditorPage(mContext,SelectedContentArgs, obj, field);
            }

            if (txt.StartsWith("{EnvURL App="))
            {
                //TODO: impl get page for Env
                
            }
            // TODO: if we are on {Actual}  - show help to explain...

            return p;
        }

        public override void UpdateSelectedContent()
        {
            if(p.GetType() == typeof(ValueExpressionVariableEditorPage))
            {
                ((ValueExpressionVariableEditorPage)p).UpdateContent();
            }
            else if(p.GetType() == typeof(ActDataSourcePage))
            {
                ((ActDataSourcePage)p).UpdateContent();
            }
            else if (p.GetType() == typeof(ValueExpressionFlowDetailsEditorPage))
            {
                ((ValueExpressionFlowDetailsEditorPage)p).UpdateContent();
            }
        }

        // if we want to add tool bar item and handler this is the place
        public override List<ITextEditorToolBarItem> Tools
        {
            get { return null; }
        }
    }
}
