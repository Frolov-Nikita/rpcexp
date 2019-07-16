using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Sim
{
    public partial class FormBoolValueSetter : Form
    {
        Tag tag;
        
        public FormBoolValueSetter(Tag tag)
        {
            InitializeComponent();
            this.tag = tag;
            Text = tag.Name;
            labelCurrentValue.Text = tag.Value.ToString();
        }
        
        private void ButtonReset_Click(object sender, EventArgs e)
        {
            tag.Value = false;
            labelCurrentValue.Text = tag.Value.ToString();
        }

        private void ButtonSet_Click(object sender, EventArgs e)
        {
            tag.Value = true;
            labelCurrentValue.Text = tag.Value.ToString();
        }
    }
}
