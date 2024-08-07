import { spawn } from "child_process";
import { ok, err, Result } from "./result";
import { getCliCommand } from "./config";

/**
 * Returns whether the configured CLI command is available.
 */
export async function checkForCli(): Promise<boolean> {
    let command = getCliCommand();
    let result = await executeCommand(command, "--version");

    return result.kind === "ok" && result.value.exitCode == 0;
}

/**
 * The result of executing a command-line command.
 */
export interface CommandResult {
    stdout: string,
    stderr: string,
    exitCode: number,
}

/**
 * Executes a command-line command.
 * @param command The command to execute.
 * @param args The arguments to pass to the command.
 * @returns An object containing either the result of the command or the error the command failed with.
 */
export async function executeCommand(command: string, ...args: string[]): Promise<Result<CommandResult>> {
    return new Promise<Result<CommandResult>>((resolve) => {
        const process = spawn(command, args);

        let stdout = "";
        let stderr = "";

        process.stdout.on("data", data => stdout += data.toString());
        process.stderr.on("data", data => stderr += data.toString());
        
        process.on("error", error => resolve(err(error)));
        process.on("exit", exitCode => resolve(ok({
            stdout,
            stderr,
            exitCode: exitCode || 0
        })));
    });
}
