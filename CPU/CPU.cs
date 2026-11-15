namespace _8_bit_CPU;

public class CPU
{
    private readonly Memory _memory;

    private readonly byte[] _r = new byte[4]; // 4 general purpose registers R0 - R3
    private byte _pc = 0x00; // program count -> keep track of instructions

    private bool ZO = false; // zero flag
    private bool CO = false; // carry flag
    private bool _run = true; // running flag

    public CPU(Memory memory)
    {
        _memory = memory;
    }

    // Load the machine code into memory for execution
    public void LoadProgram(byte[] program)
    {
        if (program == null)
        {
            throw new ArgumentNullException(nameof(program), "Program byte array cannot be null");
        }

        if (program.Length == 0)
        {
            throw new ArgumentException("Program byte array cannot be empty");
        }

        if (program.Length > 256)
        {
            throw new ArgumentOutOfRangeException(
                nameof(program),
                $"ERROR!! Program exceeds memory size: program_size={program.Length} memory_size={_memory.MemSize}"
            );
        }
        for (int i = 0; i < program.Length; i++)
        {
            _memory[i] = program[i];
        }
    }

    // fetch instructions from memory
    public ushort Fetch()
    {
        // prevent _pc from going beyond the 256 bytes memory limit
        if (_pc > 0xFE)
        {
            throw new InvalidOperationException(
                $"Incomplete instruction at PC=0x{_pc:X4}: instruction requires two bytes"
            );
        }

        // intructions are 2 bytes long
        // memory can only hold 1 byte at one location
        // so, fetch 2 consecutive bytes and combine into one whole instruction
        ushort opcode = (ushort)(_memory[_pc] << 8 | _memory[_pc + 1]);
        return opcode;
    }

    // decode the opcode into understandable variables
    public void Decode(ushort opcode, out byte x, out byte y, out byte n, out byte nn)
    {
        // Decoder variables
        x = (byte)((opcode & 0x0F00) >> 8); // second nybble
        y = (byte)((opcode & 0x00F0) >> 4); // third nybble
        n = (byte)(opcode & 0x000F); // fourth/last nybble (currently not in use)
        nn = (byte)(opcode & 0x00FF); // last byte/8-bits
    }

    // execute opcode
    public void Execute(ushort opcode)
    {
        Decode(opcode, out byte x, out byte y, out byte n, out byte nn);

        // invalid register check
        if (x > (byte)(3) || y > (byte)(3))
        {
            throw new IndexOutOfRangeException(
                $"Register index out of range in OPCODE: 0x{opcode:X4} x={x} at PC=0x{_pc:X4}"
            );
        }

        // first nybble will determine what operation to perform
        switch (opcode & 0xF000)
        {
            // Halt
            case 0x0000:
                _run = false;
                break;

            // Load imm
            case 0x1000:
                _r[x] = nn;
                break;

            // Add Rx, Ry
            case 0x2000:
                int sum = _r[x] + _r[y];
                CO = (sum > 255 ? true : false);
                _r[x] = (byte)(sum & 0xFF);
                ZO = (sum == 0);
                break;

            // Sub Rx, Ry
            case 0x3000:
                int diff = _r[x] - _r[y];
                CO = (_r[x] >= _r[y] ? true : false);
                _r[x] = (byte)(diff & 0xFF);
                ZO = (diff == 0);
                break;

            // Store mem <- Rx
            case 0x4000:
                _memory[nn] = _r[x];
                break;

            // Load Rx <- memory
            case 0x5000:
                _r[x] = _memory[nn];
                break;

            // Jump to nn
            case 0x6000:
                _pc = nn;
                break;

            // JZ -> Jump if zero
            case 0x7000:
                if (ZO)
                {
                    _pc = nn;
                }
                break;

            // Reset and halt
            case 0x8000:
                Reset();
                _run = false;
                break;

            default:
                throw new InvalidOperationException(
                    $"ERROR!! Unknown opcode: 0x{opcode:X4} at PC=0x{_pc:X4}"
                );
        }
    }

    // perform a cpu cycle
    public void Cycle()
    {
        ushort opcode = Fetch();
        Console.WriteLine($"OPCODE:0x{opcode:X4} executed at PC=0x{_pc:X4}");
        _pc = (byte)((_pc + 2) & 0xFF);
        Execute(opcode);
    }

    // the main loop to run cpu cycles
    public void RunCycles()
    {
        _run = true;

        while (_run)
        {
            Cycle();
        }

        Console.WriteLine("CPU Halted");
    }

    // reset the cpu state
    public void Reset()
    {
        _pc = 0x00;
        ZO = false;
        CO = false;
    }
}
