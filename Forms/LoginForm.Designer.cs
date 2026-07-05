namespace OSPTracker.Forms
{
    partial class LoginForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this._lblTitle    = new System.Windows.Forms.Label();
            this._lblSubtitle = new System.Windows.Forms.Label();
            this._lblUsername = new System.Windows.Forms.Label();
            this._txtUsername = new System.Windows.Forms.TextBox();
            this._lblPwd      = new System.Windows.Forms.Label();
            this._txtPassword = new System.Windows.Forms.TextBox();
            this._lblError    = new System.Windows.Forms.Label();
            this._btnLogin    = new System.Windows.Forms.Button();
            this.SuspendLayout();
            //
            // _lblTitle
            //
            this._lblTitle.Font      = new System.Drawing.Font("Trebuchet MS", 15F, System.Drawing.FontStyle.Bold);
            this._lblTitle.ForeColor = System.Drawing.Color.FromArgb(26, 82, 118);
            this._lblTitle.Location  = new System.Drawing.Point(20, 20);
            this._lblTitle.Name      = "_lblTitle";
            this._lblTitle.Size      = new System.Drawing.Size(360, 32);
            this._lblTitle.Text      = "OSP Hours Tracker";
            this._lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // _lblSubtitle
            //
            this._lblSubtitle.ForeColor = System.Drawing.Color.Gray;
            this._lblSubtitle.Location  = new System.Drawing.Point(20, 52);
            this._lblSubtitle.Name      = "_lblSubtitle";
            this._lblSubtitle.Size      = new System.Drawing.Size(360, 20);
            this._lblSubtitle.Text      = "Staff Login";
            this._lblSubtitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // _lblUsername
            //
            this._lblUsername.Location = new System.Drawing.Point(60, 90);
            this._lblUsername.Name     = "_lblUsername";
            this._lblUsername.Size     = new System.Drawing.Size(280, 20);
            this._lblUsername.Text     = "Username";
            //
            // _txtUsername
            //
            this._txtUsername.Font     = new System.Drawing.Font("Trebuchet MS", 10F);
            this._txtUsername.Location = new System.Drawing.Point(60, 110);
            this._txtUsername.Name     = "_txtUsername";
            this._txtUsername.Size     = new System.Drawing.Size(280, 25);
            //
            // _lblPwd
            //
            this._lblPwd.Location = new System.Drawing.Point(60, 150);
            this._lblPwd.Name     = "_lblPwd";
            this._lblPwd.Size     = new System.Drawing.Size(280, 20);
            this._lblPwd.Text     = "Password";
            //
            // _txtPassword
            //
            this._txtPassword.Font                  = new System.Drawing.Font("Trebuchet MS", 10F);
            this._txtPassword.Location              = new System.Drawing.Point(60, 170);
            this._txtPassword.Name                  = "_txtPassword";
            this._txtPassword.Size                  = new System.Drawing.Size(280, 25);
            this._txtPassword.UseSystemPasswordChar = true;
            //
            // _lblError
            //
            this._lblError.ForeColor = System.Drawing.Color.DarkRed;
            this._lblError.Location  = new System.Drawing.Point(60, 205);
            this._lblError.Name      = "_lblError";
            this._lblError.Size      = new System.Drawing.Size(280, 20);
            //
            // _btnLogin
            //
            this._btnLogin.BackColor = System.Drawing.Color.FromArgb(26, 82, 118);
            this._btnLogin.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnLogin.Font      = new System.Drawing.Font("Trebuchet MS", 10F, System.Drawing.FontStyle.Bold);
            this._btnLogin.ForeColor = System.Drawing.Color.White;
            this._btnLogin.Location  = new System.Drawing.Point(60, 232);
            this._btnLogin.Name      = "_btnLogin";
            this._btnLogin.Size      = new System.Drawing.Size(280, 36);
            this._btnLogin.Text      = "Sign In";
            this._btnLogin.UseVisualStyleBackColor = false;
            this._btnLogin.Click    += new System.EventHandler(this.OnLoginClick);
            //
            // LoginForm
            //
            this.AcceptButton        = this._btnLogin;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode       = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor           = System.Drawing.Color.White;
            this.ClientSize          = new System.Drawing.Size(404, 294);
            this.Controls.AddRange(new System.Windows.Forms.Control[] {
                this._lblTitle,
                this._lblSubtitle,
                this._lblUsername,
                this._txtUsername,
                this._lblPwd,
                this._txtPassword,
                this._lblError,
                this._btnLogin
            });
            this.Font            = new System.Drawing.Font("Trebuchet MS", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox     = false;
            this.MinimizeBox     = false;
            this.Name            = "LoginForm";
            this.StartPosition   = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text            = "OSP Hours Tracker — Login";
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Label    _lblTitle;
        private System.Windows.Forms.Label    _lblSubtitle;
        private System.Windows.Forms.Label    _lblUsername;
        private System.Windows.Forms.TextBox  _txtUsername;
        private System.Windows.Forms.Label    _lblPwd;
        private System.Windows.Forms.TextBox  _txtPassword;
        private System.Windows.Forms.Label    _lblError;
        private System.Windows.Forms.Button   _btnLogin;
    }
}
