#![feature(never_type)]
#![feature(assert_matches)]
#![feature(vec_push_within_capacity)]
#![feature(string_from_utf8_lossy_owned)]

pub mod opcode;
pub mod ark;
pub mod value;
pub mod vm;
pub mod exception;
pub mod heap;
mod native;
