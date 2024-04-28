pub fn get(xs: &[u8], at: usize) -> Option<(&u8, &[u8])> {
    if at < xs.len() {
        Some((&xs[at], &xs[(at + 1)..]))
    } else {
        None
    }
}

pub fn split<const I: usize>(xs: &[u8]) -> Option<(&[u8; I], &[u8])> {
    if I <= xs.len() {
        Some((xs[..I].try_into().unwrap(), &xs[I..]))
    } else {
        None
    }
}

pub fn split_as_u32(xs: &[u8]) -> Option<(u32, &[u8])> {
    let (bytes, rest) = split::<4>(xs)?;
    let x = u32::from_be_bytes(*bytes);
    Some((x, rest))
}
