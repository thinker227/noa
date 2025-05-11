use noa_runtime::vm::frame::{Frame, FrameKind};
use noa_runtime::vm::debugger::DebugInspection;

pub fn get_stack_variable_indices(inspection: &DebugInspection) -> Option<(usize, usize)> {
    let frame = match inspection.call_stack.last()? {
        Frame { kind: FrameKind::Temp { parent_function_index }, .. } =>
            inspection.call_stack.get(*parent_function_index).unwrap(),
        x => x
    };

    let id = frame.function;
    if id.is_native() { None? }

    let function = inspection.consts.functions.get(id.decode() as usize)?;

    let start = inspection.stack.head() - frame.stack_start;
    let size = (function.arity + function.locals_count) as usize;

    Some((start - size, start))
}
