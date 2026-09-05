# Advent of Code 2024 Bun skeleton

This is a Bun + TypeScript CLI skeleton that discovers `day/dayNN.ts` files and invokes the selected plugin with a dynamic import (reflection-style dispatch).

## Run

```sh
bun install
bun run src/main.ts --help
bun run src/main.ts             # list available plugins
bun run src/main.ts day01
bun run src/main.ts --verbose day01
```

Options:

- `--help`, `-h`: show brief help
- `--verbose`, `-v`: pass verbose mode to the plugin
- `dayNN`: run one plugin, such as `day01`
- no arguments: list available plugins

## Add a plugin

Create a file such as `day/day03.ts` with the default export below:

```ts
import type { DayPlugin } from "../src/plugin";

const plugin: DayPlugin = {
  name: "Day 03",
  run({ verbose }) {
    if (verbose) console.log("verbose mode");
    console.log("solve the day here");
  },
};

export default plugin;
```

Only files named `dayNN.ts` are discovered.

## Tests

Bun includes a test runner, so no test dependency is required:

```sh
bun test
```

Tests live in `tests/` and can import parser, registry, and CLI functions directly.
