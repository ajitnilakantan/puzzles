export interface CliOptions {
  readonly help: boolean;
  readonly verbose: boolean;
  readonly day: string | null;
}

export interface ParseResult {
  readonly options: CliOptions;
  readonly errors: readonly string[];
}

export function parseArgs(args: readonly string[]): ParseResult {
  let help = false;
  let verbose = false;
  let day: string | null = null;
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
      default:
        if (arg.startsWith("-")) {
          errors.push(`Unknown option: ${arg}`);
        } else if (day !== null) {
          errors.push(`Only one day may be specified (already have ${day})`);
        } else if (!/^day\d{2}$/.test(arg)) {
          errors.push(`Invalid day: ${arg}. Expected dayNN, for example day01`);
        } else {
          day = arg;
        }
    }
  }

  return { options: { help, verbose, day }, errors };
}

export function usage(): string {
  return `Usage: bun run src/main.ts [options] [dayNN]

Options:
  -h, --help       Show this help message
  -v, --verbose    Enable verbose output

Arguments:
  dayNN            Run a plugin, for example day01

With no day argument, available plugins are listed.`;
}
