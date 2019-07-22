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
        
        void EditValue(bool val)
        {
            tag.Value = val;
            labelCurrentValue.Text = tag.Value.ToString();
        }

        private void ButtonReset_Click(object sender, EventArgs e)
        {
            EditValue(false);
        }

        private void ButtonSet_Click(object sender, EventArgs e)
        {
            EditValue(true);
        }

        private void FormBoolValueSetter_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                this.Close();

            if (e.KeyCode == Keys.NumPad0)
                EditValue(false);

            if (e.KeyCode == Keys.NumPad1)
                EditValue(true);

            if (e.KeyCode == Keys.Add)
                EditValue(true);

            if (e.KeyCode == Keys.Subtract)
                EditValue(false);

            if (e.KeyCode == Keys.Multiply)
                EditValue(!(bool)tag.Value);

        }
    }
}
