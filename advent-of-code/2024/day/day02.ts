import type { DayPlugin } from "../src/plugin";

const plugin: DayPlugin = {
  name: "Day 02 example",
  run({ verbose }) {
    console.log("Running day02");
    if (verbose) console.log("day02 received verbose=true");
  },
};

export default plugin;
