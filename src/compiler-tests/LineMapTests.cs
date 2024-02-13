namespace Noa.Compiler.Tests;

public class LineMapTests
{
    private const string Text = "all\nbabel\ncs";
    
    public static IEnumerable<object[]> GetLine_ReturnsLine_InRange_Data()
    {
        yield return Data(lineNumber: 1, lineStart: 0, lineEnd: 4);
        yield return Data(lineNumber: 2, lineStart: 4, lineEnd: 10);
        yield return Data(lineNumber: 3, lineStart: 10, lineEnd: 12);

        yield break;
        
        static object[] Data(int lineNumber, int lineStart, int lineEnd) =>
            [lineNumber, new Line(lineNumber, lineStart, lineEnd)];
    }
    
    [Theory]
    [MemberData(nameof(GetLine_ReturnsLine_InRange_Data))]
    public void GetLine_ReturnsLine_InRage(int lineNumber, Line expected)
    {
        var map = LineMap.Create(Text);

        var line = map.GetLine(lineNumber);
        
        line.ShouldBe(expected);
    }

    public static IEnumerable<object[]> GetLine_ThrowsArgumentOutOfRangeException_OutOfRange_Data()
    {
        yield return Data(0);
        yield return Data(-1);
        yield return Data(4);
        yield return Data(5);
        
        yield break;
        
        static object[] Data(int lineNumber) =>
            [lineNumber];
    }

    [Theory]
    [MemberData(nameof(GetLine_ThrowsArgumentOutOfRangeException_OutOfRange_Data))]
    public void GetLine_ThrowsArgumentOutOfRangeException_OutOfRange(int lineNumber)
    {
        var map = LineMap.Create(Text);

        Should.Throw<ArgumentOutOfRangeException>(() => map.GetLine(lineNumber));
    }

    public static IEnumerable<object[]> GetCharacterPosition_ReturnsCharacterPosition_InRange_Data()
    {
        yield return Data(
            position: 0,
            lineNumber: 1,
            lineStart: 0,
            lineEnd: 4,
            characterOffset: 0);
        
        yield return Data(
            position: 3,
            lineNumber: 1,
            lineStart: 0,
            lineEnd: 4,
            characterOffset: 3);
        
        yield return Data(
            position: 4,
            lineNumber: 2,
            lineStart: 4,
            lineEnd: 10,
            characterOffset: 0);
        
        yield return Data(
            position: 6,
            lineNumber: 2,
            lineStart: 4,
            lineEnd: 10,
            characterOffset: 2);
        
        yield return Data(
            position: 11,
            lineNumber: 3,
            lineStart: 10,
            lineEnd: 12,
            characterOffset: 1);
        
        yield break;
        
        static object[] Data(int position, int lineNumber, int lineStart, int lineEnd, int characterOffset) =>
            [position, new CharacterPosition(new(lineNumber, lineStart, lineEnd), characterOffset)];
    }
    
    [Theory]
    [MemberData(nameof(GetCharacterPosition_ReturnsCharacterPosition_InRange_Data))]
    public void GetCharacterPosition_ReturnsCharacterPosition_InRange(
        int position,
        CharacterPosition expected)
    {
        var map = LineMap.Create(Text);

        var characterPosition = map.GetCharacterPosition(position);
        
        characterPosition.ShouldBe(expected);
    }

    public static IEnumerable<object[]> GetCharacterPosition_ThrowsArgumentOutOfRangeException_OutOfRange_Data()
    {
        yield return Data(-1);
        yield return Data(-2);
        yield return Data(12);
        yield return Data(13);
        
        yield break;
        
        static object[] Data(int position) =>
            [position];
    }
    
    [Theory]
    [MemberData(nameof(GetCharacterPosition_ThrowsArgumentOutOfRangeException_OutOfRange_Data))]
    public void GetCharacterPosition_ThrowsArgumentOutOfRangeException_OutOfRange(int position)
    {
        var map = LineMap.Create(Text);

        Should.Throw<ArgumentOutOfRangeException>(() => map.GetCharacterPosition(position));
    }
}
