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

using System.Diagnostics;
using System.Runtime.InteropServices;
using UIAComWrapperInternal;

namespace System.Windows.Automation
{
    public class ExpandCollapsePattern : BasePattern
    {
        
        private UIAutomationClient.IUIAutomationExpandCollapsePattern _pattern;
        public static readonly AutomationProperty ExpandCollapseStateProperty = ExpandCollapsePatternIdentifiers.ExpandCollapseStateProperty;
        public static readonly AutomationPattern Pattern = ExpandCollapsePatternIdentifiers.Pattern;

        
        private ExpandCollapsePattern(AutomationElement el, UIAutomationClient.IUIAutomationExpandCollapsePattern pattern, bool cached)
            : base(el, cached)
        {
            Debug.Assert(pattern != null);
            this._pattern = pattern;
        }

        internal static object Wrap(AutomationElement el, object pattern, bool cached)
        {
            return (pattern == null) ? null : new ExpandCollapsePattern(el, (UIAutomationClient.IUIAutomationExpandCollapsePattern)pattern, cached);
        }

        public void Collapse()
        {
                        try
            {
this._pattern.Collapse();            }
            catch (System.Runtime.InteropServices.COMException e)
            {
                Exception newEx; if (Utility.ConvertException(e, out newEx)) { throw newEx; } else { throw; }
            }
        }

        public void Expand()
        {
                        try
            {
this._pattern.Expand();            }
            catch (System.Runtime.InteropServices.COMException e)
            {
                Exception newEx; if (Utility.ConvertException(e, out newEx)) { throw newEx; } else { throw; }
            }
        }

        
        public ExpandCollapsePatternInformation Cached
        {
            get
            {
                Utility.ValidateCached(this._cached);
                return new ExpandCollapsePatternInformation(this._el, true);
            }
        }

        public ExpandCollapsePatternInformation Current
        {
            get
            {
                return new ExpandCollapsePatternInformation(this._el, false);
            }
        }

        
        [StructLayout(LayoutKind.Sequential)]
        public struct ExpandCollapsePatternInformation
        {
            private AutomationElement _el;
            private bool _isCached;
            internal ExpandCollapsePatternInformation(AutomationElement element, bool isCached)
            {
                this._el = element;
                this._isCached = isCached;
            }

            public ExpandCollapseState ExpandCollapseState
            {
                get
                {
                    return (ExpandCollapseState)this._el.GetPropertyValue(ExpandCollapsePattern.ExpandCollapseStateProperty, _isCached);
                }
            }
        }
    }
}