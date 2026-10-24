// Replace {0}, {1}, ... with the matching argument
function sprintf(format: string, ...args: unknown[]): string {
  return format.replace(/{(\d+)}/g, (match: string, index: string): string => {
    return typeof args[Number(index)] !== "undefined" ? String(args[Number(index)]) : match;
  });
}

function printNoNewline(message: string): void {
  if (typeof process !== "undefined" && typeof process.stdout.write === "function") {
    // Node.js environment: prints exactly what you provide without a trailing newline
    process.stdout.write(message);
  } else {
    // Browser environment fallback: logs normally (will include a newline)
    console.log(message);
  }
}

export function printfn(format: string, ...args: unknown[]): void {
  printNoNewline(sprintf(format, ...args) + "\n");
}

export function printf(format: string, ...args: unknown[]): void {
  printNoNewline(sprintf(format, ...args));
}

/*
 * Tagged template function to workaround logging bugs in Typescript/Javascript.
 * Allows printing out Map, Set, etc.
 * Usage:     debug.log`Hello ${world} !!!`  // no parentheses
 */
export function _log(strings: TemplateStringsArray, ...values: unknown[]): void {
  // Custom replacer to handle Maps and Sets at any depth
  const replacer = (_key: string, value: unknown): unknown => {
    if (value instanceof Map) {
      return Object.fromEntries(value);
    }
    if (value instanceof Set) {
      return Array.from(value);
    }
    if (typeof value === "bigint") {
      return value.toString();
    }
    return value;
  };

  // Process the interpolated values
  const processedValues = values.map((val) => {
    if (typeof val === "object" && val !== null) {
      return JSON.stringify(val, replacer);
    }
    return String(val);
  });

  // Reconstruct the string
  const result = strings.reduce((acc, str, i) => acc + str + (processedValues[i] ?? ""), "");
  console.log(result);
}

/*
 * Tagged template function to workaround logging bugs in Typescript/Javascript.
 * Allows printing out Map, Set, etc.
 * Usage:     debug.log`Hello ${world} !!!`  // no parentheses
 * Can specify formatting:
 *   const d = 42;
 *   debug.log`d='${d}:x'` ; // hex output: '2a'
 *   debug.log`d='${d}:b'` ; // binary output: '101010'
 *   debug.log`d='${d}:x:p:4'` ; // hex output, padded: '  2a'
 *   debug.log`d='${d}:b:p:8'` ; // binary output, padded: '  101010'
 *   const x = 3.141;
 *   debug.log`x='${x}:d:2'` ; // float output decimal: '3.14'
 *   debug.log`x='${x}:d:2:p:6'` ; // float output decimal, padded: '  3.14'
 *
 */
export function log(strings: TemplateStringsArray, ...values: unknown[]): void {
  const replacer = (_key: string, value: unknown): unknown => {
    if (value instanceof Map) return Object.fromEntries(value);
    if (value instanceof Set) return Array.from(value);
    if (typeof value === "bigint") return value.toString();
    return value;
  };

  const formatters: Record<string, (v: string, arg?: string) => string> = {
    d: (v, arg) => {
      const decimals = arg ? parseInt(arg, 10) : 0;
      const num = Number(v);
      return Number.isFinite(num) ? num.toFixed(decimals) : v;
    },
    x: (v) => {
      const num = Number(v);
      return Number.isFinite(num) ? num.toString(16) : v;
    },
    b: (v) => {
      const num = Number(v);
      return Number.isFinite(num) ? num.toString(2) : v;
    },
    p: (v, arg) => {
      const width = arg ? parseInt(arg, 10) : 0;
      return v.padStart(width, " ");
    },
  };

  function applyModifiers(value: string, mods: string[]): string {
    let result = value;
    for (const mod of mods) {
      const [key, arg] = mod.split(":");
      const fn = formatters[key] as ((v: string, arg?: string) => string) | undefined;
      if (fn) result = fn(result, arg);
    }
    return result;
  }

  function extractModifiers(rawTail: string): string[] {
    const match = /^(:[^\s]+)/.exec(rawTail);
    if (!match) return [];
    const tokens = match[1].slice(1).split(":").filter(Boolean);

    const mods: string[] = [];
    for (let i = 0; i < tokens.length; i++) {
      const key = tokens[i];
      const next = tokens[i + 1] as string | undefined;
      if (next !== undefined && /^\d+$/.test(next)) {
        mods.push(`${key}:${next}`);
        i++;
      } else {
        mods.push(key);
      }
    }
    return mods;
  }

  function stripModifiersFromTail(visibleTail: string): string {
    return visibleTail.replace(/^(:[^\s]+)/, "");
  }

  const processedValues = values.map((val, i) => {
    const base = typeof val === "object" && val !== null ? JSON.stringify(val, replacer) : String(val);

    const rawTail = strings.raw[i + 1] ?? "";
    const mods = extractModifiers(rawTail);

    return mods.length ? applyModifiers(base, mods) : base;
  });

  const strippedStrings = strings.map((s, i) => (i === 0 ? s : stripModifiersFromTail(s)));

  let result = "";
  for (let i = 0; i < strippedStrings.length; i++) {
    result += strippedStrings[i];
    if (i < processedValues.length) {
      result += processedValues[i];
    }
  }

  console.log(result);
}
