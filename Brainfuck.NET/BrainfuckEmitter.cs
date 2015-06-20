using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Brainfuck.NET
{
    internal class BrainfuckEmitter
    {
        private ILGenerator _g;
        private readonly Stack<Label> _endLabels = new Stack<Label>();
        private readonly Stack<Label> _headLabels = new Stack<Label>();

        public BrainfuckEmitter(ILGenerator generator)
        {
            if (generator == null)
                throw new ArgumentNullException("generator");
            _g = generator;
        }

        public void SetupLocals(int memoryCapacity)
        {
            _g.DeclareLocal(typeof(byte[]));
            _g.DeclareLocal(typeof(int));

            _g.Emit(OpCodes.Ldc_I4, memoryCapacity);
            _g.Emit(OpCodes.Newarr, typeof(byte));
            _g.Emit(OpCodes.Stloc_0);

            _g.Emit(OpCodes.Ldc_I4_0);
            _g.Emit(OpCodes.Stloc_1);
        }

        public void Finish()
        {
            if (_headLabels.Count > 0)
                throw new SyntaxErrorException("There are unmatched brackets.");
            if (_endLabels.Count > 0)
                throw new SyntaxErrorException("There are unmatched brackets.");

            _g.Emit(OpCodes.Ret);
        }

        internal void EmitLoopHead(int count)
        {
            for (int i = 0; i < count; i++)
                EmitLoopHead();
        }
        internal void EmitLoopHead()
        {

            Label headLbl = _g.DefineLabel();
            _headLabels.Push(headLbl);

            _g.MarkLabel(headLbl);

            PushLocals();
            _g.Emit(OpCodes.Ldelem_U1);
            _g.Emit(OpCodes.Ldc_I4_0);
            _g.Emit(OpCodes.Ceq);

            _endLabels.Push(_g.DefineLabel());
            _g.Emit(OpCodes.Brtrue, _endLabels.Peek());
        }

        internal void EmitLoopEnd(int count)
        {
            for (int i = 0; i < count; i++)
                EmitLoopEnd();
        }

        internal void EmitLoopEnd()
        {
            if (_headLabels.Count == 0)
                throw new SyntaxErrorException("Invalid ]. There was no opening token ([).");
            _g.Emit(OpCodes.Br, _headLabels.Pop());
            _g.MarkLabel(_endLabels.Pop());
        }

        internal void PushLocals()
        {
            _g.Emit(OpCodes.Ldloc_0);
            _g.Emit(OpCodes.Ldloc_1);
        }

        internal void EmitLoadMemory()
        {
            PushLocals();
            _g.Emit(OpCodes.Ldelema, typeof(byte));
            _g.Emit(OpCodes.Dup);
            _g.Emit(OpCodes.Ldobj, typeof(byte));
        }
        internal void EmitSaveMemory()
        {
            _g.Emit(OpCodes.Stobj, typeof(byte));
        }

        internal void EmitInt(int count)
        {
            switch (count)
            {
                case 0:
                    _g.Emit(OpCodes.Ldc_I4_0);
                    break;
                case 1:
                    _g.Emit(OpCodes.Ldc_I4_1);
                    break;
                case 2:
                    _g.Emit(OpCodes.Ldc_I4_2);
                    break;
                case 3:
                    _g.Emit(OpCodes.Ldc_I4_3);
                    break;
                case 4:
                    _g.Emit(OpCodes.Ldc_I4_4);
                    break;
                case 5:
                    _g.Emit(OpCodes.Ldc_I4_5);
                    break;
                case 6:
                    _g.Emit(OpCodes.Ldc_I4_6);
                    break;
                case 7:
                    _g.Emit(OpCodes.Ldc_I4_7);
                    break;
                case 8:
                    _g.Emit(OpCodes.Ldc_I4_8);
                    break;
                default:
                    if (count > 8)
                    {
                        _g.Emit(OpCodes.Ldc_I4, count);
                    }
                    else if (count < 0)
                    {
                        _g.Emit(OpCodes.Ldc_I4_M1);
                        if (count < -1)
                        {
                            _g.Emit(OpCodes.Mul);
                            EmitInt(Math.Abs(count));
                        }
                    }
                    break;
            }
        }

        internal void EmitCellIncrement(int count)
        {
            //++memory[ptr];
            EmitLoadMemory();
            EmitInt(count);
            _g.Emit(OpCodes.Add);
            _g.Emit(OpCodes.Conv_U1);
            EmitSaveMemory();
        }

        internal void EmitCellDecrement(int count)
        {
            //--memory[ptr];
            EmitLoadMemory();
            EmitInt(count);
            _g.Emit(OpCodes.Sub);
            _g.Emit(OpCodes.Conv_U1);
            EmitSaveMemory();
        }

        internal void EmitPtrIncrement(int count)
        {
            //++ptr;
            _g.Emit(OpCodes.Ldloc_1);
            EmitInt(count);
            _g.Emit(OpCodes.Add);
            _g.Emit(OpCodes.Stloc_1);
        }

        internal void EmitPtrDecrement(int count)
        {
            //--ptr;
            _g.Emit(OpCodes.Ldloc_1);
            EmitInt(count);
            _g.Emit(OpCodes.Sub);
            _g.Emit(OpCodes.Stloc_1);
        }

        private static MethodInfo _readMethod;
        private static readonly Type[] _readMethodParameters = null;

        internal void EmitConsoleRead(int counter)
        {
            _readMethod = _readMethod ?? typeof(Console).GetMethod("Read");

            for (int i = 0; i < counter; ++i)
                EmitConsoleRead();
        }
        internal void EmitConsoleRead()
        {
            //memory[ptr] = (byte)(Console.Read() & 0xFF);
            _g.Emit(OpCodes.Ldloc_0);
            _g.Emit(OpCodes.Ldloc_1);
            _g.EmitCall(OpCodes.Call, _readMethod, _readMethodParameters);
            _g.Emit(OpCodes.Ldc_I4, 0xFF);
            _g.Emit(OpCodes.And);
            _g.Emit(OpCodes.Conv_U1);
            _g.Emit(OpCodes.Stelem_I1);
        }

        private static MethodInfo _writeMethod;
        private static Type[] _writeMethodParameters;

        internal void EmitConsoleWrite(int counter)
        {
            _writeMethod = _writeMethod ?? typeof(Console).GetMethod("Write", new[] { typeof(char) });
            _writeMethodParameters = _writeMethodParameters ?? new[] { typeof(char) };

            for (int i = 0; i < counter; ++i)
                EmitConsoleWrite();
        }
        internal void EmitConsoleWrite()
        {
            //Console.Write((char)memory[ptr]);
            _g.Emit(OpCodes.Ldloc_0);
            _g.Emit(OpCodes.Ldloc_1);
            _g.Emit(OpCodes.Ldelem_U1);
            _g.EmitCall(OpCodes.Call, _writeMethod, _writeMethodParameters);
        }
    }
}
