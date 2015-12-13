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
    public class PacketContainerPacket : RecvPacket
    {
        readonly short m_packetcount;
        public List<ObjectInfoPacketNew> packets;
         

        public PacketContainerPacket(PacketReader reader)
            : base(0xF7, "Packet container")
        {
            m_packetcount = reader.ReadInt16();
            packets = new List<ObjectInfoPacketNew>(m_packetcount);
            for (int i = 0; i < m_packetcount; i++)
            {
                int packetID = reader.ReadByte();
                if (packetID == 0xF3)
                {
                    ObjectInfoPacketNew packet = new ObjectInfoPacketNew(reader);
                    packets.Add(packet);
                }
            }    
        }
    }
}
