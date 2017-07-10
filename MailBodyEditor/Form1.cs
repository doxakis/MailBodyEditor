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

namespace MailBodyEditor
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            textBoxCode.Text += "var body = MailBody" + Environment.NewLine +
                "    .CreateBody()" + Environment.NewLine +
                "    .Paragraph(\"Please confirm your email address by clicking the link below.\")" + Environment.NewLine +
                "    .Paragraph(\"We may need to send you critical information about our service and it is important that we have an accurate email address.\")" + Environment.NewLine +
                "    .Button(\"https:/" + "/example.com/\", \"Confirm Email Address\")" + Environment.NewLine +
                "    .Paragraph(\"— [Insert company name here]\")" + Environment.NewLine +
                "    .ToString(); " + Environment.NewLine;
            textBoxCode.SelectionStart = textBoxCode.TextLength;
            textBoxCode.ScrollToCaret();
        }

        private void textBoxCode_TextChanged(object sender, EventArgs e)
        {
            var results = CompileCsharpCode(textBoxCode.Text);

            StringBuilder builder = new StringBuilder();

            if (results.Errors.HasErrors)
            {
                foreach (CompilerError item in results.Errors)
                {
                    builder.Append(!item.IsWarning ? "<span style='color:red'>" : "<span style='color:orange'>")
                        .Append(item.ErrorText)
                        .Append("</span>")
                        .Append("<br />");
                }
            }
            else
            {
                builder.Append(RunCSharpCode(results));
            }

            previewBox.DocumentText = "<html><body>" +
                builder.ToString() +
                "</body></html>";
        }

        public static CompilerResults CompileCsharpCode(string function)
        {
            string code = @"
                using System;
                using MailBodyPack;
                namespace UserFunctions
                {
                    public class MyFunction
                    {
                        public static string Function()
                        {
                            myfunc;
                            return body;
                        }
                    }
                }
            ";
            
            string finalCode = code.Replace("myfunc", function);
            CSharpCodeProvider provider = new CSharpCodeProvider();
            CompilerParameters compilerParameters = new CompilerParameters();
            compilerParameters.GenerateInMemory = true;
            compilerParameters.ReferencedAssemblies.Add("System.dll");
            compilerParameters.ReferencedAssemblies.Add("System.Linq.dll");
            compilerParameters.ReferencedAssemblies.Add("System.Core.dll");
            compilerParameters.ReferencedAssemblies.Add("MailBody.dll");
            CompilerResults results = provider.CompileAssemblyFromSource(compilerParameters, finalCode);
            return results;
        }
        
        public static string RunCSharpCode(CompilerResults results)
        {
            Type myType = results.CompiledAssembly.GetType("UserFunctions.MyFunction");
            try
            {
                var result = myType.GetMethod("Function").Invoke(null, new object[] { }).ToString();
                return result;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }
}
