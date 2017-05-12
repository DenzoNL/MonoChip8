using System;
using System.IO;
using Microsoft.Xna.Framework.Content;

namespace MonoChip8.Chip8
{
    /// <summary>
    ///     The Chip8's processor
    /// </summary>
    public class CPU
    {
        #region <<< CONSTRUCTOR >>>

        /// <summary>
        ///     CPU Constructor. Also initializes the CPU state.
        /// </summary>
        public CPU()
        {
            // Initialize memory with 4K Bytes
            Memory = new byte[4096];

            // Clear memory
            for (var i = 0; i < 4096; ++i)
            {
                Memory[i] = 0x0;
            }

            // Initialize 16 CPU registers
            V = new byte[16];

            for (var i = 0; i < 16; ++i)
            {
                V[i] = 0x0;
            }

            // Program Counter starts at 0x200;
            PC = 0x200;

            // I also starts at zero.
            I = 0x0;

            // The stack can contain up to 16 memory addresses.
            Stack = new ushort[16];

            for (var i = 0; i < 16; ++i)
            {
                Stack[i] = 0x0;
            }

            // The SP starts at zero.
            SP = 0x0;

            // The first OpCode is also 0x0
            OpCode = 0x0;

            // The graphics array is 64 pixels wide and 32 pixels high, for a total of 2048 pixels.
            Graphics = new byte[64 * 32];

            // Set the graphics array to zero
            ClearScreen();

            // The timers are also initialized to zero.
            DelayTimer = 0;
            SoundTimer = 0;

            // We have 16 keys at our disposal.
            Key = new byte[16];

            // Load the fontset into memory
            for (var i = 0; i < 80; ++i)
            {
                // For each byte in the Chip8Font array, store it in the Memory array, starting at 0.
                Memory[i] = Chip8Font[i];
            }

            DrawFlag = true;
        }

        #endregion

        /// <summary>
        /// Execute one instruction.
        /// </summary>
        public void Step()
        {
            // Get the next opcode
            FetchOpCode();

            // Decode the Opcode
            switch (OpCode & 0xF000)
            {
                case 0x0000: // 0x00XX instructions
                    switch (N)
                    {
                        case 0x0: // Clears the screen
                            ClearScreen();
                            DrawFlag = true;
                            PC += 2;
                            break;

                        case 0xE: // Returns from a subroutine
                            // Decrease the stack pointer so it points to the return address
                            --SP;
                            // Set the program counter to the return address
                            PC = Stack[SP];
                            PC += 2;
                            break;

                        default:
                            throw new Exception("Unknown Opcode: 0x" + OpCode.ToString("X4"));
                    }
                    break;

                case 0x1000: // Jumps to address NNN
                    PC = NNN;
                    break;

                case 0x2000: // Calls subroutine at NNN
                    // Store the current Program counter on the stack
                    Stack[SP] = PC;
                    // Increment the Stack Pointer
                    ++SP;
                    // Jump to NNN
                    PC = NNN;
                    break;

                case 0x3000: // Skips the next instruction if VX equals NN
                    if (VX == NN)
                    {
                        PC += 4; // skip
                    }
                    else
                    {
                        PC += 2; // don't skip
                    }
                    break;

                case 0x4000: // Skips the next instruction if VX doesn't equal NN
                    if (VX != NN)
                    {
                        PC += 4;
                    }
                    else
                    {
                        PC += 2;
                    }
                    break;

                case 0x5000: // Skips the next instruction if VX equals VY
                    if (VX == VY)
                    {
                        PC += 4;
                    }
                    else
                    {
                        PC += 2;
                    }
                    break;

                case 0x6000: // Sets VX to NN.
                    VX = NN;
                    PC += 2;
                    break;

                case 0x7000: // Adds NN to VX
                    VX += NN;
                    PC += 2;
                    break;

                case 0x8000:
                    switch (N)
                    {
                        // Sets VX to the value of VY
                        case 0x0:
                            VX = VY;
                            PC += 2;
                            break;

                        case 0x1: // Sets VX to VX or VY (Bitwise OR operation). VF is reset to 0.
                            VX |= VY;
                            VF = 0;
                            PC += 2;
                            break;

                        case 0x2: // Sets VX to VX and VY. (Bitwise AND operation) VF is reset to 0.
                            VX &= VY;
                            VF = 0;
                            PC += 2;
                            break;

                        case 0x3: // Sets VX to VX xor VY. VF is reset to 0.
                            VX ^= VY;
                            VF = 0;
                            PC += 2;
                            break;

                        case 0x4: // Adds VY to VX. VF is set to 1 when there's a carry, and to 0 when there isn't.
                            VF = (byte)(VY > (0xFF - VX) ? 1 : 0); // if the result is larger than 255 (1 byte), set the carry flag to 1
                            VX += VY;
                            PC += 2;
                            break;

                        case 0x5: // VY is subtracted from VX. VF is set to 0 when there's a borrow, and 1 when there isn't.
                            VF = (byte)(VY > (0xFF - VX) ? 0 : 1); // if the result is larger than 255 (1 byte), set the carry flag to 1
                            VX -= VY;
                            PC += 2;
                            break;

                        case 0x6: // Shifts VX right by one. VF is set to the value of the least significant bit of VX before the shift.[2]
                            VF = (byte)(VX & 0x1); // set the borrow flag
                            VX >>= 1;
                            PC += 2;
                            break;

                        case 0x7: // Sets VX to VY minus VX. VF is set to 0 when there's a borrow, and 1 when there isn't.
                            VF = (byte) (VX <= VY ? 1 : 0); // set the borrow flag
                            VX = (byte) (VY - VX);
                            PC += 2;
                            break;

                        case 0xE: // Shifts VX left by one. VF is set to the value of the most significant bit of VX before the shift.[2]
                            VF = (byte) (VX >> 7);
                            VX <<= 1;
                            PC += 2;
                            break;


                        default:
                            throw new Exception("Unknown Opcode: 0x" + OpCode.ToString("X4"));
                    }
                    break;

                case 0x9000: //	Skips the next instruction if VX doesn't equal VY. (Usually the next instruction is a jump to skip a code block)
                    if (VX != VY)
                    {
                        PC += 4;
                    }
                    else
                    {
                        PC += 2;
                    }
                    break;

                case 0xA000: // Sets I to the address NNN.
                    I = NNN;
                    PC += 2;
                    break;

                case 0xB000: // Jumps to the address NNN + V0
                    PC = (ushort) (NNN + V0);
                    break;

                case 0xC000: // Sets VX to the result of a bitwise and operation on a random number (Typically: 0 to 255) and NN.
                    VX = (byte) (_rand.Next(0, 255) & NN);
                    PC += 2;
                    break;

                // Draws a sprite at coordinate (VX, VY) that has a width of 8 pixels and a height of N pixels. 
                // Each row of 8 pixels is read as bit-coded starting from memory location I; 
                // I value doesn’t change after the execution of this instruction. 
                // As described above, VF is set to 1 if any screen pixels are flipped from set to unset when the sprite is drawn, 
                // and to 0 if that doesn’t happen. Code borrowed from http://www.multigesture.net/articles/how-to-write-an-emulator-chip-8-interpreter/
                case 0xD000:
                    VF = 0;
                    for (int yline = 0; yline < N; yline++)
                    {
                        ushort pixel = Memory[I + yline];
                        for (int xline = 0; xline < 7; xline++)
                        {
                            if ((pixel & (0x80 >> xline)) != 0)
                            {
                                if (Graphics[VX + xline + (VY + yline) * 64] == 1)
                                {
                                    VF = 1;
                                }

                                Graphics[VX + xline + ((VY + yline) * 64)] ^= 1;
                            }
                        }
                    }
                    PC += 2;
                    break;

                case 0xE000:
                    switch (NN)
                    {
                        case 0x9E: // Skips the next instruction if the key stored in VX is pressed. (Usually the next instruction is a jump to skip a code block)
                            if (Key[VX] == 1)
                            {
                                PC += 4;
                            }
                            else
                            {
                                PC += 2;
                            }
                            break;

                        case 0xA1: // Skips the next instruction if the key stored in VX isn't pressed. (Usually the next instruction is a jump to skip a code block)
                            if (Key[VX] != 1)
                            {
                                PC += 4;
                            }
                            else
                            {
                                PC += 2;
                            }
                            break;

                        default:
                            throw new Exception("Unknown Opcode: 0x" + OpCode.ToString("X4"));
                    }
                    break;

                case 0xF000:
                    switch (NN)
                    {
                        case 0x07: // Sets VX to the value of the delay timer
                            VX = DelayTimer;
                            PC += 2;
                            break;

                        case 0x0A: // A key press is awaited, and then stored in VX. (Blocking Operation. All instruction halted until next key event)

                            var keyPress = false;

                            for (var i = 0; i < 16; ++i)
                            {
                                if (Key[i] == 0) continue;
                                VX = (byte) i;
                                keyPress = true;
                            }

                            // If we didn't received a keypress, skip this cycle and try again.
                            if (!keyPress)
                            {
                                return;
                            }

                            PC += 2;
                            break;

                        case 0x15: // Sets the delay timer to VX
                            DelayTimer = VX;
                            PC += 2;
                            break;

                        case 0x18: // Sets the sound timer to VX
                            SoundTimer = VX;
                            PC += 2;
                            break;

                        case 0x1E:
                            I += VX;
                            PC += 2;
                            break;

                        case 0x29: // Sets I to the location of the sprite for the character in VX. Characters 0-F (in hexadecimal) are represented by a 4x5 font.
                            I = (ushort) (VX * 0x5);
                            PC += 2;
                            break;

                        // Stores the binary-coded decimal representation of VX, with the most significant of three digits at the address in I,
                        // the middle digit at I plus 1, and the least significant digit at I plus 2. (In other words, take the decimal representation of VX, 
                        // place the hundreds digit in memory at location in I, the tens digit at location I+1, and the ones digit at location I+2.)
                        case 0x33:
                            Memory[I] = (byte)(VX / 100);
                            Memory[I + 1] = (byte)(VX / 10 % 10);
                            Memory[I + 2] = (byte)(VX % 100 % 10);
                            PC += 2;
                            break;

                        case 0x55: // Stores V0 to VX (including VX) in memory starting at address I
                            for (var i = 0; i <= X; ++i)
                            {
                                Memory[I + i] = V[i];
                            }

                            // On the original interpreter, when the operation is done, I = I + X + 1.
                            I = (ushort)(I + X + 1);

                            PC += 2;
                            break;

                        case 0x65: // 	Fills V0 to VX (including VX) with values from memory starting at address I
                            for (var i = 0; i <= X; ++i)
                            {
                                V[i] = Memory[I + i];
                            }

                            // On the original interpreter, when the operation is done, I = I + X + 1.
                            I = (ushort)(I + X + 1);

                            PC += 2;
                            break;

                        default:
                            throw new Exception("Unknown Opcode: 0x" + OpCode.ToString("X4"));
                    }
                    break;

                default:
                    throw new Exception("Unknown Opcode: 0x" + OpCode.ToString("X4"));
            }

            UpdateTimers();
        }

        /// <summary>
        /// Stores the next OpCode in the program into the OpCode variable
        /// </summary>
        private void FetchOpCode()
        {
            // Fetch the first byte, shift it left 8 bits, fetch the second byte and bitwise OR them together
            // And store them into memory.
            OpCode = (ushort)((Memory[PC] << 8) | Memory[PC + 1]);
        }

        /// <summary>
        /// Clears the graphics array.
        /// </summary>
        private void ClearScreen()
        {
            for (var i = 0; i < 2048; ++i)
            {
                Graphics[i] = 0x0;
            }
        }

        /// <summary>
        /// Updates the Delay and Sound timers
        /// </summary>
        private void UpdateTimers()
        {
            if (DelayTimer > 0)
            {
                --DelayTimer;
            }

            if (SoundTimer > 0)
            {
                // Play a beeping sound when the soundtimer is being decremented to zero.
                if (SoundTimer == 1)
                {
                    Console.WriteLine("Beep!");
                }
                --SoundTimer;
            }
        }

        /// <summary>
        /// Loads a game from the Games folder with the given name
        /// </summary>
        /// <param name="name"></param>
        public void LoadGame(string name)
        {
            string filePath = name;

            // Load ROM bytes into binaryreader.
            var memoryStream = new MemoryStream(File.ReadAllBytes(filePath));
            var binaryReader = new BinaryReader(memoryStream);

            // Load ROM into memory at address 0x200 (512 bytes from 0)
            for (var i = 0; i < binaryReader.BaseStream.Length; ++i)
            {
                Memory[i + 512] = binaryReader.ReadByte();
            }
        }

        #region <<< CPU VARIABLES >>>

        /// <summary>
        ///     The CPU's Memory. 4096 bytes large.
        /// </summary>
        public byte[] Memory;

        /// <summary>
        ///     CPU Register array. VF = Carry Flag.
        /// </summary>
        public byte[] V;

        /// <summary>
        ///     Program Counter. Stores the address of the current opcode.
        /// </summary>
        public ushort PC;

        /// <summary>
        ///     16-bit register storing the current memory address.
        /// </summary>
        public ushort I;

        /// <summary>
        ///     The stack can contain up to 16 memory addresses
        /// </summary>
        public ushort[] Stack;

        /// <summary>
        ///    Stack Pointer; points to a memory address on the stack
        /// </summary>
        public ushort SP;

        /// <summary>
        ///     The current OpCode that is being processed
        /// </summary>
        public ushort OpCode;

        /// <summary>
        ///     Returns the last three nibbles of the current OpCode
        /// </summary>
        public ushort NNN
        {
            get { return (ushort) (OpCode & 0x0FFF); }
        }

        /// <summary>
        ///     Returns the last byte of the current opcode
        /// </summary>
        public byte NN
        {
            get { return (byte) (OpCode & 0x00FF); }
        }

        /// <summary>
        ///     Gets the last nibble of the current opcode
        /// </summary>
        public byte N
        {
            get { return (byte) (OpCode & 0x000F); }
        }

        /// <summary>
        ///     Gets the second nibble of the current opcode.
        /// </summary>
        public byte X
        {
            get { return (byte) ((OpCode & 0x0F00) >> 8); }
        }

        /// <summary>
        ///     The last CPU register, also known as the carry flag.
        /// </summary>
        public byte V0
        {
            get { return V[0x0]; }
            set { V[0x0] = value; }
        }

        /// <summary>
        ///     The last CPU register, also known as the carry flag.
        /// </summary>
        public byte VF
        {
            get { return V[0xF]; }
            set { V[0xF] = value; }
        }

        /// <summary>
        ///     Shortcut variable for the V register corresponding to the X value in the opcode.
        /// </summary>
        public byte VX
        {
            get { return V[X]; }
            set { V[X] = value; }
        }

        /// <summary>
        ///     Shortcut variable for the V register corresponding to the Y value in the opcode.
        /// </summary>
        public byte VY
        {
            get { return V[Y]; }
            set { V[Y] = value; }
        }

        /// <summary>
        ///     Gets the third nibble of the current opcode
        /// </summary>
        public byte Y
        {
            get { return (byte) ((OpCode & 0x00F0) >> 4); }
        }

        public byte[] Chip8Font =
        {
            0xF0, 0x90, 0x90, 0x90, 0xF0, // 0
            0x20, 0x60, 0x20, 0x20, 0x70, // 1
            0xF0, 0x10, 0xF0, 0x80, 0xF0, // 2
            0xF0, 0x10, 0xF0, 0x10, 0xF0, // 3
            0x90, 0x90, 0xF0, 0x10, 0x10, // 4
            0xF0, 0x80, 0xF0, 0x10, 0xF0, // 5
            0xF0, 0x80, 0xF0, 0x90, 0xF0, // 6
            0xF0, 0x10, 0x20, 0x40, 0x40, // 7
            0xF0, 0x90, 0xF0, 0x90, 0xF0, // 8
            0xF0, 0x90, 0xF0, 0x10, 0xF0, // 9
            0xF0, 0x90, 0xF0, 0x90, 0x90, // A
            0xE0, 0x90, 0xE0, 0x90, 0xE0, // B
            0xF0, 0x80, 0x80, 0x80, 0xF0, // C
            0xE0, 0x90, 0x90, 0x90, 0xE0, // D
            0xF0, 0x80, 0xF0, 0x80, 0xF0, // E
            0xF0, 0x80, 0xF0, 0x80, 0x80 // F
        };

        /// <summary>
        ///     Random number generator.
        /// </summary>
        private readonly Random _rand = new Random(DateTime.Now.Millisecond);

        #endregion

        #region <<< GRAPHICS, SOUND & INPUT VARIABLES >>>

        /// <summary>
        ///     Byte array storing the 2048 black and white pixels of the Chip8.
        /// </summary>
        public byte[] Graphics;

        /// <summary>
        /// Should the graphics be drawn on the next frame?
        /// </summary>
        public bool DrawFlag;

        /// <summary>
        ///     Timer register that counts down to zero at 60Hz.
        /// </summary>
        public byte DelayTimer,
            SoundTimer;

        /// <summary>
        ///     HEX-based keyboard ranging from 0x0 to 0xF, with the following mapping:
        ///     <para></para>
        ///     Keypad          Keyboard
        ///     <para></para>
        ///     |1|2|3|C|       |1|2|3|4|
        ///     <para></para>
        ///     |4|5|6|D|       |Q|W|E|R|
        ///     <para></para>
        ///     |7|8|9|E|       |A|S|D|F|
        ///     <para></para>
        ///     |A|0|B|F|       |Z|X|C|V|
        ///     <para></para>
        /// </summary>
        public byte[] Key;

        #endregion
    }
}