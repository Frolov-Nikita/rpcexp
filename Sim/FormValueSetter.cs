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
        }
        
        private void ButtonSet_Click(object sender, EventArgs e)
        {
            tag.Value = numericUpDown1.Value;
            labelCurrentValue.Text = tag.Value.ToString();
        }
    }
}
