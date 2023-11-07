using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class MYMYInterpreter
{
    static int[] registers = new int[4];

    static List<string> rType = new List<string> { "add", "sub", "set", "jeq" };
    static List<string> jType = new List<string> { "j" };
    static List<string> specType = new List<string> { "input", "print", "printall", "exit" };

    static List<string> ReadInstruction(string filePath)
    {
        List<string> instructions = new List<string>();
        string[] lines = File.ReadAllLines(filePath);

        foreach (string line in lines)
        {
            int endIdx = line.IndexOf("//") != -1 ? line.IndexOf("//") : line.Length;
            instructions.Add(line.Substring(0, endIdx).Trim());
        }

        return instructions;
    }

    static void ReadInput()
    {
        registers[1] = Convert.ToInt32(Console.ReadLine());
    }

    static void PrintLn(int reg)
    {
        Console.WriteLine($"{registers[reg]}\n");
    }

    static (int, int, int) ParseRType(string currentInst)
    {
        string[] parts = currentInst.Split(' ');
        int reg1 = int.Parse(parts[1].Trim('#'));
        int reg2 = int.Parse(parts[2].Trim('#'));
        int imm = int.Parse(parts[3]);
        return (reg1, reg2, imm);
    }

    static int ValidInstructions(List<string> instructions)
    {
        int error = 0;
        for (int index = 0; index < instructions.Count; index++)
        {
            string[] parts = instructions[index].Split(" ");

            if (rType.Contains(parts[0]))
            {
                if (parts.Length != 4)
                {
                    Console.WriteLine($"\tError on line {index + 1}: Incorrect instruction of length {parts.Length}");
                    error = 1;
                }
                else
                {
                    for (int x = 1; x <= 2; x++)
                    {
                        if (parts[x][0] != '#')
                        {
                            Console.WriteLine($"\tError on line {index + 1}: Missing '#' to mark register register");
                            error = 1;
                        }
                    }

                    if (!"123".Contains(parts[1][1]))
                    {
                        Console.WriteLine($"\tError on line {index + 1}: {parts[1][1]} cannot be used as the first register argument for r_type instructions");
                        error = 1;
                    }

                    if (!"01".Contains(parts[3]))
                    {
                        Console.WriteLine($"\tError on line {index + 1}: invalid immediate. Can only be a 1-bit value");
                        error = 1;
                    }
                }
            }
            else if (jType.Contains(parts[0]))
            {
                if (parts.Length != 2)
                {
                    Console.WriteLine($"\tError on line {index + 1}: Incorrect instruction of length {parts.Length}");
                    error = 1;
                }

                if (!int.TryParse(parts[1], out int val) || val > 15 || val < -16)
                {
                    Console.WriteLine($"\tError on line {index + 1}: {parts[1]} does not fit in a 5-bit (signed) value");
                    error = 1;
                }
            }
            else if (specType.Contains(parts[0]))
            {
                if (parts.Length != 1)
                {
                    Console.WriteLine($"\tError on line {index + 1}: Incorrect instruction of length {parts.Length}");
                    error = 1;
                }
            }
            else
            {
                if (parts[0] != "")
                {
                    Console.WriteLine($"\tError on line {index + 1}: unknown command: {parts[0]}");
                    error = 1;
                }
            }
        }
        return error;
    }

    static void ParseInstructions(List<string> instructions)
    {
        int index = 0;
        while (index < instructions.Count)
        {
            string[] currentInst = instructions[index].Split(" ");
            string instruction = currentInst[0];

            if (instruction == "input")
            {
                ReadInput();
            }
            else if (instruction == "exit")
            {
                break;
            }
            else if (instruction == "print")
            {
                PrintLn(1);
            }
            else if (instruction == "printall")
            {
                foreach (int reg in registers)
                {
                    Console.Write($"{reg} ");
                }
                Console.WriteLine("\n");
            }
            else if (rType.Contains(instruction))
            {
                var (reg1, reg2, imm) = ParseRType(instructions[index]);
                if (instruction == "add")
                {
                    Console.WriteLine($"adding: {registers[reg2] + imm} to {registers[reg1]} in reg{reg1}\n");
                    registers[reg1] = registers[reg1] + registers[reg2] + imm;
                }
                else if (instruction == "sub")
                {
                    Console.WriteLine($"subtracting: {(registers[reg2] - imm)*-1} from {registers[reg1]}  in reg{reg1}\n");
                    registers[reg1] = registers[reg1] - registers[reg2] - imm;
                }
                else if (instruction == "set")
                {
                    registers[reg1] = registers[reg2] + imm;
                    Console.WriteLine($"setting: reg{reg1} to {registers[reg2] + imm}\n");
                }
                else if (instruction == "jeq")
                {
                    if (registers[reg1] == registers[reg2])
                    {
                        index += imm;
                    }
                    else
                    {
                        index += imm == 0 ? 1 : 0;
                    }
                }
            }
            else if (instruction == "j")
            {
                index += int.Parse(currentInst[1]);
                Console.WriteLine($"jumping to: {index + 1}\n");
            }

            index++;
        }
    }

    public static void Main(string[] args)
    {
        if (args.Length != 1)
        {
            Console.WriteLine("\tIncorrect arguments, command should be\n\t'interpreter.exe <filename>'");
        }
        else
        {
            string filePath = args[0];
            if (File.Exists(filePath))
            {
                if (Path.GetExtension(filePath) != ".mymy")
                {
                    Console.WriteLine($"\tGiven file is not of type .mymy");
                }
                else
                {
                    List<string> instructions = ReadInstruction(filePath);
                    if (ValidInstructions(instructions) == 0)
                    {
                        ParseInstructions(instructions);
                    }
                }
            }
            else
            {
                Console.WriteLine($"\tFile: {filePath} does not exist");
            }
        }
    }
}
