using System.Globalization;
using ChemDoserProxy.Tcp;
using Shouldly;
using Xunit;

namespace ChemDoserProxy.Tests;

public class DataFrameTests
{
    private DataFrameParser Sut => new();

    [Theory]
    [MemberData(nameof(TestData))]
    public void Should_parse_buffer_into_data_frame(DataFrameTestData testData)
    {
        var result = Sut.TryParse(testData.Buffer, out var frame);
        result.ShouldBe(testData.ValidFrame);
    }

    public static TheoryData<DataFrameTestData> TestData =>
        [
            new DataFrameTestData("Empty array", Array.Empty<byte>(), ValidFrame: false),
            new DataFrameTestData(nameof(AsinAquaHomeFrame), HexStringToBytes(AsinAquaHomeFrame), ValidFrame: true),
            new DataFrameTestData(nameof(AsinAquaNetFrame), HexStringToBytes(AsinAquaNetFrame), ValidFrame: true),
        ];

    private const string AsinAquaHomeFrame = "ffffffff020119011212261e200002c700020002000900fe7000e5feaa08015a00000000000383caffffffff020319011212261e48060519080010001200160002c700e5000c1e0a0a2801e00708a2d0ffffffff020219011212261e0001003c003c003c000a1e3c6e9600f01e02580f0f0f1e1effbc02de";
    private const string AsinAquaNetFrame =  "ffffffff0a01ffffffffffff0004ffff0000ffff0000ff065d0098ff000006690000000000ff002ffffffff0a03ffffffffffff464608ffffffffffffffffffffff0098ffffffffffffffffffffffe60ffffffffa02ffffffffffff0032003ffff003cffff010181ff012c0502581e";

    public record DataFrameTestData(string Name, byte[] Buffer, bool ValidFrame);

    private static byte[] HexStringToBytes(string hex)
    {
        byte[] data = new byte[hex.Length / 2];

        for (int i = 0; i < data.Length; i++)
        {
            data[i] = byte.Parse(hex.AsSpan(i * 2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        }

        return data;
    }
}
