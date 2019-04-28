For the Scoring input validation I used two open-source different calculators found on sourceforge.net

1. Flash Based Calculator - http://sourceforge.net/projects/fl-bsc

2. Javascript Based Calculator - http://sourceforge.net/projects/js-bsc/

Those calculators were useful in ensuring the scoring works.

This solution leverages the usage of OOP:
	- As each frame advances, it checks the previous frame and adjusts the score accordingly
	- The UI simply validates each input and calls the FrameHandler instance to put in the scoring.
	- Please see README_ListOfInputs.txt to see the where the test cases was used.
		
The UI consists of each group box representing a frame and each textbox inside that group box represents a throw.
Input validation is done at the UI level to ensure no bad inputs are used.
In the case of a strike, the second text box gets disabled and the focus advances to the next group box.
In the case of one throw being numeric (from 0-9), the input for the next throw (if it's not a spare) correctly deduces the remainder
 of pins and if the two throws sum up to 10 pins, it replaces that numeric in the second text box with a slash to indicate a spare.
When the end-of-game is reached, click on the Reset Button to clear out the throws and start again.
- If an error is made in the frame, a reset is required and re-enter it again (this is due to the way the classes are designed!)
--------------------------------------------------------------------
This solution works for most of the inputs.
BowlTracker.cs contains the logic required to handle all the frames.
Using the ASCII equivalent of a UML diagram...

	[BallThrow Class]->----------|- [Frame Class]->----------| [FrameHandler Class]
	
---------------------------------------------------------------------