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
	public struct FSMI_State
	{
		public const int NO_PAUSE  = (0 << 0);
		public const int PAUSE     = (1 << 0);
	}

	public enum FSMI_ParkMode
	{
		ToCenter     = 0,
		Normal       = 1,
		ForTransport = 2
	}

	public enum FSMI_Defines
	{
		SFX_MaxEffectsCount = 4,
		UserAuxCount = 8
	}
}
