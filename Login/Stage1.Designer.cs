namespace EliteDangerousDataProvider
{
    partial class Stage1
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
            this.emailLabel = new System.Windows.Forms.Label();
            this.passwordLabel = new System.Windows.Forms.Label();
            this.emailText = new System.Windows.Forms.TextBox();
            this.passwordText = new System.Windows.Forms.TextBox();
            this.loginButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // emailLabel
            // 
            this.emailLabel.AutoSize = true;
            this.emailLabel.Location = new System.Drawing.Point(26, 176);
            this.emailLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.emailLabel.Name = "emailLabel";
            this.emailLabel.Size = new System.Drawing.Size(97, 17);
            this.emailLabel.TabIndex = 0;
            this.emailLabel.Text = "Email address";
            // 
            // passwordLabel
            // 
            this.passwordLabel.AutoSize = true;
            this.passwordLabel.Location = new System.Drawing.Point(54, 209);
            this.passwordLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.passwordLabel.Name = "passwordLabel";
            this.passwordLabel.Size = new System.Drawing.Size(69, 17);
            this.passwordLabel.TabIndex = 1;
            this.passwordLabel.Text = "Password";
            // 
            // emailText
            // 
            this.emailText.Location = new System.Drawing.Point(137, 173);
            this.emailText.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.emailText.Name = "emailText";
            this.emailText.Size = new System.Drawing.Size(216, 22);
            this.emailText.TabIndex = 2;
            // 
            // passwordText
            // 
            this.passwordText.Location = new System.Drawing.Point(137, 206);
            this.passwordText.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.passwordText.Name = "passwordText";
            this.passwordText.PasswordChar = '*';
            this.passwordText.Size = new System.Drawing.Size(216, 22);
            this.passwordText.TabIndex = 3;
            // 
            // loginButton
            // 
            this.loginButton.Location = new System.Drawing.Point(137, 252);
            this.loginButton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.loginButton.Name = "loginButton";
            this.loginButton.Size = new System.Drawing.Size(100, 28);
            this.loginButton.TabIndex = 4;
            this.loginButton.Text = "Login";
            this.loginButton.UseVisualStyleBackColor = true;
            this.loginButton.Click += new System.EventHandler(this.loginButton_Click);
            // 
            // Stage1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(379, 321);
            this.Controls.Add(this.loginButton);
            this.Controls.Add(this.passwordText);
            this.Controls.Add(this.emailText);
            this.Controls.Add(this.passwordLabel);
            this.Controls.Add(this.emailLabel);
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "Stage1";
            this.Text = "Elite: Dangerous login";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label emailLabel;
        private System.Windows.Forms.Label passwordLabel;
        private System.Windows.Forms.TextBox emailText;
        private System.Windows.Forms.TextBox passwordText;
        private System.Windows.Forms.Button loginButton;
    }
}

