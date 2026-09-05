export interface PluginContext {
  readonly verbose: boolean;
}

export interface DayPlugin {
  readonly name: string;
  run(context: PluginContext): void | Promise<void>;
}

export function isDayPlugin(value: unknown): value is DayPlugin {
  if (typeof value !== "object" || value === null) return false;
  const plugin = value as Partial<DayPlugin>;
  return typeof plugin.name === "string" && typeof plugin.run === "function";
}
