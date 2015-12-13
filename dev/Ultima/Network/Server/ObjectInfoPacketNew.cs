/***************************************************************************
 *   ObjectInfoPacket.cs
 *   Copyright (c) 2009 UltimaXNA Development Team
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 3 of the License, or
 *   (at your option) any later version.
 *
 ***************************************************************************/
#region usings
using UltimaXNA.Core.Network;
using UltimaXNA.Core.Network.Packets;
#endregion

namespace UltimaXNA.Ultima.Network.Server
{
    public class ObjectInfoPacketNew : RecvPacket
    {
        public readonly Serial Serial;
        public readonly ushort ItemID;
        public readonly byte Type;
        public readonly ushort Amount;
        public readonly short X;
        public readonly short Y;
        public readonly sbyte Z;
        public readonly byte Direction;
        public readonly ushort Hue;
        public readonly byte Flags;
        public readonly byte Layer;
        public readonly short trash;


        public bool IsMulti { get { return Type == 0x02; } }

        public ObjectInfoPacketNew(PacketReader reader)
            : base(0xF3, "ObjectInfoPacketNew")
        {
            reader.ReadInt16();
            Type = reader.ReadByte();
            Serial = reader.ReadInt32();
            ItemID = reader.ReadUInt16();

            Direction = (byte)reader.ReadByte();
            Amount = reader.ReadUInt16();
            Amount = reader.ReadUInt16();
            X = reader.ReadInt16();
            Y = reader.ReadInt16();
            Z = reader.ReadSByte();
            Layer = reader.ReadByte();
            Hue = reader.ReadUInt16();
            Flags = reader.ReadByte();

            reader.ReadUInt16();
            
        }
    }
}
