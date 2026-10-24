import { readdir } from "node:fs/promises";
import { join } from "node:path";
import { pathToFileURL } from "node:url";
import { type DayPlugin, isDayPlugin } from "./plugin";

const dayPattern = /^day\d{2}\.ts$/;
const dayDirectory = join(import.meta.dir, "..", "day");

export async function discoverPlugins(): Promise<string[]> {
  const entries = await readdir(dayDirectory, { withFileTypes: true });
  return entries
    .filter((entry) => entry.isFile() && dayPattern.test(entry.name))
    .map((entry) => entry.name.slice(0, -3))
    .sort();
}

export async function loadPlugin(day: string): Promise<DayPlugin> {
  const fileName = `${day}.ts`;
  if (!dayPattern.test(fileName)) throw new Error(`Invalid day plugin: ${day}`);

  const available = await discoverPlugins();
  if (!available.includes(day)) {
    throw new Error(`Plugin ${day} was not found. Available plugins: ${available.join(", ") || "none"}`);
  }

  const modulePath = pathToFileURL(join(dayDirectory, fileName)).href;
  const module = (await import(modulePath)) as { default?: unknown };
  if (!isDayPlugin(module.default)) {
    throw new Error(`${fileName} must default-export an object with name and run()`);
  }
  return module.default;
}
