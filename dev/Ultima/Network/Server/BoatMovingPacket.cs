/***************************************************************************
 *   MobileIncomingPacket.cs
 *   Copyright (c) 2009 UltimaXNA Development Team
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 3 of the License, or
 *   (at your option) any later version.
 *
 ***************************************************************************/
#region usings
using System.Collections.Generic;
using UltimaXNA.Core.Network;
using UltimaXNA.Core.Network.Packets;
using UltimaXNA.Ultima.World.Entities.Mobiles;
#endregion

namespace UltimaXNA.Ultima.Network.Server
{
    public class BoatMovingPacket : RecvPacket
    {
        public readonly Serial m_serial;
        public readonly byte m_speed;
        public readonly byte m_movingDirection;
        public readonly byte m_facingDirection;
        public readonly short m_x;
        public readonly short m_y;
        public readonly short m_z;
        public readonly ushort m_count;

        public struct BoatItem
        {
            public Serial m_serial;
            public short m_x;
            public short m_y;
            public short m_z;
        }

        public readonly List<BoatItem> items; 


        public BoatMovingPacket(PacketReader reader)
            : base(0xF6, "Boat moving")
        {
            m_serial = reader.ReadInt32();
            m_speed = reader.ReadByte();
            m_movingDirection = reader.ReadByte();
            m_facingDirection = reader.ReadByte();
            m_x = reader.ReadInt16();
            m_y = reader.ReadInt16();
            m_z = reader.ReadInt16();
            m_count = reader.ReadUInt16();
            items = new List<BoatItem>(m_count);
            for (int i = 0; i < m_count; i++)
            {
                BoatItem item = new BoatItem();
                item.m_serial = reader.ReadInt32();
                item.m_x = reader.ReadInt16();
                item.m_y = reader.ReadInt16();
                item.m_z = reader.ReadInt16();
                items.Add(item);
            }
        }
    }
}
