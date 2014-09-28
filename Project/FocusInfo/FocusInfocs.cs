
namespace Kazyx.ImageStream.FocusInfo
{
    public enum Category
    {
        Invalid = 0x00,
        ContrastAF = 0x01,
        PhaseDetectionAF = 0x02,
        _Reserved = 0x03,
        Face = 0x04,
        Tracking = 0x05,
    }

    public enum Status
    {
        Invalid = 0x00,
        Normal = 0x01,
        Main = 0x02,
        Sub = 0x03,
        Focused = 0x04,
        _Reserved1 = 0x05,
        _Reserved2 = 0x06,
        _Reserved3 = 0x07,
    }

    public enum AdditionalStatus
    {
        Invalid = 0x00,
        Selected = 0x01,
        LargeFrame = 0x02,
    }
}
