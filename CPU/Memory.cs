namespace _8_bit_CPU;

public class Memory
{
    // memory size
    private const int _memSize = 256;

    // property to expose memory size safely
    public int MemSize => _memSize;

    // memory representation, laid out in bytes
    public byte[] RAM = new byte[_memSize];

    // access the byte array directly using object
    // eg :- Memory _memory = new Memory();
    //       _memory[x] = 0x00; -> same as _memory.RAM[x] = 0x00;
    public byte this[int addr]
    {
        get => RAM[addr & 0xFF];
        set => RAM[addr & 0xFF] = value;
    }
}
