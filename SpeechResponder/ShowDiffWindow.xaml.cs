using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
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
                Logging.Warn("Data is " + diffItem.type + ": " + diffItem.data + " (" + String.Concat(diffItem.data.Select(c => ((int)c).ToString("x2"))) + ")");
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
