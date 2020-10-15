public enum FingerType
{
    None,
    Thumb,
    Index,
    Middle,
    Ring,
    Pinky
}

public class b_Finger
{
    public FingerType type = FingerType.None;
    public float current = 0.0f;
    public float target = 0.0f;

    public b_Finger(FingerType type)
    {
        this.type = type;
    }
}
