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
            this.label1 = new System.Windows.Forms.Label();
            this.obtainButton = new System.Windows.Forms.Button();
            this.selectButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(10, 7);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(562, 222);
            this.label1.TabIndex = 1;
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // obtainButton
            // 
            this.obtainButton.Location = new System.Drawing.Point(245, 231);
            this.obtainButton.Margin = new System.Windows.Forms.Padding(2);
            this.obtainButton.Name = "obtainButton";
            this.obtainButton.Size = new System.Drawing.Size(122, 23);
            this.obtainButton.TabIndex = 2;
            this.obtainButton.Text = "Obtain";
            this.obtainButton.UseVisualStyleBackColor = true;
            this.obtainButton.Click += new System.EventHandler(this.obtainButton_Click);
            // 
            // selectButton
            // 
            this.selectButton.Location = new System.Drawing.Point(257, 231);
            this.selectButton.Name = "selectButton";
            this.selectButton.Size = new System.Drawing.Size(97, 23);
            this.selectButton.TabIndex = 3;
            this.selectButton.Text = "Select directory";
            this.selectButton.UseVisualStyleBackColor = true;
            this.selectButton.Click += new System.EventHandler(this.selectButton_Click);
            // 
            // Stage3
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(584, 261);
            this.Controls.Add(this.selectButton);
            this.Controls.Add(this.obtainButton);
            this.Controls.Add(this.label1);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "Stage3";
            this.Text = "Obtain Product Directory";
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button obtainButton;
        private System.Windows.Forms.Button selectButton;
    }
}