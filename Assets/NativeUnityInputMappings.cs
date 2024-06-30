public static class NativeUnityInputMappings
{
    public enum GamepadInput
    {
        None,
        DPadUp,
        DPadDown,
        DPadLeft,
        DPadRight,
        Start,
        Back,
        LeftThumb,
        RightThumb,
        LeftShoulder,
        RightShoulder,
        A,
        B,
        X,
        Y,
        LeftTrigger,
        RightTrigger,
        ThumbLX,
        ThumbLY,
        ThumbRX,
        ThumbRY
    }

    public struct Axis
    {
        public readonly GamepadInput name;
        public readonly int index;

        public Axis(GamepadInput name, int index)
        {
            this.name = name;
            this.index = index;
        }
    }

    public struct Button
    {
        public readonly GamepadInput name;
        public readonly string path;

        public Button(GamepadInput name, string path)
        {
            this.name = name;
            this.path = path;
        }
    }

    public static Axis[] axes = new Axis[]
    {
            new Axis(GamepadInput.ThumbLX, 0),
            new Axis(GamepadInput.ThumbLY, 1),
            new Axis(GamepadInput.LeftTrigger, 8),
            new Axis(GamepadInput.RightTrigger, 9)
    };

    public static Button[] buttons = new Button[]
    {
            new Button(GamepadInput.A, "button 14"),
            new Button(GamepadInput.Start, "button 0"),

            new Button(GamepadInput.DPadUp, "button 4"),
            new Button(GamepadInput.DPadRight, "button 5"),
            new Button(GamepadInput.DPadDown, "button 6"),
            new Button(GamepadInput.DPadLeft, "button 7"),
    };

    public static string GetVirtualInputName(GamepadInput name, int playerIndex)
    {
        return name + "_" + playerIndex;
    }
}
