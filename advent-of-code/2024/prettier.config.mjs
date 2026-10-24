/** @type {import("prettier").Config} */
const config = {
  printWidth: 120, // Wrap lines when they exceed 120 characters
  tabWidth: 2,
  useTabs: false,
  semi: true, // Print semicolons at the ends of statements
  singleQuote: false, // Use double quotes
  trailingComma: "es5", // Print trailing commas wherever possible
  bracketSpacing: true, // Space object literals
  arrowParens: "always", // Include parentheses around a sole arrow function parameter (e.g., (x) => x)

  plugins: ["@trivago/prettier-plugin-sort-imports"],
  importOrder: ["^@core/(.*)$", "^@server/(.*)$", "^@ui/(.*)$", "^[./]"],
  importOrderSeparation: false,
  importOrderSortSpecifiers: true,
};

export default config;
