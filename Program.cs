namespace _8_bit_CPU;

class Program
{
    static void Main(string[] args)
    {
        byte[] program =
        [
            0x11,
            0x1F,
            0x52,
            0x08,
            0x21,
            0x20,
            0x60,
            0x0E,
            0x41,
            0x0F,
            0x00,
            0x00,
            0x0F,
            0x00,
            0x31,
            0x20,
            0x60,
            0x0A,
        ];

        Memory memory = new Memory();
        CPU cpu = new CPU(memory);

        cpu.LoadProgram(program);
        cpu.RunCycles();
    }
}
