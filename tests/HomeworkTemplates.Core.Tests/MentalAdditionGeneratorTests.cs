using HomeworkTemplates.Core;

namespace HomeworkTemplates.Core.Tests;

public class MentalAdditionGeneratorTests
{
    [Fact]
    public void Generates_fourteen_distinct_practice_problems_without_the_example()
    {
        for (var seed = 0; seed < 100; seed++)
        {
            var problems = MentalAdditionGenerator.Generate(seed);
            Assert.Equal(14, problems.Count);
            Assert.DoesNotContain(MentalAdditionGenerator.Example, problems);
            Assert.DoesNotContain(new AdditionProblem(24, 43), problems);
            Assert.Equal(14, problems.Select(p => (Math.Min(p.Left, p.Right), Math.Max(p.Left, p.Right))).Distinct().Count());
            Assert.All(problems, p =>
            {
                Assert.InRange(p.Left, 10, 100);
                Assert.InRange(p.Right, 1, 100);
                Assert.Equal(p.Left + p.Right, p.Answer);
            });
        }
    }

    [Fact]
    public void Seed_reproduces_practice_and_different_seeds_change_it()
    {
        Assert.Equal(MentalAdditionGenerator.Generate(1), MentalAdditionGenerator.Generate(1));
        Assert.NotEqual(MentalAdditionGenerator.Generate(1), MentalAdditionGenerator.Generate(2));
    }

    [Fact]
    public void Generating_new_practice_preserves_the_reference_example()
    {
        MentalAdditionGenerator.Generate(42);
        Assert.Equal(new AdditionProblem(43, 24), MentalAdditionGenerator.Example);
        Assert.Equal(67, MentalAdditionGenerator.Example.Answer);
    }
}
