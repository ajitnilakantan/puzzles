export type CombinationDecision = "continue" | "skip" | "success";

export interface CombinationsConfig<T, C> {
  /**
   * A dynamic function to fetch the choices for the next level.
   * Return an empty array `[]` or `null` to signal there are no more levels.
   */
  getChoicesForLevel: (currentPath: T[], levelIndex: number, context: C) => T[] | null;
  /** Evaluates the current state path at each step. */
  onStep?: (currentPath: T[], levelIndex: number, context: C) => CombinationDecision;
}

export interface CombinationResult<T> {
  results: T[][];
  done: boolean;
}

/**
 * Recursively generates combinations using a dynamic provider function for level choices.
 */
export function getRecursiveCombinations<T, C>(
  config: CombinationsConfig<T, C>,
  initialContext: C,
  levelIndex = 0,
  currentPath: T[] = []
): CombinationResult<T> {
  // Dynamically resolve the choices for this specific level based on the path so far
  const currentChoices = config.getChoicesForLevel(currentPath, levelIndex, initialContext);

  // Base case: No choices returned means we have successfully completed this branch's depth
  if (!currentChoices || currentChoices.length === 0) {
    return { results: [currentPath], done: false };
  }

  const results: T[][] = [];

  for (const choice of currentChoices) {
    // Deep clone the context to keep this exploration path fully isolated
    const branchContext = structuredClone(initialContext);
    const nextPath = [...currentPath, choice];

    // Evaluate structural filters or exit conditions
    if (config.onStep) {
      const decision = config.onStep(nextPath, levelIndex, branchContext);

      if (decision === "skip") {
        continue; // Prune this choice and skip evaluating deeper levels
      }
      if (decision === "success") {
        results.push(nextPath);
        return { results, done: true }; // Halt the entire generation process
      }
    }

    // Recurse down to the dynamically calculated next level index
    const next = getRecursiveCombinations(config, branchContext, levelIndex + 1, nextPath);
    results.push(...next.results);

    // Bubble up early shutdown signal if inner loop reported a absolute 'success' state
    if (next.done) {
      return { results, done: true };
    }
  }

  return { results, done: false };
}

// All choices of k items from list.
export function getCombinations<T>(set: T[], k: number): T[][] {
  if (k === 0) return [[]];
  if (k > set.length || k < 0) return [];

  return set.reduce((acc: T[][], current: T, index: number) => {
    // Get combinations from the remaining elements
    const tailCombinations = getCombinations(set.slice(index + 1), k - 1);
    tailCombinations.forEach((tailArray) => acc.push([current, ...tailArray]));
    return acc;
  }, []);
}
