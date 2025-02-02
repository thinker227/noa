#![feature(never_type)]
#![feature(assert_matches)]
#![feature(vec_push_within_capacity)]

pub mod opcode;
pub mod ark;
pub mod value;
pub mod vm;
pub mod exception;
mod heap;
mod native;
