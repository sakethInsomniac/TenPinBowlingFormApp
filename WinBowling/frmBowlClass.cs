using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace WinBowling {
    public partial class frmBowlCalc : Form {
        #region HashGroups Method
        /// <summary>
        /// This method makes it easier to put in the GroupBox control into a dictionary, that is associated with each frame.
        /// Instance retrieval of an appropriate label/textbox based on Frame Number!
        /// </summary>
        private void HashGroups() {
            foreach (Control c in this.Controls) {
                if (c is GroupBox) {
                    GroupBox grp = (GroupBox)c as GroupBox;
                    if (grp != null) {
                        Match m = this._reGrpName.Match(grp.Name);
                        if (m.Success) {
                            int iFrameNo = int.Parse(m.Groups[REGEXP_FRAMENO].Value);
                            this._hashCtls.Add(iFrameNo, grp);
                        }
                    }
                }
            }
        }
        #endregion

        #region LookUpTextBox Function
        /// <summary>
        /// This returns back a textbox based on frame number and throw number to manipulate the control
        /// </summary>
        /// <param name="iFrameNo">An integer representing the Frame</param>
        /// <param name="sTxtBoxTargetName">A string holding the text box's control name</param>
        /// <returns>A Textbox</returns>
        private TextBox LookUpTextBox(int iFrameNo, string sTxtBoxTargetName) {
            GroupBox grpBox = this._hashCtls[iFrameNo];
            if (grpBox != null) {
                foreach (Control t in grpBox.Controls) {
                    if (t is TextBox) {
                        TextBox txtBox = (TextBox)t;
                        if (txtBox.Name.Equals(sTxtBoxTargetName)) return txtBox;
                    }
                }
            }
            return null;
        }
        #endregion

        #region LookUpLabel Function
        /// <summary>
        /// This returns back a label based on frame number to manipulate the control
        /// </summary>
        /// <param name="iFrameNo">An integer representing the Frame</param>
        /// <param name="sLblBoxTargetName">A string holding the label's control name</param>
        /// <returns>A Label</returns>
        private Label LookUpLabel(int iFrameNo, string sLblBoxTargetName) {
            GroupBox grpBox = this._hashCtls[iFrameNo];
            if (grpBox != null) {
                foreach (Control t in grpBox.Controls) {
                    if (t is Label) {
                        Label lblBox = (Label)t;
                        if (lblBox.Name.Equals(sLblBoxTargetName)) return lblBox;
                    }
                }
            }
            return null;
        }
        #endregion

        #region DisableSecondThrow Method
        /// <summary>
        /// This gets called when a first throw in a frame is a Strike (X)
        /// </summary>
        /// <param name="iFrameNo">An integer representing a Frame Number</param>
        private void DisableSecondThrow(int iFrameNo) {
            // Lookup for the other text box
            string sTxtBox2Hunt = string.Format(SECONDTHROW_ANY_FRAME, iFrameNo);
            TextBox txtBoxTarget = this.LookUpTextBox(iFrameNo, sTxtBox2Hunt);
            if (txtBoxTarget != null) txtBoxTarget.Enabled = false;
        }
        #endregion

        #region InvalidThrow Method
        /// <summary>
        /// If an invalid input was entered for a textbox, this simply clears it.
        /// </summary>
        /// <param name="txtBox">A Textbox that needs the Text's contents cleared.</param>
        private void InvalidThrow(TextBox txtBox) {
            txtBox.Text = string.Empty;
        }
        #endregion
        //
        #region HandleFirstThrow Function
        /// <summary>
        /// This takes care of the logic in handling the input for the first throw. This also handles whether the input is on
        /// the last frame or not.
        /// </summary>
        /// <param name="txtBox">A Textbox to manipulate.</param>
        /// <returns>A Boolean value indicating success (true) or false for failure</returns>
        private bool HandleFirstThrow(TextBox txtBox) {
            string sName = txtBox.Name;
            Match m = this._reTextInputName.Match(sName);
            if (m.Success) {
                int iFrameNo = int.Parse(m.Groups[REGEXP_FRAMENO].Value);
                int iThrowNo = int.Parse(m.Groups[REGEXP_THROWNO].Value);
                if (iFrameNo < BowlTracker.BTHelpers.MAX_FRAMES) {
                    #region Frames 1..9...
                    if (txtBox.Text.Equals(STRIKE)) {
                        this.DisableSecondThrow(iFrameNo);
                        this._frameHandler[iFrameNo].ThrowBall(true, false);
                        return true;
                    }
                    if (txtBox.Text.Equals(SPARE)) { // Cannot have a spare on the first throw! - illogical!
                        this.InvalidThrow(txtBox);
                        return false;
                    }
                    #endregion
                } else {
                    #region Last Frame...
                    // Last Frame...
                    if (txtBox.Text.Equals(STRIKE)) {
                        this._frameHandler[iFrameNo].ThrowBall(true, false);
                        return true;
                    }
                    #endregion
                }
                // Anything else here would be numeric...
                int iPins = int.Parse(txtBox.Text);
                this._frameHandler[iFrameNo].ThrowBall(iPins);
                // Go back to previous frame and check if a spare was used
                return true;
            }
            return false;
        }
        #endregion
        //
        #region HandleSecondThrow Function
        /// <summary>
        /// This takes care of the logic in handling the input for the second throw. This also handles whether the input is on
        /// the last frame or not.
        /// </summary>
        /// <param name="txtBox">A Textbox to manipulate.</param>
        /// <returns>A Boolean value indicating success (true) or false for failure</returns>
        private bool HandleSecondThrow(TextBox txtBox) {
            string sName = txtBox.Name;
            Match m = this._reTextInputName.Match(sName);
            if (m.Success) {
                int iFrameNo = int.Parse(m.Groups[REGEXP_FRAMENO].Value);
                int iThrowNo = int.Parse(m.Groups[REGEXP_THROWNO].Value);
                if (iFrameNo < BowlTracker.BTHelpers.MAX_FRAMES) {
                    #region Frames 1-9...
                    // We're on the other frame(s)...
                Throw2ndSpare:
                    if (txtBox.Text.Equals(SPARE)) {
                        this._frameHandler[iFrameNo].ThrowBall(false, true);
                        return true;
                    }
                    if (txtBox.Text.Equals(STRIKE)) {
                        this.InvalidThrow(txtBox);
                        return false;
                    }
                    TextBox prevThrow = this.LookUpTextBox(iFrameNo, string.Format(FIRSTTHROW_ANY_FRAME, iFrameNo));
                    int iFirstThrow = 0;
                    if (prevThrow != null) iFirstThrow = int.Parse(prevThrow.Text);
                    int iSecondThrow = int.Parse(txtBox.Text);
                    if (iFirstThrow + iSecondThrow == BowlTracker.BTHelpers.MAX_PINS) {
                        // Set it to spare
                        txtBox.Text = SPARE;
                        goto Throw2ndSpare;
                    }
                    if (iFirstThrow + iSecondThrow < BowlTracker.BTHelpers.MAX_PINS) {
                        this._frameHandler[iFrameNo].ThrowBall(iSecondThrow);
                        return true;
                    }
                    // Any thing else here is an error!
                    this.InvalidThrow(txtBox);
                    return false;
                    #endregion
                } else {
                    // We're on the last Frame...
                    #region Frame 10...
                    if (txtBox.Text.Equals(SPARE) || txtBox.Text.Equals(STRIKE)) {
                        if (txtBox.Text.Equals(STRIKE)) this._frameHandler[iFrameNo].ThrowBall(true, false);
                        if (txtBox.Text.Equals(SPARE)) this._frameHandler[iFrameNo].ThrowBall(false, true);
                        return true;
                    } else {
                        TextBox txtBoxPrevThrow = this.LookUpTextBox(BowlTracker.BTHelpers.MAX_FRAMES, string.Format(FIRSTTHROW_ANY_FRAME, BowlTracker.BTHelpers.MAX_FRAMES));
                        if (!txtBoxPrevThrow.Text.Equals(STRIKE)) {
                            // Disable the third
                            TextBox txtBoxLastFrameLastThrow = this.LookUpTextBox(BowlTracker.BTHelpers.MAX_FRAMES, THIRDTHROW_LAST_FRAME);
                            if (txtBoxLastFrameLastThrow != null) txtBoxLastFrameLastThrow.Enabled = false;
                        }
                    }
                    int iSecondThrow = int.Parse(txtBox.Text);
                    this._frameHandler[iFrameNo].ThrowBall(iSecondThrow);
                    return true;
                    #endregion
                }
            }
            return false;
        }
        #endregion
        //
        #region HandleLastThrow Function
        /// <summary>
        /// This takes care of the logic in handling the input for the last throw of the last frame.
        /// </summary>
        /// <param name="txtBox">A Textbox to manipulate.</param>
        /// <returns>A Boolean value indicating success (true) or false for failure</returns>
        private bool HandleLastThrow(TextBox txtBox) {
            string sName = txtBox.Name;
            Match m = this._reTextInputName.Match(sName);
            if (m.Success) {
                int iFrameNo = int.Parse(m.Groups[REGEXP_FRAMENO].Value);
                int iThrowNo = int.Parse(m.Groups[REGEXP_THROWNO].Value);
                if (iThrowNo == 3) {
                    if (txtBox.Text.Equals(STRIKE)) {
                        this._frameHandler[iFrameNo].ThrowBall(true, false);
                        return true;
                    }
                    if (txtBox.Text.Equals(SPARE)) {
                        this._frameHandler[iFrameNo].ThrowBall(false, true);
                        return true;
                    }
                    // Anything else here is number by default
                    int iVal = int.Parse(txtBox.Text);
                    this._frameHandler[iFrameNo].ThrowBall(iVal);
                    return true;
                }
            }
            return false;
        }
        #endregion
        //
    }
}
