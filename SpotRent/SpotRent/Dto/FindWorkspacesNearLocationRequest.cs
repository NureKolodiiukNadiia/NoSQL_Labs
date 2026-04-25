namespace SpotRent.Dto;

public record FindWorkspacesNearLocationRequest
{
    public double X { get; init; }
    public double Y { get; init; }
    public double RadiusKm { get; init; }

    public bool IsValid => X > -180 && X < 180
                                    && Y > -90 && Y < 90
                                    && RadiusKm < 20000;
    // close enough
}
