using System.Runtime.Serialization;

namespace SpotRent.Enums;

public enum BookingStatus
{
    [EnumMember(Value = "New")]
    New = 0,
    [EnumMember(Value = "Confirmed")]
    Confirmed = 1,
    [EnumMember(Value = "Cancelled")]
    Cancelled = 2,
    [EnumMember(Value = "Completed")]
    Completed = 3
}
