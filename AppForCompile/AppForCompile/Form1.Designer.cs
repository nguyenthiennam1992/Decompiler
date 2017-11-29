namespace AppForCompile
{
    partial class login_form
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(login_form));
            this.loginGrBox = new System.Windows.Forms.GroupBox();
            this.login_tbox = new System.Windows.Forms.TextBox();
            this.pass_tbox = new System.Windows.Forms.TextBox();
            this.login_btn = new System.Windows.Forms.Button();
            this.cancel_btn = new System.Windows.Forms.Button();
            this.remember_cbox = new System.Windows.Forms.CheckBox();
            this.emailpbox = new System.Windows.Forms.PictureBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.loginGrBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.emailpbox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // loginGrBox
            // 
            this.loginGrBox.Controls.Add(this.pictureBox1);
            this.loginGrBox.Controls.Add(this.emailpbox);
            this.loginGrBox.Controls.Add(this.remember_cbox);
            this.loginGrBox.Controls.Add(this.cancel_btn);
            this.loginGrBox.Controls.Add(this.login_btn);
            this.loginGrBox.Controls.Add(this.pass_tbox);
            this.loginGrBox.Controls.Add(this.login_tbox);
            this.loginGrBox.Font = new System.Drawing.Font("MS Reference Sans Serif", 26.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.loginGrBox.Location = new System.Drawing.Point(12, 12);
            this.loginGrBox.Name = "loginGrBox";
            this.loginGrBox.Size = new System.Drawing.Size(487, 191);
            this.loginGrBox.TabIndex = 1;
            this.loginGrBox.TabStop = false;
            this.loginGrBox.Text = "Login";
            // 
            // login_tbox
            // 
            this.login_tbox.Font = new System.Drawing.Font("MS Reference Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.login_tbox.Location = new System.Drawing.Point(121, 49);
            this.login_tbox.Name = "login_tbox";
            this.login_tbox.Size = new System.Drawing.Size(282, 31);
            this.login_tbox.TabIndex = 0;
            this.login_tbox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // pass_tbox
            // 
            this.pass_tbox.Font = new System.Drawing.Font("MS Reference Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.pass_tbox.Location = new System.Drawing.Point(121, 96);
            this.pass_tbox.Name = "pass_tbox";
            this.pass_tbox.Size = new System.Drawing.Size(282, 31);
            this.pass_tbox.TabIndex = 0;
            this.pass_tbox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.pass_tbox.UseSystemPasswordChar = true;
            // 
            // login_btn
            // 
            this.login_btn.Font = new System.Drawing.Font("MS Reference Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.login_btn.Location = new System.Drawing.Point(327, 143);
            this.login_btn.Name = "login_btn";
            this.login_btn.Size = new System.Drawing.Size(75, 30);
            this.login_btn.TabIndex = 1;
            this.login_btn.Text = "Login";
            this.login_btn.UseVisualStyleBackColor = true;
            // 
            // cancel_btn
            // 
            this.cancel_btn.Font = new System.Drawing.Font("MS Reference Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cancel_btn.Location = new System.Drawing.Point(246, 143);
            this.cancel_btn.Name = "cancel_btn";
            this.cancel_btn.Size = new System.Drawing.Size(75, 30);
            this.cancel_btn.TabIndex = 1;
            this.cancel_btn.Text = "Cancel";
            this.cancel_btn.UseVisualStyleBackColor = true;
            // 
            // remember_cbox
            // 
            this.remember_cbox.AutoSize = true;
            this.remember_cbox.Font = new System.Drawing.Font("MS Reference Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.remember_cbox.Location = new System.Drawing.Point(121, 153);
            this.remember_cbox.Name = "remember_cbox";
            this.remember_cbox.Size = new System.Drawing.Size(118, 20);
            this.remember_cbox.TabIndex = 2;
            this.remember_cbox.Text = "Remember me";
            this.remember_cbox.UseVisualStyleBackColor = true;
            // 
            // emailpbox
            // 
            this.emailpbox.Image = ((System.Drawing.Image)(resources.GetObject("emailpbox.Image")));
            this.emailpbox.InitialImage = ((System.Drawing.Image)(resources.GetObject("emailpbox.InitialImage")));
            this.emailpbox.Location = new System.Drawing.Point(58, 49);
            this.emailpbox.Name = "emailpbox";
            this.emailpbox.Size = new System.Drawing.Size(38, 31);
            this.emailpbox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.emailpbox.TabIndex = 4;
            this.emailpbox.TabStop = false;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.InitialImage = ((System.Drawing.Image)(resources.GetObject("pictureBox1.InitialImage")));
            this.pictureBox1.Location = new System.Drawing.Point(58, 96);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(38, 31);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 4;
            this.pictureBox1.TabStop = false;
            // 
            // login_form
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(511, 215);
            this.Controls.Add(this.loginGrBox);
            this.Name = "login_form";
            this.Text = "AppDemo";
            this.loginGrBox.ResumeLayout(false);
            this.loginGrBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.emailpbox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox loginGrBox;
        private System.Windows.Forms.CheckBox remember_cbox;
        private System.Windows.Forms.Button cancel_btn;
        private System.Windows.Forms.Button login_btn;
        private System.Windows.Forms.TextBox pass_tbox;
        private System.Windows.Forms.TextBox login_tbox;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.PictureBox emailpbox;
    }
}

