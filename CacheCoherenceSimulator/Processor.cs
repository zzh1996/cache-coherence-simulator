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

    enum Method
    {
        Snoop,
        Directory
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
        Invalid
    }

    class Processor
    {
        public State[] states;
        public int[] addrs;
        private Method method;
        private Assoc assoc;
        private Processor[] procs;
        private Random rnd;
        private TextBox log;

        public Processor(Method method, Assoc assoc, Processor[] procs, TextBox log)
        {
            states = new State[4];
            addrs = new int[4];
            this.method = method;
            this.assoc = assoc;
            this.procs = procs;
            rnd = new Random();
            this.log = log;
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
            log.AppendText("Message: " + m.ToString() + " " + addr.ToString() + Environment.NewLine);
            for (int i = 0; i < 4; i++)
            {
                if (!ReferenceEquals(procs[i], this))
                {
                    procs[i].ReceiveMessage(m, addr);
                }
            }
        }

        private void ReceiveMessage(Message m, int addr)
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
        }

        private void WriteBack(int addr)
        {
            log.AppendText("Write back to " + addr + Environment.NewLine);
        }

        public void Run(int addr, Oper rw)
        {
            int p = Find(addr);
            bool hit = addrs[p] == addr && states[p] != State.Invalid;
            log.AppendText(rw.ToString() + " " + (hit ? "hit" : "miss") + " " + addr + Environment.NewLine);
            switch (method)
            {
                case Method.Snoop:
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
                                    SendMessage(Message.ReadMiss, addr);
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
                                if (rw == Oper.Read)
                                    SendMessage(Message.ReadMiss, addr);
                                else
                                    SendMessage(Message.WriteMiss, addr);
                            }
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    break;
                case Method.Directory:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            addrs[p] = addr;
        }

    }
}
