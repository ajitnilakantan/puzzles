import eslint from "@eslint/js";
import { defineConfig } from "eslint/config";
import tseslint from "typescript-eslint";

export default defineConfig(
  eslint.configs.recommended,
  ...tseslint.configs.strictTypeChecked, // Splatting arrays directly is deprecated here
  ...tseslint.configs.stylisticTypeChecked,
  ...tseslint.configs.recommended,

  {
    languageOptions: {
      parserOptions: {
        // 1. Tell ESLint to use TypeScript's project service
        projectService: {
          // Config files are outside the tsconfig include list
          allowDefaultProject: ["eslint.config.mjs", "prettier.config.mjs"],
        },
        tsconfigRootDir: import.meta.dirname,
      },
    },
  },
  {
    ignores: ["./day/dayNN.test.ts", "./day/dayNN.ts"],
  },
  {
    rules: {
      // Force explicit return types on functions to ensure API clarity
      "@typescript-eslint/explicit-function-return-type": "error",

      "@typescript-eslint/restrict-template-expressions": "off",
      //[
      //  'error',
      //  {
      //    allowBoolean: true, // Allows booleans without throwing an error
      //    allowNumber: true, // Allows number without throwing an error
      //  },
      //],

      // Prevent leaving console logs or debuggers in production code
      //'no-console': ['warn', { allow: ['warn', 'error'] }],
      //'no-debugger': 'error',

      // Enforce clean object structures and prevent unused variables
      "@typescript-eslint/no-unused-vars": [
        "error",
        {
          argsIgnorePattern: "^_",
          varsIgnorePattern: "^_|^debug$",
        },
      ],

      "@typescript-eslint/no-non-null-assertion": "off",

      // Allow tagged templates such as debug.log`...`
      "@typescript-eslint/no-unused-expressions": ["error", { allowTaggedTemplates: true }],

      // Disallow magic numbers (extract them to meaningful constants instead)
      "no-magic-numbers": "off", // Turn off base rule

      // Ensure switch statements are exhaustive when working with union types
      // Allow switch (true) {...}
      // '@typescript-eslint/switch-exhaustiveness-check': 'error',
      "@typescript-eslint/switch-exhaustiveness-check": ["error", { requireDefaultForNonUnion: false }],

      // Force consistent use of type imports for faster bundling
      "@typescript-eslint/consistent-type-imports": [
        "error",
        {
          prefer: "type-imports",
          fixStyle: "separate-type-imports",
        },
      ],
    },
  },
  {
    // Target test files specifically
    files: ["**/*.test.ts", "**/*.spec.ts"],
    rules: {
      "@typescript-eslint/require-await": "off",
    },
  }
);

//import js from "@eslint/js";
//import { defineConfig } from "eslint/config";
//import eslint from '@eslint/js';
//import tseslint from "typescript-eslint";
//
//export default defineConfig([
//  {
//    ignores: ["./day/dayNN.test.ts", "./day/dayNN.ts"],
//  },
//  {
//    files: ["**/*.{js,mjs,cjs,ts,mts,cts}"],
//    plugins: { js },
//    extends: ["js/recommended"],
//    linterOptions: {
//      noInlineConfig: false,
//      reportUnusedDisableDirectives: "warn",
//    },
//    rules: {
//      // "no-unused-vars": "off", // turns off Javascript rules
//      "@typescript-eslint/no-unused-vars": [
//        "error",
//        {
//          vars: "all",
//          varsIgnorePattern: "^_|^debug$",
//          args: "all",
//          argsIgnorePattern: "^_",
//          caughtErrors: "all",
//          caughtErrorsIgnorePattern: "^_",
//          destructuredArrayIgnorePattern: "^_",
//          ignoreRestSiblings: true,
//        },
//      ],
//      "no-unused-vars": [
//        "error",
//        {
//          vars: "all",
//          varsIgnorePattern: "^_",
//          args: "after-used",
//          argsIgnorePattern: "^_",
//          caughtErrors: "all",
//          caughtErrorsIgnorePattern: "^_",
//        },
//      ],
//      // Allow tagged templates such as debug.log`...`
//      "@typescript-eslint/no-unused-expressions": ["error", { allowTaggedTemplates: true }],
//      "prefer-const": [
//        "error",
//        {
//          destructuring: "any", // Default configuration option
//          ignoreReadBeforeAssign: false, // Default configuration option
//        },
//      ],
//    },
//  },
//  // Inherit core ESLint and strict TypeScript rules
//  eslint.configs.recommended,
//  ...tseslint.configs.strictTypeChecked,
//  ...tseslint.configs.stylisticTypeChecked,
//  ...tseslint.configs.recommended,
//]);
