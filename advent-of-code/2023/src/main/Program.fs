// For more information see https://aka.ms/fsharp-console-apps
namespace aoc2003

module program =
    open System
    open System.Text.RegularExpressions
    open aoc2023.dayxxtest
    open aoc2023.Marker

    // Reference to bring in aoc2023.dll
    aoc2023.Marker.marker () |> ignore

    let aocdll = "aoc2023" // Our dll containing solutions
    let typeFilter = "aoc2023.day" // filter for type names
    let methodTypeRegex = Regex(@"^(day.*)$", RegexOptions.Compiled)
    let methodRegex = Regex(@"^(Solve)$", RegexOptions.Compiled)

    // List of valid "Solve" entrypoint methods
    let getValidMethods () =
        let assemblies = System.AppDomain.CurrentDomain.GetAssemblies()
        // printfn "assemblies = %A" assemblies
        // assemblies |> Seq.iter (fun x -> printfn "   '%s'" (x.GetName()).Name)
        let assembly =
            match assemblies
                  |> Array.tryFind (fun t -> t.GetName().Name = aocdll)
                with
            | Some x -> x
            | None -> failwith $"Assembly '{aocdll}' not found"
        // printfn "Assy for %s = %s" aocdll (assembly.ToString())

        // Seq of System.Type
        let types =
            assembly.GetTypes()
            |> Array.filter (fun x -> x.FullName.StartsWith typeFilter)
        // printfn "Types  = type=%A= %A" (types[0].GetType()) types

        // Retrieve the methods (including F# functions) on the module type
        let methods =
            types
            |> Array.map (fun x -> x.GetMethods())
            |> Array.fold Array.append [||]
        // printfn "AllMethods = %A" methods

        // methods |> Seq.iter (fun x -> printfn "  x= '%A' '%s' '%s' '%s'" (x.GetType()) x.Name x.DeclaringType.Name x.DeclaringType.FullName)
        let methods =
            methods
            |> Array.filter (fun x ->
                methodRegex.IsMatch x.Name
                && methodTypeRegex.IsMatch x.DeclaringType.Name)
        // printfn "AllMethods = %A type=%A" methods (methods[0].GetType())
        methods
        |> Array.sortBy (fun x -> x.DeclaringType.Name)

    let printUsage () =
        printfn "Usage: %s [moduleName]" System.AppDomain.CurrentDomain.FriendlyName
        printfn "  where moduleName is one of:"
        let methods = getValidMethods ()

        methods
        |> Seq.iter (fun x -> printfn "  '%s'" x.DeclaringType.Name)

        ()

    let getMethod moduleName =
        let methods = getValidMethods ()

        let method =
            match methods
                  |> Array.tryFind (fun x -> x.DeclaringType.Name = moduleName)
                with
            | Some x -> x
            | None ->
                printfn "Error::: module '%s' not found. Use one of the following:" moduleName

                methods
                |> Seq.iter (fun x -> printfn "  '%s'" x.DeclaringType.Name)

                failwith $"    '{moduleName}' not found"

        method


    [<EntryPoint>]
    let main args =
        if Array.length args = 0 then
            printUsage () |> ignore
            failwith "Usage"

        let methodName = args[0]

        let methods =
            if methodName = "all" then getValidMethods () else [| (getMethod methodName) |]
        // method.Invoke(null, [|"args"|]) |> ignore
        for method in methods do
            method.Invoke(null, [||]) |> ignore

        0 // return an integer exit code
