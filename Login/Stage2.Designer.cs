namespace EliteDangerousDataProvider
{
    partial class Stage2
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
            this.confirmButton = new System.Windows.Forms.Button();
            this.codeLabel = new System.Windows.Forms.Label();
            this.codeText = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // confirmButton
            // 
            this.confirmButton.Location = new System.Drawing.Point(93, 202);
            this.confirmButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.confirmButton.Name = "confirmButton";
            this.confirmButton.Size = new System.Drawing.Size(75, 23);
            this.confirmButton.TabIndex = 0;
            this.confirmButton.Text = "Confirm";
            this.confirmButton.UseVisualStyleBackColor = true;
            this.confirmButton.Click += new System.EventHandler(this.confirmButton_Click);
            // 
            // codeLabel
            // 
            this.codeLabel.AutoSize = true;
            this.codeLabel.Location = new System.Drawing.Point(21, 159);
            this.codeLabel.Name = "codeLabel";
            this.codeLabel.Size = new System.Drawing.Size(122, 17);
            this.codeLabel.TabIndex = 1;
            this.codeLabel.Text = "Confirmation code";
            // 
            // codeText
            // 
            this.codeText.Location = new System.Drawing.Point(149, 156);
            this.codeText.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.codeText.Name = "codeText";
            this.codeText.Size = new System.Drawing.Size(100, 22);
            this.codeText.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(25, 37);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(225, 84);
            this.label1.TabIndex = 3;
            this.label1.Text = "You should now receive a confirmation code to the email address you entered in th" +
    "e last step.  Enter that code here and hit \'Confirm\' to finish the process";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // Stage2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(283, 254);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.codeText);
            this.Controls.Add(this.codeLabel);
            this.Controls.Add(this.confirmButton);
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "Stage2";
            this.Text = "Elite: Dangerous login";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button confirmButton;
        private System.Windows.Forms.Label codeLabel;
        private System.Windows.Forms.TextBox codeText;
        private System.Windows.Forms.Label label1;
    }
}