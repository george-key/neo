using FluentAssertions;
using Neo.Network.P2P.Payloads;
using Neo.Persistence;
using Neo.SmartContract;
using Neo.SmartContract.Native;
using Neo.VM;
using System;
using System.IO;
using System.Linq;
using System.Numerics;

namespace Neo.UnitTests.Extensions
{
    public static class Nep5NativeContractExtensions
    {
        internal class ManualWitness : IVerifiable
        {
            private readonly UInt160[] _hashForVerify;

            public Witness[] Witnesses
            {
                get => throw new NotImplementedException();
                set => throw new NotImplementedException();
            }

            public int Size => 0;

            public ManualWitness(params UInt160[] hashForVerify)
            {
                _hashForVerify = hashForVerify ?? new UInt160[0];
            }

            public void Deserialize(BinaryReader reader) { }

            public void DeserializeUnsigned(BinaryReader reader) { }

            public UInt160[] GetScriptHashesForVerifying(StoreView snapshot) => _hashForVerify;

            public void Serialize(BinaryWriter writer) { }

            public void SerializeUnsigned(BinaryWriter writer) { }
        }

        public static bool Transfer(this NativeContract contract, StoreView snapshot, byte[] from, byte[] to, BigInteger amount, bool signFrom)
        {
            var engine = new ApplicationEngine(TriggerType.Application,
                new ManualWitness(signFrom ? new UInt160(from) : null), snapshot, 0, true);

            engine.LoadScript(contract.Script);

            var script = new ScriptBuilder();
            script.EmitPush(amount);
            script.EmitPush(to);
            script.EmitPush(from);
            script.EmitPush(3);
            script.Emit(OpCode.PACK);
            script.EmitPush("transfer");
            engine.LoadScript(script.ToArray());

            if (engine.Execute() == VMState.FAULT)
            {
                return false;
            }

            var result = engine.ResultStack.Pop();
            result.Should().BeOfType(typeof(VM.Types.Boolean));

            return result.ToBoolean();
        }

        public static string[] SupportedStandards(this NativeContract contract)
        {
            var engine = new ApplicationEngine(TriggerType.Application, null, null, 0, testMode: true);

            engine.LoadScript(contract.Script);

            var script = new ScriptBuilder();
            script.EmitPush(0);
            script.Emit(OpCode.PACK);
            script.EmitPush("supportedStandards");
            engine.LoadScript(script.ToArray());

            engine.Execute().Should().Be(VMState.HALT);

            var result = engine.ResultStack.Pop();
            result.Should().BeOfType(typeof(VM.Types.Array));

            return (result as VM.Types.Array).ToArray()
                .Select(u => u.GetString())
                .ToArray();
        }

        public static BigInteger TotalSupply(this NativeContract contract, StoreView snapshot)
        {
            var engine = new ApplicationEngine(TriggerType.Application, null, snapshot, 0, true);

            engine.LoadScript(contract.Script);

            var script = new ScriptBuilder();
            script.EmitPush(0);
            script.Emit(OpCode.PACK);
            script.EmitPush("totalSupply");
            engine.LoadScript(script.ToArray());

            engine.Execute().Should().Be(VMState.HALT);

            var result = engine.ResultStack.Pop();
            result.Should().BeOfType(typeof(VM.Types.Integer));

            return (result as VM.Types.Integer).GetBigInteger();
        }

        public static BigInteger BalanceOf(this NativeContract contract, StoreView snapshot, byte[] account)
        {
            var engine = new ApplicationEngine(TriggerType.Application, null, snapshot, 0, true);

            engine.LoadScript(contract.Script);

            var script = new ScriptBuilder();
            script.EmitPush(account);
            script.EmitPush(1);
            script.Emit(OpCode.PACK);
            script.EmitPush("balanceOf");
            engine.LoadScript(script.ToArray());

            engine.Execute().Should().Be(VMState.HALT);

            var result = engine.ResultStack.Pop();
            result.Should().BeOfType(typeof(VM.Types.Integer));

            return (result as VM.Types.Integer).GetBigInteger();
        }

        public static BigInteger Decimals(this NativeContract contract)
        {
            var engine = new ApplicationEngine(TriggerType.Application, null, null, 0, testMode: true);

            engine.LoadScript(contract.Script);

            var script = new ScriptBuilder();
            script.EmitPush(0);
            script.Emit(OpCode.PACK);
            script.EmitPush("decimals");
            engine.LoadScript(script.ToArray());

            engine.Execute().Should().Be(VMState.HALT);

            var result = engine.ResultStack.Pop();
            result.Should().BeOfType(typeof(VM.Types.Integer));

            return (result as VM.Types.Integer).GetBigInteger();
        }

        public static string Symbol(this NativeContract contract)
        {
            var engine = new ApplicationEngine(TriggerType.Application, null, null, 0, testMode: true);

            engine.LoadScript(contract.Script);

            var script = new ScriptBuilder();
            script.EmitPush(0);
            script.Emit(OpCode.PACK);
            script.EmitPush("symbol");
            engine.LoadScript(script.ToArray());

            engine.Execute().Should().Be(VMState.HALT);

            var result = engine.ResultStack.Pop();
            result.Should().BeOfType(typeof(VM.Types.ByteArray));

            return result.GetString();
        }

        public static string Name(this NativeContract contract)
        {
            var engine = new ApplicationEngine(TriggerType.Application, null, null, 0, testMode: true);

            engine.LoadScript(contract.Script);

            var script = new ScriptBuilder();
            script.EmitPush(0);
            script.Emit(OpCode.PACK);
            script.EmitPush("name");
            engine.LoadScript(script.ToArray());

            engine.Execute().Should().Be(VMState.HALT);

            var result = engine.ResultStack.Pop();
            result.Should().BeOfType(typeof(VM.Types.ByteArray));

            return result.GetString();
        }
    }
}
