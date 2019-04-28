using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace BowlTracker {
    #region FrameScoreEventArgs Class
    /// <summary>
    /// This holds the FrameNumber, FrameScore, which gets fired off to the consumer
    /// </summary>
    public class FrameScoreEventArgs : System.EventArgs {
        private int _iFrameScore = 0;
        private int _iFrameNo = 0;
        //
        #region Constructor
        public FrameScoreEventArgs(int iFrameScore, int iFrameNo) {
            this._iFrameScore = iFrameScore;
            this._iFrameNo = iFrameNo;
        }
        #endregion

        #region ReadOnly Properties...
        #region FrameScore Get Accessor
        /// <summary>
        /// This will hold the FrameScore
        /// </summary>
        public int FrameScore {
            get { return this._iFrameScore; }
        }
        #endregion
        //
        #region FrameNumber Get Accessor
        /// <summary>
        /// This will hold the FrameNumber
        /// </summary>
        public int FrameNumber {
            get { return this._iFrameNo; }
        }
        #endregion
        #endregion
    }
    #endregion
    //
    #region BTHelpers
    /// <summary>
    /// This class is a helper to hold the constants used. BT = BowlTracker
    /// </summary>
    public class BTHelpers{
        public const int MAX_PINS = 10;
        public const int MAX_FRAMES = 10;
        public const int MAX_FRAMES_SCORE = 300;
        //
        public const int MAX_SCORE = 10;
        //
        public const int FRAME_STD_THROWS = 2;
        public const int FRAME_LAST_THROWS = 3;
        //
        public const int FIRST_FRAME_INDEX = 1;
        public const int LAST_FRAME_INDEX = 10;
        //
        public const int FIRST_THROW_INDEX = 0;
        public const int SECOND_THROW_INDEX = 1;
        public const int LAST_THROW_INDEX = 2;
        //
        public const int BALL_IN_GUTTER = 0;
    }
    #endregion
    //
    #region BallThrow Class
    /// <summary>
    /// This class will hold the necessary information about a ball throw when it strikes the pins.
    /// </summary>
    public class BallThrow{
        private int _iPinsKnocked = 0;
        //
        private bool _bThrowIsStrike = false;
        private bool _bThrowIsSpare = false;
        //
        #region Constructor
        /// <summary>
        /// An empty constructor. See comment inside constructor.
        /// If we want to mark this as Spare/Strike later on after instantiation of this class!
        /// </summary>
        public BallThrow() {}
        #endregion
        //
        #region Constructor
        /// <summary>
        /// A Constructor that sets up the number of pins knocked down <see cref="PinsKnocked"/>
        /// </summary>
        /// <param name="iPinsKnocked">The number of pins knocked down</param>
        public BallThrow(int iPinsKnocked) {
            this._iPinsKnocked = iPinsKnocked;
        }
        #endregion
        //
        #region ReadOnly Properties
        #region PinsKnocked Get Accessor
        /// <summary>
        /// This returns back the number of pins knocked down by a throw.
        /// </summary>
        public int PinsKnocked {
            get { return this._iPinsKnocked; }
        }
        #endregion
        //
        #region IsAGutter Get Accessor
        /// <summary>
        /// This returns back a boolean to determine if this throw ended up in the gutter (on either side of lane)
        /// </summary>
        public bool IsAGutter {
            get { return (this._iPinsKnocked == 0); }
        }
        #endregion
        //
        #region SpareBall Get Accessor
        /// <summary>
        /// This returns back a boolean to determine if a spare was thrown, i.e. remainder of pins knocked down after a 
        /// previous throw
        /// </summary>
        public bool SpareBall {
            get { return (this._bThrowIsSpare == true); }
        }
        #endregion
        //
        #region StrikeBall Get Accessor
        /// <summary>
        /// This returns back a boolean to determine if the throw knocked all pins down on a throw.
        /// </summary>
        public bool StrikeBall {
            get { return (this._bThrowIsStrike == true); }
        }
        #endregion
        #endregion
        //
        #region MarkBall Method (First Overload)
        /// <summary>
        /// MarkBall (first overload) - This simply sets an internal value which can be accessed by readonly property 
        /// <see cref="PinsKnocked"/>
        /// </summary>
        /// <param name="iPinsKnocked">The number of pins knocked down</param>
        /// <remarks>This was necessary, if this class was instantiated using an empty constructor. We need a way to be able
        /// to set the pins knocked, if we had marked this throw as a Spare to make this more common sense.</remarks>
        public void MarkBall(int iPinsKnocked){
            this._iPinsKnocked = iPinsKnocked;
        }
        #endregion
        //
        #region MarkBall Method (Second Overload)
        /// <summary>
        /// MarkBall (second overload) This simply sets an internal value which can be accessed by a readonly property 
        /// <see cref="SpareBall"/> <seealso cref="StrikeBall"/>
        /// </summary>
        /// <param name="bStrike">A boolean to mark this throw as a strike</param>
        /// <param name="bSpare">A boolean to mark this throw as a spare</param>
        /// <remarks>When this class gets instantiated using empty constructor, we mark this as either strike or spare and
        /// set the pins knocked down accordingly by invoking the first overload of this same method
        public void MarkBall(bool bStrike, bool bSpare) {
            this._bThrowIsStrike = bStrike;
            this._bThrowIsSpare = bSpare;
        }
        #endregion
    }
    #endregion
    //
    #region Frame Class
    public class Frame {
        private int _iFrameNo;
        private int _iFrameScore;
        private int _iTotalPinsKnocked;
        //
        private int _iThrowCount;
        //
        private List<BallThrow> _throwList = null;
        /// <summary>
        /// AllFramesDone is an event which gets fired and caught by the consumer to indicate end of game play!
        /// </summary>
        public event EventHandler<System.EventArgs> AllFramesDone;
        /// <summary>
        /// EndOfFrame is an event which gets fired and caught by FrameHandler to handle the scoring.
        /// </summary>
        public event EventHandler<FrameScoreEventArgs> EndOfFrame;
        //
        #region Frame Constructor
        /// <summary>
        /// This initializes the collection _throwList and sets the frame number accordingly
        /// </summary>
        /// <param name="iFrameNo">The frame number that will be part of this instantiated class</param>
        public Frame(int iFrameNo) {
            this._iFrameNo = iFrameNo;
            this._throwList = new List<BallThrow>();
        }
        #endregion
        //
        #region Readonly Properties...
        #region this Indexer
        /// <summary>
        /// this indexer - returns back a BallThrow class as part of this frame's collection of BallThrow(s).
        /// </summary>
        /// <param name="index">An integer denoting the offset into the collection of BallThrows. This handles the last frame which could
        /// have three throws instead of two.</param>
        /// <returns>BallThrow class<see cref="BallThrow"/></returns>
        public BallThrow this[int index] {
            get {
                if (this._throwList.Count == 0) return null;
                if (!this.IsLastFrame) {
                    if (index >= BTHelpers.FIRST_THROW_INDEX && index <= BTHelpers.SECOND_THROW_INDEX) return this._throwList[index];
                    else return null; // Out of bounds....
                } else {
                    // Last Frame
                    if (index >= BTHelpers.FIRST_THROW_INDEX && index <= BTHelpers.LAST_THROW_INDEX) return this._throwList[index];
                    else return null; // Out of bounds....
                }
            }
        }
        #endregion
        //
        #region Number Get Accessor
        /// <summary>
        /// Returns back a Frame number that is being dealt with
        /// </summary>
        public int Number {
            get { return this._iFrameNo; }
        }
        #endregion
        //
        #region IsLastFrame Get Accessor
        /// <summary>
        /// Returns back a boolean to determine if we're on the last frame or not.
        /// </summary>
        public bool IsLastFrame {
            get { return (this._iFrameNo == BTHelpers.LAST_FRAME_INDEX); }
        }
        #endregion
        //
        #region IsFirstFrame Get Accessor
        /// <summary>
        /// Returns back a boolean to determine if we're on the first frame.
        /// </summary>
        public bool IsFirstFrame {
            get { return (this._iFrameNo == BTHelpers.FIRST_FRAME_INDEX); }
        }
        #endregion
        //
        #region IsStrike Get Accessor
        /// <summary>
        /// Returns back a boolean to determine if this frame counts as a strike <see cref="BallThrow.StrikeBall"/>
        /// </summary>
        public bool IsStrike {
            get {
                if (this._throwList.Count > 0) return this._throwList[BTHelpers.FIRST_THROW_INDEX].StrikeBall;
                return false;
            }
        }
        #endregion
        //
        #region IsSpare Get Accessor
        /// <summary>
        /// Returns back a boolean to determine if this frame counts as a spare <see cref="BallThrow.SpareBall"/>
        /// </summary>
        public bool IsSpare {
            get { 
                if (this._throwList.Count > 0) return this._throwList[BTHelpers.SECOND_THROW_INDEX].SpareBall;
                return false;
            }
        }
        #endregion
        //
        #region TotalPinsKnocked Get Accessor
        /// <summary>
        /// Returns back the Total of pins knocked down in this frame.
        /// </summary>
        public int TotalPinsKnocked {
            get { return this._iTotalPinsKnocked; }
        }
        #endregion
        //
        #region IsNormal Get Accessor
        /// <summary>
        /// Returns back a boolean to indicate that this frame has neither a strike or spare.
        /// </summary>
        public bool IsNormal{
            get{ return (!this.IsStrike && !this.IsSpare) == true; }
        }
        #endregion
        //
        #region CountOfThrows Get Accessor
        /// <summary>
        /// This returns back the number of throws used in this frame.
        /// </summary>
        public int CountOfThrows {
            get { return this._throwList.Count; }
        }
        #endregion
        //
        #region FirstBall Get Accessor
        /// <summary>
        /// Returns back the number of pins in this first throw.
        /// Remember - if this contains a strike it would have 10.
        /// </summary>
        public int FirstBall {
            get {
                if (this._throwList.Count > 0 && this._throwList[BTHelpers.FIRST_THROW_INDEX] != null) 
                    return this._throwList[BTHelpers.FIRST_THROW_INDEX].PinsKnocked;
                return 0;
            }
        }
        #endregion
        //
        #region SecondBall Get Accessor
        /// <summary>
        /// Returns back the number of pins knocked down in this second throw.
        /// Remember if the first throw was a strike, this will be 0 so you'd have to get the first throw of the next frame
        /// to pick up the correct score for the calculation in FrameHandler.CalcScore.
        /// </summary>
        public int SecondBall {
            get {
                if (this._throwList.Count > 0 && this._throwList[BTHelpers.SECOND_THROW_INDEX] != null) 
                    return this._throwList[BTHelpers.SECOND_THROW_INDEX].PinsKnocked;
                return 0;
            }
        }
        #endregion
        //
        #region LastBall Get Accessor
        /// <summary>
        /// This returns back the last throw, only for the last frame!
        /// </summary>
        public int LastBall {
            get {
                if (this._throwList.Count > 0 && this._throwList[BTHelpers.LAST_THROW_INDEX] != null)
                    return this._throwList[BTHelpers.LAST_THROW_INDEX].PinsKnocked;
                return 0;
            }
        }
        #endregion
        #endregion
        //
        #region Read-Write Properties...
        /// <summary>
        /// This can return back the score and sets it accordingly.
        /// </summary>
        public int Score {
            get { return this._iFrameScore; }
            set { this._iFrameScore = value; }
        }
        #endregion
        //
        #region OnAllFramesDone Event Invoker
        /// <summary>
        /// This fires off the event AllFramesDone to the consumer to let the consumer know that all frames are complete
        /// </summary>
        private void OnAllFramesDone() {
            if (this.AllFramesDone != null) this.AllFramesDone(this, System.EventArgs.Empty);
        }
        #endregion
        //
        #region OnEndOfFrame Event Invoker
        /// <summary>
        /// This fires off the event OnEndOfFrame to the consumer to let the consumer know that all throws for this frame is 
        /// complete. This aids in working out the scores automatically
        /// </summary>
        /// <param name="e">FrameScoreEventArgs that will hold the score <see cref="FrameScoreEventArgs"/>  </param>
        private void OnEndOfFrame(FrameScoreEventArgs e) {
            if (this.EndOfFrame != null)    this.EndOfFrame(this, e);
        }
        #endregion
        //
        #region ClearThrows Method
        /// <summary>
        /// This simply clears the collection for another iteration of the game play
        /// </summary>
        public void ClearThrows() {
            this._throwList.Clear();
            this._iTotalPinsKnocked = this._iFrameScore = 0;
        }
        #endregion
        //
        #region ThrowBall Method
        /// <summary>
        /// ThrowBall marks the BallThrow class by setting the number of pins knocked.<see cref="BallThrow"/>
        /// </summary>
        /// <param name="iPins">The number of pins knocked down.</param>
        /// <remarks>This will fire the OnEndOfFrame once the throws are ascertained <see cref="OnEndOfFrame"/>.
        /// For those frames that are between one and nine, processing those frames are relatively simple,
        /// However, the last frame is where things can get tricky since we have to determine if the first throw was
        /// a strike, or the second or third throw was a spare and to be able to use the third throw if BOTH conditions occurred
        /// (i.e. A Strike in the first, then n pins knocked then a spare OR n pins knocked in the first, a Spare in the second,
        /// and possibly either a strike or n pins knocked in the third. The complexity was handled by using a switch case to 
        /// determine where we are in relation to keeping track of the successive throws.
        /// </remarks>
        public void ThrowBall(int iPins) {
            if (!this.IsLastFrame) {
                #region Frames 1..9...
                if (this._throwList.Count == 0) { 
                    // First throw...
                    BallThrow throwBall = new BallThrow(iPins);
                    //
                    if (iPins == BTHelpers.MAX_PINS) throwBall.MarkBall(true, false); // A Strike was used!
                    this._throwList.AddRange(new BallThrow[] { throwBall, new BallThrow() });
                    if (throwBall.StrikeBall) {
                        throwBall.MarkBall(BTHelpers.MAX_PINS);
                        int TotalPinsKnocked = BTHelpers.MAX_SCORE;
                        this._iFrameScore = TotalPinsKnocked;
                        this._iTotalPinsKnocked = BTHelpers.MAX_SCORE;
                        this.OnEndOfFrame(new FrameScoreEventArgs(TotalPinsKnocked, this._iFrameNo));
                        return;
                    }
                } else {
                    // Check for the first throw
                    BallThrow prevThrow = this._throwList[BTHelpers.FIRST_THROW_INDEX];
                    if (prevThrow.PinsKnocked + iPins == BTHelpers.MAX_PINS) {
                        // Spare!
                        this._throwList[BTHelpers.SECOND_THROW_INDEX].MarkBall(false, true);
                    }
                    this._throwList[BTHelpers.SECOND_THROW_INDEX].MarkBall(iPins);
                    int TotalPinsKnocked = this._throwList[BTHelpers.FIRST_THROW_INDEX].PinsKnocked + this._throwList[BTHelpers.SECOND_THROW_INDEX].PinsKnocked;
                    this._iFrameScore = TotalPinsKnocked;
                    if (this._throwList.Count == BTHelpers.FRAME_STD_THROWS) {
                        this._iTotalPinsKnocked = TotalPinsKnocked;
                        this.OnEndOfFrame(new FrameScoreEventArgs(TotalPinsKnocked, this._iFrameNo));
                    }
                }
                #endregion
            } else {
                #region Frame 10...
                BallThrow prevThrow = null;
                BallThrow throwLast = new BallThrow(iPins);
                switch (this._throwList.Count) {
                    case 0:
                        #region First Throw on Last Frame
                        // Simple enough case...we're on the first throw of the last frame.
                        // This could be either n pins knocked or a strike
                        BallThrow throwFirstBall = new BallThrow(iPins);
                        if (iPins == BTHelpers.MAX_PINS) throwFirstBall.MarkBall(true, false);
                        //this._iFrameScore = throwBall.StrikeBall ? BTHelpers.MAX_SCORE : iPins;
                        this._throwList.Add(throwFirstBall);
                        #endregion
                        break;
                    case 1:
                        // Second throw of the last frame
                        #region Second Throw on Last Frame
                        // Check the previous
                        prevThrow = this._throwList[BTHelpers.FIRST_THROW_INDEX]; 
                        if (prevThrow.StrikeBall) {
                            // this... CANNOT BE A SPARE - IMPOSSIBLE! 1st Throw a Strike, IMPOSSIBLE TO HAVE A SPARE in the second
                            this._throwList.Add(throwLast);
                            return;
                        } else {
                            // Both ordinary no's...i.e. the first throw was n Pins, the second throw was (MAX_PINS - n) pins knocked down.
                            if (prevThrow.PinsKnocked + iPins < BTHelpers.MAX_PINS) {
                                #region Previous Throw + iPins < 10
                                this._throwList.Add(throwLast);
                                int TotalPinsKnocked = this._throwList[BTHelpers.FIRST_THROW_INDEX].PinsKnocked + this._throwList[BTHelpers.SECOND_THROW_INDEX].PinsKnocked;
                                this._iFrameScore = TotalPinsKnocked;
                                this._iTotalPinsKnocked = TotalPinsKnocked;
                                // Signal end of frame...no more throws at this point
                                this.OnEndOfFrame(new FrameScoreEventArgs(TotalPinsKnocked, this._iFrameNo));
                                // Signal game complete!
                                this.OnAllFramesDone();
                                #endregion
                                return;
                            } else {
                                // Both ordinary no's...i.e. the first throw was n Pins, the second throw was spare!
                                // So another throw would be used i.e. 3rd throw.
                                if (prevThrow.PinsKnocked + iPins == BTHelpers.MAX_PINS) {
                                    #region Previous Throw + iPins = 10
                                    // This is a spare
                                    throwLast.MarkBall(false, true);
                                    this._throwList.Add(throwLast);
                                    int TotalPinsKnocked = BTHelpers.MAX_PINS;
                                    this._iFrameScore = TotalPinsKnocked;
                                    #endregion
                                }
                            }
                        }
                        #endregion
                        break;
                    case 2:
                    default:
                        // We're on the last throw
                        #region Last Throw on Last Frame
                        prevThrow = this._throwList[BTHelpers.SECOND_THROW_INDEX]; // Check the second throw
                        if (prevThrow.StrikeBall) {
                            #region Previous Throw was a Strike
                            // this... CANNOT BE A SPARE - IMPOSSIBLE!
                            this._throwList.Add(throwLast);
                            int TotalPinsKnocked = this._throwList[BTHelpers.SECOND_THROW_INDEX].PinsKnocked +this._throwList[BTHelpers.LAST_THROW_INDEX].PinsKnocked;
                            this._iFrameScore = TotalPinsKnocked;
                            this._iTotalPinsKnocked = TotalPinsKnocked;
                            // Signal end of frame...no more throws at this point
                            this.OnEndOfFrame(new FrameScoreEventArgs(TotalPinsKnocked, this._iFrameNo));
                            this.OnAllFramesDone();
                            #endregion
                            return;
                        }
                        if (prevThrow.SpareBall) {
                            //System.Diagnostics.Debug.WriteLine(string.Format("ThrowBall(...)#1 - if (prevThrow.SpareBall) condition true"));
                            // Oh! the 2nd throw was a spare...
                            if (prevThrow.PinsKnocked + iPins < BTHelpers.MAX_PINS) { // Both ordinary no's...
                                #region Previous Throw + iPins < 10
                                //System.Diagnostics.Debug.WriteLine(string.Format("ThrowBall(...)#1 - if (prevThrow.PinsKnocked + iPins < BTHelpers.MAX_PINS) condition true"));
                                this._throwList.Add(throwLast);
                                int TotalPinsKnocked = BTHelpers.MAX_PINS + this._throwList[BTHelpers.LAST_THROW_INDEX].PinsKnocked;
                                this._iFrameScore = TotalPinsKnocked;
                                this._iTotalPinsKnocked = TotalPinsKnocked;
                                // Signal end of frame...no more throws at this point
                                this.OnEndOfFrame(new FrameScoreEventArgs(TotalPinsKnocked, this._iFrameNo));
                                #endregion
                            } else {
                                //System.Diagnostics.Debug.WriteLine(string.Format("ThrowBall(...)#1 - if (prevThrow.PinsKnocked + iPins < BTHelpers.MAX_PINS) condition false"));
                                if (prevThrow.PinsKnocked + iPins == BTHelpers.MAX_PINS) { // this throw + prev = spare.. Will have another shot!...
                                    #region Previous Throw + iPins = 10
                                    //System.Diagnostics.Debug.WriteLine(string.Format("ThrowBall(...)#1 - if (prevThrow.PinsKnocked + iPins == BTHelpers.MAX_PINS) condition true"));
                                    throwLast.MarkBall(false, true);
                                    this._throwList.Add(throwLast);
                                    int TotalPinsKnocked = BTHelpers.MAX_PINS + this._throwList[BTHelpers.LAST_THROW_INDEX].PinsKnocked;
                                    this._iFrameScore = TotalPinsKnocked;
                                    this._iTotalPinsKnocked = TotalPinsKnocked;
                                    // Signal end of frame...no more throws at this point
                                    this.OnEndOfFrame(new FrameScoreEventArgs(TotalPinsKnocked, this._iFrameNo));
                                    #endregion
                                } else {
                                    #region This throw was a spare
                                    // 2nd throw was a spare
                                    // 3rd throw was a normal one with n pins knocked.
                                    //System.Diagnostics.Debug.WriteLine(string.Format("ThrowBall(...)#1 - if (prevThrow.PinsKnocked + iPins == BTHelpers.MAX_PINS) condition false"));
                                    throwLast.MarkBall(false, true);
                                    this._throwList.Add(throwLast);
                                    int TotalPinsKnocked = BTHelpers.MAX_PINS + this._throwList[BTHelpers.LAST_THROW_INDEX].PinsKnocked;
                                    this._iFrameScore = TotalPinsKnocked;
                                    this._iTotalPinsKnocked = TotalPinsKnocked;
                                    // Signal end of frame...no more throws at this point
                                    this.OnEndOfFrame(new FrameScoreEventArgs(TotalPinsKnocked, this._iFrameNo));
                                    #endregion
                                }
                            }
                            // Signal game complete!
                            this.OnAllFramesDone();
                        } else {
                            #region Previous Throw was ordinary (non-strike/spare)
                            // 2nd Throw was just an ordinary pins knocked
                            // this last throw just happened
                            this._throwList.Add(throwLast);
                            // Normal digits...
                            int TotalPinsKnocked = this._throwList[BTHelpers.FIRST_THROW_INDEX].PinsKnocked + this._throwList[BTHelpers.SECOND_THROW_INDEX].PinsKnocked + this._throwList[BTHelpers.LAST_THROW_INDEX].PinsKnocked;
                            this._iFrameScore = TotalPinsKnocked;
                            this._iTotalPinsKnocked = TotalPinsKnocked;
                            // Signal End of Frame
                            this.OnEndOfFrame(new FrameScoreEventArgs(TotalPinsKnocked, this._iFrameNo));
                            // Signal game complete!
                            this.OnAllFramesDone();
                            #endregion
                        }
                        #endregion
                        // Allow it to fall through!
                        break;
                }
                #endregion
            }
        }
        #endregion
        //
        #region ThrowBall Method
        /// <summary>
        /// ThrowBall marks the BallThrow class by setting the flag to indicate if the throw was a spare/strike.<see cref="BallThrow"/>
        /// </summary>
        /// <param name="bStrike">The boolean flag to indicate a Strike</param>
        /// <param name="bSpare">The boolean flag to indicate a Spare</param>
        /// <remarks>This will fire the OnEndOfFrame once the throws are ascertained <see cref="OnEndOfFrame"/>.
        /// For those frames that are between one and nine, processing those frames are relatively simple,
        /// However, the last frame is where things can get tricky since we have to determine if the first throw was
        /// a strike, or the second or third throw was a spare and to be able to use the third throw if BOTH conditions occurred
        /// (i.e. A Strike in the first, then n pins knocked then a spare OR n pins knocked in the first, a Spare in the second,
        /// and possibly either a strike or n pins knocked in the third. The complexity was handled by using a switch case to 
        /// determine where we are in relation to keeping track of the successive throws.
        /// </remarks>
        public void ThrowBall(bool bStrike, bool bSpare) {
            if (!this.IsLastFrame) {
                #region Frames 1...9...
                if (this._throwList.Count == 0) {
                    BallThrow throwBall = new BallThrow();
                    //
                    if (bStrike) {
                        throwBall.MarkBall(true, false);
                        throwBall.MarkBall(BTHelpers.MAX_PINS);
                    }
                    this._throwList.AddRange(new BallThrow[] { throwBall, new BallThrow() });
                    if (throwBall.StrikeBall) {
                        int lTotalPinsKnocked = BTHelpers.MAX_SCORE;
                        this._iFrameScore = this._iTotalPinsKnocked = lTotalPinsKnocked;
                        this.OnEndOfFrame(new FrameScoreEventArgs(lTotalPinsKnocked, this._iFrameNo));
                        return;
                    }
                } else {
                    // Check for the first throw
                    BallThrow prevThrow = this._throwList[BTHelpers.FIRST_THROW_INDEX];
                    if (bSpare) {
                        // Spare!
                        int iRemainPinsKnocked = BTHelpers.MAX_PINS - prevThrow.PinsKnocked;
                        this._throwList[BTHelpers.SECOND_THROW_INDEX].MarkBall(false, true);
                        this._throwList[BTHelpers.SECOND_THROW_INDEX].MarkBall(iRemainPinsKnocked);
                    }
                }
                int TotalPinsKnocked = this._throwList[BTHelpers.FIRST_THROW_INDEX].PinsKnocked + this._throwList[BTHelpers.SECOND_THROW_INDEX].PinsKnocked;
                this._iFrameScore = this._iTotalPinsKnocked = TotalPinsKnocked;
                if (this._throwList.Count == BTHelpers.FRAME_STD_THROWS)  this.OnEndOfFrame(new FrameScoreEventArgs(TotalPinsKnocked, this._iFrameNo));
                #endregion
            } else {
                #region Frame 10...
                BallThrow prevThrow = null;
                switch (this._throwList.Count) {
                    case 0:
                        #region First Throw on Last Frame
                        BallThrow throwBall = new BallThrow(BTHelpers.MAX_SCORE);
                        throwBall.MarkBall(bStrike, bSpare);
                        this._throwList.Add(throwBall);
                        #endregion
                        break;
                    case 1:
                        #region Second Throw on Last Frame...
                        prevThrow = this._throwList[BTHelpers.FIRST_THROW_INDEX]; // Check the previous
                        if (bSpare) { // Both ordinary no's...
                            BallThrow throwLast = new BallThrow(BTHelpers.MAX_PINS - prevThrow.PinsKnocked);
                            throwLast.MarkBall(bStrike, bSpare);
                            this._throwList.Add(throwLast);
                            int TotalPinsKnocked = BTHelpers.MAX_PINS;
                            this._iFrameScore = this._iTotalPinsKnocked = TotalPinsKnocked;
                            return;
                        }
                        if (bStrike) {
                            BallThrow throwLast = new BallThrow(BTHelpers.MAX_PINS);
                            throwLast.MarkBall(bStrike, bSpare);
                            this._throwList.Add(throwLast);
                            int TotalPinsKnocked = this._throwList[BTHelpers.FIRST_THROW_INDEX].PinsKnocked + this._throwList[BTHelpers.SECOND_THROW_INDEX].PinsKnocked;
                            this._iFrameScore = this._iTotalPinsKnocked = TotalPinsKnocked;
                            return;
                        }
                        #endregion
                        break;
                    case 2:
                        #region Last Throw on Last Frame...
                        prevThrow = this._throwList[BTHelpers.SECOND_THROW_INDEX]; // Check the previous
                        if (bSpare) { // Both ordinary no's...
                            BallThrow throwLast = new BallThrow(BTHelpers.MAX_PINS - prevThrow.PinsKnocked);
                            throwLast.MarkBall(bStrike, bSpare);
                            this._throwList.Add(throwLast);
                            int TotalPinsKnocked = BTHelpers.MAX_PINS;
                            this._iFrameScore = this._iTotalPinsKnocked = TotalPinsKnocked;
                            this.OnEndOfFrame(new FrameScoreEventArgs(TotalPinsKnocked, this._iFrameNo));
                            this.OnAllFramesDone();
                            return;
                        }
                        if (bStrike) {
                            BallThrow throwLast = new BallThrow(BTHelpers.MAX_PINS);
                            throwLast.MarkBall(bStrike, bSpare);    
                            this._throwList.Add(throwLast);
                            int TotalPinsKnocked = this._throwList[BTHelpers.FIRST_THROW_INDEX].PinsKnocked + this._throwList[BTHelpers.SECOND_THROW_INDEX].PinsKnocked + this._throwList[BTHelpers.LAST_THROW_INDEX].PinsKnocked;
                            this._iFrameScore = this._iTotalPinsKnocked = TotalPinsKnocked;
                            this.OnEndOfFrame(new FrameScoreEventArgs(TotalPinsKnocked, this._iFrameNo));
                            this.OnAllFramesDone();
                            return;
                        }
                        #endregion
                        break;
                    default:
                        break;
                }
                #endregion
            }
        }
        #endregion
    }
    #endregion
    //
    #region FrameHandler Class
    /// <summary>
    /// This is the workhorse that glues together the usage of <see cref="Frame"/> and <see cref="BallThrow"/> classes
    /// for manipulating the frames...
    /// </summary>
    public class FrameHandler : IEnumerable<Frame>, IEnumerator<Frame> {
        private int _iPos = -1;
        //
        private List<Frame> _listFrames = new List<Frame>();
        //
        /// <summary>
        /// A public event that fires a Score Event which gets consumed and updates the score accordingly
        /// </summary>
        public event EventHandler<FrameScoreEventArgs> Score;
        /// <summary>
        /// A public event that fires a Perfect Game event which gets consumed and the consumer can handle this accordingly
        /// </summary>
        public event EventHandler<System.EventArgs> PerfectGame;
        /// <summary>
        /// A public event that fires a End of Game event which gets consumed and the consumer can handle this accordingly
        /// </summary>
        public event EventHandler<System.EventArgs> EndOfGame;
        //
        private int _iLastFramePrev = 0;
        //
        #region Constructor
        /// <summary>
        /// This initializes the collection of frames _listFrames and subscribes to the frame's event <see cref="Frame.EndOfFrame"/> and
        /// also subscribes to the end of gameplay <see cref="Frame.AllFramesDone"/>
        /// </summary>
        public FrameHandler() {
            for (int nLoopCnt = 0; nLoopCnt < BTHelpers.MAX_FRAMES; nLoopCnt++) {
                Frame f = new Frame(nLoopCnt + 1);
                f.EndOfFrame += new EventHandler<FrameScoreEventArgs>(f_EndOfFrame);
                this._listFrames.Add(f);
            }
            Frame fLastFrame = this[BTHelpers.LAST_FRAME_INDEX];
            fLastFrame.AllFramesDone += new EventHandler<EventArgs>(fLastFrame_AllFramesDone);
        }
        #endregion

        #region Readonly properties...
        #region this Indexer
        /// <summary>
        /// This returns back the appropriate frame. This takes care of correcting the offset into the collection of Frames.
        /// </summary>
        /// <param name="index">The frame number we want</param>
        /// <returns>Frame <see cref="Frame"/> </returns>
        public Frame this[int index] {
            get {
                if (index >= BTHelpers.FIRST_FRAME_INDEX && index <= BTHelpers.LAST_FRAME_INDEX) {
                    return this._listFrames[index - 1];
                }
                return null;
            }
        }
        #endregion
        #endregion
        //
        #region fLastFrame_AllFramesDone Event Handler
        /// <summary>
        /// This consumes the event <see cref="Frame.AllFramesDone"/>
        /// Standard parameters in guidelines to the majority of BCL's event handler pattern
        /// This simply posts a message to the diagnostics to indicate game is complete!
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void fLastFrame_AllFramesDone(object sender, EventArgs e) {
            System.Diagnostics.Debug.WriteLine("**** THIS GAME IS COMPLETE ****");
            Frame fCurr = this[BTHelpers.LAST_FRAME_INDEX];
            fCurr.Score = fCurr.TotalPinsKnocked + this._iLastFramePrev;
            this.OnScore(new FrameScoreEventArgs(fCurr.Score, fCurr.Number));
            this.OnEndOfGame();
            if (fCurr.Score == BTHelpers.MAX_FRAMES_SCORE) this.OnPerfectGame();
        }
        #endregion

        #region f_EndOfFrame Event Handler
        /// <summary>
        /// This consumes the event <see cref="Frame.EndOfFrame"/>
        /// This takes care of handling the score and checking the previous frames and firing off the event to the outside
        /// world to update their score display. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>    
        void f_EndOfFrame(object sender, FrameScoreEventArgs e) {
            this._iLastFramePrev = this.CalcScore(e.FrameNumber);
            this.Reset();
            foreach (Frame f in this) {
                if (!f.IsLastFrame) this.OnScore(new FrameScoreEventArgs(f.Score, f.Number));
            }
        }
        #endregion

        #region IEnumerable<Frame> Members
        public IEnumerator<Frame> GetEnumerator() {
            return this;
        }
        #endregion

        #region IEnumerable Members
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return this;
        }
        #endregion

        #region IEnumerator<Frame> Members
        public Frame Current {
            get { return this._listFrames[this._iPos]; }
        }
        #endregion

        #region IDisposable Members
        public void Dispose() {
            //throw new NotImplementedException();
        }
        #endregion

        #region IEnumerator Members
        object System.Collections.IEnumerator.Current {
            get { return this._listFrames[this._iPos]; }
        }

        public bool MoveNext() {
            this._iPos++;
            return (this._iPos < this._listFrames.Count);
        }

        public void Reset() {
            this._iPos = -1;
        }
        #endregion

        #region OnScore Event Invoker
        /// <summary>
        /// A consumer will instantiate this class FrameHandler and subscribe to this event to enable updating of frames scores.
        /// </summary>
        /// <param name="args">FrameScoreEventArgs class <see cref="FrameScoreEventArgs"/> </param>
        private void OnScore(FrameScoreEventArgs args) {
            if (this.Score != null) this.Score(this, args);
        }
        #endregion
        //
        #region OnPerfectGame Event Invoker
        /// <summary>
        /// A consumer will instantiate this class FrameHandler and subscribe to this event to enable the notification of Maximum Score in the game.
        /// </summary>
        private void OnPerfectGame(){
            if (this.PerfectGame != null) this.PerfectGame(this, System.EventArgs.Empty);
        }
        #endregion
        //
        #region OnEndOfGame Event Invoker
        /// <summary>
        /// A consumer will instantiate this class FrameHandler and subscribe to this event to enable the notification of end of gameplay.
        /// </summary>
        private void OnEndOfGame() {
            if (this.EndOfGame != null) this.EndOfGame(this, System.EventArgs.Empty);
        }
        #endregion
        //
        #region ClearAllFrames Method
        /// <summary>
        /// Clears all frames and their related throws....
        /// </summary>
        public void ClearAllFrames() {
            this.Reset();
            foreach (Frame f in this) f.ClearThrows();
        }
        #endregion
        //
        #region CalcScore Function
        /// <summary>
        /// CalcScore is the meat of the scoring...It stuffs the score tally into an array which will be used!
        /// </summary>
        /// <param name="iFrameNo">The desired frame no that we want to calculate</param>
        /// <returns>An int denoting the tally of the frames up to the desired frame</returns>
        private int CalcScore(int iFrameNo) {
            int iScore = 0;
            //System.Diagnostics.Debug.WriteLine(string.Format("[DEBUG] - CalcScore() - FRAME #{0} **********", iFrameNo));
            for (int iCurrentFrame = BTHelpers.FIRST_FRAME_INDEX; iCurrentFrame < iFrameNo; iCurrentFrame++) {
                Frame f = this[iCurrentFrame];
                Frame fNext = this[iCurrentFrame + 1];
                Frame fNextNext = this[iCurrentFrame + 2];
                int iNextTwoBalls = 0;
                if (fNext != null){
                    iNextTwoBalls += fNext.FirstBall;
                    if (fNext.SecondBall == 0) {
                        if (fNextNext != null) {
                            iNextTwoBalls += fNextNext.FirstBall;
                        }
                    } else {
                        iNextTwoBalls += fNext.SecondBall;
                    }
                }
                if (f.IsStrike)  iScore += BTHelpers.MAX_SCORE + iNextTwoBalls;
                else if (f.IsSpare) iScore += BTHelpers.MAX_SCORE + fNext.FirstBall;
                else iScore += f.TotalPinsKnocked;
                f.Score = iScore;
                //System.Diagnostics.Debug.WriteLine(string.Format("[DEBUG] - CalcScore() - Score: {0}", iScore));
            }
            return iScore;
        }
        #endregion
    }
    #endregion
}
