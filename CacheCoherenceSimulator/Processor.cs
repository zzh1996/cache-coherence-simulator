using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CacheCoherenceSimulator
{
    enum State
    {
        Invalid,
        Shared,
        Own
    }

    enum Assoc
    {
        Direct,
        Two,
        Four
    }

    enum Oper
    {
        Read,
        Write
    }

    enum Message
    {
        ReadMiss,
        WriteMiss,
        Invalid,
        DeleteDir
    }

    class Dir
    {
        private bool[] cpus;
        public Dir()
        {
            cpus = new bool[4];
        }

        public void Add(int i)
        {
            cpus[i] = true;
        }

        public void Delete(int i)
        {
            cpus[i] = false;
        }

        public void DeleteAll()
        {
            for (int i = 0; i < 4; i++)
                cpus[i] = false;
        }

        public override string ToString()
        {
            string s = "";
            for (int i = 0; i < 4; i++)
            {
                if (cpus[i])
                    s += (char)('A' + i);
                else
                    s += ' ';
            }
            return s;
        }
    }

    class Processor
    {
        public State[] states;
        public int[] addrs;
        private bool directory;
        private Assoc assoc;
        private Processor[] procs;
        private Random rnd;
        private TextBox log;
        public Dir[] dirs;
        public int no;

        public Processor(bool directory, Assoc assoc, Processor[] procs, TextBox log, int no)
        {
            states = new State[4];
            addrs = new int[4];
            this.directory = directory;
            this.assoc = assoc;
            this.procs = procs;
            rnd = new Random();
            this.log = log;
            dirs = new Dir[8];
            for (int i = 0; i < 8; i++)
            {
                dirs[i] = new Dir();
            }
            this.no = no;
        }

        private void Log(string s)
        {
            log.AppendText("P" + no + ": " + s + Environment.NewLine);
        }

        private int Find(int addr)
        {
            int start, end;
            switch (assoc)
            {
                case Assoc.Direct:
                    start = addr % 4;
                    end = addr % 4 + 1;
                    break;
                case Assoc.Two:
                    start = addr % 2 * 2;
                    end = addr % 2 * 2 + 2;
                    break;
                case Assoc.Four:
                    start = 0;
                    end = 4;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            for (int i = start; i < end; i++)
                if (addrs[i] == addr && states[i] != State.Invalid)
                    return i;
            for (int i = start; i < end; i++)
                if (states[i] == State.Invalid)
                    return i;
            return rnd.Next(start, end); //随机替换
        }

        private void SendMessage(Message m, int addr)
        {
            Log("Message: " + m.ToString() + " " + addr.ToString());
            for (int i = 0; i < 4; i++)
            {
                if (!ReferenceEquals(procs[i], this))
                {
                    procs[i].ReceiveMessage(m, addr, no);
                }
                else
                {
                    ModifyDir(m, addr, no);
                }
            }
        }

        private void ModifyDir(Message m, int addr, int sender)
        {
            if (directory)
            {
                if (addr / 8 == no) //I'm the owner
                {
                    switch (m)
                    {
                        case Message.ReadMiss:
                            dirs[addr % 8].Add(sender);
                            Log("sharers add P" + sender);
                            break;
                        case Message.WriteMiss:
                        case Message.Invalid:
                            dirs[addr % 8].DeleteAll();
                            dirs[addr % 8].Add(sender);
                            Log("sharers become P" + sender + " only");
                            break;
                        case Message.DeleteDir:
                            dirs[addr % 8].Delete(sender);
                            Log("sharers remove P" + sender);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(m), m, null);
                    }
                }
            }
        }

        private void ReceiveMessage(Message m, int addr, int sender)
        {
            int p = Find(addr);
            if (addrs[p] == addr && states[p] != State.Invalid)
            {
                switch (states[p])
                {
                    case State.Invalid:
                        break;
                    case State.Shared:
                        if (m == Message.WriteMiss || m == Message.Invalid)
                            states[p] = State.Invalid;
                        break;
                    case State.Own:
                        if (m == Message.ReadMiss)
                        {
                            WriteBack(addr);
                            states[p] = State.Shared;
                        }
                        else if (m == Message.WriteMiss)
                        {
                            WriteBack(addr);
                            states[p] = State.Invalid;
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            ModifyDir(m, addr, sender);
        }

        private void WriteBack(int addr)
        {
            Log("Write back to " + addr);
        }

        public void Run(int addr, Oper rw)
        {
            int p = Find(addr);
            bool hit = addrs[p] == addr && states[p] != State.Invalid;
            Log(rw.ToString() + " " + (hit ? "hit" : "miss") + " " + addr);
            switch (states[p])
            {
                case State.Invalid:
                    if (rw == Oper.Read)
                    {
                        SendMessage(Message.ReadMiss, addr);
                        states[p] = State.Shared;
                    }
                    else
                    {
                        SendMessage(Message.WriteMiss, addr);
                        states[p] = State.Own;
                    }
                    break;
                case State.Shared:
                    if (rw == Oper.Read)
                    {
                        if (!hit)
                        {
                            if (directory) SendMessage(Message.DeleteDir, addrs[p]);
                            SendMessage(Message.ReadMiss, addr);
                        }
                    }
                    else
                    {
                        if (hit)
                            SendMessage(Message.Invalid, addr);
                        else
                            SendMessage(Message.WriteMiss, addr);
                        states[p] = State.Own;
                    }
                    break;
                case State.Own:
                    if (!hit)
                    {
                        WriteBack(addrs[p]);
                        if (directory) SendMessage(Message.DeleteDir, addrs[p]);
                        if (rw == Oper.Read)
                        {
                            SendMessage(Message.ReadMiss, addr);
                            states[p] = State.Shared;
                        }
                        else
                            SendMessage(Message.WriteMiss, addr);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            addrs[p] = addr;
        }
    }
}
