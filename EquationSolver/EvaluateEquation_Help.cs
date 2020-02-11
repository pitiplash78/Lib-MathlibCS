using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace MathLibCS.EvaluateEquation
{
    public partial class EvaluateEquation_Help : Form
    {
        public EvaluateEquation_Help(string path)
        {
            InitializeComponent();

            if(File.Exists(path))

            richTextBox1.LoadFile(path);
            else
            {
                richTextBox1.SelectionColor = Color.Red;
                richTextBox1.SelectedText = Environment.NewLine + "Path for help file not found!";
            }
        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
