using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xml_parser.src.xml.dtd
{
    enum Instruction
    {
        REQUIRED,
        OPTIONAL,
        ONE_OR_MORE,
        ZERO_OR_MORE
    }

    internal class DTDSequence
    {
        // item in list is a pair of a name and instruction
        private List<Pair<DTDSequence, Pair<string, Instruction>>> sequence;
        private List<Pair<DTDSequence, Pair<string, Instruction>>> optionals;
        private int size;
        private string charData;

        public string CharData
        {
            get { return charData; }
            set { charData = value; }
        }

        public DTDSequence()
        {
            sequence = new List<Pair<DTDSequence, Pair<string, Instruction>>>();
            optionals = new List<Pair<DTDSequence, Pair<string, Instruction>>>();
            size = 0;
        }

        public void forEachElement(Action<Pair<DTDSequence, Pair<string, Instruction>>> action)
        {
            foreach (var pair in sequence)
                action(pair);
        }

        public void forEachOptional(Action<Pair<DTDSequence, Pair<string, Instruction>>> action)
        {
            foreach (var pair in optionals)
                action(pair);
        }

        public void forEach(Action<Pair<DTDSequence, Pair<string, Instruction>>> action)
        {
            foreach (var pair in sequence)
                action(pair);
            foreach (var pair in optionals)
                action(pair);
        }

        public Pair<DTDSequence, Pair<string, Instruction>> getItem(int index)
        {
            if (index > this.sequence.Count)
                throw new IndexOutOfRangeException();
            return this.sequence[index];
        }

        public Pair<DTDSequence, Pair<string, Instruction>> getOptional(int index)
        {
            if (index > this.optionals.Count)
                throw new IndexOutOfRangeException();
            return this.optionals[index];
        }

        public bool add(Pair<DTDSequence, Pair<string, Instruction>> item)
        {
            this.sequence.Add(item);
            bool res = this.sequence.Contains(item);
            if (res)
            {
                this.size++;
            }
            return res;
        }

        public bool addOptional(Pair<DTDSequence, Pair<string, Instruction>> item)
        {
            this.optionals.Add(item);
            bool res = this.optionals.Contains(item);
            if (res)
            {
                this.size++;
            }
            return res;
        }



        public Pair<DTDSequence, Pair<string, Instruction>> this[int i]
        {
            get
            {
                if (i > this.sequence.Count)
                {
                    return this.optionals[i - this.sequence.Count];
                }
                else
                    return this.sequence[i];
            }
            set
            {
                if (i > this.sequence.Count)
                {
                    this.optionals[i - this.sequence.Count] = value;
                }
                else
                    this.sequence[i] = value;
            }
        }
    }
}
