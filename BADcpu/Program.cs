using System;
using System.Diagnostics;
using System.IO;

namespace BADcpu
{
    internal class Program
    {
        public static bool start;
        public static Cpu cpu;

        static void Main(string[] args)
        {
            Console.WriteLine("welcome to the bcis virtual cpu.");
            Console.WriteLine("this uses the bad cpu instruction set(bcis) instruction set. \ncheck documantation for more information.\n");

            start = true;
            while (start)
                updateComand();
        }

        private static void updateComand()
        {
            string comand = Console.ReadLine();
            if(comand == "stop" || comand == "end")
            {
                start = false;
            }
            if (comand == "new")
            {
                if(cpu != null){
                    Console.WriteLine("do you want to override the old cpu?");
                     if (Console.ReadLine() != "y")
                        return;
                }
                cpu = new Cpu(64);
                return;
            }
            if (comand == "load")
            {
                if(cpu == null)
                {
                    Console.WriteLine("cpu not created!");
                    return;
                }

                Console.WriteLine("file:");
                string path = Console.ReadLine();

                cpu.setInstruction(bcisCompiler.compile(readFile(path)));
            }

            if (comand == "run")
            {
                if (cpu == null)
                {
                    Console.WriteLine("cpu not created!");
                    return;
                }

                for (int i = 0; i < 256; i++)
                    cpu.cpuCycel();
            }

            if (comand == "reset")
            {
                if (cpu == null)
                {
                    Console.WriteLine("cpu not created!");
                    return;
                }

                cpu.CpuAdres = 0;
            }
            
            if (comand == "read"){

                int i = 0;
                foreach (int adr in cpu.Ram)
                {
                    Console.Write(adr);

                    if (i == cpu.CpuAdres)
                        Console.Write("<");

                    Console.Write("\n");
                    i++;
                }
            }
        }

        private static string[] readFile(string path)
        {
            string[] file = File.ReadAllLines(path);
            return file;
        }
    }

    public class Cpu
    {
        #region api

        public int[] Ram;
        public int CpuAdres;

        public void setInstruction(int[] _ram)
        {
            Ram = _ram;
        }

        public Cpu(int ramSize)
        {
            Ram = new int[ramSize];
        }

        public void cpuCycel()
        {
            int ins = readRam(CpuAdres);
            //Console.WriteLine($"it's at addres {CpuAdres} and the instruction is {ins}");
            if (ins == 0)
                jumpIns();
            if (ins == 1)
                jumpIfIns();
            if (ins == 2)
                compareIns();
            if (ins == 3)
                addIns();
            if (ins == 4)
                subIns();
            if (ins == 5)
                divIns();
            if (ins == 6)
                mulIns();
            if (ins == 7)
                cloneIns();
            if (ins == 8)
                printIns();
            if (ins == 9)
                skipIns();

            if (ins > 9)
                jumpForward(1);
            if (ins < 0)
                jumpForward(1);
        }

        #endregion

        #region function

        private int readRam(int addres)
        {
            if(addres + 1 > Ram.Length)
                return 0;
            return Ram[addres];
        }

        private int readAddres(int addres)
        {
            return readRam(readRam(addres));
        }

        private void setRam(int addres, int value)
        {
            if (addres + 1 > Ram.Length)
                return;
            Ram[addres] = value;
        }

        private void jump(int adres)
        {
            CpuAdres = adres;
        }

        #endregion

        #region instruction

        private void jumpForward(int count)
        {
            jump(CpuAdres + count);
        }

        private void jumpIns()
        {
            jump(readRam(CpuAdres + 1));
            return;
        }

        private void jumpIfIns()
        {
            if (readAddres(CpuAdres + 1) != 0)
                jump(CpuAdres + 2);
            else
                jumpForward(3);
            return;
        }

        private void compareIns()
        {
            if (readAddres(CpuAdres + 1) == readAddres(CpuAdres + 2))
                setRam(readRam(CpuAdres + 3), 1);
            else
                setRam(readRam(CpuAdres + 3), 0);
            jumpForward(4);
            return;
        }

        private void addIns()
        {
            setRam(readRam(CpuAdres + 3), readAddres(CpuAdres + 1) + readAddres(CpuAdres + 2));
            jumpForward(4);
            return;
        }

        private void subIns()
        {
            setRam(readRam(CpuAdres + 3), readAddres(CpuAdres + 1) - readAddres(CpuAdres + 2));
            jumpForward(4);
            return;
        }

        private void mulIns()
        {
            setRam(readRam(CpuAdres + 3), readAddres(CpuAdres + 1) * readAddres(CpuAdres + 2));
            jumpForward(4);
            return;
        }

        private void divIns()
        {
            setRam(readRam(CpuAdres + 3), readAddres(CpuAdres + 1) / readAddres(CpuAdres + 2));
            jumpForward(4);
            return;
        }

        private void cloneIns()
        {
            setRam(readRam(CpuAdres + 2), readAddres(CpuAdres + 1));
            jumpForward(3);
            return;
        }

        private void printIns()
        {
            Console.WriteLine($"{readAddres(CpuAdres + 1)}");
            jumpForward(2);
            return;
        }

        private void skipIns()
        {
            jumpForward(1);
            return;
        }

        #endregion

        #region asemblyCompilation



        #endregion
    }
}

static class bcisCompiler
{
    public static int[] compile(string[] file)
    {
        int[] lines = new int[file.Length];

        Dictionary<string, int> varibles = new Dictionary<string, int>();
        setVars(ref varibles, file);

        for (int p = 0; p < file.Length; p++)
        {
            lines[p] = compileLine(file[p], file, ref varibles, p);
        }

        return(lines);
    }

    private static void setVars(ref Dictionary<string, int> varibles, string[] file)
    {
        for (int p = 0; p < file.Length; p++)
        {
            file[p] = getVarAdress(ref varibles, file[p], p);
        }

        for (int p = 0; p < file.Length; p++)
        {
            file[p] = setVarAddress(ref varibles, file[p]);
        }
    }

    private static string getVarAdress(ref Dictionary<string, int> varibles, string line, int adress)
    {
        if (line.ToArray().Contains('#'))
        {
            if(!varibles.ContainsKey(line.Remove(0)))
                varibles.Add(getVarName(line), adress);

            return (getValue(line));
        }
        return (line);
    }

    private static string setVarAddress(ref Dictionary<string, int> varibles, string line)
    {
        if (line.ToArray().Contains('@'))
        {
            if (varibles.ContainsKey(getVarName(line)))
                return varibles[getVarName(line)].ToString();
            else
                return "0";
        }
        return (line);
    }

    private static string getVarName(string _varName)
    {
        string varName = "";
        bool isCounted = false;
        for (int i = 0; i < _varName.ToArray().Length; i++)
        {
            if(isCounted)
                varName += _varName[i];
            if (_varName[i] == '#' || _varName[i] == '@')
                isCounted = true;
        }

        return (varName);
    }

    private static string getValue(string _varName)
    {
        string varName = "";
        for (int i = 0; i < _varName.ToArray().Length; i++)
        {
            if (_varName[i] == '#')
                return (varName);
            varName += _varName[i];
        }

        return (varName);
    }

    private static int compileLine(string line, string[] other, ref Dictionary<string, int> varibles, int addres)
    {
        if (line == "jump")
            return 0;
        if (line == "jumpif")
            return 1;
        if (line == "compare")
            return 2;
        if (line == "add")
            return 3;
        if (line == "sub")
            return 4;
        if (line == "mul")
            return 5;
        if (line == "div")
            return 6;
        if (line == "clone")
            return 7;
        if (line == "print")
            return 8;
        if (line == "skip")
            return 9;
        if (line == "out")
            return 10;
        if (line == "in")
            return 11;
        if (line == "")
            return 0;
        return (Convert.ToInt32(line));
    }
}