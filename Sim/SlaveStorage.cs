using NModbus;

namespace Sim
{
    internal class SlaveStorage: ISlaveDataStore
    {
        public SlaveStorage()
        {
        }

        public IPointSource<bool> CoilDiscretes { get; } = new PointSource<bool>();

        public IPointSource<bool> CoilInputs { get; } = new PointSource<bool>();

        public IPointSource<ushort> HoldingRegisters { get; } = new PointSource<ushort>();

        public IPointSource<ushort> InputRegisters { get; } = new PointSource<ushort>();

    }
}