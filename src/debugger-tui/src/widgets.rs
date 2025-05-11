use noa_runtime::ark::FuncId;
use noa_runtime::heap::HeapValue;
use noa_runtime::value::{Type, Value};
use noa_runtime::vm::frame::{Frame, FrameKind};
use ratatui::widgets::{Block, BorderType, Borders, Paragraph, Wrap};
use ratatui::prelude::*;

use noa_runtime::vm::debugger::DebugInspection;

use crate::instruction::InstructionSummary;
use crate::{utils, State};

pub struct MainWidget<'insp, 'vm, 'state> {
    pub inspection: &'insp DebugInspection<'vm>,
    pub _state: &'state State
}

impl Widget for MainWidget<'_, '_, '_> {
    fn render(self, area: Rect, buf: &mut Buffer) {    
        let super_layout = Layout::default()
            .direction(Direction::Vertical)
            .constraints([
                Constraint::Fill(1),
                Constraint::Length(1)
            ])
            .split(area);
        
        self.shortcuts(super_layout[1], buf);

        let main_layout = Layout::default()
            .direction(Direction::Horizontal)
            .constraints([
                Constraint::Percentage(30),
                Constraint::Fill(1),
                Constraint::Percentage(30)
            ])
            .split(super_layout[0]);
                
        self.stack_widget(main_layout[0], buf);
        self.exec_info_widget(main_layout[1], buf);
        self.call_stack_widget(main_layout[2], buf);
    }
}

impl MainWidget<'_, '_, '_> {
    fn shortcuts(&self, area: Rect, buf: &mut Buffer) {
        let text = Line::from(vec![
            " continue: ".into(),
            "<space> ".blue().bold()
        ]).centered();

        Block::default()
            .borders(Borders::BOTTOM)
            .border_type(BorderType::Double)
            .title_bottom(text)
            .render(area, buf);
    }

    fn stack_widget(&self, area: Rect, buf: &mut Buffer) {
        let title = Line::from(" Stack ").centered();

        Block::bordered()
            .border_type(BorderType::Rounded)
            .title_top(title)
            .render(area, buf);

        let var_indices = utils::get_stack_variable_indices(&self.inspection);

        let mut separator = String::new();
        for _ in 0..(area.width - 4) {
            separator.push('─');
        }

        let mut values = Vec::new();
        let mut i = 0;
        for val in self.inspection.stack.iter() {
            let mut line = self.show_value(*val);
            
            if let Some((start, end)) = var_indices {
                if i == start {
                    values.push(separator.clone().into());
                }
                if i == end {
                    values.push(separator.clone().into());
                }
                if i >= end {
                    for span in line.iter_mut() {
                        *span = span.clone().style(Style::new().add_modifier(Modifier::DIM));
                    }
                }
            }

            values.push(line);

            i += 1;
        }

        Paragraph::new(values)
            .wrap(Wrap { trim: false })
            .render(area.inner(Margin::new(2, 2)), buf);
    }

    fn show_type(&self, typ: Type) -> Span<'static> {
        match typ {
            Type::Number => "number".cyan(),
            Type::Bool => "bool".blue(),
            Type::Function => "function".green(),
            Type::String => "string".yellow(),
            Type::List => "list".white(),
            Type::Nil => "object".white(),
        }
    }

    fn show_value(&self, value: Value) -> Line<'static> {
        match value {
            Value::Number(x) => x.to_string().cyan().into(),

            Value::Bool(x) => x.to_string().blue().into(),

            Value::InternedString(index) =>
                self.show_istr(index).into(),
            
            Value::Function(closure) =>
                self.show_func(closure.function).into(),

            Value::Object(adr) => {
                if let Ok(obj) = self.inspection.heap.get(adr) {
                    match obj {
                        HeapValue::String(s) =>
                            format!("\"{}\"", s.clone()).light_yellow().into(),
                        
                        HeapValue::List(_) => "list".into(),

                        HeapValue::Object(_) => "object".into(),
                    }
                } else {
                    format!("bad obj {}", adr.0).red().into()
                }
            },

            Value::Nil => "()".into(),
        }
    }

    fn show_istr(&self, index: usize) -> Line<'static> {
        if let Some(s) = self.inspection.consts.strings.get(index) {
            format!("i\"{}\"", s.clone()).yellow().into()
        } else {
            "bad istr".red().into()
        }
    }

    fn show_func(&self, function: FuncId) -> Line<'static> {
        let id = function.decode() as usize;

        if function.is_native() {
            if id < self.inspection.consts.native_functions.len() {
                format!("nfunc {id}").green().into()
            } else {
                format!("bad nfunc {id}").red().into()
            }
        } else {
            if let Some(function) = self.inspection.consts.functions.get(id) {
                let mut spans = Vec::new();
                spans.push(format!("func {id} ").green());
                spans.extend(self.show_istr(function.name_index as usize).iter().cloned());
                Line::from(spans)
            } else {
                format!("bad func {id}").red().into()
            }
        }
    }

    fn exec_info_widget(&self, area: Rect, buf: &mut Buffer) {
        let title = Line::from(" Execution Info ").centered();

        Block::new()
            .borders(Borders::TOP | Borders::BOTTOM)
            .border_type(BorderType::Rounded)
            .title_top(title)
            .render(area, buf);

        let subarea = Layout::default()
            .direction(Direction::Vertical)
            .constraints([
                Constraint::Length(10)
            ])
            .split(area.inner(Margin::new(4, 2)))
            [0];

        self.code_widget(subarea, buf);
    }

    fn code_widget(&self, area: Rect, buf: &mut Buffer) {
        let ip = self.inspection.ip;
        let summary = InstructionSummary::from(self.inspection);

        let mut opcodes = self.show_opcodes(ip, &summary);
        opcodes.spans.insert(0, Span::from(format!(":{ip} | ")));

        let (layout, _) = Layout::default()
            .direction(Direction::Vertical)
            .constraints([
                Constraint::Length(1),
                Constraint::Length(1),
                Constraint::Length(summary.operands.len() as u16 + 1),
                Constraint::Length(summary.arguments.len() as u16 + 1),
                Constraint::Fill(1)
            ])
            .spacing(1)
            .split_with_spacers(area);

        Paragraph::new(vec![
                Line::from(opcodes)
            ])
            .render(layout[0], buf);

        Paragraph::new(vec![
                Line::from(vec![
                    summary.name.clone().blue()
                ])
            ])
            .render(layout[1], buf);

        self.show_operands(&summary).render(layout[2], buf);
        self.show_args(&summary).render(layout[3], buf);
    }

    fn show_opcodes(&self, ip: usize, summary: &InstructionSummary) -> Line<'static> {
        let code = &self.inspection.consts.code[ip..];
        let mut opcodes = Vec::new();

        opcodes.push(format!("{:X} ", code[0]).blue());

        let mut operands = summary.operands.iter();
        let mut operand = operands
            .next()
            .map(|op| (op, 0));
        
        for c in &code[1..] {
            let color = match &mut operand {
                Some((op, index)) => {
                    *index += 1;
                    if *index >= op.length {
                        operand = operands.next().map(|op| (op, 0));
                    }

                    Color::Magenta
                },
                None => Color::Gray,
            };

            opcodes.push(Span::from(format!("{:X} ", *c)).style(color));
        }

        opcodes.into()
    }

    fn show_operands(&self, summary: &InstructionSummary) -> Paragraph<'static> {
        let mut lines = vec![
            "Operands:".into()
        ];

        for operand in &summary.operands {
            let mut spans = vec![
                operand.name.clone().yellow(),
                ": ".into(),
                operand.typ.clone().yellow()
            ];

            spans.push(" = ".into());
            if let Some(value) = &operand.value {
                spans.push(value.clone().magenta())
            } else {
                spans.push("<!>".red());
            }

            lines.push(Line::from(spans));
        }

        Paragraph::new(lines)
    }

    fn show_args(&self, summary: &InstructionSummary) -> Paragraph<'static> {
        let mut lines = vec![
            "Arguments:".into()
        ];

        for arg in &summary.arguments {
            let mut spans = vec![
                arg.name.clone().yellow()
            ];

            if let Some(typ) = &arg.expected_type {
                spans.push(": ".into());
                spans.push(self.show_type(*typ));
            }

            spans.push(" = ".into());
            if let Some(value) = &arg.value {
                let shown = self.show_value(*value);
                spans.extend_from_slice(&shown.spans[..]);
            } else {
                spans.push("<!>".red());
            }

            lines.push(Line::from(spans));
        }

        Paragraph::new(lines)
    }

    fn call_stack_widget(&self, area: Rect, buf: &mut Buffer) {
        let title = Line::from(" Call Stack ").centered();

        Block::bordered()
            .border_type(BorderType::Rounded)
            .title_top(title)
            .render(area, buf);

        let frames = self.inspection.call_stack
            .iter()
            .rev()
            .map(|f| self.show_frame(f))
            .collect::<Vec<_>>();

        Paragraph::new(frames)
            .wrap(Wrap { trim: false })
            .render(area.inner(Margin::new(2, 2)), buf);
    }

    fn show_frame(&self, frame: &Frame) -> Line<'static> {
        match frame.kind {
            FrameKind::Temp { .. } => Line::from("temp frame".magenta()),
            _ => self.show_func(frame.function)
        }
    }
}
