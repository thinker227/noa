use std::{ops::{ControlFlow, FromResidual, Try}, process::{ExitCode, Termination}};

/// Return type from main function which can be used with the `?` operator.
/// 
/// `.into_exit()` can be used to convert [`Option`]s and [`Result`]s into [`Exit`].
/// ```
/// fn main() -> Exit<()> {
///    let x = s.parse::<u32>().into_exit()?;
///    /* ... */
///    Exit::ok()
/// }
/// ```
#[derive(PartialEq, Eq, Clone, Debug)]
pub enum Exit<T> {
    /// Continues execution with a value.
    Continue(T),
    /// Signals to terminate the program with an exit code and optionally an error message.
    Exit {
        code: u8,
        message: Option<String>,
    }
}

impl<T> Exit<T> {
    /// Terminatess with exit code 0.
    pub fn ok() -> Self {
        Self::Exit {
            code: 0,
            message: None
        }
    }

    /// Terminates with exit code 1.
    pub fn fail() -> Self {
        Self::Exit {
            code: 1,
            message: None
        }
    }

    /// Terminates with exit code 1 and an error message.
    pub fn fail_with_message(message: impl ToString) -> Self {
        Self::Exit {
            code: 1,
            message: Some(message.to_string())
        }
    }
}

impl<T> From<T> for Exit<T> {
    fn from(value: T) -> Self {
        Self::Continue(value)
    }
}

impl<T: Termination> Termination for Exit<T> {
    fn report(self) -> std::process::ExitCode {
        match self {
            Exit::Continue(x) => x.report(),
            Exit::Exit { code, message: None } => ExitCode::from(code),
            Exit::Exit { code: 0, message: Some(message) } => {
                println!("{message}");
                ExitCode::from(0)
            },
            Exit::Exit { code, message: Some(message) } => {
                eprintln!("{message}");
                ExitCode::from(code)
            },
        }
    }
}

impl<T> Try for Exit<T> {
    type Output = T;

    type Residual = (u8, Option<String>);

    fn from_output(output: Self::Output) -> Self {
        Self::Continue(output)
    }

    fn branch(self) -> ControlFlow<Self::Residual, Self::Output> {
        match self {
            Exit::Continue(x) => ControlFlow::Continue(x),
            Exit::Exit { code, message } => ControlFlow::Break((code, message))
        }
    }
}

impl<T> FromResidual for Exit<T> {
    fn from_residual((code, message): <Self as Try>::Residual) -> Self {
        Self::Exit { code, message }
    }
}

/// Trait for things which can be converted into [`Exit`].
pub trait IntoExit {
    type Output;

    fn into_exit(self) -> Exit<Self::Output>;
}

impl<T, E: ToString> IntoExit for Result<T, E> {
    type Output = T;

    fn into_exit(self) -> Exit<Self::Output> {
        match self {
            Ok(x) => x.into(),
            Err(e) => Exit::fail_with_message(e.to_string()),
        }
    }
}

impl<T> IntoExit for Option<T> {
    type Output = T;

    fn into_exit(self) -> Exit<Self::Output> {
        match self {
            Some(x) => x.into(),
            None => Exit::fail(),
        }
    }
}
