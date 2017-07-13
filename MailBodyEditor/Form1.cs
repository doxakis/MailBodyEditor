using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System.IO;
using System.Linq;

namespace MailBodyEditor
{
    public partial class Form1 : Form
    {
        Dictionary<string, string> Templates { get; set; }
        
        protected override void OnLoad(EventArgs e)
        {
            var area = Screen.AllScreens.Length > 1 ? Screen.AllScreens[1].WorkingArea : Screen.PrimaryScreen.WorkingArea;
            this.Location = new Point((area.Width - this.Width) / 2, (area.Height - this.Height) / 2);
            base.OnLoad(e);
        }

        public Form1()
        {
            InitializeComponent();
            
            Templates = GetTemplates().ToDictionary(m => m, m => m);
            comboBoxQuickTemplate.DataSource = new BindingSource(Templates, null);
            comboBoxQuickTemplate.DisplayMember = "Value";
            comboBoxQuickTemplate.ValueMember = "Key";
            
            textBoxCode.SelectionStart = textBoxCode.TextLength;
            textBoxCode.ScrollToCaret();
        }

        private async void textBoxCode_TextChangedAsync(object sender, EventArgs e)
        {
            StringBuilder builder = new StringBuilder();
            try
            {
                var projectDir = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName;
                var header = File.ReadAllText(projectDir + "/Templates/_Header.txt");
                var footer = File.ReadAllText(projectDir + "/Templates/_Footer.txt");
                var sourceCode = header + textBoxCode.Text + footer;

                builder.Append(await CSharpScript.EvaluateAsync(sourceCode,
                    ScriptOptions.Default.WithReferences(typeof(MailBodyPack.MailBody).Assembly)));
            }
            catch (CompilationErrorException ex)
            {
                builder.Append(string.Join("<br />", ex.Diagnostics));
            }
            previewBox.DocumentText = "<html><body>" +
                builder.ToString() +
                "</body></html>";
        }
        
        private IEnumerable<string> GetTemplates()
        {
            var projectDir = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName;
            return Directory.EnumerateFiles(projectDir + "/Templates")
                .Select(m => Path.GetFileNameWithoutExtension(m))
                .Where(m => !m.StartsWith("_"));
        }

        private void comboBoxQuickTemplate_SelectionChanged(object sender, EventArgs e)
        {
            var selectedText = (comboBoxQuickTemplate.Text);

            selectedText = selectedText.Replace("[", "");
            selectedText = selectedText.Replace("]", "");
            selectedText = selectedText.Split(',')[0].Trim();

            if (GetTemplates().Contains(selectedText))
            {
                // Seem legit.
                var projectDir = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName;
                var template = File.ReadAllText(projectDir + "/Templates/" + selectedText + ".txt");
                textBoxCode.Text = template;
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            // Create or save on template folder
        }

        private void saveAs_Click(object sender, EventArgs e)
        {
            // Save as (create)
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            // Remove
        }
    }
}
