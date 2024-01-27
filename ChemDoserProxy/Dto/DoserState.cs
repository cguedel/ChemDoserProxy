namespace ChemDoserProxy.Dto;

public record DoserState(DeviceState State, decimal clFree, decimal clFreeMv, decimal pH, decimal WaterTemp);
