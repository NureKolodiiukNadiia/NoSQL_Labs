using System.Runtime.Serialization;

namespace SpotRent.Enums;

public enum SpaceType
{
    [EnumMember(Value = "Meeting Room")]
    MeetingRoom = 0,
    [EnumMember(Value = "Hot Desk")]
    HotDesk = 1,
    [EnumMember(Value = "Private Office")]
    PrivateOffice = 2
}
