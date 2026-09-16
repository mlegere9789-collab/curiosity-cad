using Curiosity.Plugin.NlParser;
using Xunit;

namespace Curiosity.Tests
{
    public class LocalPatternMatcherTests
    {
        [Fact]
        public void MediumLineWeight_ResolvesToSetProperty()
        {
            var intent = LocalPatternMatcher.TryResolve("change to medium line weight");

            Assert.Equal(IntentAction.SetProperty, intent.Action);
            Assert.Equal("Lineweight", intent.Parameters["property"]);
            Assert.Equal("LineWeight035", intent.Parameters["value"]);
            Assert.Equal("local", intent.ResolvedBy);
            Assert.True(intent.Confidence > 0.9);
        }

        [Theory]
        [InlineData("thin", "LineWeight025")]
        [InlineData("light", "LineWeight025")]
        [InlineData("thick", "LineWeight060")]
        [InlineData("heavy", "LineWeight060")]
        public void OtherLineweightWords_MapCorrectly(string word, string expectedValue)
        {
            var intent = LocalPatternMatcher.TryResolve($"set line weight to {word}");

            Assert.Equal(IntentAction.SetProperty, intent.Action);
            Assert.Equal(expectedValue, intent.Parameters["value"]);
        }

        [Fact]
        public void GroundLineIntersectionAt45Degrees_ResolvesToConstrainAngle()
        {
            var intent = LocalPatternMatcher.TryResolve(
                "make sure this line intersects with the ground line at a 45 degree angle");

            Assert.Equal(IntentAction.ConstrainAngle, intent.Action);
            Assert.Equal(45.0, intent.Parameters["degrees"]);

            var reference = Assert.IsType<EntityReference>(intent.Parameters["reference"]);
            Assert.Equal("named", reference.Kind);
            Assert.Equal("ground line", reference.Name);
        }

        [Fact]
        public void UnrelatedInstruction_IsNotRecognizedLocally()
        {
            var intent = LocalPatternMatcher.TryResolve("please redesign the entire floor plan");

            Assert.Equal(IntentAction.Unrecognized, intent.Action);
            Assert.Equal(0.0, intent.Confidence);
        }
    }
}
