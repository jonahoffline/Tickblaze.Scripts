using Tickblaze.Core.Extensions;

namespace Tickblaze.Scripts.OptimizationAlgorithms;

public class Genetic : OptimizationAlgorithm
{
	[Parameter("Generations", Description = "The number of generations."), NumericRange(1)]
	public int Generations { get; set; } = 10;

	[Parameter("Chromosomes", Description = "The number of chromosomes selected in each generation."), NumericRange(1)]
	public int Chromosomes { get; set; } = 20;
	
	[Parameter("Parents", Description = "The number of parents used to create each generation other than the first."), NumericRange(1)]
	public int Parents { get; set; } = 3;
	
	[Parameter("Mutations", Description = "The number of mutations allowed from the parent's child chromosomes."), NumericRange(1)]
	public int Mutations { get; set; } = 1;

	public override int MaxOptimizations => Math.Min(Generations * Chromosomes, Vectors.Count);

	private int _generations;

	public override IEnumerable<OptimizationVector> GetNextVectorsToRun(int maxCount)
	{
		// If we've run everything already, or exceeded generations, there's nothing else to run
		if (_generations * Chromosomes >= Vectors.Count || _generations >= Generations)
			return [];

		_generations++;
		if (_generations == 1)
			return [.. Vectors.RandomSubset(Chromosomes)];

		// Select a random set of parents from the fittest vectors
		var candidateParentChromosomes = Vectors
			.OrderByDescending(v => v.Score)
			.RandomSubset(Math.Max(Parents, Vectors.Count / 2))
			.ToList();

		// Create a child vector from parents randomly selected from the fittest
		var child = Enumerable.Range(0, Vectors[0].Values.Count)
			.Select(i => candidateParentChromosomes[Random.Shared.Next(0, candidateParentChromosomes.Count)].Values[i])
			.ToArray();

		var allowedMutations = Mutations;
		var newChromosomes = new List<OptimizationVector>();
		while (newChromosomes.Count < Chromosomes)
		{
			// Find optimization vectors which match the child given the number of allowed mutations.
			var childMatches = Vectors
				.Except(newChromosomes)
				.Where(vector => !vector.IsProcessed && vector.Values.Where((value, j) => !(value?.Equals(child[j]) ?? value == child[j])).Count() <= allowedMutations)
				.ToList();

			if (childMatches.Count != 0)
				newChromosomes.AddRange(childMatches.RandomSubset(Math.Min(Chromosomes - newChromosomes.Count + 1, childMatches.Count)));
			else if (allowedMutations < Vectors[0].Values.Count)
				allowedMutations++;
			else
				break;
		}

		return newChromosomes;
	}
}