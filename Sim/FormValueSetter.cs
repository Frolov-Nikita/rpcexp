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
    public partial class FormValueSetter : Form
    {
        Tag tag;

        public FormValueSetter(Tag tag)
        {
            InitializeComponent();

            this.tag = tag;
            Text = tag.Name;

            labelCurrentValue.Text = tag.Value.ToString();

            switch (tag.ValueType)
            {
                case ModbusValueType.Float:
                    numericUpDown1.Maximum = decimal.MaxValue;
                    numericUpDown1.Minimum = decimal.MinValue;
                    numericUpDown1.Increment = 0.01M;
                    break;
                case ModbusValueType.Int16:
                    numericUpDown1.Maximum = Int16.MaxValue;
                    numericUpDown1.Minimum = Int16.MinValue;
                    numericUpDown1.Increment = 1M;
                    break;
                case ModbusValueType.Int32:
                    numericUpDown1.Maximum = Int32.MaxValue;
                    numericUpDown1.Minimum = Int32.MinValue;
                    numericUpDown1.Increment = 1M;
                    break;
                default:
                    throw new NotImplementedException();
            }
            numericUpDown1.Focus();
            numericUpDown1.Select(0, numericUpDown1.Text.Length);
        }

        void EditValue()
        {
            tag.Value = numericUpDown1.Value;
            labelCurrentValue.Text = tag.Value.ToString();
        }

        private void ButtonSet_Click(object sender, EventArgs e)
        {
            EditValue();
        }

        private void On_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                EditValue();

            if (e.KeyCode == Keys.Escape)
                this.Close();
        }
    }
}
