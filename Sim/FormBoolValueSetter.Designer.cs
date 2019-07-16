namespace Sim
{
    partial class FormBoolValueSetter
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.buttonReet = new System.Windows.Forms.Button();
            this.labelCurrentValue = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // buttonReet
            // 
            this.buttonReet.Location = new System.Drawing.Point(133, 33);
            this.buttonReet.Name = "buttonReet";
            this.buttonReet.Size = new System.Drawing.Size(77, 26);
            this.buttonReet.TabIndex = 0;
            this.buttonReet.Text = "Reset";
            this.buttonReet.UseVisualStyleBackColor = true;
            this.buttonReet.Click += new System.EventHandler(this.ButtonReset_Click);
            // 
            // labelCurrentValue
            // 
            this.labelCurrentValue.AutoSize = true;
            this.labelCurrentValue.Location = new System.Drawing.Point(12, 9);
            this.labelCurrentValue.Name = "labelCurrentValue";
            this.labelCurrentValue.Size = new System.Drawing.Size(34, 13);
            this.labelCurrentValue.TabIndex = 3;
            this.labelCurrentValue.Text = "Value";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(12, 33);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(77, 26);
            this.button1.TabIndex = 4;
            this.button1.Text = "Set";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.ButtonSet_Click);
            // 
            // FormBoolValueSetter
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(222, 70);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.labelCurrentValue);
            this.Controls.Add(this.buttonReet);
            this.Name = "FormBoolValueSetter";
            this.Text = "FormValueSetter";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonReet;
        private System.Windows.Forms.Label labelCurrentValue;
        private System.Windows.Forms.Button button1;
    }
}