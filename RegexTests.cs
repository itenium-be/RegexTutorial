using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Xunit;

namespace DotNetRegexTutorial
{
    public class RegexTests
    {
        // Not Covered:
        // Subexpression captures:
        // https://docs.microsoft.com/en-us/dotnet/api/system.text.regularexpressions.capture?view=netframework-4.8
        // Regex.Split():
        // https://docs.microsoft.com/en-us/dotnet/api/system.text.regularexpressions.regex.split?view=netframework-4.8

        [Fact]
        public void Regex_Creation_And_IsMatch()
        {
            var regex = new Regex(@"\d+", RegexOptions.None);
            Assert.Equal(RegexOptions.None, regex.Options);

            bool isMatch = regex.IsMatch("9");
            Assert.True(isMatch);

            // RegexOptions.Compiled: Increased startup time for decreased execution time
            // RegexOptions.IgnoreCase
            // RegexOptions.ExplicitCapture: Only explicitly named/numbered groups are captured. (Skip the `(?:…)`)
            // RegexOptions.Multiline: `^` and `$` match beginning of line
            // RegexOptions.Singleline: `.` matches `\n`

            var commentedRegex = new Regex(@"
                \d    # Any digit
                [0-9] # With character class
                ", RegexOptions.IgnorePatternWhitespace);
        }

        [Fact]
        public void Regex_Match()
        {
            // bool mach = Regex.IsMatch("input", @"\d+", RegexOptions.None);

            var regex = new Regex(@"(\d+)");
            Match match = regex.Match("1 22 333");
            if (match.Success)
            {
                Assert.Equal("1", match.Value);
                Assert.Equal(match.Value.Length, match.Length);
                Assert.Equal(0, match.Index);
                Assert.Equal("1", match.Result("$1"));


                var nextMatch = match.NextMatch();
                Assert.Equal("22", nextMatch.Value);
            }


            // Static methods cache the regexes
            var staticMatch = Regex.Match(@"\d+", "input");
            Assert.False(staticMatch.Success);
        }

        [Fact]
        public void Regex_Match_Groups()
        {
            Match match = Regex.Match("1-22-333", @"(\d+)-(\d+)-(\d+)(-\d+)?");

            IReadOnlyList<Group> groups = match.Groups;
            Assert.Equal(5, groups.Count);
            
            Assert.Equal("1-22-333", groups.First().Value);

            Group one = match.Groups.Skip(1).First();
            Assert.Equal("1", one.Value);

            Group fours = match.Groups.Last();
            Assert.False(fours.Success);
        }

        [Fact]
        public void Regex_Matches_NamedGroups()
        {
            IReadOnlyList<Match> matches = Regex.Matches("1-22, 1-2", @"(?<one>\d+)-(?<two>\d+)");

            Assert.Equal(2, matches.Count);

            Match first = matches.First();
            Assert.Equal("1-22", first.Value);

            Match second = matches.Last();
            Assert.Equal("1-2", second.Value);


            IReadOnlyList<Group> groups = first.Groups;
            Group one = groups.Skip(1).First();
            Assert.Equal("one", one.Name);
            Assert.Equal("1", one.Value);
        }

        [Fact]
        public void Regex_Replace()
        {
            var regex = new Regex(@"(\d)-(\d+)");
            var result = regex.Replace("1-22", "$1+$2");
            Assert.Equal("1+22", result);

            result = Regex.Replace("1", @"(?<amount>\d+)", "$$ ${amount}");
            Assert.Equal("$ 1", result);

            // $& : entire match
            // $` : before
            // $' : after
            // $+ : last captured group
            // $_ : entire input string

            //result = Regex.Replace("1-22", @"(\d)", "$1+$2");
            //Assert.Equal("(1)-(22)", result);
        }

        [Fact]
        public void Regex_Replace_With_Function()
        {
            string input = "Hello World!";
            string pattern = @"Hello";

            var result = Regex.Replace(input, pattern, (Match m) => $"<b>{m.Value}</b>");
            Assert.Equal("<b>Hello</b> World!", result);
        }

        [Fact]
        public void Regex_Timeout()
        {
            try
            {
                var rgx = new Regex(@"\b\w+\b", RegexOptions.None, TimeSpan.FromMilliseconds(1000));
                rgx.Match("input");
            }
            catch (RegexMatchTimeoutException ex)
            {
                TimeSpan timeout = ex.MatchTimeout;
                string input = ex.Input;
                string pattern = ex.Pattern;
            }
        }
    }
}
