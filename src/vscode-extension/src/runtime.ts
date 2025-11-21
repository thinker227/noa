import { getRuntime as fromCli } from "./command_line";
import { getRuntimePath as fromConfig } from "./config";

/**
 * Gets the path to the currently configured runtime.
 */
export async function getRuntime(cwd: string): Promise<string | undefined> {
    const configPath = fromConfig();
    if (configPath) return configPath;

    const cliPath = fromCli(cwd);
    if (cliPath) return cliPath;

    return undefined;
}
