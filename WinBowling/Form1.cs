using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
//
//using Bowling;
using BowlTracker;

namespace WinBowling {
    public partial class frmBowlCalc : Form {
        private const string KEY_TAB = "{TAB}";
        //
        private const string LABELNAME = "lblFrame{0}";
        private const string FIRSTTHROW_ANY_FRAME = "txtBoxF{0}Throw1";
        private const string SECONDTHROW_ANY_FRAME = "txtBoxF{0}Throw2";
        private const string THIRDTHROW_LAST_FRAME = "txtBoxF10Throw3";
        //
        private const string SPARE = "/";
        private const string STRIKE = "X";
        //
        private const string REGEXP_FRAMENO = "frameNo";
        private const string REGEXP_THROWNO = "throwNo";
        //
        private readonly string VALID_KEYS_THROW = "/0123456789X";
        //
        private System.Text.RegularExpressions.Regex _reTextInputName = new Regex(@"^txtBoxF(?<frameNo>\d+)Throw(?<throwNo>\d+)$", System.Text.RegularExpressions.RegexOptions.Compiled | System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        private System.Text.RegularExpressions.Regex _reLblName = new Regex(@"^lblFrame(?<frameNo>\d+)", System.Text.RegularExpressions.RegexOptions.Compiled | System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        private System.Text.RegularExpressions.Regex _reGrpName = new Regex(@"^grpFrame(?<frameNo>\d+)", System.Text.RegularExpressions.RegexOptions.Compiled | System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        //
        private BowlTracker.FrameHandler _frameHandler = null;
        //
        private System.Collections.Generic.Dictionary<int, GroupBox> _hashCtls = new Dictionary<int, GroupBox>();
        //
        public frmBowlCalc() {
            InitializeComponent();
            this.HashGroups();
            this._frameHandler = new FrameHandler();
            this._frameHandler.Score += new EventHandler<FrameScoreEventArgs>(_frameHandler_Score);
            this._frameHandler.EndOfGame += new EventHandler<EventArgs>(_frameHandler_EndOfGame);
            this._frameHandler.PerfectGame += new EventHandler<EventArgs>(_frameHandler_PerfectGame);
        }

        #region _frameHandler_PerfectGame Event Handler
        void _frameHandler_PerfectGame(object sender, EventArgs e) {
            this.lblPG.Visible = true;
        }
        #endregion

        #region _frameHandler_EndOfGame Event Handler
        void _frameHandler_EndOfGame(object sender, EventArgs e) {
            this.lblEoG.Visible = true;
        }
        #endregion

        #region _frameHandler_Score Event Handler
        void _frameHandler_Score(object sender, FrameScoreEventArgs e) {
            if (e.FrameScore > 0) {
                Label lblFrame = this.LookUpLabel(e.FrameNumber, string.Format(LABELNAME, e.FrameNumber));
                //System.Diagnostics.Debug.WriteLine(string.Format("_frameHandler_Score(...) - Frame: {0}; Score: {1}", e.FrameNumber, e.FrameScore));
                lblFrame.Text = e.FrameScore.ToString();
            }
        }
        #endregion

        #region txtBoxThrowOne_KeyUp Event Handler
        private void txtBoxThrowOne_KeyUp(object sender, KeyEventArgs e) {
            TextBox txtBox = (TextBox)sender as TextBox;
            if (txtBox != null && txtBox.Text.Length > 0) {
                if (this.HandleFirstThrow(txtBox)) {
                    e.Handled = true;
                    SendKeys.Send(KEY_TAB);
                } else e.Handled = false;
            }
        }
        #endregion
        //
        #region txtBoxThrowTwo_KeyUp Event Handler
        private void txtBoxThrowTwo_KeyUp(object sender, KeyEventArgs e) {
            TextBox txtBox = (TextBox)sender as TextBox;
            if (txtBox != null && txtBox.Text.Length > 0) {
                if (this.HandleSecondThrow(txtBox)) {
                    e.Handled = true;
                    SendKeys.Send(KEY_TAB);
                } else e.Handled = false;
            }
        }
        #endregion
        //
        #region txtBoxThrowThree_KeyUp Event Handler
        private void txtBoxThrowThree_KeyUp(object sender, KeyEventArgs e) {
            TextBox txtBox = (TextBox)sender as TextBox;
            if (txtBox != null && txtBox.Text.Length > 0) {
                if (this.HandleLastThrow(txtBox)) {
                    e.Handled = true;
                    SendKeys.Send(KEY_TAB);
                } else e.Handled = false;
            }
        }
        #endregion
        //
        #region btnExit_Click Event Handler
        private void btnExit_Click(object sender, EventArgs e) {
            this.Close();
        }
        #endregion
        //
        #region txtBoxThrow_KeyPress Event Handler
        private void txtBoxThrow_KeyPress(object sender, KeyPressEventArgs e) {
            if (VALID_KEYS_THROW.IndexOf(char.ToUpper(e.KeyChar)) != -1 || e.KeyChar == (char)8)
                e.Handled = false;
            else
                e.Handled = true;
        }
        #endregion
        //
        #region btnClear_Click Event Handler
        private void btnClear_Click(object sender, EventArgs e) {
            foreach(GroupBox grpBox in this._hashCtls.Values){
                foreach (Control t in grpBox.Controls) {
                    if (t is TextBox || t is Label){
                        t.Text = string.Empty;
                        t.Tag = 0;
                    }
                    if (t is TextBox) t.Enabled = true;
                }
            }
            this.lblEoG.Visible = false;
            this.lblPG.Visible = false;
            this._frameHandler.ClearAllFrames();
            this.txtBoxF1Throw1.Focus();
        }
        #endregion

       
    }
}
