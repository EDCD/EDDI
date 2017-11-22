using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using Utilities;

namespace EddiSpeechResponder
{
    public partial class ShowDiffWindow : Window
    {
        public ShowDiffWindow(string oldScript, string newScript)
        {
            InitializeComponent();
            List<DiffItem> diffItems = Diff.DiffTexts(oldScript, newScript);
            for (int i = 0; i < diffItems.Count; i++)
            {
                DiffItem diffItem = diffItems[i];

                BrushConverter bc = new BrushConverter();
                TextRange tr = new TextRange(scriptText.Document.ContentEnd, scriptText.Document.ContentEnd)
                {
                    Text = diffItem.data + Environment.NewLine
                };
                if (diffItem.type == "Deleted")
                {
                    tr.ApplyPropertyValue(TextElement.BackgroundProperty, "#FFF8FF");
                }
                else if (diffItem.type == "Inserted")
                {
                    tr.ApplyPropertyValue(TextElement.BackgroundProperty, "#F8FFF8");
                }
            }
        }
    }
}
