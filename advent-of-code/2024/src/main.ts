import { parseArgs, usage } from "./cli";
import { discoverPlugins, loadPlugin } from "./registry";

export async function runCli(args: readonly string[]): Promise<number> {
  const { options, errors } = parseArgs(args);

  if (errors.length > 0) {
    for (const error of errors) console.error(`Error: ${error}`);
    console.error(usage());
    return 2;
  }
  if (options.help) {
    console.log(usage());
    return 0;
  }

  if (options.day === null) {
    const plugins = await discoverPlugins();
    console.log(plugins.length === 0 ? "No plugins available." : "Available plugins:");
    for (const name of plugins) {
      const plugin = await loadPlugin(name);
      console.log(`  ${name} - ${plugin.name}`);
    }
    return 0;
  }

  try {
    const plugin = await loadPlugin(options.day);
    if (options.verbose) console.log(`Loading ${options.day} (${plugin.name})`);
    await plugin.run({ verbose: options.verbose });
    return 0;
  } catch (error) {
    console.error(error instanceof Error ? error.message : String(error));
    return 1;
  }
}

if (import.meta.main) {
  process.exitCode = await runCli(Bun.argv.slice(2));
}
