﻿using EnvDTE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Share.Extension;
using Shoring.SharePlus.Util;
using Microsoft.VisualStudio.Shell.Interop;

namespace Shoring.SharePlus.Operate
{
    internal class EditorPlus : OperateBase
    {
        IVsUIShell uiShell = null;

        internal EditorPlus(DTE dte, IVsUIShell vsUiShell = null)
            : base(dte)
        {
            uiShell = vsUiShell;
        }

        internal void InsertTodoCallback(object sender, EventArgs e)
        {
            if (CurrentDocument == null)
            {
                return;
            }

            var selection = CurrentDocument.Selection as TextSelection;
            selection.Insert(@"//TODO:");
        }

        internal void OpenExplore(object sender, EventArgs e)
        {
            if (CurrentDocument == null)
            {
                return;
            }
            System.Diagnostics.Process.Start("Explorer.exe", " /select," + CurrentDocument.Path + CurrentDocument.Name);
        }

        internal void GenerateProperty(object sender, EventArgs e)
        {
            var selection = dte.ActiveDocument.Selection as TextSelection;
            if (selection.IsEmpty)
            {
                return;
            }

            var text = selection.Text;
            Regex reg = new Regex(Property + ".?[ ].*[ ].*");
            var matches = reg.Matches(text);
            if (matches.Count == 0)
            {
                return;
            }
            foreach (var match in matches)
            {
                var properties = match.ToString().Trim().Split(' ');
                if (properties.Length != 3)
                {
                    continue;
                }
                var sign = properties[0];
                var keyWord = properties[1];
                var propertyName = properties[2];

                var script = "";
                if (sign == Property)
                {
                    script =
@"/// <summary>
/// 
/// </summary>
"
+ "public " + keyWord + " " + propertyName.ToUpperFirstLetter() + "{ get; set; }\n\n";
                }
                else if (sign == PropertyComplete)
                {
                    script =
@"/// <summary>
/// 
/// </summary>
private {keyword} {m_name};
public {keyword} {name}
{
    get
    {
        return {m_name};
    }
    set
    {
        {m_name} = value;
    }
}

".Replace("{keyword}", keyWord)
  .Replace("{m_name}", propertyName)
  .Replace("{name}", propertyName.ToUpperFirstLetter());
                }

                selection.SelectAll();
                selection.ReplacePattern(match.ToString(), script);
                selection.SelectAll();
                selection.SmartFormat();
            }
        }

        internal void Commit(object sender, EventArgs e)
        {
            if (CurrentDocument == null)
            {
                return;
            }
            Share.Command cmd = new Share.Command( errorReceived: (x) =>
            {
                if (!string.IsNullOrEmpty(x))
                {
                    MessageBox.Show(uiShell, x); 
                }
            });
            cmd.Excute("svn commit -m 'commit' " + CurrentDocument.Path + CurrentDocument.Name);
        }
    }
}
