import { execFileSync } from "node:child_process";
import path from "node:path";
import { pathToFileURL } from "node:url";

const forbiddenNames = new Set([
  "agents.md", "coderteam.md", "github verhalten.md", "lessonslearned.md", "tokenoptimierung.md"
]);

export function checkTrackedTree(directory) {
  const output = execFileSync("git", ["ls-files", "-z"], { cwd: directory, encoding: "utf8" });
  const forbidden = output.split("\0").filter((file) => forbiddenNames.has(path.posix.basename(file).toLowerCase()));
  if (forbidden.length) {
    throw new Error(`Central agent control files must not be tracked:\n${forbidden.join("\n")}`);
  }
}

if (process.argv[1] && pathToFileURL(path.resolve(process.argv[1])).href === import.meta.url) {
  checkTrackedTree(process.cwd());
  console.log("No central agent control files are tracked.");
}
