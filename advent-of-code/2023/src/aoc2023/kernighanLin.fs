namespace aoc2023

module kernighanLin =
    /// Calculates the total number of cut edges across the entire partitioned system.
    /// This acts as our definitive convergent metric.
    let calculateTotalCuts (partitions: Set<'a> list) (getNeighbors: 'a -> 'a list) =
        partitions

        |> List.mapi (fun i currentSet ->
            currentSet
            |> Set.fold
                (fun acc node ->
                    let externalCuts =
                        getNeighbors node

                        |> List.filter (fun neighbor ->
                            // Count neighbors that live in a partition with a higher index
                            let targetIdx = partitions |> List.findIndex (Set.contains neighbor)
                            targetIdx > i)

                        |> List.length

                    acc + externalCuts)
                0)
        |> List.sum

    /// Calculates the internal and external cost difference for a single node
    /// relative to its own subgraph and another target subgraph.
    let computeDValue node currentSubgraph targetSubgraph getNeighbors =
        let neighbors = getNeighbors node |> Set.ofList
        let internalCost = Set.intersect neighbors currentSubgraph |> Set.count
        let externalCost = Set.intersect neighbors targetSubgraph |> Set.count
        externalCost - internalCost

    /// Performs a single optimization pass between two specific subgraphs.
    let optimizePair subA subB getNeighbors =
        let initialD =
            let mapA =
                subA
                |> Set.fold (fun acc node -> Map.add node (computeDValue node subA subB getNeighbors) acc) Map.empty

            subB
            |> Set.fold (fun acc node -> Map.add node (computeDValue node subB subA getNeighbors) acc) mapA

        let initialIterationState = (subA, subB, initialD, [])
        let totalPairsToSwap = min (Set.count subA) (Set.count subB)

        if totalPairsToSwap = 0 then
            (subA, subB, false)
        else
            let _, _, _, allSwaps =
                List.init totalPairsToSwap id

                |> List.fold
                    (fun (unlockedA, unlockedB, dValues, swaps) _ ->
                        if Set.isEmpty unlockedA || Set.isEmpty unlockedB then
                            (unlockedA, unlockedB, dValues, swaps)
                        else
                            let bestPair =
                                seq {
                                    for a in unlockedA do
                                        for b in unlockedB do
                                            let da = Map.find a dValues
                                            let db = Map.find b dValues
                                            let neighborsA = getNeighbors a |> Set.ofList
                                            let edgeCost = if Set.contains b neighborsA then 1 else 0
                                            let gain = da + db - (2 * edgeCost)
                                            yield (a, b, gain)
                                }
                                |> Seq.maxBy (fun (_, _, gain) -> gain)

                            let (bestA, bestB, maxGain) = bestPair

                            let updatedDValues =
                                dValues

                                |> Map.map (fun node d ->
                                    let neighbors = getNeighbors node |> Set.ofList

                                    if Set.contains node unlockedA && node <> bestA then
                                        let c_node_a = if Set.contains bestA neighbors then 1 else 0
                                        let c_node_b = if Set.contains bestB neighbors then 1 else 0
                                        d + (2 * c_node_a) - (2 * c_node_b)
                                    elif Set.contains node unlockedB && node <> bestB then
                                        let c_node_b = if Set.contains bestB neighbors then 1 else 0
                                        let c_node_a = if Set.contains bestA neighbors then 1 else 0
                                        d + (2 * c_node_b) - (2 * c_node_a)
                                    else
                                        d)

                            (Set.remove bestA unlockedA, Set.remove bestB unlockedB, updatedDValues, (bestA, bestB, maxGain) :: swaps))
                    initialIterationState

            let reversedSwaps = List.rev allSwaps

            let _, maxPartialSumIndex, maxGainValue =
                reversedSwaps

                |> List.fold
                    (fun (currentSum, bestIdx, bestSum) (_, _, gain) ->
                        let newSum = currentSum + gain
                        let newIdx = bestIdx + 1

                        if newSum > bestSum then
                            (newSum, newIdx, newSum)
                        else
                            (newSum, bestIdx, bestSum))
                    (0, -1, 0)

            // STRICT POSITIVE GAIN GUARD: Prevents infinite 0-gain oscillations
            if maxGainValue > 0 && maxPartialSumIndex >= 0 then
                let optimalSwaps = List.take (maxPartialSumIndex + 1) reversedSwaps

                let finalA, finalB =
                    optimalSwaps
                    |> List.fold (fun (aSet, bSet) (a, b, _) -> (aSet |> Set.remove a |> Set.add b, bSet |> Set.remove b |> Set.add a)) (subA, subB)

                (finalA, finalB, true)
            else
                (subA, subB, false)

    /// Kernighan–Lin Multi-way Graph Partitioning Algorithm
    let partitionGraph (nodes: 'a list) (getNeighbors: 'a -> 'a list) (n: int) : ('a list list * ('a * 'a) list) when 'a: comparison =
        if n <= 1 then
            ([ nodes ], [])
        else
            // 1. Initial Balanced Chunking Partitioning
            let initialPartitions =
                nodes

                |> List.mapi (fun i node -> (i % n, node))
                |> List.groupBy fst
                |> List.map (fun (_, group) -> group |> List.map snd |> Set.ofList)

            let totalSubgraphs = List.length initialPartitions

            let pairIndices =
                [ for i in 0 .. totalSubgraphs - 1 do
                      for j in i + 1 .. totalSubgraphs - 1 do
                          yield (i, j) ]

            // 2. Iterative Optimization Loop with strict termination criteria
            let mutable currentPartitions = initialPartitions
            let mutable currentCutCount = calculateTotalCuts currentPartitions getNeighbors
            let mutable globalImprovement = true
            let mutable safetyIterationCounter = 0

            // HARD BOUND GUARD: Standard graph theory caps structural passes to prevent
            // adversarial multi-way partitioning oscillation cycles.
            let maxGlobalPasses = nodes.Length * n

            while globalImprovement && safetyIterationCounter < maxGlobalPasses do
                globalImprovement <- false
                safetyIterationCounter <- safetyIterationCounter + 1

                let mutable temporaryPartitions = currentPartitions
                let mutable localPairChanged = false

                for (i, j) in pairIndices do
                    let subA = List.item i temporaryPartitions
                    let subB = List.item j temporaryPartitions

                    let updatedA, updatedB, wasImproved = optimizePair subA subB getNeighbors

                    if wasImproved then
                        localPairChanged <- true

                        temporaryPartitions <-
                            temporaryPartitions

                            |> List.mapi (fun idx set ->
                                if idx = i then updatedA
                                elif idx = j then updatedB
                                else set)

                if localPairChanged then
                    let newCutCount = calculateTotalCuts temporaryPartitions getNeighbors
                    // CONVERGENCE GUARD: Only accept the global pass if it strictly lowers
                    // or improves the overall network cuts.
                    if newCutCount < currentCutCount then
                        currentPartitions <- temporaryPartitions
                        currentCutCount <- newCutCount
                        globalImprovement <- true

            // 3. Finalize output and compute edge cuts
            let cutEdges =
                seq {
                    for i in 0 .. totalSubgraphs - 1 do
                        let currentSet = List.item i currentPartitions

                        for node in currentSet do
                            let neighbors = getNeighbors node

                            for neighbor in neighbors do
                                let neighborPartitionIdx =
                                    currentPartitions |> List.findIndex (Set.contains neighbor)

                                if neighborPartitionIdx > i then yield (node, neighbor)
                }

                |> Seq.toList

            let finalPartitions = currentPartitions |> List.map Set.toList
            (finalPartitions, cutEdges)

module kernighanLin_test =

    open Xunit
    open kernighanLin

    [<Fact>]
    let ``simple test`` () =
        let sampleNodes = [ 1; 2; 3; 4; 5; 6 ]

        let getNeighbors node =
            match node with

            | 1 -> [ 2 ]
            | 2 -> [ 1; 3 ]
            | 3 -> [ 2; 4 ]

            | 4 -> [ 3; 5 ]
            | 5 -> [ 4; 6 ]
            | 6 -> [ 5 ]
            | _ -> []

        let partitions, cuts = partitionGraph sampleNodes getNeighbors 2
        // Output:
        // Partitions: [[1; 2; 3]; [4; 5; 6]]
        // Cut Edges: [(3, 4)]
        Assert.Equivalent([ [ 1; 2; 3 ]; [ 4; 5; 6 ] ], partitions)
        Assert.Equivalent([ (3, 4) ], cuts)
