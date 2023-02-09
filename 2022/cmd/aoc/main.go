package main

import (
	"aoc/ajitn/2022/internal"
	"flag"
	"fmt"
	"os"
	"reflect"
	"sort"

	"golang.org/x/text/cases"
	"golang.org/x/text/language"
)

func printUsage() {
	fmt.Println("Usage!")
}

func main() {

	// Get list of exported functions for error messages
	aocType := reflect.TypeOf(&internal.AOC{})
	exportedFunctions := []string{}
	for i := 0; i < aocType.NumMethod(); i++ {
		method := aocType.Method(i)
		exportedFunctions = append(exportedFunctions, method.Name)
	}
	sort.Strings(exportedFunctions)

	// Define and parse the global options
	verbose := flag.Bool("verbose", false, "enable verbose logging")
	help := flag.Bool("help", false, "show help")
	flag.Parse()

	if *help {
		printUsage()
		os.Exit(0)
	}

	// Pull the rest of the original arguments into a "subcommand line"
	args := flag.Args()

	var command string
	// Check we have a subcommand to run
	if len(args) == 0 {
		command = ""
	} else {
		runCmd := flag.NewFlagSet("runCmd", flag.ExitOnError)
		verbose = runCmd.Bool("verbose", *verbose, "enable verbose logging")
		command = args[0]
		runCmd.Parse(args[1:])
	}

	if command == "" {
		fmt.Printf("Available methods: %+v\n", exportedFunctions)
		fmt.Printf("Enter DayNN: ")
		fmt.Scanf("%s", &command)
	}
	command = cases.Title(language.English).String(command)

	fmt.Printf("command = '%v' verbose=%v\n", command, *verbose)

	log := internal.NewLogger()

	in_args := make([]reflect.Value, 1) // 1 argument
	in_args[0] = reflect.ValueOf(log)

	// use of Call() method
	var t internal.AOC
	method := reflect.ValueOf(&t).MethodByName(command)
	if !method.IsValid() {
		fmt.Printf("Error: cannot find method '%v'\n", command)
		fmt.Printf("Available methods: %+v\n", exportedFunctions)
		os.Exit(1)
	}
	method.Call(in_args)
	// fmt.Println(val)
}
