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
            this.SuspendLayout();
            // 
            // profileButton
            // 
            this.profileButton.Location = new System.Drawing.Point(71, 183);
            this.profileButton.Name = "profileButton";
            this.profileButton.Size = new System.Drawing.Size(127, 23);
            this.profileButton.TabIndex = 0;
            this.profileButton.Text = "Obtain profile";
            this.profileButton.UseVisualStyleBackColor = true;
            this.profileButton.Click += new System.EventHandler(this.profileButton_Click);
            // 
            // Stage3
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(282, 254);
            this.Controls.Add(this.profileButton);
            this.Name = "Stage3";
            this.Text = "Stage3";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button profileButton;
    }
}