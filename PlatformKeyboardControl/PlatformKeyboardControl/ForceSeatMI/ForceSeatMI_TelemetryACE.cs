/*
 * Copyright (C) 2012-2022 MotionSystems
 * 
 * This file is part of ForceSeatMI SDK.
 *
 * www.motionsystems.eu
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */

using System;
using System.Runtime.InteropServices;

namespace MotionSystems
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct FSMI_TelemetryRUF
	{
		public float right;   // + is right,   - is left
		public float upward;  // + is upward,  - is downward
		public float forward; // + is forward, - is backward
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct FSMI_TelemetryPRY
	{
		public float pitch; // + front goes up,        - front goes down
		public float roll;  // + right side goes down, - right sidegoes up
		public float yaw;   // + front goes left,      - front goes right
	}

	/*
	 * This structure is used to forward vehicle physics data from application/game to the ForceSeatPM
	 * where the data is next processed and transformed into motion platform top table's movements.
	 */
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct FSMI_TelemetryACE
	{
		// Put here sizeof(FSMI_TelemetryACE).
		// MANDATORY: Yes
		public byte structSize;

		// Only single bit is used in current version.
		//  (state & 0x01) == 0 -> no pause
		//  (state & 0x01) == 1 -> paused, FSMI_STATE_PAUSE
		public byte state;

		public sbyte gearNumber;                // Current gear number, -1 for reverse, 0 for neutral, 1, 2, 3, ...
		public byte  accelerationPedalPosition; // 0 to 100 (in percent)
		public byte  brakePedalPosition;        // 0 to 100 (in percent)
		public byte  clutchPedalPosition;       // 0 to 100 (in percent)

		public byte  dummy1;
		public byte  dummy2;

		public uint  rpm;                 // Engine RPM
		public uint  maxRpm;              // Engine max RPM
		public float vehicleForwardSpeed; // Forward speed, in [m/s]. For dashboard applications.

		// Lateral, vertical and longitudinal acceleration from the simulation.
		// UNITS: [m/s^2]
		// MANDATORY: Yes
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
		public FSMI_TelemetryRUF[] bodyLinearAcceleration;

		// Angular rotation velocity about vertical, lateral and longitudinal axes.
		// UNITS: [radians/s]
		// MANDATORY: Yes
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
		public FSMI_TelemetryPRY[] bodyAngularVelocity;

		// Position of the user head for accelerations and angular velocity effects generation
		// in relation to the top frame center.
		// UNITS: [meters]
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
		public FSMI_TelemetryRUF[] headPosition;

		// Additional translation for the top frame.
		// UNITS: [meters]
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
		public FSMI_TelemetryRUF[] extraTranslation;

		// Additional rotation for the top frame.
		// UNITS: [rad]
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
		public FSMI_TelemetryPRY[] extraRotation;

		// Rotation center for the 'extra' transformation in relation to the top frame center.
		// UNITS: [meters]
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
		public FSMI_TelemetryRUF[] extraRotationCenter;

		// Custom field that can be used in script in ForceSeatPM to trigger user defined actions.
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = (int)FSMI_Defines.UserAuxCount)]
		public float[]  userAux;

		// Custom field that can be used in script in ForceSeatPM to trigger user defined actions.
		public uint  userFlags;

		// Helper method to create a structure with preallocated space for nested structures
		public static FSMI_TelemetryACE Prepare()
		{
			var telemetry                    = new FSMI_TelemetryACE();
			telemetry.structSize             = (byte)Marshal.SizeOf(telemetry);
			telemetry.bodyLinearAcceleration = new FSMI_TelemetryRUF[1];
			telemetry.bodyAngularVelocity    = new FSMI_TelemetryPRY[1];
			telemetry.headPosition           = new FSMI_TelemetryRUF[1];
			telemetry.extraTranslation       = new FSMI_TelemetryRUF[1];
			telemetry.extraRotation          = new FSMI_TelemetryPRY[1];
			telemetry.extraRotationCenter    = new FSMI_TelemetryRUF[1];
			telemetry.userAux                = new float[(int)FSMI_Defines.UserAuxCount];

			return telemetry;
		}
	}

}
