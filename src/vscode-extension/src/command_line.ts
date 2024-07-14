import { spawn } from "child_process";
import { ok, err, Result } from "./result";
import { getCliCommand } from "./config";

export async function checkForCli(): Promise<boolean> {
    let command = getCliCommand();
    let result = await executeCommand(command, "--version");

    return result.kind === "ok" && result.value.exitCode == 0;
}

export interface CommandResult {
    stdout: string,
    stderr: string,
    exitCode: number,
}

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
