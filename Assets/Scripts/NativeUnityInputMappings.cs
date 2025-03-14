public static class NativeUnityInputMappings
{
    public enum GamepadInput
    {
        None,
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
        ThumbRY,
        DPadX,
        DPadY
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
        // 13 14 15 16 do nothing
        // 2 3 4 7 neither (some of them are right thumb or a trigger)

            new Axis(GamepadInput.ThumbLX, 0),
            new Axis(GamepadInput.ThumbLY, 1),
            new Axis(GamepadInput.LeftTrigger, 8), // => 9
            new Axis(GamepadInput.RightTrigger, 9), // => 10
            new Axis(GamepadInput.DPadX, 5), // => 6
            new Axis(GamepadInput.DPadY, 6), // => 7
    };

    public static Button[] buttons = new Button[]
    {
    };

    public static string GetVirtualInputName(GamepadInput name, int playerIndex)
    {
        return name + "_" + playerIndex;
    }
}
