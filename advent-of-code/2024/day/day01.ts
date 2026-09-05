import type { DayPlugin } from "../src/plugin";

const plugin: DayPlugin = {
  name: "Day 01 example",
  run({ verbose }) {
    console.log("Running day01");
    if (verbose) console.log("day01 received verbose=true");
  },
};

export default plugin;
