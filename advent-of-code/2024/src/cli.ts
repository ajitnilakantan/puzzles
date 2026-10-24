export interface CliOptions {
  readonly help: boolean;
  readonly verbose: boolean;
  readonly days: readonly string[];
}

export interface ParseResult {
  readonly options: CliOptions;
  readonly errors: readonly string[];
}

export function parseArgs(args: readonly string[]): ParseResult {
  let help = false;
  let verbose = false;
  const days: string[] = [];
  const errors: string[] = [];

  for (const arg of args) {
    switch (arg) {
      case "--help":
      case "-h":
        help = true;
        break;
      case "--verbose":
      case "-v":
        verbose = true;
        break;
      case "all":
        days.push("all");
        break;
      default:
        if (arg.startsWith("-")) {
          errors.push(`Unknown option: ${arg}`);
        } else if (!/^day\d{2}$/.test(arg)) {
          errors.push(`Invalid day: ${arg}. Expected dayNN, for example day01`);
        } else {
          days.push(arg);
        }
    }
  }

  if (days.includes("all") && days.some((day) => day !== "all")) {
    errors.push(`Cannot mix "all" with specific days`);
  }

  return { options: { help, verbose, days }, errors };
}

export function usage(): string {
  return `Usage: bun run src/main.ts [options] [dayNN ...]

Options:
  -h, --help       Show this help message
  -v, --verbose    Enable verbose output

Arguments:
  dayNN            Run one or more plugins, for example day01 day03
  all              Run every available plugin

With no day argument, available plugins are listed.`;
}
