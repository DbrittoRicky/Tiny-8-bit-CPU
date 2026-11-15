# Tiny 8-bit CPU Emulator

A minimal 8-bit CPU emulator written in C#.  
It executes hand-written machine-code programs from a 256-byte memory space.

This project is intended as a learning exercise for understanding the complete
CPU instruction lifecycle: **fetch → decode → execute**.

## Features

- 4 general-purpose 8-bit registers: `R0`, `R1`, `R2`, and `R3`
- 8-bit program counter (PC)
- Zero (`ZO`) and carry (`CO`) flags
- 256 bytes of memory
- Fixed 2-byte instruction encoding
- Manual opcode decoding using bit masks
- Basic arithmetic, memory access, and branching instructions
- Simple instruction tracing through `Console.WriteLine`
- Halt and reset instructions

## CPU Architecture

| Component | Size | Description |
|---|---:|---|
| Registers | 4 x 8-bit | `R0` through `R3` |
| Program counter | 8-bit | Points to the next instruction |
| Memory | 256 bytes | Addresses range from `0x00` to `0xFF` |
| Zero flag | 1-bit | Set when an arithmetic result is zero |
| Carry flag | 1-bit | Set on arithmetic overflow or borrow behavior |

Every instruction is exactly **2 bytes** long. The CPU fetches two consecutive
memory bytes, combines them into a 16-bit opcode, decodes the embedded fields,
and executes the operation.

## Instruction Format

Instructions are stored as two bytes in big-endian order:

```text
[high byte] [low byte]
```

The 16-bit opcode is formed as:

```text
opcode = (high byte << 8) | low byte
```

| Field | Bits | Meaning |
|---|---|---|
| Opcode | `0xF000` | Instruction type |
| `x` | `0x0F00` | First register index |
| `y` | `0x00F0` | Second register index |
| `n` | `0x000F` | Reserved / currently unused |
| `nn` | `0x00FF` | 8-bit immediate value or memory address |

## Instruction Set

| Opcode | Instruction | Encoding | Description |
|---:|---|---|---|
| `0x0000` | `HALT` | `0x0000` | Stops CPU execution |
| `0x1000` | `LOAD Rx, nn` | `0x1XNN` | Loads immediate value `nn` into `Rx` |
| `0x2000` | `ADD Rx, Ry` | `0x2XY0` | `Rx = Rx + Ry` |
| `0x3000` | `SUB Rx, Ry` | `0x3XY0` | `Rx = Rx - Ry` |
| `0x4000` | `STORE Rx, nn` | `0x4XNN` | Writes `Rx` to memory address `nn` |
| `0x5000` | `LOAD Rx, [nn]` | `0x5XNN` | Loads memory value at `nn` into `Rx` |
| `0x6000` | `JUMP nn` | `0x6NNN` | Sets PC to `nn` |
| `0x7000` | `JZ nn` | `0x7NNN` | Jumps to `nn` if zero flag is set |
| `0x8000` | `RESET` | `0x8000` | Resets CPU state and halts |

Register indices:

| Register | Index |
|---|---:|
| `R0` | `0x0` |
| `R1` | `0x1` |
| `R2` | `0x2` |
| `R3` | `0x3` |

## Flags

### Zero flag (`ZO`)

The zero flag is updated by arithmetic instructions.

For `SUB`, it is set when the result is zero:

```text
5 - 5 = 0 -> ZO = true
```

### Carry flag (`CO`)

For `ADD`, `CO` is set when the 8-bit result overflows:

```text
255 + 1 = 0 -> CO = true
```

For `SUB`, the current implementation sets `CO` when no borrow occurs:

```text
5 - 3 = 2 -> CO = true
3 - 5 = -2 -> CO = false
```

In other words, `CO` currently means **“no borrow occurred”** after subtraction.

## Example Program

This program loads `7` into `R0`, loads `5` into `R1`, adds them, stores the
result at memory address `0x20`, and halts.

```text
0x1007   ; LOAD R0, 7
0x1105   ; LOAD R1, 5
0x2010   ; ADD R0, R1
0x4020   ; STORE R0, 0x20
0x0000   ; HALT
```

Machine-code bytes:

```csharp
byte[] program =
{
    0x10, 0x07, // LOAD R0, 7
    0x11, 0x05, // LOAD R1, 5
    0x20, 0x10, // ADD R0, R1
    0x40, 0x20, // STORE R0, 0x20
    0x00, 0x00  // HALT
};
```

Expected result:

```text
R0 = 0x0C
R1 = 0x05
memory[0x20] = 0x0C
```

## CPU Lifecycle

Each CPU cycle follows this sequence:

1. **Fetch** — Read two bytes from memory at the current PC.
2. **Decode** — Extract the opcode family, register indices, and operands.
3. **Execute** — Update registers, flags, memory, or PC.
4. **Advance PC** — Move PC forward by 2 unless a jump changes it.

The emulator separates these stages into `Fetch()`, `Decode()`, and
`Execute()` methods.

## Error Handling

The CPU throws exceptions for invalid programs or instructions:

| Condition | Exception |
|---|---|
| `LoadProgram` receives `null` | `ArgumentNullException` |
| `LoadProgram` receives an empty array | `ArgumentException` |
| Program is larger than 256 bytes | `ArgumentOutOfRangeException` |
| Instruction starts at `0xFF`, leaving no second byte | `InvalidOperationException` |
| Register index is greater than `3` | `IndexOutOfRangeException` |
| Opcode is unknown | `InvalidOperationException` |

## Learning Goals

This project focuses on:

- Understanding how a CPU fetches and interprets machine code
- Separating CPU state, memory, decoding, and execution
- Working with binary encoding and bit manipulation
- Implementing flags and conditional branching
- Writing unit tests for low-level behavior