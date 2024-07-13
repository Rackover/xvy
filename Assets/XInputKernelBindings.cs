// 
// XInput.cs
// 
// The MIT License (MIT)
// 
// Copyright (c) 2014-2015 Luminawesome Games Ltd.
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

namespace XInputBindings
{
    using System;
    using System.Collections;
    using System.Runtime.InteropServices;

    public struct Vector2
    {
        public float x;
        public float y;
    }

    public enum XInputButton : ushort
    {
        None = 0x0000,
        DPadUp = 0x0001,
        DPadDown = 0x0002,
        DPadLeft = 0x0004,
        DPadRight = 0x0008,
        Start = 0x0010,
        Back = 0x0020,
        LeftThumb = 0x0040,
        RightThumb = 0x0080,
        LeftShoulder = 0x0100,
        RightShoulder = 0x0200,
        A = 0x1000,
        B = 0x2000,
        X = 0x4000,
        Y = 0x8000
    }

    public static class XInputKernelBindings
    {
        private const uint ERROR_SUCCESS = 0;
        private const uint ERROR_DEVICE_NOT_CONNECTED = 0x48F;
        private const int XUSER_MAX_COUNT = 4;
        private const int XINPUT_GAMEPAD_LEFT_THUMB_DEADZONE = 7849;
        private const int XINPUT_GAMEPAD_RIGHT_THUMB_DEADZONE = 8689;
        private const int XINPUT_GAMEPAD_TRIGGER_THRESHOLD = 30;
        private const int STICK_MAXIMUM = 32767;

        private class XInputState
        {
            public void CopyTo(XInputState target)
            {
                target.IsConnected = IsConnected;
                target.PacketNumber = PacketNumber;
                target.Buttons = Buttons;
                target.TriggerLeft = TriggerLeft;
                target.TriggerRight = TriggerRight;
                target.ThumbLeft = ThumbLeft;
                target.ThumbRight = ThumbRight;
                target.ThumbLeftRaw = ThumbLeftRaw;
                target.ThumbRightRaw = ThumbRightRaw;
            }

            public void Reset()
            {
                IsConnected = false;
                PacketNumber = 0;
                Buttons = 0;
                TriggerLeft = 0.0f;
                TriggerRight = 0.0f;
                ThumbLeft = new Vector2();
                ThumbRight = new Vector2();
                ThumbLeftRaw = new Vector2();
                ThumbRightRaw = new Vector2();
            }


            public bool IsConnected;
            public uint PacketNumber;
            public ushort Buttons;
            public float TriggerLeft;
            public float TriggerRight;
            public Vector2 ThumbLeft;
            public Vector2 ThumbRight;
            public Vector2 ThumbLeftRaw;
            public Vector2 ThumbRightRaw;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct XINPUT_STATE_GAMEPAD
        {
            public uint PacketNumber;
            public ushort Buttons;
            public byte LeftTrigger;
            public byte RightTrigger;
            public short ThumbLX;
            public short ThumbLY;
            public short ThumbRX;
            public short ThumbRY;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct XINPUT_VIBRATION
        {
            public ushort LeftMotorSpeed;
            public ushort RightMotorSpeed;
        }

        private static XInputState[] mLastFrame;
        private static XInputState[] mThisFrame;

        static XInputKernelBindings()
        {
            mLastFrame = new XInputState[XUSER_MAX_COUNT];
            mThisFrame = new XInputState[XUSER_MAX_COUNT];
            for (int i = 0; i < XUSER_MAX_COUNT; i++)
            {
                mLastFrame[i] = new XInputState();
                mLastFrame[i].Reset();
                mThisFrame[i] = new XInputState();
                mThisFrame[i].Reset();
            }
        }

        public static void UpdateState(uint userIndex)
        {
            UpdateStateInternal(userIndex);
        }

        /// <summary>
        /// Returns true while the button on the identified controller is held down. 
        /// </summary>
        public static bool GetButton(uint userIndex, XInputButton button)
        {
            XInputState thisState = mThisFrame[userIndex];

            if (!thisState.IsConnected) return false;

            return (thisState.Buttons & (ushort)button) != 0;
        }

        /// <summary>
        /// Returns true during the frame the user pressed down the button on the identified controller.
        /// </summary>
        public static bool GetButtonDown(uint userIndex, XInputButton button)
        {
            XInputState thisState = mThisFrame[userIndex];
            XInputState lastState = mLastFrame[userIndex];

            if (!thisState.IsConnected) return false;

            return (lastState.Buttons & (ushort)button) == 0 && (thisState.Buttons & (ushort)button) != 0;
        }

        /// <summary>
        /// Returns true the first frame the user releases the button on the identified controller.
        /// </summary>
        public static bool GetButtonUp(uint userIndex, XInputButton button)
        {
            XInputState thisState = mThisFrame[userIndex];
            XInputState lastState = mLastFrame[userIndex];

            if (!thisState.IsConnected) return false;

            return (lastState.Buttons & (ushort)button) != 0 && (thisState.Buttons & (ushort)button) == 0;
        }

        /// <summary>
        /// Returns the value of the left thumbstick on the identified controller.
        /// </summary>
        public static Vector2 GetThumbStickLeft(uint userIndex) { return mThisFrame[userIndex].ThumbLeft; }

        /// <summary>
        /// Returns the value of the right thumbstick on the identified controller.
        /// </summary>
        public static Vector2 GetThumbStickRight(uint userIndex) { return mThisFrame[userIndex].ThumbRight; }

        /// <summary>
        /// Returns the raw value of the left thumbstick on the identified controller.
        /// </summary>
        public static Vector2 GetThumbStickLeftRaw(uint userIndex) { return mThisFrame[userIndex].ThumbLeftRaw; }

        /// <summary>
        /// Returns the raw value of the right thumbstick on the identified controller.
        /// </summary>
        public static Vector2 GetThumbStickRightRaw(uint userIndex) { return mThisFrame[userIndex].ThumbRightRaw; }

        /// <summary>
        /// Returns the value of the left trigger on the identified controller.
        /// </summary>
        public static float GetTriggerLeft(uint userIndex) { return mThisFrame[userIndex].TriggerLeft; }

        /// <summary>
        /// Returns the value of the right trigger on the identified controller.
        /// </summary>
        public static float GetTriggerRight(uint userIndex) { return mThisFrame[userIndex].TriggerRight; }

        /// <summary>
        /// Sets the motor speed of the left and right motors in the identified controller.
        /// </summary>
        public static void SetVibration(uint userIndex, float leftMotorSpeed, float rightMotorSpeed)
        {
            XINPUT_VIBRATION vibration = new XINPUT_VIBRATION();
            vibration.LeftMotorSpeed = (ushort)(Math.Min(1.0f, leftMotorSpeed) * ushort.MaxValue);
            vibration.RightMotorSpeed = (ushort)(Math.Min(1.0f, rightMotorSpeed) * ushort.MaxValue);
            XInputSetState(userIndex, ref vibration);
        }

        public static bool IsControllerConnected(uint userIndex) { return mThisFrame[userIndex].IsConnected; }

        private static void UpdateStateInternal(uint userIndex)
        {
            XINPUT_STATE_GAMEPAD rawState = new XINPUT_STATE_GAMEPAD();
            bool isConnected = XInputGetState(userIndex, out rawState) == ERROR_SUCCESS;

            XInputState thisState = mThisFrame[userIndex];
            XInputState lastState = mLastFrame[userIndex];

            if (thisState.PacketNumber != lastState.PacketNumber)
            {
                thisState.CopyTo(lastState);
            }

            if (!isConnected)
            {
                thisState.Reset();
                lastState.Reset();
            }
            else if (rawState.PacketNumber != thisState.PacketNumber)
            {
                thisState.PacketNumber = rawState.PacketNumber;
                thisState.IsConnected = true;

                //
                // Buttons
                //

                thisState.Buttons = rawState.Buttons;

                //
                // Triggers
                //

                thisState.TriggerLeft = (Math.Max(0, (float)rawState.LeftTrigger - XINPUT_GAMEPAD_TRIGGER_THRESHOLD)) / (255 - XINPUT_GAMEPAD_TRIGGER_THRESHOLD);
                thisState.TriggerRight = (Math.Max(0, (float)rawState.RightTrigger - XINPUT_GAMEPAD_TRIGGER_THRESHOLD)) / (255 - XINPUT_GAMEPAD_TRIGGER_THRESHOLD);

                //
                // Thumbstick
                //

                // data from the driver
                float thumbLXRaw = rawState.ThumbLX;
                float thumbLYRaw = rawState.ThumbLY;
                float thumbRXRaw = rawState.ThumbRX;
                float thumbRYRaw = rawState.ThumbRY;

                // Raw undeadzoned -1 : +1		
                float thumbLXUnitRaw = Math.Max(-1, thumbLXRaw / STICK_MAXIMUM);
                float thumbLYUnitRaw = Math.Max(-1, thumbLYRaw / STICK_MAXIMUM);
                float thumbRXUnitRaw = Math.Max(-1, thumbRXRaw / STICK_MAXIMUM);
                float thumbRYUnitRaw = Math.Max(-1, thumbRYRaw / STICK_MAXIMUM);

                // Circular normalized deadzones
                float thumbLMag = thumbLXRaw * thumbLXRaw + thumbLYRaw * thumbLYRaw;
                float thumbRMag = thumbRXRaw * thumbRXRaw + thumbRYRaw * thumbRYRaw;
                thumbLMag = GetCircularDeadzone(thumbLMag, XINPUT_GAMEPAD_LEFT_THUMB_DEADZONE, STICK_MAXIMUM);
                thumbRMag = GetCircularDeadzone(thumbRMag, XINPUT_GAMEPAD_RIGHT_THUMB_DEADZONE, STICK_MAXIMUM);

                thisState.ThumbLeft.x = thumbLXUnitRaw / thumbLMag;
                thisState.ThumbLeft.y = thumbLYUnitRaw / thumbLMag;
                thisState.ThumbRight.x = thumbRXUnitRaw / thumbRMag;
                thisState.ThumbRight.y = thumbRYUnitRaw / thumbRMag;

                thisState.ThumbLeftRaw.x = thumbLXUnitRaw;
                thisState.ThumbLeftRaw.y = thumbLYUnitRaw;
                thisState.ThumbRightRaw.x = thumbRXUnitRaw;
                thisState.ThumbRightRaw.y = thumbRYUnitRaw;
            }
        }

        private static float GetCircularDeadzone(float magnitude, float deadzone, float maximum)
        {
            // check if the controller is outside a circular dead zone
            if (magnitude > deadzone)
            {
                // clip the magnitude at its expected maximum value
                if (magnitude > maximum) magnitude = maximum;

                // adjust magnitude relative to the end of the dead zone
                magnitude -= deadzone;

                // normalize the magnitude with respect to its expected range
                // giving a magnitude value of 0.0 to 1.0			
                return magnitude / (maximum - deadzone);
            }

            return 0.0f;
        }


#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
        [DllImport("XINPUT9_1_0.DLL")]
        private static extern uint XInputGetState(uint userIndex, out XINPUT_STATE_GAMEPAD state);

        [DllImport("XINPUT9_1_0.DLL")]
        private static extern uint XInputSetState(uint userIndex, ref XINPUT_VIBRATION vibration);
#else
        private static uint XInputGetState(uint userIndex, out XINPUT_STATE_GAMEPAD vibration)
        {
            return XamInputGetState(userIndex, 1, out vibration);
        }
    
        private static uint XInputSetState(uint userIndex, ref XINPUT_VIBRATION vibration)
        {
            return XamInputSetState(userIndex, 0, ref vibration, 0, 0, 0);
        }

        /*
        
        private static uint XamInputGetState(uint userIndex, uint unknown, out XINPUT_STATE_GAMEPAD state)
        {
            state = new XINPUT_STATE_GAMEPAD();
            return 0;
        }

        private static uint XamInputSetState(uint userIndex, uint unknown, ref XINPUT_VIBRATION vibration, uint unknownA, uint unknownB, uint unknownC)
        {
            return 0;
        }
        */

        [DllImport("__Internal")]
        private static extern uint XamInputGetState(uint userIndex, uint unknown, out XINPUT_STATE_GAMEPAD state);
    
        [DllImport("__Internal")]
        private static extern uint XamInputSetState(uint userIndex, uint unknown, ref XINPUT_VIBRATION vibration, uint unknownA, uint unknownB, uint unknownC);
#endif

    }
}