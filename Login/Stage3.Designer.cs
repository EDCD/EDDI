namespace EliteDangerousDataProvider
{
    partial class Stage3
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
            this.profileButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // profileButton
            // 
            this.profileButton.Location = new System.Drawing.Point(53, 149);
            this.profileButton.Margin = new System.Windows.Forms.Padding(2);
            this.profileButton.Name = "profileButton";
            this.profileButton.Size = new System.Drawing.Size(95, 19);
            this.profileButton.TabIndex = 0;
            this.profileButton.Text = "Test connection";
            this.profileButton.UseVisualStyleBackColor = true;
            this.profileButton.Click += new System.EventHandler(this.profileButton_Click);
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(26, 32);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(160, 69);
            this.label1.TabIndex = 1;
            this.label1.Text = "At this point you should be logged in.  Hit \'Test connection\' below to confirm th" +
    "at it works";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // Stage3
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(212, 206);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.profileButton);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "Stage3";
            this.Text = "Test connection";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button profileButton;
        private System.Windows.Forms.Label label1;
    }
}