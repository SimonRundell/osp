namespace OSPTracker.Forms
{
    partial class ChangePasswordForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this._lblForced   = new System.Windows.Forms.Label();
            this._lblCurrent  = new System.Windows.Forms.Label();
            this._txtCurrent  = new System.Windows.Forms.TextBox();
            this._lblNew      = new System.Windows.Forms.Label();
            this._txtNew      = new System.Windows.Forms.TextBox();
            this._lblHint     = new System.Windows.Forms.Label();
            this._lblConfirm  = new System.Windows.Forms.Label();
            this._txtConfirm  = new System.Windows.Forms.TextBox();
            this._lblError    = new System.Windows.Forms.Label();
            this._btnSave     = new System.Windows.Forms.Button();
            this._btnCancel   = new System.Windows.Forms.Button();
            this.SuspendLayout();
            //
            // _lblForced
            //
            this._lblForced.ForeColor = System.Drawing.Color.FromArgb(230, 126, 34);
            this._lblForced.Location  = new System.Drawing.Point(20, 15);
            this._lblForced.Name      = "_lblForced";
            this._lblForced.Size      = new System.Drawing.Size(340, 34);
            this._lblForced.Text      = "You must set a new password before continuing.";
            this._lblForced.Visible   = false;
            //
            // _lblCurrent
            //
            this._lblCurrent.Location = new System.Drawing.Point(20, 55);
            this._lblCurrent.Name     = "_lblCurrent";
            this._lblCurrent.Size     = new System.Drawing.Size(340, 18);
            this._lblCurrent.Text     = "Current Password";
            //
            // _txtCurrent
            //
            this._txtCurrent.Font                  = new System.Drawing.Font("Trebuchet MS", 9.5F);
            this._txtCurrent.Location              = new System.Drawing.Point(20, 74);
            this._txtCurrent.Name                  = "_txtCurrent";
            this._txtCurrent.Size                  = new System.Drawing.Size(340, 24);
            this._txtCurrent.UseSystemPasswordChar = true;
            //
            // _lblNew
            //
            this._lblNew.Location = new System.Drawing.Point(20, 106);
            this._lblNew.Name     = "_lblNew";
            this._lblNew.Size     = new System.Drawing.Size(340, 18);
            this._lblNew.Text     = "New Password";
            //
            // _txtNew
            //
            this._txtNew.Font                  = new System.Drawing.Font("Trebuchet MS", 9.5F);
            this._txtNew.Location              = new System.Drawing.Point(20, 125);
            this._txtNew.Name                  = "_txtNew";
            this._txtNew.Size                  = new System.Drawing.Size(340, 24);
            this._txtNew.UseSystemPasswordChar = true;
            //
            // _lblHint
            //
            this._lblHint.Font      = new System.Drawing.Font("Trebuchet MS", 7.5F, System.Drawing.FontStyle.Italic);
            this._lblHint.ForeColor = System.Drawing.Color.Gray;
            this._lblHint.Location  = new System.Drawing.Point(20, 151);
            this._lblHint.Name      = "_lblHint";
            this._lblHint.Size      = new System.Drawing.Size(340, 16);
            this._lblHint.Text      = "Min 8 characters, one uppercase letter, one digit.";
            //
            // _lblConfirm
            //
            this._lblConfirm.Location = new System.Drawing.Point(20, 172);
            this._lblConfirm.Name     = "_lblConfirm";
            this._lblConfirm.Size     = new System.Drawing.Size(340, 18);
            this._lblConfirm.Text     = "Confirm New Password";
            //
            // _txtConfirm
            //
            this._txtConfirm.Font                  = new System.Drawing.Font("Trebuchet MS", 9.5F);
            this._txtConfirm.Location              = new System.Drawing.Point(20, 191);
            this._txtConfirm.Name                  = "_txtConfirm";
            this._txtConfirm.Size                  = new System.Drawing.Size(340, 24);
            this._txtConfirm.UseSystemPasswordChar = true;
            //
            // _lblError
            //
            this._lblError.ForeColor = System.Drawing.Color.DarkRed;
            this._lblError.Location  = new System.Drawing.Point(20, 220);
            this._lblError.Name      = "_lblError";
            this._lblError.Size      = new System.Drawing.Size(340, 34);
            //
            // _btnSave
            //
            this._btnSave.BackColor = System.Drawing.Color.FromArgb(26, 82, 118);
            this._btnSave.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnSave.Font      = new System.Drawing.Font("Trebuchet MS", 9.5F, System.Drawing.FontStyle.Bold);
            this._btnSave.ForeColor = System.Drawing.Color.White;
            this._btnSave.Location  = new System.Drawing.Point(140, 262);
            this._btnSave.Name      = "_btnSave";
            this._btnSave.Size      = new System.Drawing.Size(110, 34);
            this._btnSave.Text      = "Save";
            this._btnSave.UseVisualStyleBackColor = false;
            this._btnSave.Click    += new System.EventHandler(this.OnSave);
            //
            // _btnCancel
            //
            this._btnCancel.BackColor = System.Drawing.Color.FromArgb(140, 140, 150);
            this._btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnCancel.Font      = new System.Drawing.Font("Trebuchet MS", 9.5F);
            this._btnCancel.ForeColor = System.Drawing.Color.White;
            this._btnCancel.Location  = new System.Drawing.Point(250, 262);
            this._btnCancel.Name      = "_btnCancel";
            this._btnCancel.Size      = new System.Drawing.Size(110, 34);
            this._btnCancel.Text      = "Cancel";
            this._btnCancel.UseVisualStyleBackColor = false;
            this._btnCancel.Click    += new System.EventHandler(this.OnCancel);
            //
            // ChangePasswordForm
            //
            this.AcceptButton       = this._btnSave;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode      = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor          = System.Drawing.Color.White;
            this.ClientSize         = new System.Drawing.Size(384, 314);
            this.Controls.AddRange(new System.Windows.Forms.Control[] {
                this._lblForced,
                this._lblCurrent,
                this._txtCurrent,
                this._lblNew,
                this._txtNew,
                this._lblHint,
                this._lblConfirm,
                this._txtConfirm,
                this._lblError,
                this._btnSave,
                this._btnCancel
            });
            this.Font            = new System.Drawing.Font("Trebuchet MS", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox     = false;
            this.MinimizeBox     = false;
            this.Name            = "ChangePasswordForm";
            this.StartPosition   = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text            = "Change Password";
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Label   _lblForced;
        private System.Windows.Forms.Label   _lblCurrent;
        private System.Windows.Forms.TextBox _txtCurrent;
        private System.Windows.Forms.Label   _lblNew;
        private System.Windows.Forms.TextBox _txtNew;
        private System.Windows.Forms.Label   _lblHint;
        private System.Windows.Forms.Label   _lblConfirm;
        private System.Windows.Forms.TextBox _txtConfirm;
        private System.Windows.Forms.Label   _lblError;
        private System.Windows.Forms.Button  _btnSave;
        private System.Windows.Forms.Button  _btnCancel;
    }
}
